using UnityEngine;
using UnityEngine.UI;

public class CurrencyTxt : MonoBehaviour
{
    public Text currencyTxt;
    [SerializeField] private CreditsManager creditsManager;

    private void Awake()
    {
        if (creditsManager == null)
        {
            creditsManager = CreditsManager.Instance;
        }

        if (creditsManager != null)
        {
            creditsManager.OnCreditsChanged += HandleCreditsChanged;
            HandleCreditsChanged(creditsManager.Credits);
        }
    }

    private void OnDisable()
    {
        if (creditsManager != null)
        {
            creditsManager.OnCreditsChanged -= HandleCreditsChanged;
        }
    }

    private void HandleCreditsChanged(int balance)
    {
        if (currencyTxt != null)
        {
            currencyTxt.text = "$" + balance;
        }
    }
}
