using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGame : MonoBehaviour
{
    [SerializeField] private Transform emptySpace = null;
    private Camera _camera;
    private const float CellSize = 6f;
    private const float Tolerance = 0.1f;
    [SerializeField] private TilesScript[] tiles;

    void Start()
    {
        _camera = Camera.main;
        Shuffle();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                Vector3 tilePos = hit.collider.transform.position;
                Vector3 emptyPos = emptySpace.position;

                float dx = Mathf.Abs(tilePos.x - emptyPos.x);
                float dy = Mathf.Abs(tilePos.y - emptyPos.y);

                bool canMoveVert = dx < Tolerance && dy <= CellSize + Tolerance;
                bool canMoveHoriz = dy < Tolerance && dx <= CellSize + Tolerance;

                if (canMoveVert || canMoveHoriz)
                {
                    TilesScript thisTile = hit.collider.GetComponent<TilesScript>();
                    if (thisTile == null) return;

                    Vector3 lastEmpty = emptySpace.position;
                    emptySpace.position = thisTile.targetPosition;
                    thisTile.targetPosition = lastEmpty;
                }
            }
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < 8; i++)
        {
            var lastPos = tiles[i].targetPosition;
            int randomIndex = Random.Range(0, 8);
            tiles[i].targetPosition = tiles[randomIndex].targetPosition;
            tiles[randomIndex].targetPosition = lastPos;
        }
    }
}
