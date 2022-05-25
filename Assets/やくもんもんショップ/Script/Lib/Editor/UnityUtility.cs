using UnityEngine;

namespace Yakumo890.Util
{
    public static class Extensions
    {
        private const string EditorOnlyTag = "EditorOnly";

        //https://amagamina.jp/blog/gameobject-fullpath/
        public static string GetFullPath(this GameObject obj, bool ignoreRoot = false)
        {
            return GetFullPath(obj.transform, ignoreRoot);
        }

        public static string GetFullPath(this Transform t, bool ignoreRoot = false)
        {
            string path = t.name;
            var parent = t.parent;
            while (parent)
            {
                if (ignoreRoot && parent.parent == null)
                {
                    break;
                }

                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }

        public static Transform FindRecursive(this GameObject parent, string childName)
        {
            return FindRecursive(parent.transform, childName);
        }

        //https://stackoverflow.com/questions/33437244/find-children-of-children-of-a-gameobject
        public static Transform FindRecursive(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = FindRecursive(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }


        public static void ToEditorOnly(this GameObject obj)
        {
            obj.tag = EditorOnlyTag;
        }


        public static bool IsEditorOnly(this GameObject obj)
        {
            return obj.tag == EditorOnlyTag;
        }
    }
}
