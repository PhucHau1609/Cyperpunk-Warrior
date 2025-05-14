using UnityEngine;

public class TilesScript : MonoBehaviour
{
    public Vector3 targetPosition;
    private Vector3 correctPosition;
    private SpriteRenderer _sprite;

    private const float Tolerance = 0.1f;

    void Awake()
    {
        targetPosition = transform.position;
        correctPosition = transform.position;
        _sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);

        if (Vector3.Distance(targetPosition, correctPosition) < Tolerance)
        {
            _sprite.color = Color.green;
        }
        else
        {
            _sprite.color = Color.white;
        }
    }

    public Vector3 GetCorrectPosition()
    {
        return correctPosition;
    }

    public void SetCorrectPosition(Vector3 newPosition)
    {
        correctPosition = newPosition;
    }
}
