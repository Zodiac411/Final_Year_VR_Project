using UnityEngine;

public class HandModelSwitcher : MonoBehaviour
{
    public HandModelSelector hms;

    [SerializeField] private int HandModelId = 1;

    private void Start()
    {
        if (hms == null)
        {
            hms = FindObjectOfType<HandModelSelector>();
        }

        if (hms == null)
        {
            Debug.Log("No Hand Model Selector Found in Scene. Will not be able to switch hand models");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Grabber>() != null)
        {
            hms.ChangeHandsModel(HandModelId, false);
        }
    }
}