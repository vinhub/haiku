using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public float speed = 3.5f;
    private float X;
    private float Y;
    private bool isDragging = false;

    void Update()
    {
        // click / tap and drag handler
        if (Input.GetMouseButton(0))
        {
            float x, y;
            
            if (Input.touchCount > 0)
            {
                x = Input.touches[0].position.x;
                y = Input.touches[0].position.y;
            }
            else
            {
                x = Input.GetAxis("Mouse X");
                y = Input.GetAxis("Mouse Y");
            }

            transform.Rotate(new Vector3(y * speed, -x * speed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);

            isDragging = true;
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            GameController.DragOver();
            isDragging = false;
        }
    }
}
