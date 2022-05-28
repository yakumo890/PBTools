using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Yakumo890.Util.VRC;

namespace Yakumo890.VRC.PhysicsBone.Test
{
    public class PhysicsBoneExtractorTestBase<T>
        where T : Component
    {
        private PhysicsBoneExtractorEngine m_engine;

        // TはVRCPhysBoneである
        private bool m_forVRCPhysBone;

        public PhysicsBoneExtractorTestBase()
        {
            m_engine = new PhysicsBoneExtractorEngine(null);

            m_forVRCPhysBone = typeof(T) == typeof(VRCPhysBone);

            // TがVRCPhysBoneかVRCPhysBoneColliderのいずれでもないならテストを落とす
            Assert.IsTrue(m_forVRCPhysBone || typeof(T) == typeof(VRCPhysBoneCollider));
        }


        /// <summary>
        /// 指定したオブジェクトにComponentがまとめられる ことをテスト
        /// </summary>
        public void ExtractTestWithTragetObjectTest()
        {
            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            var targetObject = avatar.CreateObject("TestTarget");

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObject);
            }
            else
            {
                m_engine.ExtractColliders(targetObject, "C");
            }

            // 指定のオブジェクトにすべてのPhysBoneが付いているかのテスト
            AddedComponentsToTargetTest(avatar, targetObject);

