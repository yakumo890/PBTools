using UnityEngine;

namespace Yakumo890.Util
{
    /// <summary>
    /// コンポーネントに関するユーティリティ
    /// </summary>
    public class ComponentUtility : MonoBehaviour
    {
        public static bool MoveComponents(Component[] components, GameObject target)
        {
            if (!CopyPasteComponentsAsNew(components, target))
            {
                return false;
            }

            return DeleteComponents(components);
        }


        public static bool MoveComponent(Component component, GameObject target)
        {
            if (!CopyPaseteComponentAsNew(component, target))
            {
                return false;
            }

            return DeleteComponent(component);
        }

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

        public static bool DeleteComponent(Component component)
        {
            if (component == null)
            {
                return false;
            }

            DestroyImmediate(component);
            return true;
        }

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
