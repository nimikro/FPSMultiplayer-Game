using UnityEngine;
using Mirror.FizzySteam;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{

    [SerializeField]
    private GameObject sceneCamera = null;

    private string roomName;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        // Error check is steam is open
        if(!SteamManager.Initialized)
        {
            return;
        }

        // Create a lobby on steam's servers, if it is OK, we become a host in Mirror
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        // When a request to join the game is received, add player to lobby
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        // Find host id and join lobby
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

    }


    public void HostLobby()
    {
        // disable UI
        sceneCamera.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);


    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        // if callback is not OK return
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            sceneCamera.SetActive(true);
            return;
        }
        // else start hosting
        networkManager.StartHost();

        // Set data to the lobby we created
        // new CSteamID(callback.m_ulSteamIDLobby) -> created lobby
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());


    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // if we are the host and are running the server we don't want to join using our own lobby using this method
        if(NetworkServer.active)
        {
            return;
        }
        // else if we are a normal client trying to join, get the host address key from steam
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        // set the host address in mirror as a client and join as a client
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
        // disable UI
        sceneCamera.SetActive(false);
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }
}
