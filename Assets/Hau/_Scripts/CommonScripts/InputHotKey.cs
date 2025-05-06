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
        this.OpenMusic();
        this.OpenSetting();
        this.ToggleHotKey();
    }

    protected virtual void ToggleHotKey()
    {
        this.isPlaceTower = Input.GetKeyUp(KeyCode.X);


        for (int i = 0; i <= 5; i++)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" +  i);
            if(Input.GetKeyDown(key))
            {
                this.keyCode = this.keyCode == key ? KeyCode.None : key;
                break;
            }
        }
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
