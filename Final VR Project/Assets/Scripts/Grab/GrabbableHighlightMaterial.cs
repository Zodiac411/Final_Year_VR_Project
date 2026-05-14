using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableHighlightMaterial : GrabbableEvents
{
    public bool HighlightOnGrabbable = true;
    public bool HighlightOnRemoteGrabbable = true;

    [Tooltip("Materials to assign to Renderer when not being highlighted. ")]
    public List<Material> StandardMaterials;

    [Tooltip("Materials to assign to Renderer when being highlighted")]
    public List<Material> HighlightMaterials;

    public Renderer Renderer;

    void Start()
    {
        if (Renderer == null)
        {
            Renderer = GetComponentInChildren<Renderer>();
        }

        if (StandardMaterials == null || StandardMaterials.Count == 0 && Renderer != null)
        {
            StandardMaterials = new List<Material>();
            foreach (var m in Renderer.materials)
            {
                StandardMaterials.Add(m);
            }
        }
    }

    public override void OnGrab(Grabber grabber)
    {
        UnhighlightItem();
    }

    public override void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnGrabbable)
        {
            HighlightItem();
        }
    }

    public override void OnNoLongerClosestGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnGrabbable)
        {
            UnhighlightItem();
        }
    }

    public override void OnBecomesClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnRemoteGrabbable)
        {
            HighlightItem();
        }
    }

    public override void OnNoLongerClosestRemoteGrabbable(ControllerHand touchingHand)
    {
        if (HighlightOnRemoteGrabbable)
        {
            UnhighlightItem();
        }
    }
    public void HighlightItem()
    {
        if (Renderer != null && HighlightMaterials != null)
        {
            Renderer.materials = HighlightMaterials.ToArray();
        }
    }

    public void UnhighlightItem()
    {
        if (Renderer != null && StandardMaterials != null)
        {
            Renderer.materials = StandardMaterials.ToArray();
        }
    }
}
