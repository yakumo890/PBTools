/*
Copyright (c) 2022 yakumo/yakumonmon shop
https://yakumonmon-shop.booth.pm/
This software is released under the MIT License
https://github.com/yakumo890/PBTools/License.txt
*/

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
            // avatarがアバターであると判定されるか
            Assert.IsTrue(AvatarUtility.IsAvatar(avatar));
        }


        [Test]
        public void CreateAvatarObjectSuccessTest()
        {
            string objectName = "TestObject";
            AvatarUtility.CreateAvatarObject(avatar, objectName);

            // avatarに子オブジェクトが一つ作られているか
            Assert.AreEqual(avatar.transform.childCount, 1);
            // 指定の名前のオブジェクトが存在しているか
            Assert.IsNotNull(avatar.transform.Find(objectName));

            AvatarUtility.CreateAvatarObject(avatar, objectName, true);
            // 同名のオブジェクトを作成することを許して、オブジェクトが作成されているか
            Assert.AreEqual(avatar.transform.childCount, 2);

            AvatarUtility.CreateAvatarObject(avatar, objectName, false);
            // 同名のオブジェクトを作成することを許さず、オブジェクトが作成されて_いない_か
            Assert.AreEqual(avatar.transform.childCount, 2);
        }


        [Test]
        public void CreateAvatarObjectFailureTest()
        {
            // nullを渡してnullが帰ってくるか
           Assert.IsNull(AvatarUtility.CreateAvatarObject(null, "null_test"));
        }
    }
}
