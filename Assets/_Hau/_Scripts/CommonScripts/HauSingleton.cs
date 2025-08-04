using UnityEngine;

public abstract class HauSingleton<T> : HauMonoBehaviour where T : HauMonoBehaviour
{
    private static T _instance;
    public static bool HasInstance => _instance != null;


    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tìm thử trong scene trước khi ném lỗi
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    Debug.LogError($"Singleton instance of type {typeof(T)} has not been created yet!");
                }
            }

            return _instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        this.LoadInstance();
    }

    protected virtual void LoadInstance()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if(transform.parent == null) DontDestroyOnLoad(gameObject);
            return;
        }

        if (_instance != this) Debug.LogError("Another instance of SingletonExample already exists!" + _instance.name);
    }
}
