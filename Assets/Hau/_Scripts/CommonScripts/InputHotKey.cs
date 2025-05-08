using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHotKey : HauSingleton<InputHotKey> //E76 Create
{
    [SerializeField] protected bool isToogleInventoryUI = false;
    public bool IsToogleInventoryUI => isToogleInventoryUI;

    public bool isToogleMusic = false;
    public bool isToogleSetting = false;

    [SerializeField] protected KeyCode keyCode;
    public KeyCode KeyCode => keyCode;

    [SerializeField] protected bool isPlaceTower;
    public bool IsPlaceTower => isPlaceTower;

    private void Update()
    {
        this.OpenInventory();
        //this.OpenMusic();
        //this.OpenSetting();
    }

    protected virtual void OpenInventory()
    {
        this.isToogleInventoryUI = Input.GetKeyUp(KeyCode.I);
    }

    protected virtual void OpenMusic()
    {
        this.isToogleMusic = Input.GetKeyUp(KeyCode.M);
    }

    protected virtual void OpenSetting()
    {
        this.isToogleSetting = Input.GetKeyUp(KeyCode.N);
    }
}
