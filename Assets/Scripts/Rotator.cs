using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Speed of rotation in degrees per second
    public float rotationSpeed = 30f;

    // Update is called once per frame
    void Update()
    {
        // Rotate only along the Y-axis, clockwise
        transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
    }
}