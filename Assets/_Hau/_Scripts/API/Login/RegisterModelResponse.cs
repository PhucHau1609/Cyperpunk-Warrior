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

[System.Serializable]
public class SaveData
{
    public int userId;
    public float posX;
    public float posY;
    public float posZ;
    public float health;
}


[Serializable]
public class RegisterDTO
{
    public string username;
    public string email;
    public string password;
}

[System.Serializable]
public class SaveDataResponse
{
    public bool isSuccess;
    public float posX;
    public float posY;
    public float posZ;
    public float health;
}

