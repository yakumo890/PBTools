// TODO: TestBaseの共通化

using NUnit.Framework;
using System;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util;

namespace Yakumo890.VRC.PhysicsBone.Test
{
    class PhysicsBoneMoverTestBase
    {
        private PhysicsBoneMoverEngine m_engine;

        public PhysicsBoneMoverTestBase()
        {
            m_engine = new PhysicsBoneMoverEngine();
        }


        public void MovePhysBoneTest()
        {
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            // 移動先にないオブジェクトを作っておく
            srcAvatar.CreateObject<VRCPhysBone>("Dummy");

            m_engine.MovePhysBones();

            var srcComponents = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>();
            // 元のアバターからPBが消えていないか
            Assert.AreNotEqual(0, srcComponents.Length);

            var destComponents = destAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>();
            foreach (var destComponent in destComponents)
            {
                var originalObjectTransform = srcAvatar.gameObject.FindRecursive(destComponent.name);
                Assert.IsNotNull(originalObjectTransform);

                var srcComponent = originalObjectTransform.GetComponent<VRCPhysBone>();
                Assert.IsNotNull(srcComponent);

                // 移動したPBの値が変わっていない
                Assert.IsTrue(AreEqualsPB(srcComponent, destComponent));

                if (srcComponent.rootTransform == null)
                {
                    continue;
                }

                // rootTransformがnullでないとき、コンポーネントがついているオブジェクトに対応するオブジェクトが指定されるか
                // オブジェクト名を比較する
                Assert.AreEqual(srcComponent.name, destComponent.rootTransform.name);
                // 移動先のオブジェクトを参照しているか
                Assert.AreEqual(destAvatar.gameObject.name, destComponent.rootTransform.root.name);
            }

            // 移動元には移動先にはないオブジェクトがあるため、移動後のPBの数は1すくなる
            Assert.AreEqual(srcComponents.Length - 1, destComponents.Length);
        }


        public void IgnoreHasNoRootTransformTest(bool ignoreHasNoRootTransform)
        {
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            m_engine.IgnoreHasNoRootTransform = ignoreHasNoRootTransform;

            // 移動先にないオブジェクトをRoot Transformに設定する
            var dummy = srcAvatar.CreateObject<VRCPhysBone>("RootTransformDummy");

            var srcPBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>();
            var noRootTransformPB = Array.Find(srcPBs, x => { return x.rootTransform == null; });
            Assert.IsNotNull(noRootTransformPB);

            noRootTransformPB.rootTransform = dummy.transform;

            m_engine.MovePhysBones();

            var destNoRootTransformPBObjectTransform = destAvatar.gameObject.FindRecursive(noRootTransformPB.name);
            var destNoRootTransformPB = destNoRootTransformPBObjectTransform.GetComponent<VRCPhysBone>();
            if (ignoreHasNoRootTransform)
            {
                Assert.IsNull(destNoRootTransformPB);
            }
            else
            {
                Assert.IsNotNull(destNoRootTransformPB);
                Assert.IsNull(destNoRootTransformPB.rootTransform);
            }
        }


        public void IgnoreHasNoColliderTest(bool ignoreHasNoCollider)
        {
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            m_engine.IgnoreHasNoColliders = ignoreHasNoCollider;

            // 移動先にないコライダーを、すでにコライダーを参照しているPBに追加する
            // (コライダーのどれか一つでも見つからなければ無視するという仕様のため、コライダーが複数あるPBでテストする)
            var dummy = srcAvatar.CreateObject<VRCPhysBoneCollider>("ColliderDummy");

            var srcPBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>();
            Assert.IsNotNull(srcPBs);
            var hasColliderPB = Array.Find(srcPBs, x => { return x.colliders.Count != 0; });
            Assert.IsNotNull(hasColliderPB);

            hasColliderPB.colliders.Add(dummy.GetComponent<VRCPhysBoneCollider>());

            m_engine.MovePhysBones();

            var destHasColliderPBTransform = destAvatar.gameObject.FindRecursive(hasColliderPB.name);
            Assert.IsNotNull(destHasColliderPBTransform);
            var destHasColliderPB = destHasColliderPBTransform.GetComponent<VRCPhysBone>();

            if (ignoreHasNoCollider)
            {
                Assert.IsNull(destHasColliderPB);
            }
            else
            {
                Assert.IsNotNull(destHasColliderPB);
                Assert.AreEqual(hasColliderPB.colliders.Count - 1, destHasColliderPB.colliders.Count);
            }
        }