            // もとのPhysBoneが消えているかのテスト
            DeletedOriginalComponentsTest(avatar, 0, targetObject);
        }


        /// <summary>
        /// オブジェクトを新規に作ってComponentがまとめられる ことをテスト
        /// </summary>
        public void ExtractTestWithoutTragetObjectTest()
        {
            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            var targetObjectName = "TestTarget";

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObjectName);
            }
            else
            {
                m_engine.ExtractColliders(targetObjectName, "C");
            }

            var targetObjectTransform = avatar.gameObject.transform.Find(targetObjectName);

            // 新規にオブジェクトが作られているか
            Assert.IsNotNull(targetObjectTransform);

            var targetObject = targetObjectTransform.gameObject;

            // 指定のオブジェクトにすべてのPhysBoneが付いているかのテスト
            AddedComponentsToTargetTest(avatar, targetObject);

            // もとのPhysBoneが消えているかのテスト
            DeletedOriginalComponentsTest(avatar, 0, targetObject);
        }


        /// <summary>
        /// ComponentがないオブジェクトではComponentがまとめられない ことをテスト
        /// </summary>
        public void ExtractTestWithoutComponent()
        {
            var avatar = new AvatarForTest(false);
            m_engine.AvatarObject = avatar.gameObject;

            var targetObject = avatar.CreateObject("TestTarget");

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObject);
            }
            else
            {
                m_engine.ExtractColliders(targetObject, "C");
            }

            // 指定のオブジェクトについてるコンポーネントの数は0であるか
            Assert.AreEqual(0, targetObject.GetComponentsInChildren<T>().Length);
            
            // コライダーの場合、子要素が存在しないか
            if (!m_forVRCPhysBone)
            {
                Assert.AreEqual(0, targetObject.transform.childCount);
            }
        }


        // OverridedPhysBoneRootTransformTestとOverridedColliderRootTransformTestは
        // VRCPhysBoneとVRCPhysBoneColliderが共通のインターフェイスを持たないので、Genericなどでうまく共通化出来なかった
        /// <summary>
        /// Root Transformが書き換えられる ことのテスト
        /// </summary>
        /// <param name="canReplaceRootTransform">書き換えるかどうか</param>
        public void OverrideComponentRootTransformTest(bool canReplaceRootTransform)
        {
            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            m_engine.CanReplaceRootTransform = canReplaceRootTransform;

            var targetObject = AvatarUtility.CreateAvatarObject(avatar.gameObject, "TestTarget", false);

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObject);
            }
            else
            {
                m_engine.ExtractColliders(targetObject, "C");
            }

            if (m_forVRCPhysBone)
            {
                OverridedPhysBoneRootTransformTest(avatar, targetObject, canReplaceRootTransform);
            }
            else
            {
                OverridedColliderRootTransformTest(avatar, targetObject, canReplaceRootTransform);
            }
        }


        public void OverrideColliderTest()
        {
            if (m_forVRCPhysBone)
            {
                return;
            }

            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            var targetObject = AvatarUtility.CreateAvatarObject(avatar.gameObject, "TestTarget", false);

            // PBが参照しているコライダーオブジェクトの名前を取得しておく
            var correspondings = new Dictionary<VRCPhysBone, string[]>();
            var pbs = avatar.gameObject.GetComponentsInChildren<VRCPhysBone>();
            foreach (var pb in pbs)
            {
                if (pb.colliders.Count == 0)
                {
                    continue;
                }

                if (!correspondings.ContainsKey(pb))
                {
                    correspondings[pb] = new string[pb.colliders.Count];
                }

                for (int i = 0; i < pb.colliders.Count; ++i)
                {
                    correspondings[pb][i] = pb.colliders[i].name;
                }
            }

            m_engine.ExtractColliders(targetObject, "C");

            foreach (var cor in correspondings)
            {                
                foreach (var name in cor.Value)
                {
                    Assert.IsNotNull(cor.Key.colliders.Find(x => { return x.name == "C_" + name; }));
                }
            }
        }


        public void IgnoreInactiveObjectTest(bool ignoreInactive)
        {
            var avatar = new AvatarForTest();
            m_engine.IgnoreInactive = ignoreInactive;

            m_engine.AvatarObject = avatar.gameObject;

            var targetObject = avatar.CreateObject("TestTarget");

            avatar.CreateObject<T>("Inactive", new AvatarForTest.ObjectCreationOptions(false, false));

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObject);
            }
            else
            {
                m_engine.ExtractColliders(targetObject, "C");
            }

            // 指定の非アクティブのオブジェクト以外のすべてのComponentが付いているかのテスト
            AddedComponentsToTargetTest(avatar, targetObject, ignoreInactive ? 1 : 0);

            // アバターから1つを除くすべてのComponentが消えているか
            DeletedOriginalComponentsTest(avatar, ignoreInactive ? 1 : 0, targetObject);
        }


        public void IgnoreEditorOnlyObjectTest(bool ignoreEditorOnly)
        {
            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            m_engine.IgnoreEditorOnly = ignoreEditorOnly;

            var targetObject = avatar.CreateObject("TestTarget");

            avatar.CreateObject<T>("EditorOnly", new AvatarForTest.ObjectCreationOptions(true, true));

            if (m_forVRCPhysBone)
            {
                m_engine.ExtractPhysBones(targetObject);
            }
            else
            {
                m_engine.ExtractColliders(targetObject, "C");
            }

            // 指定の、非アクティブのオブジェクト以外のすべてのPhysBoneが付いているかのテスト
            AddedComponentsToTargetTest(avatar, targetObject, ignoreEditorOnly ? 1 : 0);

            // アバターから1つを除くすべてのPBが消えているか
            DeletedOriginalComponentsTest(avatar, ignoreEditorOnly ? 1 : 0, targetObject);
        }


        /// <summary>
        /// PBから参照されているコライダーが無視の対象の場合、適切にPBのコライダーが書き換えられるか
        /// </summary>
        public void IgnoredColliderTest()
        {
            if (m_forVRCPhysBone)
            {
                return;
            }

            var avatar = new AvatarForTest();
            m_engine.AvatarObject = avatar.gameObject;

            m_engine.IgnoreInactive = true;

            var inactive = avatar.CreateObject<VRCPhysBoneCollider>("Inactive", new AvatarForTest.ObjectCreationOptions(false, false));

            var targetObject = AvatarUtility.CreateAvatarObject(avatar.gameObject, "TestTarget", false);

            var pb = avatar.gameObject.GetComponentInChildren<VRCPhysBone>();
            Assert.IsNotNull(pb);

            var colliderName = pb.colliders[0].name;
            var nColliders = pb.colliders.Count;

            // 非アクティブなコライダーオブジェクトを追加
            pb.colliders.Add(inactive.GetComponent<VRCPhysBoneCollider>());

            m_engine.ExtractColliders(targetObject, "C");

            // 非アクティブなコライダーは無視されているか
            Assert.AreEqual(nColliders, pb.colliders.Count);

            var result = pb.colliders.Find(x => { return x != null && x.name == "C_" + colliderName; });
            Assert.IsNotNull(result);
        }


        public void NullTest()
        {
            // 例外が起きなければOKのテスト

            var engine = new PhysicsBoneExtractorEngine(null);

            if (m_forVRCPhysBone)
            {
                engine.ExtractPhysBones(new GameObject());
            }
            else
            {
                engine.ExtractColliders(new GameObject(), "");
            }

            engine = new PhysicsBoneExtractorEngine(new GameObject());

            GameObject obj = null;
            string str = null;
            if (m_forVRCPhysBone)
            {
                engine.ExtractPhysBones(obj);
                engine.ExtractPhysBones(str);
            }
            else
            {
                engine.ExtractColliders(obj, "");
                engine.ExtractColliders(str, "");
                engine.ExtractColliders(new GameObject(), null);
            }
        }


        private void OverridedPhysBoneRootTransformTest(AvatarForTest avatar, GameObject targetObject, bool canReplaceRootTransform)
        {
            var movedPhysBones = targetObject.GetComponents<VRCPhysBone>();

            // Root TransformがnullのオブジェクトのRoot Transformが
            // オーバーライドする設定なら、変更されるか
            // オーバーライドしない設定なら、変更されないか
            var result = Array.Find(movedPhysBones,
                x =>
                {
                    if (canReplaceRootTransform)
                    {
                        return x.rootTransform != null &&
                        x.rootTransform.name == avatar.NameOfObjectWithNoComponentRootTransform<VRCPhysBone>();
                    }
                    else
                    {
                        return x.rootTransform == null;
                    }
                }
                );
            Assert.IsNotNull(result);

            if (!canReplaceRootTransform)
            {
                result = Array.Find(movedPhysBones, 
                    x => 
                    {
                        return x.rootTransform != null &&
                        x.rootTransform.name == avatar.NameOfObjectWithNoComponentRootTransform<VRCPhysBone>(); 
                    }
                    );
                Assert.IsNull(result);
            }

            // Root TransformがnullでないオブジェクトのRoot Transformが変更されて_いない_か
            result = Array.Find(movedPhysBones,
                x =>
                {
                    return x.rootTransform != null &&
                    x.rootTransform.name == avatar.NameOfComponentRootTransform<VRCPhysBone>();
                }
                );
            Assert.IsNotNull(result);
        }


        private void OverridedColliderRootTransformTest(AvatarForTest avatar, GameObject targetObject, bool canReplaceRootTransform)
        {
            var movedPhysBones = targetObject.GetComponentsInChildren<VRCPhysBoneCollider>(true);

            // Root TransformがnullのオブジェクトのRoot Transformが変更されているか
            var result = Array.Find(movedPhysBones,
                x =>
                {
                    if (canReplaceRootTransform)
                    {
                        return x.rootTransform != null &&
                        x.rootTransform.name == avatar.NameOfObjectWithNoComponentRootTransform<VRCPhysBoneCollider>();
                    }
                    else
                    {
                        return x.rootTransform == null;
                    }
                }
                );
            Assert.IsNotNull(result);

            if (!canReplaceRootTransform)
            {
                result = Array.Find(movedPhysBones, 
                    x => 
                    { 
                        return x.rootTransform != null && 
                        x.rootTransform.name == avatar.NameOfObjectWithNoComponentRootTransform<VRCPhysBoneCollider>(); 
                    }
                    );
                Assert.IsNull(result);
            }

            // Root TransformがnullでないオブジェクトのRoot Transformが変更されて_いない_か
            result = Array.Find(movedPhysBones,
                x =>
                {
                    return x.rootTransform != null &&
                    x.rootTransform.name == avatar.NameOfComponentRootTransform<VRCPhysBoneCollider>();
                }
                );
            Assert.IsNotNull(result);
        }


        private void AddedComponentsToTargetTest(AvatarForTest avatar, GameObject targetObject, int nIgnoredObjects = 0)
        {
            var movedPhysBones = targetObject.GetComponentsInChildren<T>();

            Assert.AreEqual(avatar.GetNumberOfComponents<T>() - nIgnoredObjects, movedPhysBones.Length);

            if (!m_forVRCPhysBone)
            {
                AddedColliderObjectsInParentTest(avatar, targetObject, nIgnoredObjects);
            }
        }


        private void AddedColliderObjectsInParentTest(AvatarForTest avatar, GameObject targetObject, int nIgnoredObjects = 0)
        {
            // 子オブジェクトがコライダーの数だけあるか
            Assert.AreEqual(avatar.GetNumberOfComponents<T>() - nIgnoredObjects, targetObject.transform.childCount);

            // 各子オブジェクトは1つずつコライダーを持つか
            for (int i = 0; i < targetObject.transform.childCount; ++i)
            {
                var components = targetObject.transform.GetChild(i).GetComponents<T>();
                Assert.AreEqual(1, components.Length);
            }
        }


        private void DeletedOriginalComponentsTest(AvatarForTest avatar, int nRemainder, GameObject targetObject)
        {
            avatar.RecountComponents(targetObject);
            Assert.AreEqual(nRemainder, avatar.GetNumberOfComponents<T>());
        }
    }


    public class PhysicsBoneExtractorTest
    {
        private PhysicsBoneExtractorTestBase<VRCPhysBone> m_physBoneTestBase;
        private PhysicsBoneExtractorTestBase<VRCPhysBoneCollider> m_colliderTestBase;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_physBoneTestBase = new PhysicsBoneExtractorTestBase<VRCPhysBone>();
            m_colliderTestBase = new PhysicsBoneExtractorTestBase<VRCPhysBoneCollider>();
        }

        [Test]
        public void ExtractPhysBonesTest()
        {
            // オブジェクトを指定してまとめるテスト
            m_physBoneTestBase.ExtractTestWithTragetObjectTest();

            // オブジェクトを指定せずにまとめるテスト
            m_physBoneTestBase.ExtractTestWithoutTragetObjectTest();

            // コンポーネントを付けずにまとめるテスト
            m_physBoneTestBase.ExtractTestWithoutComponent();

            // オプションが有効のとき、ルートトランスフォームが書き換えられているかテスト
            m_physBoneTestBase.OverrideComponentRootTransformTest(true);

            // オプションが無効のとき、ルートトランスフォームが書き換えられ_ない_かテスト
            m_physBoneTestBase.OverrideComponentRootTransformTest(false);

            // オプションが有効のとき非アクティブ要素が無視されるかのテスト
            m_physBoneTestBase.IgnoreInactiveObjectTest(true);

            // オプションが無効のとき非アクティブ要素が無視され_ない_かのテスト
            m_physBoneTestBase.IgnoreInactiveObjectTest(false);

            // オプションが有効のときEditorOnly要素が無視されるか
            m_physBoneTestBase.IgnoreEditorOnlyObjectTest(true);

            // オプションが無効のときEditorOnly要素が無視され_ない_か
            m_physBoneTestBase.IgnoreEditorOnlyObjectTest(false);

            // Nullテスト
            m_physBoneTestBase.NullTest();
        }

        [Test]
        public void ExtractColliderTest()
        {
            // オブジェクトを指定してまとめるテスト
            m_colliderTestBase.ExtractTestWithTragetObjectTest();

            // オブジェクトを指定せずにまとめるテスト
            m_colliderTestBase.ExtractTestWithoutTragetObjectTest();

            // コンポーネントを付けずにまとめるテスト
            m_colliderTestBase.ExtractTestWithoutComponent();

            // オプションが有効のとき、ルートトランスフォームが書き換えられているかテスト
            m_colliderTestBase.OverrideComponentRootTransformTest(true);

            // オプションが無効のとき、ルートトランスフォームが書き換えられ_ない_かテスト
            m_colliderTestBase.OverrideComponentRootTransformTest(false);

            // PBのコライダーを書き換えてるかのテスト
            m_colliderTestBase.OverrideColliderTest();

            // オプションが有効のとき非アクティブ要素が無視されるかのテスト
            m_colliderTestBase.IgnoreInactiveObjectTest(true);

            // オプションが無効のとき非アクティブ要素が無視され_ない_かのテスト
            m_colliderTestBase.IgnoreInactiveObjectTest(false);

            // オプションが有効のときEditorOnly要素が無視されるか
            m_colliderTestBase.IgnoreEditorOnlyObjectTest(true);

            // オプションが無効のときEditorOnly要素が無視され_ない_か
            m_colliderTestBase.IgnoreEditorOnlyObjectTest(false);

            //PBのコライダーが無視すべきオブジェクトを参照しているとき無視されるか
            m_colliderTestBase.IgnoredColliderTest();

            // Nullテスト
            m_colliderTestBase.NullTest();
        }
    }
}
