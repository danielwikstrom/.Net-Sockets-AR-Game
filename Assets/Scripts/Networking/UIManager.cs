using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject MainUI;
    public Image Background;
    public GameObject ConnectUI;
    public GameObject LobbyUI;

    public string username;
    public TMP_InputField usernameInput;

    public TMP_Text[] Players;
    public TMP_Text[] Spectators;
    public Button PlayButton;

    private bool updateLobby = true;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }
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
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].text = "";
            }
            for (int i = 0; i < Spectators.Length; i++)
            {
                Spectators[i].text = "";
            }
            int playerNum = 0;
            int spectatorNum = 0;
            for (int i = 1; i <= GameManager.instance.players.Count + GameManager.instance.spectators.Count; i++)
            {
                //Players[i].gameObject.SetActive(true);
                if (GameManager.instance.players.ContainsKey(i))
                {
                    Players[playerNum].text = GameManager.instance.players[i].username;
                    playerNum++;
                }
                else
                {
                    Spectators[spectatorNum].text = GameManager.instance.spectators[i].username;
                    spectatorNum++;
                }
            }
            //Get info from game manager
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ShowUI(bool show)
    {
        MainUI.SetActive(show);
        if (!Client.instance.isPC)
        {
            Color c = Background.color;
            c.a = 0.5f;
            Background.color = c;
        }
    }


}
