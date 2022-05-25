using UnityEngine;
using NUnit.Framework;

namespace Yakumo890.Util.Test
{
    public class ComponentUtilityTest
    {
        [Test]
        public void CopyPasteComponentAsNewSuccessTest()
        {
            var srcObject = new GameObject();
            var distObject = new GameObject(); //なぜかテスト後にこのオブジェクトがシーンに残る、DestroyImmediateなどを試みたが消えず

            var rigidBody = srcObject.AddComponent<Rigidbody>();
            rigidBody.mass = 100;

            Assert.IsTrue(ComponentUtility.CopyPaseteComponentAsNew(rigidBody, distObject));

            var copiedRigidBody = distObject.GetComponent<Rigidbody>();
            Assert.IsNotNull(copiedRigidBody);
            Assert.AreEqual(rigidBody.mass, copiedRigidBody.mass);

            var originalRigidBody = srcObject.GetComponent<Rigidbody>();
            Assert.IsNotNull(originalRigidBody);
        }


        [Test]
        public void CopyPasteComponentAsNewFailureTest()
        {
            Assert.IsFalse(ComponentUtility.CopyPaseteComponentAsNew(null, new GameObject()));
            Assert.IsFalse(ComponentUtility.CopyPaseteComponentAsNew(new Rigidbody(), null));
        }


        [Test]
        public void CopyPasteComponentsAsNewFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            Assert.IsFalse(ComponentUtility.CopyPasteComponentsAsNew(emptyRigidBodies, new GameObject()));

            for (int i = 0; i < emptyRigidBodies.Length; ++i)
            {
                emptyRigidBodies[i] = new Rigidbody();
            }
            Assert.False(ComponentUtility.CopyPasteComponentsAsNew(emptyRigidBodies, null));
        }


        [Test]
        public void MoveComopnentSuccessTest()
        {
            var srcObject = new GameObject();
            var distObject = new GameObject(); //なぜかテスト後にこのオブジェクトがシーンに残る
            float mass = 100f;

            var rigidBody = srcObject.AddComponent<Rigidbody>();
            rigidBody.mass = mass;

            Assert.IsTrue(ComponentUtility.MoveComponent(rigidBody, distObject));

            var copiedRigidBody = distObject.GetComponent<Rigidbody>();
            Assert.IsNotNull(copiedRigidBody);
            Assert.AreEqual(copiedRigidBody.mass, mass);

            var originalRigidBody = srcObject.GetComponent<Rigidbody>();
            Assert.IsNull(originalRigidBody);
        }


        [Test]
        public void MoveComponentFailureTest()
        {
            Assert.IsFalse(ComponentUtility.MoveComponent(null, new GameObject()));
            Assert.IsFalse(ComponentUtility.MoveComponent(new Rigidbody(), null));
        }


        [Test]
        public void MoveComponentsFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            Assert.IsFalse(ComponentUtility.MoveComponents(emptyRigidBodies, new GameObject()));

            for (int i = 0; i < emptyRigidBodies.Length; ++i)
            {
                emptyRigidBodies[i] = new Rigidbody();
            }
            Assert.False(ComponentUtility.MoveComponents(emptyRigidBodies, null));
        }


        [Test]
        public void DeleteComponentSuccessTest()
        {
            var gameObject = new GameObject();
            var rigidBody = gameObject.AddComponent<Rigidbody>();

            Assert.IsTrue(ComponentUtility.DeleteComponent(rigidBody));

            Assert.IsNull(gameObject.GetComponent<Rigidbody>());
        }


        [Test]
        public void DeleteComponentFailureTest()
        {
            Assert.IsFalse(ComponentUtility.DeleteComponent(null));
        }


        [Test]
        public void DeleteComponentsFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            Assert.IsFalse(ComponentUtility.DeleteComponents(emptyRigidBodies));
        }
    }
}
