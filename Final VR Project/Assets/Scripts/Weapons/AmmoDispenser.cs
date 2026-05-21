using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class AmmoDispenser : MonoBehaviour
{
    [Header("Grabbers")]
    [SerializeField] private Grabber LeftGrabber;
    [SerializeField] private Grabber RightGrabber;

    [Header("Ammo Pouches")]
    [SerializeField] private GameObject LeftAmmoDispenserObject;
    [SerializeField] private GameObject RightAmmoDispenserObject;

    [Header("Clip Prefabs")]
    [SerializeField] private GameObject PistolClip;
    [SerializeField] private GameObject ShotgunShell;
    [SerializeField] private GameObject SCARClip;
    [SerializeField] private GameObject M4A4Clip;
    [SerializeField] private GameObject AK74UClip;
    [SerializeField] private GameObject SIGMCXClip;
    [SerializeField] private GameObject Leader_50Clip;
    [SerializeField] private GameObject Ultimax100Clip;

    [Header("Clip Amounts")]
    public int CurrentPistolClips = 5;
    public int CurrentSCARClips = 5;
    public int CurrentM4A4Clips = 5;
    public int CurrentAK74UClip = 5;
    public int CurrentSIGMCXClip = 5;
    public int CurrentLeader_50Clip = 5;
    public int CurrentShotgunShells = 30;
    public int CurrentUltimax100Clip = 3;

    private void Update()
    {
        bool leftHandEquipped = grabberHasWeapon(LeftGrabber);
        bool rightHandEquipped = grabberHasWeapon(RightGrabber);

        if (RightAmmoDispenserObject.activeSelf != leftHandEquipped)
        {
            RightAmmoDispenserObject.SetActive(leftHandEquipped);
        }
        
        if (LeftAmmoDispenserObject.activeSelf != rightHandEquipped)
        {
            LeftAmmoDispenserObject.SetActive(rightHandEquipped);
        }
    }

    private bool grabberHasWeapon(Grabber grabbable)
    {

        if (grabbable == null || grabbable.HeldGrabbable == null)
        {
            return false;
        }

        string grabName = grabbable.HeldGrabbable.transform.name;

        if (grabName.Contains("Shotgun") ||
            grabName.Contains("M1911") ||
            grabName.Contains("SCAR") ||
            grabName.Contains("M4A4") ||
            grabName.Contains("AK-74U") ||
            grabName.Contains("Leader .50") ||
            grabName.Contains("SIG MCX") ||
            grabName.Contains("Ultimax 100"))
        {
            return true;
        }

        return false;
    }

    public GameObject GetAmmo()
    {

        bool leftGrabberValid = LeftGrabber != null && LeftGrabber.HeldGrabbable != null;
        bool rightGrabberValid = RightGrabber != null && RightGrabber.HeldGrabbable != null;

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("Shotgun") && CurrentShotgunShells > 0)
        {
            CurrentShotgunShells--;
            return ShotgunShell;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("Shotgun") && CurrentShotgunShells > 0)
        {
            CurrentShotgunShells--;
            return ShotgunShell;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("SCAR") && CurrentSCARClips > 0)
        {
            CurrentSCARClips--;
            return SCARClip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("SCAR") && CurrentSCARClips > 0)
        {
            CurrentSCARClips--;
            return SCARClip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("M4A4") && CurrentM4A4Clips > 0)
        {
            CurrentM4A4Clips--;
            return M4A4Clip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("M4A4") && CurrentM4A4Clips > 0)
        {
            CurrentM4A4Clips--;
            return M4A4Clip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("M1911") && CurrentPistolClips > 0)
        {
            CurrentPistolClips--;
            return PistolClip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("M1911") && CurrentPistolClips > 0)
        {
            CurrentPistolClips--;
            return PistolClip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("AK-74U") && CurrentAK74UClip > 0)
        {
            CurrentAK74UClip--;
            return AK74UClip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("AK-74U") && CurrentAK74UClip > 0)
        {
            CurrentAK74UClip--;
            return AK74UClip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("SIG MCX") && CurrentSIGMCXClip > 0)
        {
            CurrentSIGMCXClip--;
            return SIGMCXClip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("SIG MCX") && CurrentSIGMCXClip > 0)
        {
            CurrentSIGMCXClip--;
            return SIGMCXClip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("Leader .50") && CurrentLeader_50Clip > 0)
        {
            CurrentLeader_50Clip--;
            return Leader_50Clip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("Leader .50") && CurrentLeader_50Clip > 0)
        {
            CurrentLeader_50Clip--;
            return Leader_50Clip;
        }

        if (leftGrabberValid && LeftGrabber.HeldGrabbable.transform.name.Contains("Ultimax 100") && CurrentUltimax100Clip > 0)
        {
            CurrentUltimax100Clip--;
            return Ultimax100Clip;
        }
        else if (rightGrabberValid && RightGrabber.HeldGrabbable.transform.name.Contains("Ultimax 100") && CurrentUltimax100Clip > 0)
        {
            CurrentUltimax100Clip--;
            return Ultimax100Clip;
        }

        return null;
    }

    public void GrabAmmo(Grabber grabber)
    {
        GameObject ammoClip = GetAmmo();
        if (ammoClip != null)
        {
            GameObject ammo = Instantiate(ammoClip, grabber.transform.position, grabber.transform.rotation) as GameObject;
            Grabbable grabbable = ammo.GetComponent<Grabbable>();

            GrabbableRingHelper grh = ammo.GetComponentInChildren<GrabbableRingHelper>();
            if (grh)
            {
                Destroy(grh);
                RingHelper r = ammo.GetComponentInChildren<RingHelper>();
                Destroy(r.gameObject);
            }

            ammo.transform.parent = grabber.transform;
            ammo.transform.localPosition = -grabbable.GrabPositionOffset;
            ammo.transform.parent = null;

            grabber.GrabGrabbable(grabbable);
        }
    }

    public virtual void AddAmmo(string AmmoName)
    {
        if (AmmoName.Contains("SIG MCX Clip"))
        {
            CurrentSIGMCXClip++;
        }
        else if (AmmoName.Contains("SCAR Clip"))
        {
            CurrentSCARClips++;
        }
        else if (AmmoName.Contains("M1911 Clip"))
        {
            CurrentPistolClips++;
        }
        else if (AmmoName.Contains("M4A4 Clip"))
        {
            CurrentM4A4Clips++;
        }
        else if (AmmoName.Contains("Shell"))
        {
            CurrentShotgunShells++;
        }
        else if (AmmoName.Contains("AK-74U Clip"))
        {
            CurrentAK74UClip++;
        }
        else if (AmmoName.Contains("Leader .50 Clip"))
        {
            CurrentLeader_50Clip++;
        }
        else if (AmmoName.Contains("Ultimax 100"))
        {
            CurrentUltimax100Clip++;
        }
    }
}
