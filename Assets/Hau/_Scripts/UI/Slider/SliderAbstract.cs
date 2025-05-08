using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SliderAbstract : HauMonoBehaviour
{
    [SerializeField] protected Slider slider;

    protected override void Start()
    {
        this.slider.onValueChanged.AddListener(OnSliderValueChanged);

    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSlider();
    }

    protected virtual void LoadSlider()
    {
        if (this.slider != null) return;

        this.slider = GetComponent<Slider>();
        Debug.LogWarning(transform.name + ": LoadSlider", gameObject);
    }

    protected virtual void OnSliderValueChanged(float value)
    {
        //
    }
}
