using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject MainUI;

    [HideInInspector]
    public string username;

    public TMP_InputField usernameInput;

    public void OnConnectIsPressed()
    {
        if (usernameInput.text.Equals(""))
        {
            Debug.Log("USERNAME IS NEEDED");
            //TODO: this should be seen on screen
            return;
        }
        username = usernameInput.text;
        //TODO: set username value on client and send to server
        Client.instance.SetUsername(username);

        Client.instance.Connect();
    }
}
