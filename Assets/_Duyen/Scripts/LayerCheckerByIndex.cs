//using UnityEngine;

//public class LayerCheckerByIndex : MonoBehaviour
//{
//    [Tooltip("Nhập số layer cần kiểm tra (0–31)")]
//    public int targetLayer = 8;

//    void Start()
//    {
//        CheckObjectsInLayer(targetLayer);
//    }

//    void CheckObjectsInLayer(int layer)
//    {
//        if (layer < 0 || layer > 31)
//        {
//            Debug.LogWarning("Số layer không hợp lệ! Layer phải từ 0 đến 31.");
//            return;
//        }

//        GameObject[] allObjects = FindObjectsOfType<GameObject>(true); // Bao gồm cả inactive
//        int count = 0;

//        foreach (GameObject obj in allObjects)
//        {
//            if (obj.layer == layer)
//            {
//                Debug.Log($"Object '{obj.name}' đang ở layer {layer}", obj);
//                count++;
//            }
//        }

//        Debug.Log($"Tổng cộng có {count} object ở layer {layer}.");
//    }
//}
