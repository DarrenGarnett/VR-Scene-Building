using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 100.0f;
    public float sprintMultiplier = 2.0f;

    private float rotationY = 0.0f;
    private float rotationX = 0.0f;

    void Update()
    {
        // Mouse rotation
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        transform.localEulerAngles = new Vector3(rotationY, rotationX, 0.0f);

        // Movement
        float moveSpeed = speed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1);
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveY = (Input.GetKey(KeyCode.Space) ? moveSpeed : Input.GetKey(KeyCode.LeftControl) ? -moveSpeed : 0) * Time.deltaTime;

        transform.Translate(new Vector3(moveX, moveY, moveZ), Space.Self);
    }
}
