using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace EasyUI.PickerWheelUI
{
    // Enum cho các loại mũi tên
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    // Class quản lý sequence mũi tên
    [System.Serializable]
    public class ArrowSequence
    {
        public ArrowDirection[] arrows;
        public float timeLimit = 5f;

        public ArrowSequence(int length, float timeLimit = 5f)
        {
            this.timeLimit = timeLimit;
            arrows = new ArrowDirection[length];

            // Generate random sequence
            for (int i = 0; i < length; i++)
            {
                arrows[i] = (ArrowDirection)Random.Range(0, 4);
            }
        }
    }
}