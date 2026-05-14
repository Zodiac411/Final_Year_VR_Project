using UnityEngine;
using UnityEngine.UI;

public class HealthUISlider : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Image fillArea;

    private void Start()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        healthSlider.maxValue = 100;
        healthSlider.value = 100;
        healthSlider.onValueChanged.AddListener(UpdateFillColor);
        UpdateFillColor(healthSlider.value);
    }

    private void UpdateFillColor(float value)
    {
        float percentage = value / healthSlider.maxValue;

        if (percentage > 0.66f)
        {
            fillArea.color = Color.green;
        }
        else if (percentage <= 0.66f && percentage >= 0.33f)
        {
            fillArea.color = Color.yellow;
        }
        else if (percentage < 0.33f)
        {
            fillArea.color = Color.red;
        }
    }

    public void UpdateHealth(int newHealth)
    {
        healthSlider.value = newHealth;
        healthText.text = newHealth.ToString();
    }
}
