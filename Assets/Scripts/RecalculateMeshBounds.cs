using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RecalculateMeshBounds : MonoBehaviour
{
    private void OnEnable()
    {
        foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        }
        Resources.UnloadUnusedAssets();
    }
}
