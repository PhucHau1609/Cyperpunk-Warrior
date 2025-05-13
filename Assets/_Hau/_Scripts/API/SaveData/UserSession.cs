using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance;
    public int UserId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(gameObject);
    }
}

