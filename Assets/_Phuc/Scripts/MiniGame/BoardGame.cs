using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardGame : MonoBehaviour
{
    [SerializeField] private Transform emptySpace = null;
    [SerializeField] private TilesScript[] tiles;
    [SerializeField] private GameObject miniGameUI;
    [SerializeField] private TeleportPortal teleportPortal;
    [SerializeField] private GameObject player;

    private Camera _camera;
    private const float CellSize = 6f;
    private const float Tolerance = 0.1f;

    void Start()
    {
        _camera = Camera.main;
        Shuffle();
        teleportPortal.ResetPortal();
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Đã hoàn thành mini game bằng phím P (debug)");
            StartCoroutine(WaitAndCloseMiniGame()); // hoặc gọi CloseMiniGame() trực tiếp nếu không cần delay
        }

        if (IsBoardSolved())
        {
            StartCoroutine(WaitAndCloseMiniGame());
        }
    }

    public bool IsBoardSolved()
    {
        foreach (var tile in tiles)
        {
            if (Vector3.Distance(tile.GetCorrectPosition(), tile.targetPosition) > Tolerance)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator WaitAndCloseMiniGame()
    {
        yield return new WaitForSeconds(1f);
        CloseMiniGame();
    }

    public void CloseMiniGame()
    {
        miniGameUI.SetActive(false);
        if (teleportPortal != null)
        {
            teleportPortal.UnlockPortal();
        }

        if (player != null)
        {
            PlayerMovement movementScript = player.GetComponent<PlayerMovement>();
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }
        }
    }

    public void Shuffle()
    {
        int shuffleCount = 100;
        for (int i = 0; i < shuffleCount; i++)
        {
            List<TilesScript> movableTiles = new List<TilesScript>();
            Vector3 emptyPos = emptySpace.position;

            foreach (var tile in tiles)
            {
                float dx = Mathf.Abs(tile.targetPosition.x - emptyPos.x);
                float dy = Mathf.Abs(tile.targetPosition.y - emptyPos.y);

                bool canMoveVert = dx < Tolerance && dy <= CellSize + Tolerance;
                bool canMoveHoriz = dy < Tolerance && dx <= CellSize + Tolerance;

                if (canMoveVert || canMoveHoriz)
                {
                    movableTiles.Add(tile);
                }
            }

            if (movableTiles.Count > 0)
            {
                TilesScript selected = movableTiles[Random.Range(0, movableTiles.Count)];
                Vector3 lastEmpty = emptySpace.position;
                emptySpace.position = selected.targetPosition;
                selected.targetPosition = lastEmpty;
            }
        }
    }
}
