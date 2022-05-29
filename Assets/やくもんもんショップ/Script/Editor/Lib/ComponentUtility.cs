using UnityEngine;

namespace Yakumo890.Util
{
    /// <summary>
    /// コンポーネントに関するユーティリティ
    /// </summary>
    public class ComponentUtility : MonoBehaviour
    {
        /// <summary>
        /// 複数のコンポーネントをオブジェクトに移動する
        /// </summary>
        /// <param name="components">移動するコンポーネント群</param>
        /// <param name="target">移動先のオブジェクト</param>
        /// <returns>true: 移動成功<br />false: 移動失敗</returns>
        public static bool MoveComponents(Component[] components, GameObject target)
        {
            if (!CopyPasteComponentsAsNew(components, target))
            {
                return false;
            }

            return DeleteComponents(components);
        }


        /// <summary>
        /// コンポーネントをオブジェクトに移動する
        /// </summary>
        /// <param name="component">移動するコンポーネント</param>
        /// <param name="target">移動先のオブジェクト</param>
        /// <returns>true: 移動成功<br />false: 移動失敗</returns>
        public static bool MoveComponent(Component component, GameObject target)
        {
            if (!CopyPaseteComponentAsNew(component, target))
            {
                return false;
            }

            return DeleteComponent(component);
        }


        /// <summary>
        /// 複数のコンポーネントを削除する
        /// </summary>
        /// <param name="components">削除するコンポーネント群</param>
        /// <returns>true: 削除成功<br />false: 削除失敗</returns>
        public static bool DeleteComponents(Component[] components)
        {
            foreach (var component in components)
            {
                if (!DeleteComponent(component))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// コンポーネントを削除する
        /// </summary>
        /// <param name="component">削除するコンポーネント</param>
        /// <returns>true: 削除成功<br />false: 削除失敗</returns>
        public static bool DeleteComponent(Component component)
        {
            if (component == null)
            {
                return false;
            }

            DestroyImmediate(component);
            return true;
        }


        /// <summary>
        /// 複数のコンポーネントをコピー＆ペーストする
        /// </summary>
        /// <param name="components">コピー＆ペーストするコンポーネント群</param>
        /// <param name="target">ペースト対象のオブジェクト</param>
        /// <returns>true: ペースト成功<br />false: ペースト失敗</returns>
        public static bool CopyPasteComponentsAsNew(Component[] components, GameObject target)
        {
            foreach (var component in components)
            {
                if (!CopyPaseteComponentAsNew(component, target))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// コンポーネントをコピー＆ペーストする
        /// </summary>
        /// <param name="component">コピー＆ペーストするコンポーネント</param>
        /// <param name="target">ペースト対象のオブジェクト</param>
        /// <returns>true: ペースト成功<br />false: ペースト失敗</returns>
        public static bool CopyPaseteComponentAsNew(Component component, GameObject target)
        {
            if (component == null || target == null)
            {
                return false;
            }

            if (!UnityEditorInternal.ComponentUtility.CopyComponent(component))
            {
                return false;
            }

            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
            return true;
        }
    }
}
