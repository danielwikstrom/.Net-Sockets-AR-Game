using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject ConnectUI;
    public GameObject LobbyUI;

    public string username;
    public TMP_InputField usernameInput;

    public TMP_Text[] Players;
    public TMP_Text[] Spectators;
    public Button PlayButton;

    private bool updateLobby = true;

    public void OnConnectIsPressed()
    {
        if (usernameInput.text == "")
        {
            Debug.Log("USERNAME IS NEEDED");
            //TODO: this should be seen on screen
            return;
        }
        username = usernameInput.text;
        //TODO: set username value on client and send to server
        Client.instance.SetUsername(username);

        Client.instance.Connect();

        ConnectUI.SetActive(false);
        StartCoroutine("UpdateLobby");
    }

    private IEnumerator UpdateLobby()
    {
        LobbyUI.SetActive(true);
        while (updateLobby)
        {
            if (Client.instance.id == 1)
            {
                PlayButton.gameObject.SetActive(true);
            }
            for (int i = 1; i <= GameManager.instance.players.Count; i++)
            {
                //Players[i].gameObject.SetActive(true);
                Players[i - 1].text = GameManager.instance.players[i].username;
            }
            //Get info from game manager
            yield return new WaitForSeconds(0.5f);
        }
    }
}
