using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class PunctureCollider : MonoBehaviour
{
    [Header("Puncture properties")]
    [SerializeField] private float FRequiredPenetrationForce = 150f;
    [SerializeField] private float MinPenetration = 0.01f;
    [SerializeField] private float MaxPenetration = 0.2f;
    [SerializeField] private float BreakDistance = 0.2f;

    [SerializeField] private List<Collider> PunctureColliders;

    [Header("Debug Values")]
    [SerializeField] private GameObject PuncturedObject;
    [SerializeField] private bool HasPunctured = false;
    [SerializeField] private float PunctureValue;

    private float previousPunctureValue;

    Collider col;
    Collider hitCol;
    Collider[] ignoreCols;
    Rigidbody rb;
    GameObject jointHelper;
    Rigidbody jointHelperRB;
    ConfigurableJoint configJoint;
    Grabbable grab;
    FixedJoint fixedJoint;

    private float yPuncture, yPunctureMin, yPunctureMax;

    private void Start()
    {
        col = GetComponent<Collider>();
        rb = col.attachedRigidbody;
        ignoreCols = GetComponentsInChildren<Collider>();
        grab = GetComponent<Grabbable>();
    }

    public float TargetDistance;

    public void FixedUpdate()
    {
        UpdatePunctureValue();
        CheckBreakDistance();
        CheckPunctureRelease();
        AdjustJointMass();
        ApplyResistanceForce();

        if (configJoint)
        {
            TargetDistance = Vector3.Distance(configJoint.targetPosition, configJoint.transform.position);
        }
    }

    public virtual void UpdatePunctureValue()
    {

        if (HasPunctured && PuncturedObject != null && jointHelper != null)
        {
            PunctureValue = transform.InverseTransformVector(jointHelper.transform.position - PuncturedObject.transform.position).y * -1;
            if (PunctureValue > 0 && PunctureValue < 0.0001f)
            {
                PunctureValue = 0;
            }
            if (PunctureValue < 0 && PunctureValue > -0.0001f)
            {
                PunctureValue = 0;
            }

            if (PunctureValue > 0.001f)
            {
                MovePunctureUp();
            }
            else if (PunctureValue < -0.001f)
            {
                MovePunctureDown();
            }
        }
        else
        {
            PunctureValue = 0;
        }
    }

    public virtual void MovePunctureUp()
    {

        configJoint.autoConfigureConnectedAnchor = false;

        float updatedYValue = configJoint.connectedAnchor.y + (Time.deltaTime);

        if (updatedYValue > yPunctureMin)
        {
            updatedYValue = yPunctureMin;
        }
        else if (updatedYValue < yPunctureMax)
        {
            updatedYValue = yPunctureMax;
        }

        configJoint.connectedAnchor = new Vector3(configJoint.connectedAnchor.x, updatedYValue, configJoint.connectedAnchor.z);
    }

    public virtual void MovePunctureDown()
    {
        configJoint.autoConfigureConnectedAnchor = false;

        float updatedYValue = configJoint.connectedAnchor.y - (Time.deltaTime);

        if (updatedYValue > yPunctureMin)
        {
            updatedYValue = yPunctureMin;
        }
        else if (updatedYValue < yPunctureMax)
        {
            updatedYValue = yPunctureMax;
        }

        configJoint.connectedAnchor = new Vector3(configJoint.connectedAnchor.x, updatedYValue, configJoint.connectedAnchor.z);
    }

    public virtual void CheckBreakDistance()
    {
        if (BreakDistance != 0 && HasPunctured && PuncturedObject != null && jointHelper != null)
        {
            if (PunctureValue > BreakDistance)
            {
                ReleasePuncture();
            }
        }
    }

    public virtual void CheckPunctureRelease()
    {
        if (HasPunctured && (PuncturedObject == null || jointHelper == null))
        {
            ReleasePuncture();
        }
    }

    public virtual void AdjustJointMass()
    {
        if (grab != null && configJoint != null)
        {
            if (HasPunctured && grab.BeingHeld)
            {
                configJoint.massScale = 1f;
                configJoint.connectedMassScale = 0.0001f;
            }
            else
            {
                configJoint.massScale = 1f;
                configJoint.connectedMassScale = 1f;
            }
        }
    }

    public virtual void ApplyResistanceForce()
    {
        if (HasPunctured)
        {
            if (grab != null && grab.BeingHeld)
            {
                float punctureDifference = previousPunctureValue - PunctureValue;
                if (punctureDifference != 0 && Mathf.Abs(punctureDifference) > 0.0001f)
                {
                    rb.AddRelativeForce(rb.transform.up * punctureDifference, ForceMode.VelocityChange);
                }
            }

            previousPunctureValue = PunctureValue;
        }
        else
        {
            previousPunctureValue = 0;
        }
    }

    public virtual void DoPuncture(Collider colliderHit, Vector3 connectPosition)
    {

        if (colliderHit == null || colliderHit.attachedRigidbody == null)
        {
            return;
        }

        hitCol = colliderHit;
        PuncturedObject = hitCol.attachedRigidbody.gameObject;

        for (int x = 0; x < ignoreCols.Length; x++)
        {
            Physics.IgnoreCollision(ignoreCols[x], hitCol, true);
        }

        if (jointHelper == null)
        {
            jointHelper = new GameObject("JointHelper");
            jointHelper.transform.parent = null;
            jointHelperRB = jointHelper.AddComponent<Rigidbody>();

            jointHelper.transform.position = PuncturedObject.transform.position;
            jointHelper.transform.rotation = transform.rotation;

            configJoint = jointHelper.AddComponent<ConfigurableJoint>();
            configJoint.connectedBody = rb;
            configJoint.autoConfigureConnectedAnchor = true;

            configJoint.xMotion = ConfigurableJointMotion.Locked;
            configJoint.yMotion = ConfigurableJointMotion.Limited;
            configJoint.zMotion = ConfigurableJointMotion.Locked;

            configJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configJoint.angularZMotion = ConfigurableJointMotion.Locked;

            yPuncture = configJoint.connectedAnchor.y;
            yPunctureMin = yPuncture - MinPenetration;
            yPunctureMax = yPuncture - MaxPenetration;

            SetPenetration(MinPenetration);
        }

        fixedJoint = PuncturedObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = jointHelperRB;

        HasPunctured = true;
    }

    public void SetPenetration(float penetrationAmount)
    {
        float minPenVal = yPuncture - MinPenetration;
        float maxPenVal = yPuncture - MaxPenetration;
        float currentPenVal = yPuncture - penetrationAmount;

        float formattedPenVal = Mathf.Clamp(currentPenVal, maxPenVal, minPenVal);

        if (configJoint != null && configJoint.connectedAnchor != null)
        {

            configJoint.autoConfigureConnectedAnchor = false;
            configJoint.connectedAnchor = new Vector3(configJoint.connectedAnchor.x, formattedPenVal, configJoint.connectedAnchor.z);
        }
    }

    public void ReleasePuncture()
    {
        if (HasPunctured)
        {
            for (int x = 0; x < ignoreCols.Length; x++)
            {
                if (ignoreCols[x] != null && hitCol != null)
                {
                    Physics.IgnoreCollision(ignoreCols[x], hitCol, false);
                }
            }

            if (configJoint)
            {
                configJoint.connectedBody = null;
                GameObject.Destroy(jointHelper);
            }

            if (fixedJoint)
            {
                fixedJoint.connectedBody = null;
            }

            GameObject.Destroy(fixedJoint);
        }

        PuncturedObject = null;
        HasPunctured = false;
    }

    public virtual bool CanPunctureObject(GameObject go)
    {
        Rigidbody rigid = go.GetComponent<Rigidbody>();

        if (rigid != null && rigid.isKinematic)
        {
            return false;
        }

        if (go.isStatic)
        {
            return false;
        }

        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 hitPosition = contact.point;
        Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

        if (collisionForce > FRequiredPenetrationForce && CanPunctureObject(collision.collider.gameObject) && !HasPunctured)
        {
            DoPuncture(collision.collider, hitPosition);
        }
    }
}
