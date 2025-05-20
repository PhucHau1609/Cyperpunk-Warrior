using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class panel : MonoBehaviour
{
    public GameObject panelSignIn;
    public GameObject panelSignUp;

    // Gọi khi nhấn nút "ungdung"
    public void ShowSignIn()
    {
        AudioManager.Instance.PlayClickSFX();
        panelSignIn.SetActive(true);
        panelSignUp.SetActive(false);
    }

    // Gọi khi nhấn nút "Sign_up"
    public void ShowSignUp()
    {
        AudioManager.Instance.PlayClickSFX();
        panelSignIn.SetActive(false);
        panelSignUp.SetActive(true);
    }
}
