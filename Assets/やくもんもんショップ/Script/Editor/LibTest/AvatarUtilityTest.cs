using UnityEngine;
using NUnit.Framework;

namespace Yakumo890.Util.VRC.Test
{
    public class AvatarUtilityTest
    {
        private GameObject avatar;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            avatar = new GameObject();
            avatar.name = "avatar_for_unit_test";
            avatar.AddComponent<Animator>();
        }


        [Test]
        public void IsAvatarTest()
        {
            Assert.IsTrue(AvatarUtility.IsAvatar(avatar));
        }


        [Test]
        public void CreateAvatarObjectSuccessTest()
        {
            string objectName = "TestObject";
            AvatarUtility.CreateAvatarObject(avatar, objectName);

            Assert.AreEqual(avatar.transform.childCount, 1);
            Assert.IsNotNull(avatar.transform.Find(objectName));

            AvatarUtility.CreateAvatarObject(avatar, objectName, true);
            Assert.AreEqual(avatar.transform.childCount, 2);

            AvatarUtility.CreateAvatarObject(avatar, objectName, false);
            Assert.AreEqual(avatar.transform.childCount, 2);
        }


        [Test]
        public void CreateAvatarObjectFailureTest()
        {
           Assert.IsNull(AvatarUtility.CreateAvatarObject(null, "null_test"));
        }
    }
}
