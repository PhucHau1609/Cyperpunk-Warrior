using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class login : MonoBehaviour
{
    [Header("Sign_in")]
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;

    [Header("Sign_up")]
    public TMP_InputField registerEmail;
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword;

    [Header("Message")]
    public Text messageText;

    [Header("Hardcoded Account")]
    public string fixedUsername = "DuAnTotNghiep";
    public string fixedPassword = "123456";

    public string nextSceneName = "HauDemo";

    public void Login()
    {
        string savedUsername = PlayerPrefs.GetString("username", "");
        string savedPassword = PlayerPrefs.GetString("password", "");

        string inputUsername = loginUsername.text;
        string inputPassword = loginPassword.text;

        if (loginUsername.text == savedUsername && loginPassword.text == savedPassword || inputUsername == fixedUsername && inputPassword == fixedPassword)
        {
            messageText.text = "Login successful!";
            StartCoroutine(LoadSceneAfterDelay(3f));
        }
        else
        {
            messageText.text = "Incorrect username or password!";
        }
    }

    public void Register()
    {
        string email = registerEmail.text;
        string username = registerUsername.text;
        string password = registerPassword.text;

        if (!email.EndsWith("@gmail.com"))
        {
            messageText.text = "Email must end with @gmail.com!";
            return;
        }

        if (username == "" || password == "")
        {
            messageText.text = "Please fill in all required fields!";
            return;
        }

        if (password.Length < 6)
        {
            messageText.text = "Password must be at least 6 characters!";
            return;
        }

        PlayerPrefs.SetString("email", email);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.Save();

        messageText.text = "Registration successful! You can now log in.";
    }
    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(1);
    }
}
