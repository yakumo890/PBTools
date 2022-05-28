using NUnit.Framework;
using System;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Yakumo890.VRC.PhysicsBone.Test
{
    public class PhysicsBoneIsAnimatedChangerTest
    {
        private PhysicsBoneIsAnimatedChangerEngine m_engine;

        private GameObject m_rootObject;
        private const string RootObjectName = "RootObject";

        private GameObject m_childObject;
        private const string ChildObjectName = "ChildObject";

        private GameObject m_rootTransformObject;
        private const string RootTransformObjectName = "RootTransformObject";

        private VRCPhysBone[] m_physBones;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_engine = new PhysicsBoneIsAnimatedChangerEngine();

            m_rootObject = new GameObject();
            m_rootObject.name = RootObjectName;

            m_childObject = new GameObject();
            m_childObject.name = ChildObjectName;
            m_childObject.transform.SetParent(m_rootObject.transform);

            m_rootTransformObject = new GameObject();
            m_rootTransformObject.name = RootTransformObjectName;
            m_rootTransformObject.transform.SetParent(m_rootObject.transform);

            m_physBones = new VRCPhysBone[2];
            m_physBones[0] = m_rootObject.AddComponent<VRCPhysBone>();

            m_physBones[1] = m_childObject.AddComponent<VRCPhysBone>();
            m_physBones[1].rootTransform = m_rootTransformObject.transform;

            m_engine.AvatarObject = m_rootObject;
        }


        [Test]
        public void PropertyTest()
        {
            Assert.AreEqual(m_engine.Count, m_physBones.Length);

            string[] objectNames = m_engine.ObjectNames;
            Assert.AreEqual(objectNames.Length, 2);
            Assert.IsNotNull(Array.Find(objectNames, x => { return x == RootObjectName; }));
            Assert.IsNotNull(Array.Find(objectNames, x => { return x == ChildObjectName; }));

            string[] rootTransformNames = m_engine.RootTransformNames;
            Assert.AreEqual(rootTransformNames.Length, 2);
            Assert.IsNotNull(Array.Find(rootTransformNames, x => { return x == RootTransformObjectName; }));
            Assert.IsNotNull(Array.Find(rootTransformNames, x => { return x == "None"; }));
        }

        [Test]
        public void TurnOffAllTest()
        {
            foreach (var pb in m_physBones)
            {
                pb.isAnimated = true;
            }

            m_engine.TurnOffAll();

            foreach (var pb in m_physBones)
            {
                Assert.IsFalse(pb.isAnimated);
            }
        }

        [Test]
        public void ChangeIsAniamtedTest()
        {
            foreach (var pb in m_physBones)
            {
                pb.isAnimated = false;
            }

            for (int i = 0; i < m_engine.Count; ++i)
            {
                m_engine[i] = true;
            }

            foreach (var pb in m_physBones)
            {
                Assert.IsTrue(pb.isAnimated);
            }
        }
    }
}
