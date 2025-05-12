using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegisterModelResponse
{
    public bool isSuccess;
    public string notification;
    public Account data;
}

[Serializable]
public class Account
{
    public string Id;
    public string Email;
    public string Password;
    public string Name;
}

[Serializable]
public class GetAllLevel
{
    public bool isSuccess;
    public string notification;
    public List<GetLevel> data;
}

public class GetLevel
{
    public int levelId;
    public string levelName;
}
