using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyTxt : MonoBehaviour
{
    public Text currencyTxt;
    private float currencyval = 0;
    GameObject credManager;
    // Start is called before the first frame update
    private void Start()
    {
       credManager = GameObject.Find("CreditsManager");
    }

    // Update is called once per frame
    void Update()
    {
        currencyval = credManager.GetComponent<CreditsManager>().Credits;
        currencyTxt.text = "$" + currencyval;
    }
}
