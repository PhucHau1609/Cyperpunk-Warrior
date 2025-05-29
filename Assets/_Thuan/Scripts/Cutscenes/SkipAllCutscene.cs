using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;
using System.Collections.Generic;

public class SkipAllCutscene : MonoBehaviour
{
    [Header("Danh sách các Timeline")]
    public List<PlayableDirector> timelines;

    private List<SignalReceiver> cachedReceivers = new List<SignalReceiver>();
    private bool hasSkipped = false;

    void Start()
    {
        SignalReceiver[] receivers = FindObjectsByType<SignalReceiver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        cachedReceivers.AddRange(receivers);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space) && !hasSkipped)
        {
            SkipAllCutscenes();
        }
    }

    void SkipAllCutscenes()
    {
        hasSkipped = true;

        foreach (var receiver in cachedReceivers)
        {
            receiver.enabled = false;
        }

        foreach (var timeline in timelines)
        {
            if (timeline != null && timeline.playableAsset != null)
            {
                timeline.time = timeline.duration;
                timeline.Evaluate();
                timeline.Stop();
            }
        }
    }
}
