using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int localPlayerID;
    public struct PlayerData
    {
        public int id;
        public string username;

        /// this list will always have 3 elements, containing the current position [0] and the two previous known positions[1] and [2].
        /// These positions are used to interpolate the players positions between packets, and also to extrapolate the next position in 
        /// case of packet loss
        public List<Vector3> positions;

        /// this list will always have 3 elements, containing the current rotation [0] and the two previous known rotations[1] and [2].
        /// These rotation are used to interpolate the players rotation between packets, and also to extrapolate the next rotation in 
        /// case of packet loss
        public List<Quaternion> rotations;

    }

    public struct SpectatorData
    {
        public int id;
        public string username;
    }

    public static GameManager instance;
    public Dictionary<int, PlayerData> players;
    public Dictionary<int, SpectatorData> spectators;
    public Dictionary<int, Transform> playerTransforms;
    public GameObject playerPrefab;
    public GameObject remotePlayerPrefab;
    private Transform Map;
    private Transform spawner;
    private bool updatePositions = false;


    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
            Destroy(this);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        players = new Dictionary<int, PlayerData>();
        spectators = new Dictionary<int, SpectatorData>();
        playerTransforms = new Dictionary<int, Transform>();
    }

    public void StartGame()
    {
        //TODO: check if PC
        if(Client.instance.isPC)
            SceneManager.LoadScene("PCScene");
        else
        {
            UIManager.instance.ShowUI(false);
            SpawnPlayers();
        }
    }

    private void FixedUpdate()
    {
        if (updatePositions)
        {
            for (int i = 1; i <= players.Count; i++)
            {
                Debug.Log(playerTransforms[i].name + " new position is : " + players[i].positions[0]);
                if (i != Client.instance.id)
                {
                    
                    //TODO: Change to interpolate between ticks
                    playerTransforms[i].localPosition = players[i].positions[0];
                    playerTransforms[i].localRotation = players[i].rotations[0];
                }
            }
        }
    }

    public void InitPlayer(int id, string username, Vector3 initPos, Quaternion initRot)
    {
        if (players.ContainsKey(id))
            return;
        PlayerData p = new PlayerData();
        p.id = id;
        p.username = username;
        p.positions = new List<Vector3>();
        for (int i = 0; i < 3; i++)
        {
            p.positions.Add(initPos);
        }
        p.rotations = new List<Quaternion>();
        for (int i = 0; i < 3; i++)
        {
            p.rotations.Add(initRot);
        }
        players.Add(id, p);
    }

    public void InitSpectator(int id, string username)
    {
        SpectatorData s = new SpectatorData();
        s.id = id;
        s.username = username;
        spectators.Add(id, s);
    }

    public void PlayerDisconnected(int playerID)
    {
        players.Remove(playerID);
    }

    public void SpectatorDisconnected(int playerID)
    {
        spectators.Remove(playerID);
    }

    public void NetworkStartGame()
    {
        ClientSend.StartGame();
    }

    private void OnLevelWasLoaded(int level)
    {
        //if (SceneManager.GetSceneAt(level).name == "PCScene")
        //{
        //    SpawnPlayers();
        //}
        if(Client.instance.isPC)
            SpawnPlayers();
    }

    public void UpdatePlayerTransform(int playerID, Vector3 position, Quaternion rotation)
    {
        AddNewPos(players[playerID].positions, position);
        AddNewRot(players[playerID].rotations, rotation);
    }

    public void AddNewPos(List<Vector3> list, Vector3 newPos)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            list[i + 1] = list[i];
        }
        list[0] = newPos;
    }

    public void AddNewRot(List<Quaternion> list, Quaternion newRot)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            list[i + 1] = list[i];
        }
        list[0] = newRot;
    }


    private void SpawnPlayers()
    {
        Map = GameObject.FindGameObjectWithTag("Map").transform;
        spawner = GameObject.FindGameObjectWithTag("Spawner").transform;
        for (int i = 1; i <= players.Count; i++)
        {
            int spawnerChild = (i-1) < (spawner.childCount) ? i - 1 : (i-1)%spawner.childCount;
            Transform spawnPos = spawner.GetChild(spawnerChild);
            GameObject go;
            if (players[i].id == Client.instance.id)
                go = playerPrefab;
            else
                go = remotePlayerPrefab;
            var player = GameObject.Instantiate(go, Map);
            player.transform.position = spawnPos.transform.position;
            player.transform.rotation = spawnPos.transform.rotation;

            playerTransforms.Add(i, player.transform);
            for (int j = 0; j < 3; j++)
            {
                players[i].positions[j] = player.transform.position;
                players[i].rotations[j] = player.transform.rotation;
            }
        }

        updatePositions = true;
    }
}
