using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone
{
    public class PhyisicsBoneIsAnimatedChanger : EditorWindow
    {
        private static PhysicsBoneIsAnimatedChangerEngine m_engine = new PhysicsBoneIsAnimatedChangerEngine();

        private bool m_showDetails = false;

        private Vector2 m_scrollPosition = Vector2.zero;

        [MenuItem("Yakumo890/PBTools/PhyisicsBoneIsAnimatedChanger")]
        static void ShowWindow()
        {
            var window = GetWindow<PhyisicsBoneIsAnimatedChanger>();
            window.titleContent = new GUIContent("Phyiscs Bone IsAnimated Changer");
        }

        private void OnGUI()
        {
            GUILayout.Label("PhysicsBoneのIsAnimatedを一括でOFFにするツール", EditorStyles.boldLabel);
            GUILayout.Space(20);

            m_engine.AvatarObject = EditorGUILayout.ObjectField("対象のアバター", m_engine.AvatarObject, typeof(GameObject), true) as GameObject;

            if (m_engine.hasNullAvatar())
            {
                return;
            }

            if (!m_engine.AvatarObjectIsAvatar())
            {
                EditorGUILayout.HelpBox("アバターでありません。Animatorコンポーネントが付いているか確認してください。", MessageType.Error, true);
                return;
            }
            GUILayout.Space(20);

            if (GUILayout.Button("一括でOFFにする"))
            {
                m_engine.TurnOffAll();

                Debug.Log("すべてのIsAnimatedをOFFに変更");
            }

            m_showDetails = EditorGUILayout.Foldout(m_showDetails, "詳細設定");
            if (m_showDetails)
            {
                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("PBがついているオブジェクト", EditorStyles.boldLabel);
                foreach (var objectName in m_engine.ObjectNames)
                {
                    EditorGUILayout.LabelField(objectName);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("PBのRoot Transform", EditorStyles.boldLabel);
                foreach (var rootTransformName in m_engine.RootTransformNames)
                {
                    EditorGUILayout.LabelField(rootTransformName);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("IsAnimated", EditorStyles.boldLabel);
                for (int i = 0; i < m_engine.Count; ++i)
                {
                    m_engine[i] = EditorGUILayout.Toggle(m_engine[i]);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
        }
    }

    public class PhysicsBoneIsAnimatedChangerEngine
    {
        delegate void PhysicsBoneHandler(VRCPhysBone physBone);

        private GameObject m_avatarObject = null;

        private VRCPhysBone[] m_physBones = null;
        private string[] m_objectNames = null;
        private string[] m_rootTransformNames = null;

        public GameObject AvatarObject
        {
            get
            {
                return m_avatarObject;
            }
            set
            {
                m_avatarObject = value;
                if (m_avatarObject != null)
                {
                    LoadPhysicsBones();
                }
            }
        }

        public int Count
        {
            get 
            {
                return m_physBones.Length;
            }
        }

        public string[] ObjectNames
        {
            get
            {
                return m_objectNames;
            }
        }

        public string[] RootTransformNames
        {
            get
            {
                return m_rootTransformNames;
            }
        }

        public bool this[int index]
        {
            get
            {
                return m_physBones[index].isAnimated;
            }
            set
            {
                m_physBones[index].isAnimated = value;
            }
        }


        public bool hasNullAvatar()
        {
            return AvatarObject == null;
        }


        public bool AvatarObjectIsAvatar()
        {
            return AvatarObject != null && AvatarUtility.IsAvatar(AvatarObject);
        }


        public void TurnOffAll()
        {
            foreach (var pb in m_physBones)
            {
                pb.isAnimated = false;
            }
        }


        public void LoadPhysicsBones()
        {
            m_physBones = m_avatarObject.GetComponentsInChildren<VRCPhysBone>(true);
            CreateObjectNames();
            CreateRootTransformNames();
        }


        private void CreateRootTransformNames()
        {
            m_rootTransformNames = new string[Count];

            for (int i = 0; i < Count; ++i)
            {
                var rootTransform = m_physBones[i].rootTransform;
                m_rootTransformNames[i] = rootTransform != null ? rootTransform.name : "None";
            }
        }


        private void CreateObjectNames()
        {
            m_objectNames = new string[Count];

            for (int i = 0; i < Count; ++i)
            {
                m_objectNames[i] = m_physBones[i].name;
            }
        }

    }
}
