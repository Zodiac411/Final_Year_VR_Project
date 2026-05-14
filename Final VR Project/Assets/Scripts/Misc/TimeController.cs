using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private InputActionReference SlowTimeAction;

    [SerializeField] private AudioClip SlowTimeClip;
    [SerializeField] private AudioClip SpeedupTimeClip;

    [SerializeField] private bool YKeySlowsTime = true;
    [SerializeField] private bool ForceTimeScale = false;

    [SerializeField] private float SlowTimeScale = 0.5f;

    private bool SetFixedDelta = false;
    private bool CheckInput = true;

    public bool TimeSlowing
    {
        get { return slowingTime; }
    }

    bool slowingTime = false;
    bool routineRunning = false;

    private float originalFixedDelta;
    private AudioSource audioSource;

    private IEnumerator resumeRoutine;

    private void Start()
    {
        if (SetFixedDelta)
        {
            Time.fixedDeltaTime = (Time.timeScale / UnityEngine.XR.XRDevice.refreshRate);
        }

        originalFixedDelta = Time.fixedDeltaTime;

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {

        if (CheckInput)
        {
            if (SlowTimeInputDown() || ForceTimeScale)
            {
                SlowTime();
            }
            else
            {
                ResumeTime();
            }
        }
    }

    public virtual bool SlowTimeInputDown()
    {
        if ((YKeySlowsTime && XRInput.Instance.YButton))
        {
            return true;
        }

        if (SlowTimeAction != null)
        {
            return SlowTimeAction.action.ReadValue<float>() > 0f;
        }

        return false;
    }

    public void SlowTime()
    {
        if (!slowingTime)
        {
            if (resumeRoutine != null)
            {
                StopCoroutine(resumeRoutine);
            }

            audioSource.clip = SlowTimeClip;
            audioSource.Play();

            if (SpeedupTimeClip)
            {
                XRInput.Instance.VibrateController(0.1f, 0.2f, SpeedupTimeClip.length, ControllerHand.Left);
            }

            Time.timeScale = SlowTimeScale;
            Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;

            slowingTime = true;
        }
    }

    public void ResumeTime()
    {
        if (slowingTime && !audioSource.isPlaying && !routineRunning)
        {

            resumeRoutine = resumeTimeRoutine();
            StartCoroutine(resumeRoutine);
        }
    }

    private IEnumerator resumeTimeRoutine()
    {
        routineRunning = true;

        audioSource.clip = SpeedupTimeClip;
        audioSource.Play();

        XRInput.Instance.VibrateController(0.1f, 0.2f, SpeedupTimeClip.length, ControllerHand.Left);

        yield return new WaitForSeconds(0.35f);

        Time.timeScale = 1;
        Time.fixedDeltaTime = originalFixedDelta;

        slowingTime = false;
        routineRunning = false;
    }
}

