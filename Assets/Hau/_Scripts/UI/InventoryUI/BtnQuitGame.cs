using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnQuitGame : ButtonAbstract
{
    protected override void OnClick()
    {
        QuitGame();
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
        #else
                Application.Quit(); 
        #endif
    }
}