        public void CanDeleteSourcePBsTest(bool canDeleteSourcePBs)
        {
            // 移動したあとに元のコンポーネントを消せるか
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            var nBeforePBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>().Length;

            m_engine.MovePhysBones();
            if (canDeleteSourcePBs)
            {
                m_engine.RemoveCopiedComponent();
            }

            var nAfterPBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBone>().Length;
            if (canDeleteSourcePBs)
            {
                Assert.AreEqual(0, nAfterPBs);
            }
            else
            {
                Assert.AreEqual(nBeforePBs, nAfterPBs);
            }
        }


        public void IgnoreNoMatchPathObjectTest(bool ignoreNoMatchPathObject)
        {
            // 移動したあとに元のコンポーネントを消せるか
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);

            // あらたなオブジェクトを階層を変更して作る
            var testObject = "Test-for-match-path";
            srcAvatar.CreateObject<VRCPhysBone>($"{testObject}");

            var parent = destAvatar.CreateObject("Parent");
            var destTestObject = destAvatar.CreateObject($"{testObject}");
            destTestObject.transform.SetParent(parent.transform);

            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            m_engine.IgnoreHasNoMatchPathObject = ignoreNoMatchPathObject;

            m_engine.MovePhysBones();

            var destTestObjectPB = destTestObject.GetComponentInChildren<VRCPhysBone>();
            if (ignoreNoMatchPathObject)
            {
                Assert.IsNull(destTestObjectPB);
            }
            else
            {
                Assert.IsNotNull(destTestObjectPB);
            }
        }


