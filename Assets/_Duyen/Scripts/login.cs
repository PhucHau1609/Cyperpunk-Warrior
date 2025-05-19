using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class login : MonoBehaviour
{
    private Dictionary<TMP_InputField, string> inputHistory = new Dictionary<TMP_InputField, string>();
    private TMP_InputField currentInputField;

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

    [Header("Scene Settings")]
    public int nextSceneIndex = 1;


    void Start()
    {
        TMP_InputField[] inputs = {
            loginUsername, loginPassword,
            registerEmail, registerUsername, registerPassword
        };

        foreach (var input in inputs)
        {
            TMP_InputField localInput = input;
            input.onSelect.AddListener(delegate { OnInputSelected(input); });
            inputHistory[input] = input.text;
        }
    }

    void Update()
    {
        if (currentInputField != null)
        {
            string currentText = currentInputField.text;
            string lastText = inputHistory[currentInputField];

            if (currentText.Length > lastText.Length)
            {
                AudioManager.Instance?.PlayTypingSFX();
            }

            inputHistory[currentInputField] = currentText;
        }
    }

    void OnInputSelected(TMP_InputField input)
    {
        currentInputField = input;
    }

    void OnDisable()
    {
        currentInputField = null;
    }

    public void Login()
    {
        AudioManager.Instance.PlayClickSFX(); // Âm click
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
        AudioManager.Instance.PlayClickSFX(); //  Âm click
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
        AudioManager.Instance.StopBGM();
    }

}
