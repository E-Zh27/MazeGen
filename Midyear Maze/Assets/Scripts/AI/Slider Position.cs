using UnityEngine;

public class SliderPosition : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public Vector3 offset = new Vector3(0, 1, 0);  // Adjust this offset to control slider position above the player

    void Update()
    {
        transform.position = player.position + offset;
        transform.LookAt(Camera.main.transform);  // Ensure the slider always faces the camera
    }
}
