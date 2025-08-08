using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHotKey : HauSingleton<InputHotKey> 
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
        this.OpenCrafting();
        //this.Weapon_Toogle();
        //this.Weapon_Swap();
        //this.OpenMusic();
        //this.OpenSetting();
    }

    protected virtual void OpenInventory()
    {
        if (!GameStateManager.Instance.IsGameplay) return; // ❌ Không gửi sự kiện nếu không phải gameplay
        if (pausegame.IsPaused) return; // ✅ Chặn mở inventory nếu đang pause


        if (Input.GetKeyUp(KeyCode.I))
        {
            isToogleInventoryUI = true;
            ObserverManager.Instance.PostEvent(EventID.OpenInventory);
            GameStateManager.Instance.SetState(GameState.Inventory); // 👉 chuyển sang trạng thái inventory
            pausegame.Instance.ToggleBTNPause();
        }
    }


    /*  protected virtual void OpenInventory()
      {
          //if (!GameStateManager.Instance.IsGameplay) return; // 👉 Chặn mở inventory khi không ở trạng thái gameplay

          if (Input.GetKeyUp(KeyCode.I))
          {
              isToogleInventoryUI = true;
              ObserverManager.Instance.PostEvent(EventID.OpenInventory);
              //GameStateManager.Instance.SetState(GameState.Inventory); // 👉 Gán lại trạng thái
          }

      }*/

    protected virtual void OpenCrafting()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            // Chỉ cho phép mở crafting nếu inventory đang mở
            if (NewInventoryUI.Instance != null && NewInventoryUI.Instance.IsShowUI)
            {
                CraftingUI.Instance.Toggle();
                GameStateManager.Instance.SetState(GameState.Crafting);
            }
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
