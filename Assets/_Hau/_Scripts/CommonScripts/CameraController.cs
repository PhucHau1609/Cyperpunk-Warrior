using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _player;
    [SerializeField] Vector2 _cameraOffset;// điều chỉnh x,y camera
    void Update()
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        if (_player == null) return; 

        Vector3 positionCam = _player.position + (Vector3)_cameraOffset;
        positionCam.z = Camera.main.transform.position.z;
        Camera.main.transform.position = positionCam;
    }
}
