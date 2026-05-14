using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

[System.Serializable] 
public class WeaponSlotUI
{
    public TextMeshProUGUI textMeshProUGUI;
    public int weaponSlotIndex;
}

public class VendingMachine : MonoBehaviour
{
    public WeaponSlot[] weaponSlots;
    public GameObject spawner; 
    public Animation spawnerAnimation; 
    public WeaponSlotUI[] weaponSlotUIs; 

    private Dictionary<TextMeshProUGUI, int> slotTextMapping;

    void Start()
    {
        InitializeSlotTextMapping();

        for (int i = 0; i < weaponSlotUIs.Length; i++)
        {
            WeaponSlotUI slotUI = weaponSlotUIs[i];

            
            if (slotUI.weaponSlotIndex < 0 || slotUI.weaponSlotIndex >= weaponSlots.Length)
            {
                Debug.LogError($"WeaponSlotUI at index {i} has an out-of-range weaponSlotIndex of {slotUI.weaponSlotIndex}.");
                continue;
            }

            
            weaponSlots[slotUI.weaponSlotIndex].HideWeapons();
            weaponSlots[slotUI.weaponSlotIndex].RandomizeWeapon();
            UpdateUIForSlot(weaponSlots[slotUI.weaponSlotIndex]);
        }

    }



    private void InitializeSlotTextMapping()
    {
        slotTextMapping = new Dictionary<TextMeshProUGUI, int>();

        foreach (WeaponSlotUI slotUI in weaponSlotUIs)
        {
            if (slotUI.textMeshProUGUI != null)
            {
                slotTextMapping[slotUI.textMeshProUGUI] = slotUI.weaponSlotIndex;
            }
            else
            {
                Debug.LogError("One of the TextMeshProUGUI components is not assigned in the inspector.");
            }
        }
    }

    public void VendWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length)
        {
            Debug.LogError("Invalid slot index.");
            return;
        }

        WeaponSlot slot = weaponSlots[index];
        if (slot != null)
        {
            int price = slot.GetPrice();
            print(price);
            if (CreditsManager.Instance.Credits >= slot.GetPrice())
            {
                CreditsManager.Instance.SpendCredits(price);
                slot.VendWeapon(spawner);
                Debug.Log("Dispensed Weapon: " + slot.selectedWeaponName);

                
            }
            else
            {
                Debug.Log("Not enough credits to purchase.");
            }
        }
        else
        {
            Debug.LogError("Weapon Slot at index " + index + " is not assigned.");
        }
    }

    public static VendingMachine Instance;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateUIForSlot(WeaponSlot slot)
    {
       
        foreach (var slotUI in weaponSlotUIs)
        {
            if (slotUI.weaponSlotIndex == Array.IndexOf(weaponSlots, slot))
            {
                slotUI.textMeshProUGUI.text = $"{slot.selectedWeaponName}\n${slot.GetPrice()}";
                break;
            }
        }
    }

    
}
