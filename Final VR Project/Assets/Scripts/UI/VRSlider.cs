using UnityEngine;


public class VRSlider : MonoBehaviour
{
    public float SlidePercentage
    {
        get
        {
            return sliderPercent;
        }
    }
    
    private float sliderPercent;

    [SerializeField] private FloatEvent onSliderChange;

    [SerializeField] private float lastSliderPercentage;
    [SerializeField] private float slideRangeLow = -0.15f;
    [SerializeField] private float slideRangeHigh = 0.15f;
    [SerializeField] private float slideRange;

    private void Start()
    {
        ConfigurableJoint cj = GetComponent<ConfigurableJoint>();
        if (cj)
        {
            slideRangeLow = cj.linearLimit.limit * -1;
            slideRangeHigh = cj.linearLimit.limit;
        }

        slideRange = slideRangeHigh - slideRangeLow;
    }

    private void Update()
    {
        sliderPercent = ((transform.localPosition.x - 0.001f) + slideRangeHigh) / slideRange;
        sliderPercent = Mathf.Ceil(sliderPercent * 100);

        if (sliderPercent != lastSliderPercentage)
        {
            OnSliderChange(sliderPercent);
        }

        lastSliderPercentage = sliderPercent;
    }

    public virtual void OnSliderChange(float percentage)
    {

        if (onSliderChange != null)
        {
            onSliderChange.Invoke(percentage);
        }
    }
}