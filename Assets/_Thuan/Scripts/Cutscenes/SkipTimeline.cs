using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SkipTimeline : MonoBehaviour
{
  public TypeWriterText[] typeWriterTexts;     // Gán 5 Text vào đây
    public PlayableDirector timelineDirector;    // Gán Timeline Director vào
    public double skipTime = 23.0;               // Thời gian cần skip đến (giây)

    private bool hasSkipped = false;

    void Update()
    {
        if (!hasSkipped && Input.GetKeyDown(KeyCode.LeftShift))
        {
            SkipCutscene();
            hasSkipped = true;
        }
    }

    void SkipCutscene()
    {
        foreach (var writer in typeWriterTexts)
        {
            if (writer != null)
            {
                writer.SkipText();
                writer.gameObject.SetActive(false); // Tắt toàn bộ Text + Script + AudioSource
            }
        }

        if (timelineDirector != null)
        {
            timelineDirector.time = skipTime;
            timelineDirector.Evaluate();
        }
    }
}
