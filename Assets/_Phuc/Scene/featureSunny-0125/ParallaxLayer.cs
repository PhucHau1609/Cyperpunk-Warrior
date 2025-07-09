using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor = 0.5f;

    public void Move(float elevatorDelta)
    {
        transform.position -= new Vector3(0f, elevatorDelta * parallaxFactor, 0f);
    }
}
