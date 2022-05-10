using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class VTProxy : MonoBehaviour
{
    private Material originMaterial;
    private int vtIndex;
    // Start is called before the first frame update
    void Start()
    {
        var render = GetComponent<MeshRenderer>();
        originMaterial = render.sharedMaterial;
        if (VTManager.Instance.vtIndexMap.TryGetValue(originMaterial.mainTexture.GetInstanceID(), out vtIndex))
        {
            render.sharedMaterial = VTManager.Instance.VTMaterial;
            var mpb = new MaterialPropertyBlock();
            mpb.SetInteger("_MainTexIndex", vtIndex);
            render.SetPropertyBlock(mpb);
        }
    }
}
