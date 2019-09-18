using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float speed = 3.5f;
    private float X;
    private float Y;
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * speed, -Input.GetAxis("Mouse X") * speed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);

            isDragging = true;
        } else if (isDragging && Input.GetMouseButtonUp(0))
        {
            GameController.DragOver();
            isDragging = false;
        }
    }
}
