using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LaserManager : MonoBehaviour
{
    [Header("Laser Origin (UI Image)")]
    public RectTransform laserOrigin;  // Đổi từ Transform sang RectTransform

    public LineRenderer laserPrefab;
    public float laserLength = 100f;
    public LayerMask laserLayerMask;
    public Color laserColor = Color.red;

    private List<LineRenderer> activeLasers = new();

    public void FireLaser()
    {
        ClearLasers();

        Vector3 worldOrigin;
        // Chuyển vị trí UI sang world position
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
            laserOrigin,
            laserOrigin.position,
            Camera.main,
            out worldOrigin))
        {
            Debug.LogError("Không thể chuyển LaserOrigin sang World Position.");
            return;
        }

        // Bắn laser từ 4 hướng
        CastLaserRecursive(worldOrigin, Vector2.right, 0);
        CastLaserRecursive(worldOrigin, Vector2.left, 0);
        CastLaserRecursive(worldOrigin, Vector2.up, 0);
        CastLaserRecursive(worldOrigin, Vector2.down, 0);
    }

    void CastLaserRecursive(Vector2 start, Vector2 dir, int bounce)
    {
        if (bounce > 10) return;

        RaycastHit2D hit = Physics2D.Raycast(start, dir, laserLength, laserLayerMask);
        Vector2 end = hit.collider != null ? hit.point : start + dir * laserLength;

        // Vẽ laser
        LineRenderer lr = Instantiate(laserPrefab, transform);
        lr.startColor = lr.endColor = laserColor;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        activeLasers.Add(lr);

        if (hit.collider != null)
        {
            Block_guongController block = hit.collider.GetComponent<Block_guongController>();
            if (block == null) return;

            switch (block.blockType)
            {
                case Block_guongController.BlockType.Obstacle:
                    return;

                case Block_guongController.BlockType.MirrorSlash:
                    Vector2 reflect1 = new Vector2(-dir.y, -dir.x);
                    CastLaserRecursive(end + reflect1 * 0.01f, reflect1, bounce + 1);
                    break;

                case Block_guongController.BlockType.MirrorBackslash:
                    Vector2 reflect2 = new Vector2(dir.y, dir.x);
                    CastLaserRecursive(end + reflect2 * 0.01f, reflect2, bounce + 1);
                    break;
            }
        }
    }

    void ClearLasers()
    {
        foreach (var lr in activeLasers)
            Destroy(lr.gameObject);
        activeLasers.Clear();
    }
    public void FireLaserFromButton()
    {
        FireLaser();
    }

}
