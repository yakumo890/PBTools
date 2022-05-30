/*
Copyright (c) 2022 yakumo/yakumonmon shop
https://yakumonmon-shop.booth.pm/
This software is released under the MIT License
https://github.com/yakumo890/PBTools/License.txt
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone
{
    /// <summary>
    /// PhyisicsBoneIsAnimatedChangerのUI部分
    /// </summary>
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

            EditorApplication.hierarchyChanged += OnChanged;
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


        /// <summary>
        /// ヒエラルキーに変更があった場合の処理<br/>
        /// アバターが持つPB一覧を取得し直す
        /// </summary>
        private static void OnChanged()
        {
            m_engine.LoadPhysicsBones();
        }
    }


    /// <summary>
    /// PhysicsBoneIsAnimatedChangerのエンジン
    /// </summary>
    public class PhysicsBoneIsAnimatedChangerEngine
    {
        delegate void PhysicsBoneHandler(VRCPhysBone physBone);

        private GameObject m_avatarObject = null;

        private VRCPhysBone[] m_physBones = null;
        private string[] m_objectNames = null;
        private string[] m_rootTransformNames = null;

        /// <summary>
        /// 対象のアバターオブジェクト
        /// </summary>
        public GameObject AvatarObject
        {
            get
            {
                return m_avatarObject;
            }
            set
            {
                m_avatarObject = value;
                LoadPhysicsBones();
            }
        }


        /// <summary>
        /// アバターが持つPhysBoneの数
        /// </summary>
        public int Count
        {
            get 
            {
                return m_physBones.Length;
            }
        }


        /// <summary>
        /// PhysBoneがついているオブジェクト名の一覧
        /// </summary>
        public string[] ObjectNames
        {
            get
            {
                return m_objectNames;
            }
        }


        /// <summary>
        /// PhysBoneのRootTransformにセットされているオブジェクト名の一覧<br/>
        /// nullの場合は"None"になる
        /// </summary>
        public string[] RootTransformNames
        {
            get
            {
                return m_rootTransformNames;
            }
        }


        /// <summary>
        /// PhysBoneに配列のようにアクセスできる
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>指定したインデックスにあるPhysBone</returns>
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


        /// <summary>
        /// アバターオブジェクトがnullかどうか
        /// </summary>
        /// <returns>true: オブジェクトがnull<br/>false: nullでない</returns>
        public bool hasNullAvatar()
        {
            return AvatarObject == null;
        }


        /// <summary>
        /// アバターオブジェクトがアバターであるかどうか
        /// </summary>
        /// <returns>true: アバターオブジェクト<br/>false: アバターオブジェクトでない</returns>
        public bool AvatarObjectIsAvatar()
        {
            return AvatarObject != null && AvatarUtility.IsAvatar(AvatarObject);
        }


        /// <summary>
        /// PhysBoneのIsAnimatedをすべてOFFにする
        /// </summary>
        public void TurnOffAll()
        {
            foreach (var pb in m_physBones)
            {
                pb.isAnimated = false;
            }
        }


        /// <summary>
        /// アバターのPhysBoneをすべて取得する
        /// </summary>
        public void LoadPhysicsBones()
        {
            if (hasNullAvatar())
            {
                return;
            }
            m_physBones = m_avatarObject.GetComponentsInChildren<VRCPhysBone>(true);
            CreateObjectNames();
            CreateRootTransformNames();
        }


        /// <summary>
        /// すべてのPhysBoneのRoot Transformのオブジェクトの名前を取得し格納する
        /// </summary>
        private void CreateRootTransformNames()
        {
            m_rootTransformNames = new string[Count];

            for (int i = 0; i < Count; ++i)
            {
                var rootTransform = m_physBones[i].rootTransform;
                m_rootTransformNames[i] = rootTransform != null ? rootTransform.name : "None";
            }
        }



        /// <summary>
        /// すべてのPhysBoneがついているオブジェクトの名前を取得し格納する
        /// </summary>
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
