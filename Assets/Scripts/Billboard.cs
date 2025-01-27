using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Use Late update so everything should have finished moving.
    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;

        Vector3 rotation = transform.rotation.eulerAngles;
        if (rotation.x >= 15f) {
            rotation.x = 15f;
        }

        transform.rotation = Quaternion.Euler(rotation);

    }
}