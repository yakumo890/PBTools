/*
Copyright (c) 2022 yakumo/yakumonmon shop
https://yakumonmon-shop.booth.pm/
This software is released under the MIT License
https://github.com/yakumo890/PBTools/License.txt
*/

using UnityEngine;
using NUnit.Framework;


namespace Yakumo890.Util.Test
{
    public class UnityUtilityTest
    {
        private GameObject rootObject;
        private GameObject childObject;

        private const string RootObjectName = "RootObject";
        private const string ParentObjectName = "ParentObject";
        private const string ChildObjectName = "ChildObject";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            rootObject = new GameObject();
            rootObject.name = RootObjectName;

            var parentObject = new GameObject();
            parentObject.name = ParentObjectName;

            childObject = new GameObject();
            childObject.name = ChildObjectName;

            childObject.transform.SetParent(parentObject.transform);
            parentObject.transform.SetParent(rootObject.transform);
        }


        [Test]
        public void GetFullPathTest()
        {
            var rootFullPath = rootObject.transform.GetFullPath();
            // ルートオブジェクトのフルパスが取得できる
            Assert.AreEqual(rootFullPath, RootObjectName);

            var expectedChildFullPathIgnoredRoot = $"{ParentObjectName}/{ChildObjectName}";
            var expectedChildFullPath = $"{RootObjectName}/{expectedChildFullPathIgnoredRoot}";

            var childFullPath = childObject.transform.GetFullPath();
            // 子オブジェクトのフルパスが取得できる
            Assert.AreEqual(childFullPath, expectedChildFullPath);

            var childFullPathIgnoredRoot = childObject.transform.GetFullPath(true);
            // ルートオブジェクトを抜かしてフルパスが取得できる
            Assert.AreEqual(childFullPathIgnoredRoot, expectedChildFullPathIgnoredRoot);
        }


        [Test]
        public void FindRecursiveSuccessTest()
        {
            // ルートオブジェクトが再帰的検索で見つかる
            Assert.IsNotNull(rootObject.FindRecursive(ParentObjectName));
            // 子オブジェクトが再帰的検索で見つかる
            Assert.IsNotNull(rootObject.FindRecursive(ChildObjectName));
        }


        [Test]
        public void FindRecursiveFailureTest()
        {
            // 存在しないオブジェクトが見つからない
            Assert.IsNull(rootObject.FindRecursive("NotFoundObjectName"));
        }


        [Test]
        public void ToEditorOnlyTest()
        {
            var obj = new GameObject();
            obj.ToEditorOnly();
            // EditorOnlyタグになっているか
            Assert.AreEqual(obj.tag, "EditorOnly");
        }


        [Test]
        public void IsEditorOnlyTest()
        {
            var obj = new GameObject();
            obj.tag = "EditorOnly";
            // EditorOnlyタグであるオブジェクトに対してtrueになる
            Assert.IsTrue(obj.IsEditorOnly());
        }
    }
}
