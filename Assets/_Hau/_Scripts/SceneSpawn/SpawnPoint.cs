using UnityEngine;

public class SpawnPoint : HauMonoBehaviour
{
    public SpawnSceneName sceneName; // ví dụ: "StartPoint", "FromForest", "FromCave"
}



public enum SpawnSceneName
{
    MapLevel1,
    MapLevel2,
    MapLevel3,
    MapLevel4,

}
