using UnityEngine;

namespace Yakumo890.Util.VRC
{
    public class AvatarUtility
    {
        /// <summary>
        /// アバターオブジェクトの直下に空のゲームオブジェクトを生成する
        /// </summary>
        /// <param name="avatar">アバターのゲームオブジェクト</param>
        /// <param name="name">ゲームオブジェクトの名前</param>
        /// <param name="ignoreExisting">trueなら、同名のオブジェクトが存在しても新規に作成する</param>
        /// <returns>生成したゲームオブジェクト</returns>
        /// <remarks>ここではavatarはバリデーションチェックを行わない(アバターかどうかを気にしない)</remarks>
        public static GameObject CreateAvatarObject(GameObject avatar, string name, bool ignoreExisting = true)
        {
            if (avatar == null)
            {
                return null;
            }

            // ignoreExistingがfalseで、同名オブジェクトが存在したらそれを返す
            if (!ignoreExisting)
            {
                var existingTransform = avatar.transform.Find(name);
                if (existingTransform != null)
                {
                    return existingTransform.gameObject;
                }
            }

            var obj = new GameObject();
            obj.name = name;
            obj.transform.SetParent(avatar.transform);

            return obj;
        }


        /// <summary>
        /// ゲームオブジェクトがAvatarかどうかを確認する。
        /// AvatarとはAnimatorコンポーネントを持っていることとする。
        /// </summary>
        /// <param name="target">Avatarか確認するゲームオブジェクト</param>
        /// <returns>
        /// Avatarならtrue
        /// Avatarでないか、targetがnullならfalse
        /// </returns>
        public static bool IsAvatar(GameObject target)
        {
            if (target == null)
            {
                return false;
            }

            return target.GetComponent<Animator>() != null;
        }
    }
}

