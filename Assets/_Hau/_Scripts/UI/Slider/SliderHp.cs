using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SliderHp : SliderAbstract
{
    private void FixedUpdate()
    {
        this.UpdatingSliderHp();
    }

    protected virtual void UpdatingSliderHp()
    {
        this.slider.value = this.GetValueHp();
    }

    protected abstract float GetValueHp();
}
