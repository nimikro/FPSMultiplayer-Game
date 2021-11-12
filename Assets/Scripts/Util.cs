using UnityEngine;

public class Util
{
    // Sets a GameObject and all its children to a specific Layer with an index of newLayer
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if(obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach(Transform child in obj.transform)
        {
            if(child == null)
            {
                continue;
            }

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
