using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnCloseSetting : ButtonAbstract
{
    protected virtual void CloseSettingUI()
    {
        UISetting.Instance.Toggle();
    }

    protected override void OnClick()
    {
        this.CloseSettingUI();
    }
}
