using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone
{
    public class PhysicsBoneMover : EditorWindow
    {
        private static PhysicsBoneMoverEngine m_engine = new PhysicsBoneMoverEngine();

        [MenuItem("Yakumo890/PBTools/PhysicsBoneMover")]
        static void ShowWindow()
        {
            var window = GetWindow<PhysicsBoneMover>();
            window.titleContent = new GUIContent("Physics Bone Mover");

            EditorApplication.hierarchyChanged += OnChanged;
        }


        private void OnGUI()
        {
            GUILayout.Label("PhysicsBoneを別のアバターに移動するツール", EditorStyles.boldLabel);
            GUILayout.Space(20);

            m_engine.SrcAvatarObject = EditorGUILayout.ObjectField("移動元のアバター", m_engine.SrcAvatarObject, typeof(GameObject), true) as GameObject;

            if (m_engine.CheckSrcIsAvatar())
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }

            m_engine.DestAvatarObject = EditorGUILayout.ObjectField("移動先のアバター", m_engine.DestAvatarObject, typeof(GameObject), true) as GameObject;

            if (m_engine.CheckDestIsAvatar())
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }

            if (m_engine.HasNullAvatar())
            {
                return;
            }

            GUILayout.Space(20);

            m_engine.IgnoreHasNoRootTransform = EditorGUILayout.Toggle(
                new GUIContent(
                    "移動先にRoot Transformがないなら無視する",
                    "PB(Collider)のRoot Transformと同じ名前のオブジェクトが移動先にないならPB(Collider)の移動をしない"),
                m_engine.IgnoreHasNoRootTransform);

            m_engine.IgnoreHasNoColliders = EditorGUILayout.Toggle(
                new GUIContent(
                    "移動先にColliderがないなら無視する",
                    "PBについているColliderのどれか1つでも同じ名前のオブジェクトが移動先にないならPBの移動をしない"),
                m_engine.IgnoreHasNoColliders);

            m_engine.IgnoreHasNoMatchPathObject = EditorGUILayout.Toggle(
                new GUIContent(
                    "パスが一致しなければ無視する",
                    "移動先に同じ名前のオブジェクトがあったとしても、ルートオブジェクトからのパス(階層)が合っていないなら無視する"
                ),
                m_engine.IgnoreHasNoMatchPathObject);

            m_engine.CanDeleteSourcePBs = EditorGUILayout.Toggle("移動後に元のPB(Collider)を削除する", m_engine.CanDeleteSourcePBs);

            if (GUILayout.Button("移動"))
            {
                m_engine.MovePhysBoneColliders();
                m_engine.MovePhysBones();
            }
        }


        private static void OnChanged()
        {
            m_engine.ReloadSourceComponents();
        }
    }

    public class PhysicsBoneMoverEngine
    {
        private GameObject m_srcAvatarObject;
        private GameObject m_destAvatarObject;

        private VRCPhysBone[] m_physicsBones;
        private VRCPhysBoneColliderBase[] m_physBoneColliderBases;

        // Root Transformと同じ名前のオブジェクトが移動先になかったら、このPB(Collider)を無視する
        private bool m_ignoreHasNoRootTransform;

        // PBのCollidersのうちの1つでも移動先に同じオブジェクトがなかったら、このPBを無視する
        private bool m_ignoreHasNoColliders;

        // 移動したあとに元のアバターのPBを消す
        private bool m_canDeleteSourcePBs;

        // 移動先にあるオブジェクトが同じ名前でも、パスが一致しなければ無視する
        private bool m_ignoreHasNoMatchPathObject;


        public PhysicsBoneMoverEngine()
        {
            m_ignoreHasNoRootTransform = true;
            m_ignoreHasNoColliders = true;
            m_canDeleteSourcePBs = false;
            m_ignoreHasNoMatchPathObject = true;
        }


        public GameObject SrcAvatarObject
        {
            get
            {
                return m_srcAvatarObject;
            }
            set
            {
                m_srcAvatarObject = value;
                ReloadSourceComponents();
            }
        }


        public GameObject DestAvatarObject
        {
            get
            {
                return m_destAvatarObject;
            }
            set
            {
                m_destAvatarObject = value;
            }
        }


        public bool IgnoreHasNoRootTransform
        {
            get
            {
                return m_ignoreHasNoRootTransform;
            }
            set
            {
                m_ignoreHasNoRootTransform = value;
            }
        }


        public bool IgnoreHasNoColliders
        {
            get
            {
                return m_ignoreHasNoColliders;
            }
            set
            {
                m_ignoreHasNoColliders = value;
            }
        }


        public bool CanDeleteSourcePBs
        {
            get
            {
                return m_canDeleteSourcePBs;
            }
            set
            {
                m_canDeleteSourcePBs = value;
            }
        }


        public bool IgnoreHasNoMatchPathObject
        {
            get
            {
                return m_ignoreHasNoMatchPathObject;
            }
            set
            {
                m_ignoreHasNoMatchPathObject = value;
            }
        }


        public bool CheckSrcIsAvatar()
        {
            return CheckIsAvatar(m_srcAvatarObject);
        }


        public bool CheckDestIsAvatar()
        {
            return CheckIsAvatar(m_destAvatarObject);
        }


        public bool HasNullAvatar()
        {
            return m_srcAvatarObject == null || m_destAvatarObject == null;
        }


        public void MovePhysBoneColliders()
        {
            if (HasNullAvatar())
            {
                return;
            }


            // PBColliderを移動するための一時オブジェクト
            var tmp = new GameObject();
            tmp.name = "tmp_physicsBoneCollider_it_can_be_deleted";

            foreach (var pbc in m_physBoneColliderBases)
            {
                // Colliderがついているオブジェクトと同じ名前の、移動先のオブジェクトを取得
                Transform targetTransform = GetSameNameObjectFromDest(pbc.gameObject);

                // 無いなら無視
                if (targetTransform == null)
                {
                    continue;
                }

                Transform targetRootTransform = null;
                if (pbc.rootTransform != null)
                {
                    // Root Transformが設定されている場合、移動先にある同じ名前のオブジェクトを取得
                    targetRootTransform = GetSameNameObjectFromDest(pbc.rootTransform.gameObject);

                    // Root Transformが移動先にない場合に無視する設定で、移動先にないなら無視
                    if (m_ignoreHasNoRootTransform && targetRootTransform == null)
                    {
                        continue;
                    }
                }

                // コンポーネントを、コピー元 → tmpオブジェクト → コピー先とコピーする
                // なぜ、このようにするかというと
                // コンポーネントをコピーしたあとに、値の変更をすることになるが
                // ComponentUtility.PasteComponentのあとは、ペーストしたコンポーネントを取得できない
                // そのため、GetComponentでペースト後のコンポーネントを取得するが
                // 複数のPBCが存在すると、どれに対して値を適用すればよいかわからなくなってしまうので
                // (Transform以外の)コンポーネントがついていないオブジェクトに一旦退避する
                ComponentUtility.CopyPaseteComponentAsNew(pbc, tmp);

                var tmpPBC = tmp.GetComponent<VRCPhysBoneColliderBase>();
                tmpPBC.rootTransform = targetRootTransform;

                ComponentUtility.CopyPaseteComponentAsNew(tmpPBC, targetTransform.gameObject);

                // tmpオブジェクトは使い回すので、付けたコンポーネントは毎回削除する
                ComponentUtility.DeleteComponent(tmpPBC);

                //もしコピー元のコンポーネントを削除して良いなら、削除する
                if (m_canDeleteSourcePBs)
                {
                    ComponentUtility.DeleteComponent(pbc);
                }
            }

            Object.DestroyImmediate(tmp);
        }


        public void MovePhysBones()
        {
            if (HasNullAvatar())
            {
                return;
            }


            // PBColliderを移動するための一時オブジェクト
            var tmp = new GameObject();
            tmp.name = "tmp_physicsBone_it_can_be_deleted";

            foreach (var pb in m_physicsBones)
            {
                // PBがついているオブジェクトと同じ名前の、移動先のオブジェクトを取得
                Transform targetTransform = GetSameNameObjectFromDest(pb.gameObject);

                if (targetTransform == null)
                {
                    continue;
                }

                Transform targetRootTransform = null;
                if (pb.rootTransform != null)
                {
                    // Root Transformが設定されている場合、移動先にある同じ名前のオブジェクトを取得
                    targetRootTransform = GetSameNameObjectFromDest(pb.rootTransform.gameObject);

                    // Root Transformが移動先にない場合に無視する設定で、移動先にないなら無視
                    if (m_ignoreHasNoRootTransform && targetRootTransform == null)
                    {
                        continue;
                    }
                }

                // PBのコライダーを移動先のアバターから探す
                var colliders = new List<VRCPhysBoneColliderBase>();
                var ignore = false;
                foreach (var col in pb.colliders)
                {
                    var colliderObjectTransform = GetSameNameObjectFromDest(col.gameObject);
                    if (m_ignoreHasNoColliders && colliderObjectTransform == null)
                    {
                        ignore = true;
                        break;
                    }

                    if (colliderObjectTransform != null)
                    {
                        colliders.Add(colliderObjectTransform.gameObject.GetComponent<VRCPhysBoneColliderBase>());
                    }
                }

                // コライダーが1つでも移動先になければ無視
                if (ignore)
                {
                    continue;
                }

                // コンポーネントを、コピー元 → tmpオブジェクト → コピー先とコピーする
                // なぜ、このようにするかというと
                // コンポーネントをコピーしたあとに、値の変更をすることになるが
                // ComponentUtility.PasteComponentのあとは、ペーストしたコンポーネントを取得できない
                // そのため、GetComponentでペースト後のコンポーネントを取得するが
                // 複数のPBCが存在すると、どれに対して値を適用すればよいかわからなくなってしまうので
                // (Transform以外の)コンポーネントがついていないオブジェクトに一旦退避する
                ComponentUtility.CopyPaseteComponentAsNew(pb, tmp);

                var tmpPB = tmp.GetComponent<VRCPhysBone>();
                tmpPB.rootTransform = targetRootTransform;
                tmpPB.colliders = colliders;

                // 移動先のコライダーコンポーネントををリストに追加
                ComponentUtility.CopyPaseteComponentAsNew(tmpPB, targetTransform.gameObject);

                // tmpオブジェクトは使い回すので、付けたコンポーネントは毎回削除する
                ComponentUtility.DeleteComponent(tmpPB);

                //もしコピー元のコンポーネントを削除して良いなら、削除する
                if (m_canDeleteSourcePBs)
                {
                    ComponentUtility.DeleteComponent(pb);
                }
            }

            Object.DestroyImmediate(tmp);
        }


        public void ReloadSourceComponents()
        {
            if (m_srcAvatarObject != null)
            {
                m_physicsBones = m_srcAvatarObject.GetComponentsInChildren<VRCPhysBone>(true);
                m_physBoneColliderBases = m_srcAvatarObject.GetComponentsInChildren<VRCPhysBoneColliderBase>(true);
            }
        }

        /// <summary>
        /// 移動先のアバターから、同じ名前のオブジェクトを取得する
        /// </summary>
        /// <param name="srcObject"></param>
        /// <returns>同じ名前があればそのオブジェクトのTransform<br />なければnull</returns>
        /// <remarks>パスが一致しなければ無視する設定の場合、パスが一致しなければnull</remarks>
        private Transform GetSameNameObjectFromDest(GameObject srcObject)
        {
            if (m_ignoreHasNoMatchPathObject)
            {
                // パスが一致しなければ無視する設定ならば、フルパス(ルートを除く)を取得してFindする
                return m_destAvatarObject.transform.Find(srcObject.GetFullPath(true));
            }
            else
            {
                //オブジェクトの名前を再帰的にFindする
                return m_destAvatarObject.transform.FindRecursive(srcObject.name);
            }
        }


        private bool CheckIsAvatar(GameObject avatar)
        {
            return avatar != null && AvatarUtility.IsAvatar(avatar);
        }
    }
}