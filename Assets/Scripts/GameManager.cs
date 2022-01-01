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

        /// <summary>
        /// The time when the positions and rotations were recorded, used to do linear prediction
        /// </summary>
        public List<float> time;

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
    public GameObject floor;
    public GameObject transparentFloor;
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
            foreach(KeyValuePair<int, PlayerData> entry in players)
            {
                if (entry.Key != Client.instance.id)
                {
                    
                    //TODO: Change to interpolate between ticks
                    playerTransforms[entry.Key].localPosition = LinearPrediction(entry.Value.positions, entry.Value.time);
                    playerTransforms[entry.Key].localRotation = AngularInterpolation(entry.Value.rotations, entry.Value.time);
                }
            }
        }
    }

    /// <summary>
    /// This method calculates the predicted position for a given player, using its latest known positions and the time when thos positions were recorded
    /// </summary>
    /// <param name="recordedPositions"></param>
    /// <param name="recordedTimes"></param>
    /// <returns></returns>
    private Vector3 LinearPrediction(List<Vector3> recordedPositions, List<float> recordedTimes)
    {
        Vector3 predictedPosition = Vector3.zero;
        float speedX = 0.0f, speedY = 0.0f, speedZ = 0.0f;
        if (recordedTimes[0] > recordedTimes[1])
        { 
        speedX = ((recordedPositions[0].x - recordedPositions[1].x)) / (recordedTimes[0] - recordedTimes[1]);
        speedY = ((recordedPositions[0].y - recordedPositions[1].y)) / (recordedTimes[0] - recordedTimes[1]);
        speedZ = ((recordedPositions[0].z - recordedPositions[1].z)) / (recordedTimes[0] - recordedTimes[1]);
        }
        predictedPosition.x = recordedPositions[0].x + (speedX * (Time.time - recordedTimes[0]));
        predictedPosition.y = recordedPositions[0].y + (speedY * (Time.time - recordedTimes[0]));
        predictedPosition.z = recordedPositions[0].z + (speedZ * (Time.time - recordedTimes[0]));

        return predictedPosition;
    }

    private Quaternion AngularInterpolation(List<Quaternion> recordedRotations, List<float> recordedTimes)
    {
        Quaternion predictedRotation = Quaternion.identity;
        float timeSinceUpdate = Time.time - recordedTimes[0];
        float ticksSinceUpdate = timeSinceUpdate / Time.fixedDeltaTime;
        float LerpAmount = (Time.fixedDeltaTime * ticksSinceUpdate)/(timeSinceUpdate);
        predictedRotation = Quaternion.Lerp(recordedRotations[1], recordedRotations[0], LerpAmount);
        return predictedRotation;
    }

    public void InitPlayer(int id, string username, Vector3 initPos, Quaternion initRot)
    {
        if (players.ContainsKey(id))
            return;
        PlayerData p = new PlayerData();
        p.id = id;
        p.username = username;
        p.positions = new List<Vector3>();
        p.rotations = new List<Quaternion>();
        p.time = new List<float>();
        for (int i = 0; i < 3; i++)
        {
            p.positions.Add(initPos);
            p.rotations.Add(initRot);
            p.time.Add(Time.time);
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
        if (players.ContainsKey(playerID))
        {
            Debug.Log("P{layer " + players[playerID].username + " disconnected");
            Destroy(playerTransforms[playerID].gameObject);
            playerTransforms.Remove(playerID);
            players.Remove(playerID);
        }
        else if (spectators.ContainsKey(playerID))
        {
            Debug.Log("Spectator " + spectators[playerID].username + " disconnected");
            spectators.Remove(playerID);

        }
        else
        {
            Debug.Log("No player found with id " + playerID);
        }
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
        RecordTime(players[playerID].time);
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

    public void RecordTime(List<float> list)
    {
        Debug.Log(Time.time);
        for (int i = 0; i < list.Count - 1; i++)
        {
            list[i + 1] = list[i];
        }
        list[0] = Time.time;
    }


    private void SpawnPlayers()
    {
        Map = GameObject.FindGameObjectWithTag("Map").transform;
        spawner = GameObject.FindGameObjectWithTag("Spawner").transform;
        int i = 0;
        foreach (KeyValuePair<int, PlayerData> entry in players)
        {
            int spawnerChild = (i) < (spawner.childCount - 1) ? i : (i)%(spawner.childCount-1);
            Transform spawnPos = spawner.GetChild(spawnerChild);
            GameObject go;
            if (entry.Value.id == Client.instance.id)
                go = playerPrefab;
            else
                go = remotePlayerPrefab;
            var player = GameObject.Instantiate(go, Map);
            player.transform.position = spawnPos.transform.position;
            player.transform.rotation = spawnPos.transform.rotation;

            playerTransforms.Add(entry.Key, player.transform);
            for (int j = 0; j < 3; j++)
            {
                entry.Value.positions[j] = player.transform.position;
                entry.Value.rotations[j] = player.transform.rotation;
                entry.Value.time[j] = Time.time;
            }
            i++;
        }

        updatePositions = true;
    }

}
