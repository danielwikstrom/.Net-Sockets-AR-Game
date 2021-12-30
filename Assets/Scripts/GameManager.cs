using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
