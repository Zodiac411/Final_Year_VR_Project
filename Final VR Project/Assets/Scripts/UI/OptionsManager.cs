using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider leftPouchSlider;
    [SerializeField] private Slider rightPouchSlider;

    [SerializeField] private Slider leftHolsterSlider;
    [SerializeField] private Slider rightHolsterSlider;

    [SerializeField] private Slider volumeSlider;

    [Header("GameObjects")]
    [SerializeField] private GameObject leftPouch;
    [SerializeField] private GameObject rightPouch;
    [SerializeField] private GameObject leftHolster;
    [SerializeField] private GameObject rightHolster;

    [Space(20)]
    private AudioSource gameAudioSource;

    private readonly float defaultLeftPouchValue = -0.393999994f;
    private readonly float defaultRightPouchValue = 0.404000014f;
    private readonly float defaultLeftHolsterValue = -0.209999993f;
    private readonly float defaultRightHolsterValue = 0.209999993f;

    private void Awake()
    {
        SetSliderRange(leftPouchSlider, -0.5f, 0);
        SetSliderRange(rightPouchSlider, 0, 0.5f);
        SetSliderRange(leftHolsterSlider, -0.5f, 0);
        SetSliderRange(rightHolsterSlider, 0, 0.5f);

        leftPouch.transform.localPosition = new Vector3(-0.393999994f, leftPouch.transform.localPosition.y, leftPouch.transform.localPosition.z);
        rightPouch.transform.localPosition = new Vector3(0.404000014f, rightPouch.transform.localPosition.y, rightPouch.transform.localPosition.z);
        leftHolster.transform.localPosition = new Vector3(-0.209999993f, leftHolster.transform.localPosition.y, leftHolster.transform.localPosition.z);
        rightHolster.transform.localPosition = new Vector3(0.209999993f, rightHolster.transform.localPosition.y, rightHolster.transform.localPosition.z);

        leftPouchSlider.value = leftPouch.transform.localPosition.x;
        rightPouchSlider.value = rightPouch.transform.localPosition.x;
        leftHolsterSlider.value = leftHolster.transform.localPosition.x;
        rightHolsterSlider.value = rightHolster.transform.localPosition.x;

        leftPouchSlider.onValueChanged.AddListener(UpdateLeftPouchPosition);
        rightPouchSlider.onValueChanged.AddListener(UpdateRightPouchPosition);
        leftHolsterSlider.onValueChanged.AddListener(UpdateLeftHolsterPosition);
        rightHolsterSlider.onValueChanged.AddListener(UpdateRightHolsterPosition);
        volumeSlider.onValueChanged.AddListener(UpdateVolume);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = 0.5f;

        UpdateLeftPouchPosition(leftPouchSlider.value);
        UpdateRightPouchPosition(rightPouchSlider.value);
        UpdateLeftHolsterPosition(leftHolsterSlider.value);
        UpdateRightHolsterPosition(rightHolsterSlider.value);
        UpdateVolume(volumeSlider.value);
    }

    private void Start()
    {
        gameAudioSource = FindObjectOfType<AudioSource>();
    }

    private void SetSliderRange(Slider slider, float min, float max)
    {
        slider.minValue = min;
        slider.maxValue = max;
    }

    private float MapPositionToSliderValue(float position, float minSliderValue, float maxSliderValue)
    {
        return Mathf.InverseLerp(minSliderValue, maxSliderValue, position);
    }

    private void UpdateLeftPouchPosition(float value)
    {
        leftPouch.transform.localPosition = new Vector3(value, leftPouch.transform.localPosition.y, leftPouch.transform.localPosition.z);
    }

    private void UpdateRightPouchPosition(float value)
    {
        rightPouch.transform.localPosition = new Vector3(value, rightPouch.transform.localPosition.y, rightPouch.transform.localPosition.z);
    }

    private void UpdateLeftHolsterPosition(float value)
    {
        leftHolster.transform.localPosition = new Vector3(value, leftHolster.transform.localPosition.y, leftHolster.transform.localPosition.z);
    }

    private void UpdateRightHolsterPosition(float value)
    {
        rightHolster.transform.localPosition = new Vector3(value, rightHolster.transform.localPosition.y, rightHolster.transform.localPosition.z);
    }

    private void UpdateVolume(float value)
    {
        AudioListener.volume = value;
        if (gameAudioSource != null)
        {
            gameAudioSource.volume = value;
        }
    }

    public void ResetLeftPouchToDefault()
    {
        leftPouchSlider.value = defaultLeftPouchValue;
        UpdateLeftPouchPosition(defaultLeftPouchValue);
    }

    public void ResetRightPouchToDefault()
    {
        rightPouchSlider.value = defaultRightPouchValue;
        UpdateRightPouchPosition(defaultRightPouchValue);
    }

    public void ResetLeftHolsterToDefault()
    {
        leftHolsterSlider.value = defaultLeftHolsterValue;
        UpdateLeftHolsterPosition(defaultLeftHolsterValue);
    }

    public void ResetRightHolsterToDefault()
    {
        rightHolsterSlider.value = defaultRightHolsterValue;
        UpdateRightHolsterPosition(defaultRightHolsterValue);
    }
}
