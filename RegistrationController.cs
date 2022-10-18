using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace log_in_system
{
    public class RegistrationController : MonoBehaviour
    {
        [Header("Ввод данных для входа")]
        [SerializeField] private InputField Input_Email;
        [SerializeField] private InputField Input_logIn;
        [SerializeField] private InputField Input_Password;
        [SerializeField] private InputField Input_SecondPassword;

        [Header("Кнопки")]
        [SerializeField] private Button b_Back;
        [SerializeField] private Button b_Registration;

        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text t_errorText;
        [SerializeField]

        [HideInInspector]
        public AuthWindowsController mainController;

        private void Awake()
        {
            t_errorText.text = "";
            errorPanel.SetActive(false);
            b_Back.onClick.AddListener(() => GoToLogInWindows());
            b_Registration.onClick.AddListener(() => StartCoroutine(Registration()));
        }

        private IEnumerator Registration()
        {
            b_Registration.interactable = false;

            if (Input_logIn.text.Length <= 0 || Input_Password.text.Length <= 0 || Input_Email.text.Length <= 0)
            {
                errorPanel.SetActive(true);
                t_errorText.text = "Какие-то поля незаполнены";
                b_Registration.interactable = true;
                yield break;
            }

            if (Input_Password.text != Input_SecondPassword.text)
            {
                errorPanel.SetActive(false);
                errorPanel.SetActive(true);
                t_errorText.text = "Введенные пароли не совпадают";
                b_Registration.interactable = true;
                yield break;
            }

            WWWForm form = new WWWForm();
            form.AddField("name", Input_logIn.text);
            form.AddField("password", Input_Password.text);
            form.AddField("email", Input_Email.text);

            // отправляем запрос с данными на сервер и ждем подтверждения
            // если данные верны сохраняем логин в префсы

            using (UnityWebRequest www = UnityWebRequest.Post(WebData.RegisterPath, form))
            {
                www.SetRequestHeader(WebData.HeaderName, WebData.HeaderValue);
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    if (ConsoleScript.Instance) ConsoleScript.Instance.AddConsoleText(www.error, "RegistrationController");
                    b_Registration.interactable = true;
                    errorPanel.SetActive(true);
                    t_errorText.text = "Ошибка подключения \n" + www.error;
                    www.Dispose();
                    yield break;
                }

                if (www.downloadHandler.text.Length < 1)
                {
                    if (ConsoleScript.Instance) ConsoleScript.Instance.AddConsoleText("www.downloadHandler.text.Length < 1", "RegisterController");
                    b_Registration.interactable = true;
                    errorPanel.SetActive(true);
                    t_errorText.text = "Ошибка подключения \n" + "Сервер не отвечает";
                    www.Dispose();
                    yield break;
                }

                EnterGameData data = JsonUtility.FromJson<EnterGameData>(www.downloadHandler.text);

                if (!data.success)
                {
                    if (ConsoleScript.Instance) ConsoleScript.Instance.AddConsoleText(data.message + " |error code: " + data.code, "RegisterController ");
                    errorPanel.SetActive(true);
                    b_Registration.interactable = true;
                    t_errorText.text = "Ошибка подключения \n" + data.message + " |error code: " + data.code;
                    www.Dispose();
                    yield break;
                }

                WWWForm logIn_form = new WWWForm();
                logIn_form.AddField("login", Input_Email.text);
                logIn_form.AddField("password", Input_Password.text);

                using (UnityWebRequest www_logIn = UnityWebRequest.Post(WebData.LoginPath, logIn_form))
                {
                    www_logIn.SetRequestHeader(WebData.HeaderName, WebData.HeaderValue);
                    yield return www_logIn.SendWebRequest();

                    if (www_logIn.downloadHandler.text.Length < 1)
                    {
                        if (ConsoleScript.Instance) ConsoleScript.Instance.AddConsoleText("www.downloadHandler.text.Length < 1", "LogInController ");
                        errorPanel.SetActive(true);
                        t_errorText.text = "Ошибка подключения \n" + "Сервер не отвечает";
                        www_logIn.Dispose();
                        yield break;
                    }

                    data = JsonUtility.FromJson<EnterGameData>(www_logIn.downloadHandler.text);

                    if (!data.success)
                    {
                        if (ConsoleScript.Instance) ConsoleScript.Instance.AddConsoleText(data.message + " |error code: " + data.code, "RegisterController ");
                        errorPanel.SetActive(true);
                        t_errorText.text = "Ошибка подключения \n" + data.message + " |error code: " + data.code;
                        www_logIn.Dispose();
                        yield break;
                    }

                    mainController.SetNewResponse(data);
                    yield return new WaitForFixedUpdate();

                    b_Registration.interactable = true;

                    Input_Email.text = "";
                    Input_logIn.text = "";
                    Input_Password.text = "";
                    Input_SecondPassword.text = "";
                    mainController.OpenWindow(SupportClass.windows.selectServer);

                    www_logIn.Dispose();
                }
                www.Dispose();
            }
        }

        private void GoToLogInWindows()
        {
            Input_Email.text = "";
            Input_logIn.text = "";
            Input_Password.text = "";
            Input_SecondPassword.text = "";

            mainController.OpenWindow(SupportClass.windows.logIn);
        }
    }
}