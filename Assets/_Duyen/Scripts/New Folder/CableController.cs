using System.Collections.Generic;
using UnityEngine;

public class CableController : MonoBehaviour
{
    public Transform plug;
    public Transform startPoint; // Gốc của dây (vị trí bóng đèn)
    public LineRenderer lineRenderer;
    public float moveStep = 0.1f;
    public LayerMask obstacleMask;

    private List<Vector3> cablePoints = new List<Vector3>();

    void Start()
    {
        cablePoints.Clear();
        cablePoints.Add(startPoint.position); // gốc
        cablePoints.Add(plug.position);       // đầu plug
        lineRenderer.positionCount = 1;
        lineRenderer.SetPositions(cablePoints.ToArray());
    }

    void Update()
    {
        HandleInput();
        UpdateCablePath();
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            plug.position += transform.up * moveStep;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            plug.position += -transform.up * moveStep;
        }
    }

    void UpdateCablePath()
    {
        // Raycast từ điểm trước đó đến plug xem có bị chắn không
        Vector3 lastPoint = startPoint.position;
        Vector3 toPlug = plug.position - lastPoint;

        if (Physics2D.Raycast(lastPoint, toPlug.normalized, toPlug.magnitude, obstacleMask))
        {
            // Nếu có vật cản, thêm điểm gấp khúc
            Vector3 hitNormal = Vector3.zero;
            RaycastHit2D hit = Physics2D.Raycast(lastPoint, toPlug.normalized, toPlug.magnitude, obstacleMask);
            if (hit.collider != null)
            {
                hitNormal = hit.normal;

                // Tạo điểm bo quanh vật cản (thêm góc vuông)
                Vector3 bendPoint = hit.point + Vector2.Perpendicular(hitNormal) * 0.2f;
                cablePoints = new List<Vector3> { startPoint.position, bendPoint, plug.position };
            }
        }
        else
        {
            cablePoints = new List<Vector3> { startPoint.position, plug.position };
        }

        lineRenderer.positionCount = cablePoints.Count;
        lineRenderer.SetPositions(cablePoints.ToArray());
    }
}

