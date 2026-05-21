using UnityEngine;

public readonly struct DamageContext
{
    public float Amount { get; }
    public GameObject Source { get; }
    public Vector3 HitPoint { get; }
    public Vector3 HitNormal { get; }
    public float BodyPartMultiplier { get; }

    public DamageContext(
        float amount,
        GameObject source,
        Vector3 hitPoint,
        Vector3 hitNormal,
        float bodyPartMultiplier = 1f)
    {
        Amount = amount;
        Source = source;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        BodyPartMultiplier = bodyPartMultiplier;
    }
}
