using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    // behaviour stores all components that can be enabled/disabled
   [SerializeField]
    Behaviour[] componentsToDisable;

    private Camera sceneCamera;

    // if we are not the active player we must disable components (for all the other players)
    private void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
        else
        {
            // if we are playing, disable scene camera (scene camera has a Main Camera tag)
            sceneCamera = Camera.main;
            if (sceneCamera != null)
            {
               sceneCamera.gameObject.SetActive(false);
            }
        }
    }

    // OnDisable is called when a component gets disabled
    private void OnDisable()
    {
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }

}
