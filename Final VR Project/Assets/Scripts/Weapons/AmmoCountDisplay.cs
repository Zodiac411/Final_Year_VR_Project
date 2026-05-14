using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AmmoCountDisplay : MonoBehaviour
{
    [SerializeField] private AmmoDispenser ammoDispenser;
    [SerializeField] private string weaponType;

    [SerializeField] private int lowAmmoThreshold = 5;

    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color noAmmoColor = Color.red;

    private Text displayText;
    private Color originalColor;

    void Start()
    {
        displayText = GetComponent<Text>();
        originalColor = displayText.color;

        if (ammoDispenser == null)
        {
            Debug.LogError("AmmoDispenser reference not set on " + gameObject.name);
        }
    }

    void Update()
    {
        if (ammoDispenser != null)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        int ammoCount = 0;
        switch (weaponType)
        {
            case "Pistol":
                ammoCount = int.MaxValue;
                break;
            case "SCAR":
                ammoCount = ammoDispenser.CurrentSCARClips;
                break;
            case "M4A4":
                ammoCount = ammoDispenser.CurrentM4A4Clips;
                break;
            case "AK74U":
                ammoCount = ammoDispenser.CurrentAK74UClip;
                break;
            case "SIGMCX":
                ammoCount = ammoDispenser.CurrentSIGMCXClip;
                break;
            case "Leader_50":
                ammoCount = ammoDispenser.CurrentLeader_50Clip;
                break;
            case "Ultimax 100":
                ammoCount = ammoDispenser.CurrentUltimax100Clip;
                break;
            case "Shotgun":
                ammoCount = ammoDispenser.CurrentShotgunShells;
                break;
            default:
                displayText.text = "Unknown Weapon Type";
                return;
        }

        displayText.text = ammoCount == int.MaxValue ? "INF" : ammoCount.ToString();

        if (ammoCount == 0)
        {
            displayText.color = noAmmoColor;
        }
        else if (ammoCount <= lowAmmoThreshold)
        {
            displayText.color = lowAmmoColor;
        }
        else
        {
            displayText.color = originalColor;
        }
    }
}
