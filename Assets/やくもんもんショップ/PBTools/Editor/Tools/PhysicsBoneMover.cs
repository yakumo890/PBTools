/*
Copyright (c) 2022 yakumo/yakumonmon shop
https://yakumonmon-shop.booth.pm/
This software is released under the MIT License
https://github.com/yakumo890/PBTools/License.txt
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone
{
    /// <summary>
    /// PhysicsBoneMoverのUI部分
    /// </summary>
    public class PhysicsBoneMover : EditorWindow
    {
        private static PhysicsBoneMoverEngine m_engine = new PhysicsBoneMoverEngine();

        private bool m_canDeleteSourcePBs;

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

            if (m_engine.SrcAvatarObject == null)
            {
                return;
            }

            if (!m_engine.CheckSrcIsAvatar())
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }

            m_engine.DestAvatarObject = EditorGUILayout.ObjectField("移動先のアバター", m_engine.DestAvatarObject, typeof(GameObject), true) as GameObject;

            if (m_engine.HasNullAvatar())
            {
                return;
            }

            if (!m_engine.CheckDestIsAvatar())
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }

            GUILayout.Space(20);

            m_engine.IgnoreHasNoRootTransform = GUILayout.Toggle(
                m_engine.IgnoreHasNoRootTransform,
                new GUIContent(
                    "移動先にRoot Transformがないなら無視する",
                    "PB(Collider)のRoot Transformと同じ名前のオブジェクトが移動先にないならPB(Collider)の移動をしない"));

            m_engine.IgnoreHasNoColliders = GUILayout.Toggle(
                m_engine.IgnoreHasNoColliders,
                new GUIContent(
                    "移動先にColliderがないなら無視する",
                    "PBについているColliderのどれか1つでも同じ名前のオブジェクトが移動先にないならPBの移動をしない"));

            m_engine.IgnoreHasNoMatchPathObject = GUILayout.Toggle(
                m_engine.IgnoreHasNoMatchPathObject,
                new GUIContent(
                    "パスが一致しなければ無視する",
                    "移動先に同じ名前のオブジェクトがあったとしても、ルートオブジェクトからのパス(階層)が合っていないなら無視する"));

            m_canDeleteSourcePBs = GUILayout.Toggle(m_canDeleteSourcePBs, 
                new GUIContent(
                    "移動後に元のPB(Collider)を削除する",
                    "移動したコライダーを元のアバターから削除する。移動していないものは削除しない"));

            if (GUILayout.Button("移動"))
            {
                var resultMoveColliders = m_engine.MovePhysBoneColliders();
                var resultMoveBones = m_engine.MovePhysBones();

                if (m_canDeleteSourcePBs)
                {
                    m_engine.RemoveCopiedComponent();
                }

                if (resultMoveBones && resultMoveColliders)
                {
                    Debug.Log("[PhysicsBoneMover] 移動完了");
                }
                else
                {
                    Debug.LogError("[PhysicsBoneMover] 移動失敗");
                }
            }
        }


        /// <summary>
        /// ヒエラルキーに変更があった場合の処理<br/>
        /// 移動元のアバターのPhysBone(Collider)を読み込む
        /// </summary>
        private static void OnChanged()
        {
            m_engine.ReloadSourceComponents();            
        }
    }

    /// <summary>
    /// PhysicsBoneMoverのエンジン
    /// </summary>
    public class PhysicsBoneMoverEngine
    {
        private GameObject m_srcAvatarObject;
        private GameObject m_destAvatarObject;

        private VRCPhysBone[] m_physBones;
        private List<VRCPhysBone> m_copiedPhysBones;

        private VRCPhysBoneColliderBase[] m_colliderBases;
        private List<VRCPhysBoneColliderBase> m_copiedColliderBase;

        // Root Transformと同じ名前のオブジェクトが移動先になかったら、このPB(Collider)を移動しない
        private bool m_ignoreHasNoRootTransform;

        // PBのCollidersのうちの1つでも移動先に同じオブジェクトがなかったら、このPBを移動しない
        private bool m_ignoreHasNoColliders;

        // 移動先にあるオブジェクトが同じ名前でも、パスが一致しなければ移動しない
        private bool m_ignoreHasNoMatchPathObject;


        /// <summary>
        /// コンストラクタ<br/>
        /// <br/>
        /// Root Transformのオブジェクトに対応するオブジェクトが移動先になかったら、移動しない<br/>
        /// PhysBoneのColliderオブジェクトに対応するオブジェクトが移動先になかったら、移動しない<br/>
        /// 対応するオブジェクト名が、パスが完全に一致しなければ移動しない<br/>
        /// </summary>
        public PhysicsBoneMoverEngine()
        {
            m_ignoreHasNoRootTransform = true;
            m_ignoreHasNoColliders = true;
            m_ignoreHasNoMatchPathObject = true;

            m_copiedPhysBones = new List<VRCPhysBone>();
            m_copiedColliderBase = new List<VRCPhysBoneColliderBase>();
        }


        /// <summary>
        /// 移動元のアバターオブジェクト
        /// </summary>
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
                m_copiedPhysBones.Clear();
                m_copiedColliderBase.Clear();
            }
        }


        /// <summary>
        /// 移動先のアバターオブジェクト
        /// </summary>
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


        /// <summary>
        /// Root Transformと同じ名前のオブジェクトが移動先になかったら、このPB(Collider)を移動しない
        /// </summary>
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


        /// <summary>
        /// PBのCollidersのうちの1つでも移動先に同じオブジェクトがなかったら、このPBを移動しない
        /// </summary>
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


        /// <summary>
        /// 移動先にあるオブジェクトが同じ名前でも、パスが一致しなければ移動しない
        /// </summary>
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


        /// <summary>
        /// 移動元のアバターオブジェクトがアバターであるか
        /// </summary>
        /// <returns>true: アバター<br/>false: アバターでない</returns>
        public bool CheckSrcIsAvatar()
        {
            return CheckIsAvatar(m_srcAvatarObject);
        }


        /// <summary>
        /// 移動先のアバターオブジェクトがアバターであるか
        /// </summary>
        /// <returns>true: アバター<br/>false: アバターでない</returns>
        public bool CheckDestIsAvatar()
        {
            return CheckIsAvatar(m_destAvatarObject);
        }


        /// <summary>
        /// アバターオブジェクトがnullか
        /// </summary>
        /// <returns>true: 移動元か移動先のアバターのどちらかがnull<br/>false: どちらもnullでない</returns>
        public bool HasNullAvatar()
        {
            return m_srcAvatarObject == null || m_destAvatarObject == null;
        }


        /// <summary>
        /// PhsyBoneColliderを移動する
        /// </summary>
        /// <returns>true: 処理成功<br/>false: 処理失敗</returns>
        public bool MovePhysBoneColliders()
        {
            if (HasNullAvatar())
            {
                return false;
            }


            // PBColliderを移動するための一時オブジェクト
            var tmp = new GameObject();
            tmp.name = "tmp_physicsBoneCollider_it_can_be_deleted";

            foreach (var pbc in m_colliderBases)
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

                //コンポーネントをコピー済みのコンポーネントとして登録する
                m_copiedColliderBase.Add(pbc);

            }

            if (tmp != null)
            {
                Object.DestroyImmediate(tmp);
            }

            return true;
        }


        /// <summary>
        /// PhsyBoneを移動する
        /// </summary>
        /// <returns>true: 処理成功<br/>false: 処理失敗</returns>
        public bool MovePhysBones()
        {
            if (HasNullAvatar())
            {
                return false;
            }


            // PBColliderを移動するための一時オブジェクト
            var tmp = new GameObject();
            tmp.name = "tmp_physicsBone_it_can_be_deleted";

            foreach (var pb in m_physBones)
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
                    Transform colliderObjectTransform = null;
                    colliderObjectTransform = GetSameNameObjectFromDest(col.gameObject);

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

                // コンポーネントをコピーしたら、コピー済みのコンポーネントとして登録する
                m_copiedPhysBones.Add(pb);
            }

            if (tmp != null)
            {
                Object.DestroyImmediate(tmp);
            }

            return true;
        }


        /// <summary>
        /// 移動したPhysBone(Collider)を移動元から削除する
        /// </summary>
        public void RemoveCopiedComponent()
        {
            foreach (var pb in m_copiedPhysBones)
            {
                if (pb != null)
                {
                    Object.DestroyImmediate(pb);
                }
            }

            foreach (var col in m_copiedColliderBase)
            {
                if (col != null)
                {
                    Object.DestroyImmediate(col);
                }
            }
        }


        /// <summary>
        /// 移動元のアバターのPhysBone(Collider)を削除する
        /// </summary>
        public void ReloadSourceComponents()
        {
            if (m_srcAvatarObject != null)
            {
                m_physBones = m_srcAvatarObject.GetComponentsInChildren<VRCPhysBone>(true);
                m_colliderBases = m_srcAvatarObject.GetComponentsInChildren<VRCPhysBoneColliderBase>(true);
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


        /// <summary>
        /// オブジェクトがアバターかどうか
        /// </summary>
        /// <param name="avatar">対象のオブジェクト</param>
        /// <returns>true: アバターである<br/>false: アバターでない</returns>
        private bool CheckIsAvatar(GameObject avatar)
        {
            return avatar != null && AvatarUtility.IsAvatar(avatar);
        }
    }
}