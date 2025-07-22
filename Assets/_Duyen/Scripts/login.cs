using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class login : MonoBehaviour
{
    private Dictionary<InputField, string> inputHistory = new Dictionary<InputField, string>();
    private InputField currentInputField;
    private float typingTimer = 0f;

    public float typeSoundCooldown = 0.08f;

    [Header("Sign_in")]
    public InputField loginUsername;
    public InputField loginPassword;

    [Header("Sign_up")]
    public InputField registerEmail;
    public InputField registerUsername;
    public InputField registerPassword;

    [Header("Message")]
    public Text messageText;

    [Header("Hardcoded Account")]
    public string fixedUsername = "DuAnTotNghiep";
    public string fixedPassword = "123456";

    [Header("Scene Settings")]
    public int nextSceneIndex = 1;

    [Header("UI Panels")]
    public GameObject loadingSpinner;



    void Start()
    {
        InputField[] inputs = {
            loginUsername, loginPassword,
            registerEmail, registerUsername, registerPassword
        };

        foreach (var input in inputs)
        {
            //input.onSelect.AddListener((_) => OnInputSelected(input));
            input.onValueChanged.AddListener((_) => OnInputTyping());
            inputHistory[input] = input.text;
        }

    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (selected != null && selected != currentInputField)
            {
                currentInputField = selected;
            }
        }

        if (typingTimer > 0f)
            typingTimer -= Time.deltaTime;
    }

    void OnInputTyping()
    {
        if (currentInputField == null) return;

        string currentText = currentInputField.text;
        string previousText = inputHistory[currentInputField];

        if (currentText != previousText)
        {
            AudioManager.Instance?.PlayTypingSFX();
        }

        // Cập nhật lại text để theo dõi tiếp
        inputHistory[currentInputField] = currentText;
    }

    void OnInputSelected(InputField input)
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

        bool isSaved = inputUsername == savedUsername && inputPassword == savedPassword;
        bool isFixed = inputUsername == fixedUsername && inputPassword == fixedPassword;

        if (isSaved || isFixed)
        {
            messageText.text = "Login successful!";
            
            StartCoroutine(LoadSceneAfterDelay());
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
    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        loadingSpinner.SetActive(true);
        yield return new WaitForSeconds(3f);
        AudioManager.Instance.StopBGM();
        SceneManager.LoadScene(nextSceneIndex);
    }

}
