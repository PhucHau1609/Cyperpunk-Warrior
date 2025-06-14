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

    private void Update()
    {
        this.OpenInventory();
        this.Weapon_Toogle();
        this.Weapon_Swap();
        //this.OpenMusic();
        //this.OpenSetting();
    }

    protected virtual void OpenInventory()
    {
        //this.isToogleInventoryUI = Input.GetKeyUp(KeyCode.I);

        if (Input.GetKeyUp(KeyCode.I))
        {
            isToogleInventoryUI = true;
            ObserverManager.Instance.PostEvent(EventID.OpenInventory);
        }

    }

    protected virtual void OpenMusic()
    {
        this.isToogleMusic = Input.GetKeyUp(KeyCode.M);
    }

    protected virtual void OpenSetting()
    {
        this.isToogleSetting = Input.GetKeyUp(KeyCode.N);
    }

    protected virtual void Weapon_Toogle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ObserverManager.Instance.PostEvent(EventID.Weapon_Toggle);
        }
    }

    protected virtual void Weapon_Swap()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ObserverManager.Instance.PostEvent(EventID.Weapon_Swap);
        }
    }
}
