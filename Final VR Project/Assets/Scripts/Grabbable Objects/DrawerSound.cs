using UnityEngine;

public class DrawerSound : MonoBehaviour
{
    [SerializeField] private AudioClip DrawerOpenSound;
    [SerializeField] private float DrawerOpenValue = 80f;

    [SerializeField] private AudioClip DrawerCloseSound;
    [SerializeField] private float DrawerCloseValue = 20f;

    bool playedOpenSound = false;
    bool playedCloseSound = false;

    public void OnDrawerUpdate(float drawerValue)
    {
        if (drawerValue < DrawerOpenValue && !playedOpenSound && DrawerOpenSound != null)
        {
            XRManager.Instance.PlaySpatialClipAt(DrawerOpenSound, transform.position, 1f);
            playedOpenSound = true;
        }
        if (drawerValue > DrawerOpenValue)
        {
            playedOpenSound = false;
        }

        if (drawerValue > DrawerCloseValue && !playedCloseSound && DrawerCloseSound != null)
        {
            XRManager.Instance.PlaySpatialClipAt(DrawerCloseSound, transform.position, 1f);
            playedCloseSound = true;
        }

        if (drawerValue < DrawerCloseValue)
        {
            playedCloseSound = false;
        }
    }
}

