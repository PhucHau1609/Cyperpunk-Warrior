using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class SceneTransition : MonoBehaviour
{
    public PlayableDirector playableDirector;

    void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnTimelineFinished;
        }
    }

    public void OnTimelineFinished(PlayableDirector director)
    {
        if (director == playableDirector)
        {
           int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene + 1);
        }
    }

    void OnDestroy()
    {
        //Debug.Log("OnDestroy was called", this);

        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineFinished;
        }
    }
}
