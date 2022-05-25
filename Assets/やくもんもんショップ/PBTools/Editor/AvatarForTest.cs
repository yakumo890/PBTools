using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone.Test
{
    public class AvatarForTest
    {
        public struct ObjectCreationOptions
        {
            public bool Active;
            public bool EditorOnly;
            //public System.Type ComponentType;

            public ObjectCreationOptions(bool active, bool editorOnly)
            {
                Active = active;
                EditorOnly = editorOnly;
            }
        }

        private GameObject m_avatar;

        private int m_nPhysBones;
        private int m_nColliders;

        private bool m_willAddComponents;

        private GameObject m_bone1;
        private GameObject m_bone2;
        private GameObject m_colliderObject1;
        private GameObject m_colliderObject2;

        private string m_nameOfPhysBoneRootTransform;
        private string m_nameOfObjectWithNoPhysBoneRootTransform;
        private string m_nameOfColliderRootTransform;
        private string m_nameOfObjectWithNoColliderRootTransform;


        public AvatarForTest() : this("AvatarForTest", true)
        {
        }


        public AvatarForTest(string name) : this(name, true)
        {
        }


        public AvatarForTest(bool willAddComponents) : this("AvatarForTest", willAddComponents)
        {
        }


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


        public GameObject gameObject
        {
            get
            {
                return m_avatar;
            }
        }


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


        public GameObject CreateObject<T>(string name)
            where T : Component
        {
            return CreateObject<T>(name, new ObjectCreationOptions(true, false));
        }


        public GameObject CreateObject<T>(string name, ObjectCreationOptions options)
            where T : Component
        {
            var obj = CreateObjectWithComponent<T>(name);
            SetOptions(obj, options);

            return obj;
        }


        public GameObject CreateObject(string name)
        {
            return CreateObject(name, new ObjectCreationOptions(true, false));
        }


        public GameObject CreateObject(string name, ObjectCreationOptions options)
        {
            var obj = AvatarUtility.CreateAvatarObject(m_avatar, name, false);
            SetOptions(obj, options);

            return obj;
        }


        public void RecountComponents(GameObject targetObject)
        {
            m_nPhysBones = RecountComponent<VRCPhysBone>(targetObject);
            m_nColliders = RecountComponent<VRCPhysBoneCollider>(targetObject);
        }


        public int RecountComponent<T>(GameObject targetObject)
            where T : Component
        {
            var count = m_avatar.GetComponentsInChildren<T>(true).Length;
            return count - targetObject.GetComponentsInChildren<T>(true).Length;
        }


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