using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : HauSingleton<CameraFollow>
{
	public float FollowSpeed = 2f;
	public Transform Target;

	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	private Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.1f;
	public float decreaseFactor = 1.0f;

	Vector3 originalPos;

	protected override void Awake()
	{
		//Cursor.visible = false;
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}

		DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    protected override void Start()
    {
        // Nếu vào Map 2 mà Start gọi trước khi Player có thể tìm thấy, ta cũng thử tìm ở đây.
        TryFindPlayer();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindPlayer();
    }

    void TryFindPlayer()
    {
        if (Target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Target = player.transform;
            }
        }
    }

    protected override void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	private void Update()
	{
		Vector3 newPosition = Target.position;
		newPosition.z = -10;
		transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);

		if (shakeDuration > 0)
		{
			camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
	}

	public void ShakeCamera()
	{
		originalPos = camTransform.localPosition;
		shakeDuration = 0.2f;
	}
}
