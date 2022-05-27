using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone
{
    public class PhysicsBoneExtractor : EditorWindow
    {
        private PhysicsBoneExtractorEngine m_engine;

        private GameObject m_avatarObject = null;
        private GameObject m_physBoneObject = null;
        private GameObject m_collidersRoot = null;

        private bool m_isIgnoreInactive = false;
        private bool m_isIgnoreEditorOnly = false;

        private bool m_canReplaceTransform = true;

        private bool m_willExtractPhysBone = true;
        private bool m_willExtractColliders = false;

        // コンテンツにインデントを付けるときのインデント幅(px)
        private const int IndentWidth = 25;

        private const string PBPhysBoneObjectName = "PB";
        private const string ColliderRootObjectName = "PBC";
        private string m_colliderPrefix = "PBC";

        [MenuItem("Yakumo890/PBTools/PhysicsBoneExtractor")]
        static void ShowWindow()
        {
            var window = GetWindow<PhysicsBoneExtractor>();
            window.titleContent = new GUIContent("Physics Bone Extractor");
        }


        private void OnGUI()
        {
            GUILayout.Label("PhysicsBoneを一箇所にまとめるツール", EditorStyles.boldLabel);
            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            m_avatarObject = EditorGUILayout.ObjectField("対象のアバター", m_avatarObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                if (m_engine == null)
                {
                    m_engine = new PhysicsBoneExtractorEngine(m_avatarObject);
                }
                else
                {
                    m_engine.AvatarObject = m_avatarObject;
                }
            }

            if (m_avatarObject == null)
            {
                return;
            }

            if (!AvatarUtility.IsAvatar(m_avatarObject))
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }
            GUILayout.Space(20);

            EditorGUILayout.LabelField("無視するオブジェクトの設定");

            GUILayout.BeginHorizontal();
            GUILayout.Space(IndentWidth);
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            m_isIgnoreInactive = GUILayout.Toggle(m_isIgnoreInactive, "非アクティブオブジェクトを無視");
            if (EditorGUI.EndChangeCheck())
            {
                m_engine.IgnoreInactive = m_isIgnoreInactive;
            }

            EditorGUI.BeginChangeCheck();
            m_isIgnoreEditorOnly = GUILayout.Toggle(m_isIgnoreEditorOnly, "EditorOnlyタグが付いたオブジェクトを無視");
            if (EditorGUI.EndChangeCheck())
            {
                m_engine.IgnoreEditorOnly = m_isIgnoreEditorOnly;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("プロパティに関する設定");

            GUILayout.BeginHorizontal();
            GUILayout.Space(IndentWidth);
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            m_canReplaceTransform = GUILayout.Toggle(
                m_canReplaceTransform,
                new GUIContent("Root Transformの変更を許可",
                "Root TransoformがNoneの場合、PhysBone(Collider)がついていたオブジェクトをRoot Transformにセットする"));
            if (EditorGUI.EndChangeCheck())
            {
                m_engine.CanReplaceRootTransform = m_canReplaceTransform;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Physics Boneに関する設定");

            GUILayout.BeginHorizontal();
            GUILayout.Space(IndentWidth);

            GUILayout.BeginVertical();
            m_willExtractPhysBone = GUILayout.Toggle(m_willExtractPhysBone, "PhysicsBoneをまとめる");
            if (m_willExtractPhysBone)
            {
                m_physBoneObject = EditorGUILayout.ObjectField(
                    new GUIContent("対象のオブジェクト",
                    "このオブジェクトにすべてのPhyisBoneを付けます。\n指定しなければ\"PB\"というオブジェクトを自動生成します。"),
                    m_physBoneObject, typeof(GameObject), true) as GameObject;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Phyisics Bone Colliderに関する設定");

            GUILayout.BeginHorizontal();
            GUILayout.Space(IndentWidth);

            GUILayout.BeginVertical();
            m_willExtractColliders = GUILayout.Toggle(m_willExtractColliders, "コライダーもまとめる");
            if (m_willExtractColliders)
            {
                m_collidersRoot = EditorGUILayout.ObjectField(
                    new GUIContent("ルートオブジェクト",
                    "複数生成されるコライダーオブジェクトをまとめるためのルートオブジェクトです。\nオブジェクトを指定しなければ\"PBC\"というオブジェクトを自動生成します。"),
                    m_collidersRoot,
                    typeof(GameObject), true) as GameObject;
                m_colliderPrefix = EditorGUILayout.TextField(
                    new GUIContent("プレフィックス", "コライダーオブジェクトの名前の先頭につける識別子です。\n{プレフィックス}_{元のコライダーオブジェクトの名前}といった命名になります。"),
                    m_colliderPrefix);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("まとめる"))
            {
                if (m_engine == null)
                {
                    return;
                }

                bool extractedBones = false;
                bool extractedColliders = false;
                if (m_willExtractPhysBone)
                {
                    if (m_physBoneObject)
                    {
                        extractedBones = m_engine.ExtractPhysBones(m_physBoneObject);
                    }
                    else
                    {
                        extractedBones = m_engine.ExtractPhysBones(PBPhysBoneObjectName);
                    }
                }

                if (m_willExtractColliders)
                {
                    if (m_collidersRoot)
                    {
                        extractedColliders = m_engine.ExtractColliders(m_collidersRoot, m_colliderPrefix);
                    }
                    else
                    {
                        extractedColliders = m_engine.ExtractColliders(ColliderRootObjectName, m_colliderPrefix);
                    }
                }

                if (extractedBones && extractedColliders)
                {
                    Debug.Log("[PhysicsBoneExtractor] まとめ成功");
                }
                else
                {
                    Debug.LogError("[PhysicsBoneExtractor] まとめ失敗");
                }
            }
        }
    }


    public class PhysicsBoneExtractorEngine
    {
        private GameObject m_avatarObject;

        private bool m_ignoreInactive;
        private bool m_ignoreEditorOnly;
        private bool m_canReplaceRootTransform;


        public PhysicsBoneExtractorEngine(GameObject avatarObject) : this(avatarObject, false, false, true)
        {
        }


        public PhysicsBoneExtractorEngine(GameObject avatarObject, bool ignoreInactive, bool ignoreEditorOnly, bool canReplaceTransform)
        {
            m_avatarObject = avatarObject;
            m_ignoreInactive = ignoreInactive;
            m_ignoreEditorOnly = ignoreEditorOnly;
            m_canReplaceRootTransform = canReplaceTransform;
        }


        public GameObject AvatarObject
        {
            get
            {
                return m_avatarObject;
            }

            set
            {
                m_avatarObject = value;
            }
        }


        public bool IgnoreInactive
        {
            get
            {
                return m_ignoreInactive;
            }

            set
            {
                m_ignoreInactive = value;
            }
        }


        public bool IgnoreEditorOnly
        {
            get
            {
                return m_ignoreEditorOnly;
            }

            set
            {
                m_ignoreEditorOnly = value;
            }
        }


        public bool CanReplaceRootTransform
        {
            get
            {
                return m_canReplaceRootTransform;
            }

            set
            {
                m_canReplaceRootTransform = value;
            }
        }


        public bool ExtractColliders(GameObject target, string colliderObjectPrefix)
        {
            if (m_avatarObject == null)
            {
                return false;
            }

            if (colliderObjectPrefix == null)
            {
                return false;
            }

            var colliders = m_avatarObject.GetComponentsInChildren<VRCPhysBoneCollider>(!m_ignoreInactive);

            // コライダーとそれを参照するPBの対応を保存するコンテナ
            var correspondings = new Dictionary<VRCPhysBoneColliderBase, List<VRCPhysBone>>();

            // PBがついているオブジェクトが非アクティブであるかどうかに関係なく、PBすべてを取得
            // PBが非アクディブでも、それが参照するコライダーが非アクティブかどうかは不明なため
            var pbs = m_avatarObject.GetComponentsInChildren<VRCPhysBone>(true);
            foreach (var pb in pbs)
            {
                AddColliderCorrespondings(pb, correspondings);
                pb.colliders.Clear();
            }

            // まずは、PBから参照されているコライダーを移動する
            foreach (var pair in correspondings)
            {
                var collider = pair.Key;

                // 非アクティブのオブジェクトを無視する設定で、オブジェクトが非アクティブなら無視
                if (m_ignoreInactive && !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                // EditorOnlyのオブジェクトを無視する設定で、オブジェクトがEditorOnlyなら無視
                if (m_ignoreEditorOnly && collider.gameObject.IsEditorOnly())
                {
                    continue;
                }

                //Root Transformを変更して良い場合、Root Transformに何もセットされていなかったら自身のオブジェクトをセットする
                if (m_canReplaceRootTransform && collider.rootTransform == null)
                {
                    collider.rootTransform = collider.transform;
                }

                var objectNmae = colliderObjectPrefix + "_" + collider.name;
                var pbc = AvatarUtility.CreateAvatarObject(target, objectNmae);

                ComponentUtility.MoveComponent(collider, pbc);

                // PhysBoneに移動後のコライダーを付け直す
                foreach (var pb in pair.Value)
                {
                    pb.colliders.Add(pbc.GetComponent<VRCPhysBoneColliderBase>());
                }
            }

            foreach (var collider in colliders)
            {
                if (collider == null)
                {
                    continue;
                }

                // EditorOnlyのオブジェクトを無視する設定で、オブジェクトがEditorOnlyなら無視
                if (m_ignoreEditorOnly && collider.gameObject.IsEditorOnly())
                {
                    continue;
                }

                //Root Transformを変更して良い場合、Root Transformに何もセットされていなかったら自身のオブジェクトをセットする
                if (m_canReplaceRootTransform && collider.rootTransform == null)
                {
                    collider.rootTransform = collider.transform;
                }

                var objectNmae = colliderObjectPrefix + "_" + collider.name;
                var pbc = AvatarUtility.CreateAvatarObject(target, objectNmae);

                ComponentUtility.MoveComponent(collider, pbc);
            }

            return true;
        }


        public bool ExtractColliders(string pbColliderRootObjectName, string colliderObjectPrefix)
        {
            if (pbColliderRootObjectName == null)
            {
                return false;
            }

            var target = AvatarUtility.CreateAvatarObject(m_avatarObject, pbColliderRootObjectName, false);
            ExtractColliders(target, colliderObjectPrefix);

            return true;
        }


        public bool ExtractPhysBones(GameObject targetObject)
        {
            if (m_avatarObject == null)
            {
                return false;
            }

            var components = m_avatarObject.GetComponentsInChildren<VRCPhysBone>(!m_ignoreInactive);

            foreach (var pb in components)
            {
                // EditorOnlyのオブジェクトを無視する設定で、オブジェクトがEditorOnlyなら無視
                if (m_ignoreEditorOnly && pb.gameObject.IsEditorOnly())
                {
                    continue;
                }

                //Root Transformを変更して良い場合、Root Transformに何もセットされていなかったら自身のオブジェクトをセットする
                if (m_canReplaceRootTransform && pb.rootTransform == null)
                {
                    pb.rootTransform = pb.gameObject.transform;
                }

                ComponentUtility.MoveComponent(pb, targetObject);
            }

            return true;
        }


        public bool ExtractPhysBones(string pbObjectName)
        {
            if (pbObjectName == null)
            {
                return false;
            }

            var target = AvatarUtility.CreateAvatarObject(m_avatarObject, pbObjectName, false);
            ExtractPhysBones(target);

            return true;
        }


        private void AddColliderCorrespondings(VRCPhysBone pb, Dictionary<VRCPhysBoneColliderBase, List<VRCPhysBone>> correspondings)
        {
            foreach (var collider in pb.colliders)
            {
                if (!correspondings.ContainsKey(collider))
                {
                    correspondings.Add(collider, new List<VRCPhysBone>());
                }
                correspondings[collider].Add(pb);
            }
        }
    }
}