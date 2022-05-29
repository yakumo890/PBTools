using UnityEngine;

namespace Yakumo890.Util
{

    public static class Extensions
    {
        private const string EditorOnlyTag = "EditorOnly";

        /// <summary>
        /// オブジェクトのフルパスを取得する
        /// </summary>
        /// <param name="ignoreRoot">ルートのオブジェクトをパスから除くか(trueなら除く)</param>
        /// <returns>フルパス</returns>
        public static string GetFullPath(this GameObject obj, bool ignoreRoot = false)
        {
            return GetFullPath(obj.transform, ignoreRoot);
        }


        /// <summary>
        /// Transformがついているオブジェクトのフルパスを取得する
        /// </summary>
        /// <param name="ignoreRoot">ルートのオブジェクトをパスから除くか(trueなら除く)</param>
        /// <returns>フルパス</returns>
        /// 参考: https://amagamina.jp/blog/gameobject-fullpath/
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


        /// <summary>
        /// オブジェクトから再帰的に子オブジェクトを検索する
        /// </summary>
        /// <param name="childName">探したいオブジェクトの名前</param>
        /// <returns>最初に見つかったオブジェクト<br />見つからなければnull</returns>
        public static Transform FindRecursive(this GameObject parent, string childName)
        {
            return FindRecursive(parent.transform, childName);
        }


        /// <summary>
        /// Transformがついているオブジェクトから再帰的に子オブジェクトを検索する
        /// </summary>
        /// <param name="childName">探したいオブジェクトの名前</param>
        /// <returns>最初に見つかったオブジェクト<br />見つからなければnull</returns>
        /// 参考: https://stackoverflow.com/questions/33437244/find-children-of-children-of-a-gameobject
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


        /// <summary>
        /// オブジェクトのタグをEditorOnlyに変更する
        /// </summary>
        public static void ToEditorOnly(this GameObject obj)
        {
            obj.tag = EditorOnlyTag;
        }

        /// <summary>
        /// オブジェクトのタグがEditorOnlyかどうか
        /// </summary>
        /// <returns>true: EditorOnly<br/>false: EditorOnlyではない</returns>
        public static bool IsEditorOnly(this GameObject obj)
        {
            return obj.tag == EditorOnlyTag;
        }
    }
}
