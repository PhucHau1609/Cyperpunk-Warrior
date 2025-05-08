using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class SceneTransition : MonoBehaviour
{
    public PlayableDirector playableDirector;

    void Start()
    {
        // Đảm bảo PlayableDirector đã được gán vào
        if (playableDirector != null)
        {
            // Lắng nghe sự kiện timeline hoàn thành
            playableDirector.stopped += OnTimelineFinished;
        }
    }

    public void OnTimelineFinished(PlayableDirector director)
    {
        // Kiểm tra xem PlayableDirector có đang hoàn thành hay không
        if (director == playableDirector)
        {
            // Chuyển sang scene mới
            SceneManager.LoadScene("MapLevel1");
        }
    }

    // Đảm bảo tắt sự kiện khi không cần thiết
    void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineFinished;
        }
    }
}
