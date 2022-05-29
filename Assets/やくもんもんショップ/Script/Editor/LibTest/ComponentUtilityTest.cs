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

            // CopyPaseteComponentAsNewが成功するか
            Assert.IsTrue(ComponentUtility.CopyPaseteComponentAsNew(rigidBody, distObject));

            var copiedRigidBody = distObject.GetComponent<Rigidbody>();
            // コンポーネントが指定のオブジェクトにコピーされているか
            Assert.IsNotNull(copiedRigidBody);
            // コピーしたコンポーネントの値が元のコンポーネントと同じ値になっているか
            Assert.AreEqual(rigidBody.mass, copiedRigidBody.mass);

            var originalRigidBody = srcObject.GetComponent<Rigidbody>();
            // 元のコンポーネントが消えずに存在しているか
            Assert.IsNotNull(originalRigidBody);
        }


        [Test]
        public void CopyPasteComponentAsNewFailureTest()
        {
            // コンポーネントをnullにして、処理が失敗するか
            Assert.IsFalse(ComponentUtility.CopyPaseteComponentAsNew(null, new GameObject()));
            // 対象のオブジェクトをnullにして、処理が失敗するか
            Assert.IsFalse(ComponentUtility.CopyPaseteComponentAsNew(new Rigidbody(), null));
        }


        [Test]
        public void CopyPasteComponentsAsNewFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            // 中身がnullの配列を渡して、CopyPasteComponentsAsNewが失敗するか
            Assert.IsFalse(ComponentUtility.CopyPasteComponentsAsNew(emptyRigidBodies, new GameObject()));

            for (int i = 0; i < emptyRigidBodies.Length; ++i)
            {
                emptyRigidBodies[i] = new Rigidbody();
            }

            // 対象のオブジェクトをnullにして、CopyPasteComponentsAsNewが失敗するか
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

            // MoveComponentが成功するか
            Assert.IsTrue(ComponentUtility.MoveComponent(rigidBody, distObject));

            var copiedRigidBody = distObject.GetComponent<Rigidbody>();
            // 移動後のコンポーネントが存在するか
            Assert.IsNotNull(copiedRigidBody);
            // 移動前と移動後のコンポーネントの値が同じか
            Assert.AreEqual(copiedRigidBody.mass, mass);

            var originalRigidBody = srcObject.GetComponent<Rigidbody>();
            // 元のコンポーネントが消えているか
            Assert.IsNull(originalRigidBody);
        }


        [Test]
        public void MoveComponentFailureTest()
        {
            // コンポーネントをnullにしてMoveComponentが失敗するか
            Assert.IsFalse(ComponentUtility.MoveComponent(null, new GameObject()));
            // 対象のオブジェクトをnullにしてMoveComponentが失敗するか
            Assert.IsFalse(ComponentUtility.MoveComponent(new Rigidbody(), null));
        }


        [Test]
        public void MoveComponentsFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            // 中身がnullの配列を渡して、MoveComponentsが失敗するか
            Assert.IsFalse(ComponentUtility.MoveComponents(emptyRigidBodies, new GameObject()));

            for (int i = 0; i < emptyRigidBodies.Length; ++i)
            {
                emptyRigidBodies[i] = new Rigidbody();
            }
            // 対象のオブジェクトをnullにして、MoveComponentsが失敗するか
            Assert.False(ComponentUtility.MoveComponents(emptyRigidBodies, null));
        }


        [Test]
        public void DeleteComponentSuccessTest()
        {
            var gameObject = new GameObject();
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            // DeleteComponentの処理が成功するか
            Assert.IsTrue(ComponentUtility.DeleteComponent(rigidBody));
            // オブジェクトからコンポーネントが消えているか
            Assert.IsNull(gameObject.GetComponent<Rigidbody>());
        }


        [Test]
        public void DeleteComponentFailureTest()
        {
            // nullを渡してDeleteComponentが失敗するか
            Assert.IsFalse(ComponentUtility.DeleteComponent(null));
        }


        [Test]
        public void DeleteComponentsFailureTest()
        {
            var emptyRigidBodies = new Rigidbody[1];
            // 中身がnullの配列を渡して、DeleteComponentsが失敗するか
            Assert.IsFalse(ComponentUtility.DeleteComponents(emptyRigidBodies));
        }
    }
}
