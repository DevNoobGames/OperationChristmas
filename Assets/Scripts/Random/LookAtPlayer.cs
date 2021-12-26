using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    Camera cam;

    void Update()
    {
        if (cam == null)
            cam = FindObjectOfType<Camera>();

        if (cam == null)
            return;

        Vector3 targetPostition = new Vector3(cam.transform.position.x,
                        this.transform.position.y,
                        cam.transform.position.z);
        this.transform.LookAt(targetPostition);
    }
}
