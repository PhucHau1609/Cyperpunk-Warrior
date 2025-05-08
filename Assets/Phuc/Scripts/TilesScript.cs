using UnityEngine;

public class TilesScript : MonoBehaviour
{
    public Vector3 targetPosition; // Serializable
    private Vector3 correctPosition;
    private SpriteRenderer _sprite;

    void Awake()
    {
        targetPosition = transform.position;
        correctPosition = transform.position;
        _sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
        if (targetPosition == correctPosition)
        {
            _sprite.color = Color.green;
        }
        else
        {
            _sprite.color = Color.white;
        }
    }
}