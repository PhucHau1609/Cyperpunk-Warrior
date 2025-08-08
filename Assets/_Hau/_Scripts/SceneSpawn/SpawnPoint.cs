using UnityEngine;

public class SpawnPoint : HauMonoBehaviour
{
    public SpawnSceneName sceneName; // ví dụ: "StartPoint", "FromForest", "FromCave"
}



public enum SpawnSceneName
{
    MapLevel1,
    map1level2,
    map1level3,
    map1level4,

    map2level1,
    map2level2,
    map2level3,
    map2level4,
    MapBoss_01Test,
    Map_Boss02,
    MapBoss_01Test__,
    MapBoss_01Test_01,
}
