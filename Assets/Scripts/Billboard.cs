using UnityEngine;

/// <summary>
/// Simple billboard component — always faces the main camera.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
