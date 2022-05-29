using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone.Test
{
    /// <summary>
    /// 単体テスト用のアバターを表すクラス
    /// </summary>
    public class AvatarForTest
    {
        /// <summary>
        /// アバターオブジェクトを生成する際のオプション
        /// </summary>
        public struct ObjectCreationOptions
        {
            /// <summary>
            /// オブジェクトをアクティブにするかどうか(true: アクティブ)
            /// </summary>
            public bool Active;
            /// <summary>
            /// オブジェクトをEditorOnlyにするかどうか(true: EditorOnly)
            /// </summary>
            public bool EditorOnly;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="active">オブジェクトをアクティブにするかどうか(true: アクティブ)</param>
            /// <param name="editorOnly">オブジェクトをEditorOnlyにするかどうか(true: EditorOnly)</param>
            public ObjectCreationOptions(bool active, bool editorOnly)
            {
                Active = active;
                EditorOnly = editorOnly;
            }
        }

        private GameObject m_avatar;

        private int m_nPhysBones;
        private int m_nColliders;

        // 子オブジェクトを生成する時に、PhysBone関連のコンポーネントを生成するかどうか
        private bool m_willAddComponents;

        private GameObject m_bone1;
        private GameObject m_bone2;
        private GameObject m_colliderObject1;
        private GameObject m_colliderObject2;

        private string m_nameOfPhysBoneRootTransform;
        private string m_nameOfObjectWithNoPhysBoneRootTransform;
        private string m_nameOfColliderRootTransform;
        private string m_nameOfObjectWithNoColliderRootTransform;


        /// <summary>
        /// コンストラクタ<br/>
        /// <br/>
        /// Avatarのルートオブジェクトを"AvatarForTest"という名前にする<br/>
        /// 子オブジェクトを生成する際にPhysBone関連オブジェクトを生成する
        /// </summary>
        public AvatarForTest() : this("AvatarForTest", true)
        {
        }


        /// <summary>
        /// コンストラクタ<br/>
        /// <br/>
        /// 子オブジェクトを生成する際にPhysBone関連オブジェクトを生成する
        /// </summary>
        /// <param name="name">Avatarのルートオブジェクト名</param>
        public AvatarForTest(string name) : this(name, true)
        {
        }


        /// <summary>
        /// コンストラクタ<br/>
        /// <br/>
        /// Avatarのルートオブジェクトを"AvatarForTest"という名前にする
        /// </summary>
        /// <param name="willAddComponents">子オブジェクトを生成する際にPhysBone関連オブジェクトを生成するか(true: する)</param>
        public AvatarForTest(bool willAddComponents) : this("AvatarForTest", willAddComponents)
        {
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">Avatarのルートオブジェクト名</param>
        /// <param name="willAddComponents">子オブジェクトを生成する際にPhysBone関連オブジェクトを生成するか(true: する)</param>
        public AvatarForTest(string name, bool willAddComponents)
        {
            m_nPhysBones = 0;
            m_nColliders = 0;
            m_willAddComponents = willAddComponents;

            m_avatar = new GameObject();
            m_avatar.name = name;
            m_avatar.AddComponent<Animator>();

            CreateAvatar(m_willAddComponents);
        }


        /// <summary>
        /// アバターオブジェクト
        /// </summary>
        public GameObject gameObject
        {
            get
            {
                return m_avatar;
            }
        }


        /// <summary>
        /// アバターについてるコンポーネントTの数を計算する
        /// </summary>
        /// <typeparam name="T">数えたいコンポーネントの型(VRCPhysBoneかVRCPhysBoneCollider)</typeparam>
        /// <returns>n: コンポーネントの数<br/>-1: TがVRCPhysBoneかVRCPhysBoneCollider以外</returns>
        public int GetNumberOfComponents<T>()
            where T : Component
        {
            if (typeof(T) == typeof(VRCPhysBone))
            {
                return m_nPhysBones;
            }
            else if (typeof(T) == typeof(VRCPhysBoneCollider))
            {
                return m_nColliders;
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// Root Transformに設定されているオブジェクト名
        /// </summary>
        /// <typeparam name="T">コンポーネントの型</typeparam>
        /// <returns>オブジェクト名</returns>
        /// <remarks>アバターの初期状態では、Root Tranformが設定されていないコンポーネントTは一つしか無い</remarks>
        public string NameOfComponentRootTransform<T>()
            where T : Component
        {
            if (typeof(T) == typeof(VRCPhysBone))
            {
                return m_nameOfPhysBoneRootTransform;
            }
            else if (typeof(T) == typeof(VRCPhysBoneCollider))
            {
                return m_nameOfColliderRootTransform;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Root Transformが設定されていないコンポーネントTがついているオブジェクトの名前を取得
        /// </summary>
        /// <typeparam name="T">コンポーネントの型(VRCPhysBoneかVRCPhysBoneCollider)</typeparam>
        /// <returns>オブジェクト名<br/>オブジェクトの名前<br/>TがVRCPhysBoneかVRCPhysBoneColliderでなければnull</returns>
        /// <remarks>アバターの初期状態では、Root Transformが設定されていないコンポーネントTは一つしか無い</remarks>
        public string NameOfObjectWithNoComponentRootTransform<T>()
            where T : Component
        {
            if (typeof(T) == typeof(VRCPhysBone))
            {
                return m_nameOfObjectWithNoPhysBoneRootTransform;
            }
            else if (typeof(T) == typeof(VRCPhysBoneCollider))
            {
                return m_nameOfObjectWithNoColliderRootTransform;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// アバターの子オブジェクトをコンポーネントを付けて生成する<br/>
        /// <br/>
        /// アクティブオブジェクトにする<br/>
        /// EditorOnlyタグにしない
        /// </summary>
        /// <typeparam name="T">コンポーネントの型(VRCPhysBoneかVRCPhysBoneCollider)</typeparam>
        /// <param name="name">オブジェクトの名前</param>
        /// <returns>生成したオブジェクト</returns>
        public GameObject CreateObject<T>(string name)
            where T : Component
        {
            return CreateObject<T>(name, new ObjectCreationOptions(true, false));
        }


        /// <summary>
        /// アバターの子オブジェクトをコンポーネントを付けて生成する
        /// </summary>
        /// <typeparam name="T">コンポーネントの型(VRCPhysBoneかVRCPhysBoneCollider)</typeparam>
        /// <param name="name">オブジェクトの名前</param>
        /// <param name="options">オブジェクトの生成オプション</param>
        /// <returns></returns>
        public GameObject CreateObject<T>(string name, ObjectCreationOptions options)
            where T : Component
        {
            var obj = CreateObjectWithComponent<T>(name);
            SetOptions(obj, options);

            return obj;
        }


        /// <summary>
        /// アバターの子オブジェクトを生成する(コンポーネントは付加しない)<br/>
        /// <br/>
        /// オブジェクトをアクティブにする<br/>
        /// EditorOnlyにしない
        /// </summary>
        /// <param name="name">子オブジェクト名</param>
        /// <returns>生成したオブジェクト</returns>
        public GameObject CreateObject(string name)
        {
            return CreateObject(name, new ObjectCreationOptions(true, false));
        }


        /// <summary>
        /// アバターの子オブジェクトを生成する(コンポーネントは付加しない)
        /// </summary>
        /// <param name="name">子オブジェクト名</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public GameObject CreateObject(string name, ObjectCreationOptions options)
        {
            var obj = AvatarUtility.CreateAvatarObject(m_avatar, name, false);
            SetOptions(obj, options);

            return obj;
        }


        /// <summary>
        /// PhysBone、PhysBoneColliderの数を計算する
        /// </summary>
        /// <param name="excludeObject">計算から省くオブジェクト</param>
        public void RecountComponents(GameObject excludeObject)
        {
            m_nPhysBones = RecountComponent<VRCPhysBone>(excludeObject);
            m_nColliders = RecountComponent<VRCPhysBoneCollider>(excludeObject);
        }


        /// <summary>
        /// コンポーネントTの数を計算する
        /// </summary>
        /// <typeparam name="T">コンポーネントの型(VRCPhysBoneかVRCPhysBoneCollider)</typeparam>
        /// <param name="excludeObject">計算から省くオブジェクト</param>
        /// <returns>コンポーネントの数</returns>
        public int RecountComponent<T>(GameObject excludeObject)
            where T : Component
        {
            var count = m_avatar.GetComponentsInChildren<T>(true).Length;
            return count - excludeObject.GetComponentsInChildren<T>(true).Length;
        }


        /// <summary>
        /// VRCPhysBone、VRCPhysBoneColliderを素体のボーンオブジェクトにつける
        /// </summary>
        private void AttachComponentsToBaseObjects()
        {
            var physbone1 = m_bone1.GetComponent<VRCPhysBone>();
            if (physbone1 == null)
            {
                physbone1 = AddComponent<VRCPhysBone>(m_bone1);
            }
            m_nameOfObjectWithNoPhysBoneRootTransform = m_bone1.name;
            physbone1.pull = Random.value; // 適当に値を変えておく

            var physbone2 = m_bone2.GetComponent<VRCPhysBone>();
            if (physbone2 == null)
            {
                physbone2 = AddComponent<VRCPhysBone>(m_bone2);
            }
            physbone2.rootTransform = m_bone2.transform;
            m_nameOfPhysBoneRootTransform = m_bone2.name;
            physbone2.pull = Random.value; // 適当に値を変えておく

            var collider1 = m_colliderObject1.GetComponent<VRCPhysBoneCollider>();
            if (collider1 == null)
            {
                collider1 = AddComponent<VRCPhysBoneCollider>(m_colliderObject1);
            }
            physbone1.colliders.Add(collider1);
            m_nameOfObjectWithNoColliderRootTransform = m_colliderObject1.name;
            collider1.radius = Random.value;

            var collider2 = m_colliderObject2.GetComponent<VRCPhysBoneCollider>();
            if (collider2 == null)
            {
                collider2 = AddComponent<VRCPhysBoneCollider>(m_colliderObject2);
            }
            collider2.rootTransform = m_colliderObject2.transform;
            m_nameOfColliderRootTransform = m_colliderObject2.name;
            physbone1.colliders.Add(collider2);
            collider2.radius = Random.value;
        }


        /// <summary>
        /// アバターの素体を構築する
        /// </summary>
        /// <param name="addPhys">VRCPhysBone、VRCPhysBoneColliderを付けるかどうか(true: 付ける)</param>
        private void CreateAvatar(bool addPhys)
        {
            var armature = AvatarUtility.CreateAvatarObject(m_avatar, "Armature");

            m_bone1 = AvatarUtility.CreateAvatarObject(armature, "Bone1");
            m_bone2 = AvatarUtility.CreateAvatarObject(armature, "Bone2");

            m_colliderObject1 = AvatarUtility.CreateAvatarObject(m_bone1, "Collider1");
            m_colliderObject2 = AvatarUtility.CreateAvatarObject(m_bone1, "Collider2");

            if (addPhys)
            {
                AttachComponentsToBaseObjects();
            }
        }


        /// <summary>
        /// アバターの子オブジェクトをコンポーネントを付けて生成する
        /// </summary>
        /// <typeparam name="T">コンポーネントの型</typeparam>
        /// <param name="name">子オブジェクトの名前</param>
        /// <returns>生成したオブジェクト</returns>
        private GameObject CreateObjectWithComponent<T>(string name)
                where T : Component
        {
            var obj = AvatarUtility.CreateAvatarObject(m_avatar, name, false);

            if (obj.GetComponent<T>() == null)
            {
                AddComponent<T>(obj);
            }

            return obj;
        }


        /// <summary>
        /// オブジェクトにコンポーネントを付ける
        /// </summary>
        /// <typeparam name="T">コンポーネントの型</typeparam>
        /// <param name="target">対象のオブジェクト</param>
        /// <returns>付加したコンポーネント</returns>
        private T AddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target.AddComponent<T>();

            if (typeof(T) == typeof(VRCPhysBone))
            {
                m_nPhysBones++;
            }

            if (typeof(T) == typeof(VRCPhysBoneCollider))
            {
                m_nColliders++;
            }

            return component;
        }


        /// <summary>
        /// オブジェクトの状態を変更
        /// </summary>
        /// <param name="obj">対象のオブジェクト</param>
        /// <param name="options">状態のオプション</param>
        private void SetOptions(GameObject obj, ObjectCreationOptions options)
        {
            obj.SetActive(options.Active);

            if (options.EditorOnly)
            {
                obj.ToEditorOnly();
            }
        }
    }


}