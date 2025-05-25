using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Block : MonoBehaviour
{
    public string blockName;

    public void Rotate()
    {
        transform.DORotate(transform.eulerAngles + new Vector3(0, 0, -90), 0.25f, RotateMode.Fast);
    }
}
