using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager_01 : MonoBehaviour
{
    public Transform elevator; // Thang máy đi lên
    public float elevatorSpeed = 2f;
    public float targetHeight = 20f;

    public ParallaxLayer[] parallaxLayers; // Gồm các layer nền

    private float startY;

    void Start()
    {
        startY = elevator.position.y;
    }

    void Update()
    {
        // Di chuyển thang máy lên
        if (elevator.position.y < startY + targetHeight)
        {
            float moveAmount = elevatorSpeed * Time.deltaTime;
            elevator.Translate(Vector3.up * moveAmount);

            foreach (var layer in parallaxLayers)
            {
                layer.Move(moveAmount);
            }
        }
        else
        {
            // Khi tới đỉnh → Load scene đích
            SceneManager.LoadScene("NextScene"); // Đổi thành tên scene thật
        }
    }
}
