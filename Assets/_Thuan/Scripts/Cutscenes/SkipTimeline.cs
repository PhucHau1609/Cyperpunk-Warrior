using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SkipTimeline : MonoBehaviour
{
 [Header("Thành phần Timeline")]
    public PlayableDirector playableDirector;

    [Header("Cài đặt Skip")]
    public double skipToTime = 10.0;
    private bool hasSkipped = false;

    void Update()
    {
        if (!hasSkipped && Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (playableDirector != null && playableDirector.time < skipToTime)
            {
                playableDirector.time = skipToTime;
                playableDirector.Evaluate();
                hasSkipped = true;
            }
        }
    }
}
