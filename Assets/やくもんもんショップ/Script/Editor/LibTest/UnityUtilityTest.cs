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
            Assert.AreEqual(rootFullPath, RootObjectName);

            var expectedChildFullPathIgnoredRoot = $"{ParentObjectName}/{ChildObjectName}";
            var expectedChildFullPath = $"{RootObjectName}/{expectedChildFullPathIgnoredRoot}";

            var childFullPath = childObject.transform.GetFullPath();
            Assert.AreEqual(childFullPath, expectedChildFullPath);

            var childFullPathIgnoredRoot = childObject.transform.GetFullPath(true);
            Assert.AreEqual(childFullPathIgnoredRoot, expectedChildFullPathIgnoredRoot);
        }


        [Test]
        public void FindRecursiveSuccessTest()
        {
            Assert.IsNotNull(rootObject.FindRecursive(ParentObjectName));
            Assert.IsNotNull(rootObject.FindRecursive(ChildObjectName));
        }


        [Test]
        public void FindRecursiveFailureTest()
        {
            Assert.IsNull(rootObject.FindRecursive("NotFoundObjectName"));
        }


        [Test]
        public void ToEditorOnlyTest()
        {
            var obj = new GameObject();
            obj.ToEditorOnly();

            Assert.AreEqual(obj.tag, "EditorOnly");
        }


        [Test]
        public void IsEditorOnlyTest()
        {
            var obj = new GameObject();
            obj.tag = "EditorOnly";

            Assert.IsTrue(obj.IsEditorOnly());
        }
    }
}
