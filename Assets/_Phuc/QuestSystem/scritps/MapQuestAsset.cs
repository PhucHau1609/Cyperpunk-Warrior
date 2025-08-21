using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Map Quest", fileName = "MapQuest_XXXX")]
public class MapQuestAsset : ScriptableObject
{
    public string mapId; // Ví dụ: "MapCave", "Map1-1", ...
    public Step[] steps;

    [System.Serializable]
    public class Step
    {
        [TextArea] public string text;
        public bool showOnStart = false;      // Hiện ngay khi vào map/đến bước này
        public string eventName;              // Tên sự kiện cần để sang bước này (để trống nếu chỉ là showOnStart)
        public int requiredCount = 1;         // Dùng cho các bước cần số lượng (vd: thu thập 3 mã, diệt 10 địch)
    }
}
