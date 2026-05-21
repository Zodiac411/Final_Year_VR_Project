using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class CalibratePlayerHeight : MonoBehaviour
{
    [SerializeField] private float DesiredPlayerHeight = 1.65f;
    [SerializeField] private PlayerController PlayerController;

    [Header("Input :")]
    [SerializeField] private InputAction CalibrateHeightAction;

    private float _initialOffset = 0;

    private bool CalibrateOnStart = true;

    private void Start()
    {
        if (CalibrateHeightAction != null)
        {
            CalibrateHeightAction.Enable();
            CalibrateHeightAction.performed += context => { CalibrateHeight(); };
        }

        if (PlayerController == null)
        {
            PlayerController = GetComponentInChildren<PlayerController>();
        }

        if (CalibrateOnStart)
        {
            StartCoroutine(setupInitialOffset());
        }
    }

    public void CalibrateHeight()
    {
        CalibrateHeight(DesiredPlayerHeight);
    }

    public void CalibrateHeight(float calibrateHeight)
    {
        float physicalHeight = GetCurrentPlayerHeight();

        PlayerController.CharacterControllerYOffset = calibrateHeight - physicalHeight;
    }

    public void ResetPlayerHeight()
    {
        PlayerController.CharacterControllerYOffset = _initialOffset;
    }

    public float GetCurrentPlayerHeight()
    {
        if (PlayerController != null)
        {
            return PlayerController.CameraHeight;
        }

        return 0;
    }

    public virtual void SetInitialOffset()
    {
        if (PlayerController)
        {
            _initialOffset = PlayerController.CharacterControllerYOffset;
        }
    }

    private IEnumerator setupInitialOffset()
    {
        yield return new WaitForSeconds(0.1f);

        SetInitialOffset();

        CalibrateHeight();
    }
}

