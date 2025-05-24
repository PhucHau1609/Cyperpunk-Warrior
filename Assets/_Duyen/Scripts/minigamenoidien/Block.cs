using UnityEngine;

[System.Serializable]
public class Block : MonoBehaviour
{
    public string blockName;

    public void Rotate()
    {
        transform.Rotate(0, 0, 90);
    }
}