        // TestForAvatarはpullの値だけ違う値を入れているので
        // とりあえずpullの値が等しいかどうかで判定する
        private bool AreEqualsPB(VRCPhysBone pb1, VRCPhysBone pb2)
        {
            return Mathf.Abs(pb1.pull - pb2.pull) < 1e-5f;
        }
    }


    class ColliderMoverTestBase
    {
        private PhysicsBoneMoverEngine m_engine;

        public ColliderMoverTestBase()
        {
            m_engine = new PhysicsBoneMoverEngine();
        }


        public void MoveColliderTest()
        {
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            // 移動先にないオブジェクトを作っておく
            srcAvatar.CreateObject<VRCPhysBoneCollider>("Dummy");

            m_engine.MovePhysBoneColliders();

            var srcComponents = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>();
            // 元のアバターからコライダーが消えていないか
            Assert.AreNotEqual(0, srcComponents.Length);

            var destComponents = destAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>();
            foreach (var destComponent in destComponents)
            {
                var originalObjectTransform = srcAvatar.gameObject.FindRecursive(destComponent.name);
                Assert.IsNotNull(originalObjectTransform);

                var srcComponent = originalObjectTransform.GetComponent<VRCPhysBoneCollider>();
                Assert.IsNotNull(srcComponent);

                // 移動したコライダーの値が変わっていない
                Assert.IsTrue(AreEqualsCollider(srcComponent, destComponent));

                if (srcComponent.rootTransform == null)
                {
                    continue;
                }

                // rootTransformがnullでないとき、コンポーネントがついているオブジェクトに対応するオブジェクトが指定されるか
                // オブジェクト名を比較する
                Assert.AreEqual(srcComponent.name, destComponent.rootTransform.name);
                // 移動先のオブジェクトを参照しているか
                Assert.AreEqual(destAvatar.gameObject.name, destComponent.rootTransform.root.name);
            }

            // 移動元には移動先にはないオブジェクトがあるため、移動後のコライダーの数は1すくなる
            Assert.AreEqual(srcComponents.Length - 1, destComponents.Length);
        }


        public void IgnoreHasNoRootTransformTest(bool ignoreHasNoRootTransform)
        {
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            m_engine.IgnoreHasNoRootTransform = ignoreHasNoRootTransform;

            // 移動先にないオブジェクトをRoot Transformに設定する
            var dummy = srcAvatar.CreateObject<VRCPhysBoneCollider>("RootTransformDummy");

            var srcColliders = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>();
            var noRootTransformCollider = Array.Find(srcColliders, x => { return x.rootTransform == null; });
            Assert.IsNotNull(noRootTransformCollider);

            noRootTransformCollider.rootTransform = dummy.transform;

            m_engine.MovePhysBoneColliders();

            var destNoRootTransformColliderObjectTransform = destAvatar.gameObject.FindRecursive(noRootTransformCollider.name);
            var destNoRootTransformCollider = destNoRootTransformColliderObjectTransform.GetComponent<VRCPhysBoneCollider>();
            if (ignoreHasNoRootTransform)
            {
                Assert.IsNull(destNoRootTransformCollider);
            }
            else
            {
                Assert.IsNotNull(destNoRootTransformCollider);
                Assert.IsNull(destNoRootTransformCollider.rootTransform);
            }
        }


        public void CanDeleteSourcePBsTest(bool canDeleteSourcePBs)
        {
            // 移動したあとに元のコンポーネントを消せるか
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);
            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            var nBeforePBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>().Length;

            m_engine.MovePhysBoneColliders();
            if (canDeleteSourcePBs)
            {
                m_engine.RemoveCopiedComponent();
            }

            var nAfterPBs = srcAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneCollider>().Length;
            if (canDeleteSourcePBs)
            {
                Assert.AreEqual(0, nAfterPBs);
            }
            else
            {
                Assert.AreEqual(nBeforePBs, nAfterPBs);
            }
        }


        public void IgnoreNoMatchPathObjectTest(bool ignoreNoMatchPathObject)
        {
            // 移動したあとに元のコンポーネントを消せるか
            var srcAvatar = new AvatarForTest("SrcAvatar", true);
            var destAvatar = new AvatarForTest("DestAvatar", false);

            // あらたなオブジェクトを階層を変更して作る
            var testObject = "Test-for-match-path";
            srcAvatar.CreateObject<VRCPhysBoneCollider>($"{testObject}");

            var parent = destAvatar.CreateObject("Parent");
            var destTestObject = destAvatar.CreateObject($"{testObject}");
            destTestObject.transform.SetParent(parent.transform);

            m_engine.SrcAvatarObject = srcAvatar.gameObject;
            m_engine.DestAvatarObject = destAvatar.gameObject;

            m_engine.IgnoreHasNoMatchPathObject = ignoreNoMatchPathObject;

            m_engine.MovePhysBoneColliders();

            var destTestObjectPB = destTestObject.GetComponentInChildren<VRCPhysBoneCollider>();
            if (ignoreNoMatchPathObject)
            {
                Assert.IsNull(destTestObjectPB);
            }
            else
            {
                Assert.IsNotNull(destTestObjectPB);
            }
        }


        // TestForAvatarはpullの値だけ違う値を入れているので
        // とりあえずpullの値が等しいかどうかで判定する
        private bool AreEqualsCollider(VRCPhysBoneCollider pb1, VRCPhysBoneCollider pb2)
        {
            return Mathf.Abs(pb1.radius - pb2.radius) < 1e-6f;
        }
    }


    class PhysicsBoneMoverTest
    {
        private PhysicsBoneMoverTestBase m_physBoneTestBase;
        private ColliderMoverTestBase m_colliderTestBase;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_physBoneTestBase = new PhysicsBoneMoverTestBase();
            m_colliderTestBase = new ColliderMoverTestBase();
        }


        [Test]
        public void MovePhysiscsBoneTest()
        {
            m_physBoneTestBase.MovePhysBoneTest();

            m_physBoneTestBase.IgnoreHasNoRootTransformTest(true);

            m_physBoneTestBase.IgnoreHasNoRootTransformTest(false);

            m_physBoneTestBase.IgnoreHasNoColliderTest(true);

            m_physBoneTestBase.IgnoreHasNoColliderTest(false);

            m_physBoneTestBase.CanDeleteSourcePBsTest(true);

            m_physBoneTestBase.CanDeleteSourcePBsTest(false);

            m_physBoneTestBase.IgnoreNoMatchPathObjectTest(true);

            m_physBoneTestBase.IgnoreNoMatchPathObjectTest(false);
        }


        [Test]
        public void MoveColliderTest()
        {
            m_colliderTestBase.MoveColliderTest();

            m_colliderTestBase.IgnoreHasNoRootTransformTest(true);

            m_colliderTestBase.IgnoreHasNoRootTransformTest(false);

            m_colliderTestBase.CanDeleteSourcePBsTest(true);

            m_colliderTestBase.CanDeleteSourcePBsTest(false);

            m_colliderTestBase.IgnoreNoMatchPathObjectTest(true);

            m_colliderTestBase.IgnoreNoMatchPathObjectTest(false);
        }
    }
}
