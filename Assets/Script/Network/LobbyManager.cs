using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Gere toda a lógica do Lobby com Photon
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Configurações")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private byte maxPlayersPerRoom = 4;

    [Header("UI")]
    [SerializeField] private LobbyUI lobbyUI;

    // Singleton
    public static LobbyManager Instance { get; private set; }

    // Cache de salas disponíveis
    private List<RoomInfo> availableRooms = new List<RoomInfo>();

    #region Inicialização

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UnityEngine.Debug.Log("=== LOBBY MANAGER START ===");

        //LIGAR AO PHOTON AUTOMATICAMENTE
        /*if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.Log("A ligar ao Photon...");
            lobbyUI.ShowLoading("A ligar ao servidor...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            UnityEngine.Debug.Log("Já está ligado ao Photon");
            lobbyUI.ShowMainMenu();
        }*/

        // Atualiza a UI
        UpdateUI();
    }

    #endregion

    #region Conexão ao Photon

    /// <summary>
    /// Conecta ao servidor Photon
    /// </summary>
    /*public void ConnectToPhoton(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            lobbyUI.ShowError("Por favor, insira um nome!");
            return;
        }

        lobbyUI.ShowLoading("A ligar ao servidor...");

        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();

        UnityEngine.Debug.Log($"A ligar ao Photon como: {playerName}");
    }*/

    /// <summary>
    /// Liga ao Photon com o nome do jogador
    /// </summary>
    public void ConnectToPhoton(string playerName)
    {
        UnityEngine.Debug.Log($"=== CONECTAR AO PHOTON ===");
        UnityEngine.Debug.Log($"Nome do jogador: {playerName}");

        // Define o nome do jogador
        PhotonNetwork.NickName = playerName;

        // Mostra loading
        lobbyUI.ShowLoading("A ligar ao servidor...");

        // Liga ao Photon
        PhotonNetwork.ConnectUsingSettings();
    }


    /*public override void OnConnectedToMaster()
    {
        UnityEngine.Debug.Log("Ligar ao Master Server");
        lobbyUI.ShowMainMenu();
        lobbyUI.HideLoading();
        UpdateUI();
    }*/

    public override void OnConnectedToMaster()
    {
        UnityEngine.Debug.Log("Ligar ao Master Server");

        // Esconde loading e mostra o lobby
        lobbyUI.ShowMainMenu();  // ← Mostra LobbyPanel
        lobbyUI.HideLoading();

        // Entra no lobby para ver salas
        PhotonNetwork.JoinLobby();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.Debug.LogWarning($"Desligado: {cause}");
        lobbyUI.ShowError($"Desligado: {cause}");
        UpdateUI();
    }

    public override void OnJoinedLobby()
    {
        UnityEngine.Debug.Log("Entrou no Lobby");

        // Limpa a cache de salas
        availableRooms.Clear();

        UpdateUI();
    }


    public override void OnLeftLobby()
    {
        UnityEngine.Debug.Log("Saiu do Lobby");
        UpdateUI();
    }

    #endregion

    #region Gestão de Salas

    /// <summary>
    /// Cria uma nova sala
    /// </summary>
    public void CreateRoom(string roomName)
    {
        UnityEngine.Debug.Log($"=== CREATE ROOM CHAMADO ===");
        UnityEngine.Debug.Log($"Nome da sala: '{roomName}'");
        UnityEngine.Debug.Log($"Ligado ao Photon: {PhotonNetwork.IsConnected}");
        UnityEngine.Debug.Log($"No Lobby: {PhotonNetwork.InLobby}");

        if (string.IsNullOrEmpty(roomName))
        {
            UnityEngine.Debug.LogError("Nome da sala está vazio!");
            lobbyUI.ShowError("Nome da sala inválido!");
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.LogError("Não está ligado ao Photon!");
            lobbyUI.ShowError("Não está ligado ao servidor!");
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            UnityEngine.Debug.LogWarning("Não está no Lobby! A entrar...");
            PhotonNetwork.JoinLobby();
            return;
        }

        UnityEngine.Debug.Log($"Tudo OK! Criar a sala: {roomName}");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        lobbyUI.ShowLoading($"A criar sala '{roomName}'...");
    }


    /// <summary>
    /// Entra numa sala específica
    /// </summary>
    public void JoinRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            lobbyUI.ShowError("Por favor, insira o nome da sala!");
            return;
        }

        lobbyUI.ShowLoading("A entrar na sala...");
        PhotonNetwork.JoinRoom(roomName);
        UnityEngine.Debug.Log($"A entrar na sala: {roomName}");
    }

    /// <summary>
    /// Entra numa sala aleatória
    /// </summary>
    public void JoinRandomRoom()
    {
        lobbyUI.ShowLoading("A procurar sala...");
        PhotonNetwork.JoinRandomRoom();
        UnityEngine.Debug.Log("A procurar sala aleatória...");
    }

    /// <summary>
    /// Sai da sala atual
    /// </summary>
    public void LeaveRoom()
    {
        lobbyUI.ShowLoading("A sair da sala...");
        PhotonNetwork.LeaveRoom();
        UnityEngine.Debug.Log("A sair da sala...");
    }

    // Callbacks de Salas
    public override void OnCreatedRoom()
    {
        UnityEngine.Debug.Log($"Sala criada: {PhotonNetwork.CurrentRoom.Name}");
        lobbyUI.HideLoading();
        UpdateUI();
    }

    public override void OnJoinedRoom()
    {
        UnityEngine.Debug.Log($"Entrou na sala: {PhotonNetwork.CurrentRoom.Name}");
        lobbyUI.HideLoading();
        lobbyUI.ShowRoomPanel();
        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.Debug.Log("Saiu da sala");
        lobbyUI.ShowMainMenu();
        UpdateUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UnityEngine.Debug.LogError($"Erro ao criar sala: {message}");
        lobbyUI.ShowError($"Erro ao criar sala: {message}");
        lobbyUI.HideLoading();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UnityEngine.Debug.LogError($"Erro ao entrar na sala: {message}");
        lobbyUI.ShowError($"Erro ao entrar na sala: {message}");
        lobbyUI.HideLoading();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        UnityEngine.Debug.LogWarning($"Nenhuma sala disponível: {message}");
        lobbyUI.ShowError("Nenhuma sala disponível. Crie uma nova!");
        lobbyUI.HideLoading();
    }

    #endregion

    #region Lista de Salas

    /// <summary>
    /// Atualiza a lista de salas (chamado automaticamente pelo Photon)
    /// </summary>
    public void RefreshRoomList()
    {
        UnityEngine.Debug.Log("A atualizar lista de salas...");

        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.LogWarning("Não ligado ao Photon!");
            lobbyUI.ShowError("Não está ligado ao servidor!");
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            UnityEngine.Debug.LogWarning("Não está no Lobby! A entrar...");
            PhotonNetwork.JoinLobby();
        }
        else
        {
            UnityEngine.Debug.Log("Já está no Lobby!");

            // Mostra as salas que já temos em cache
            if (availableRooms.Count > 0)
            {
                UnityEngine.Debug.Log($"Mostrar {availableRooms.Count} salas em cache");
                lobbyUI.UpdateRoomList(availableRooms);
            }
            else
            {
                UnityEngine.Debug.Log("Nenhuma sala em cache. A forçar atualização...");

                // Força o Photon a enviar a lista de salas
                // Sai e volta a entrar no Lobby
                StartCoroutine(ForceRefreshRoomList());
            }
        }
    }

    private System.Collections.IEnumerator ForceRefreshRoomList()
    {
        UnityEngine.Debug.Log("A sair do Lobby...");
        PhotonNetwork.LeaveLobby();

        // Aguarda 0.5 segundos
        yield return new WaitForSeconds(0.5f);

        UnityEngine.Debug.Log("A voltar ao Lobby...");
        PhotonNetwork.JoinLobby();
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UnityEngine.Debug.Log($"OnRoomListUpdate chamado! {roomList.Count} salas recebidas");

        // Atualiza a cache de salas
        foreach (RoomInfo room in roomList)
        {
            UnityEngine.Debug.Log($"   - Sala: {room.Name} | Jogadores: {room.PlayerCount}/{room.MaxPlayers} | Removida: {room.RemovedFromList} | Visível: {room.IsVisible}");

            // Remove salas que foram apagadas
            if (room.RemovedFromList)
            {
                availableRooms.RemoveAll(r => r.Name == room.Name);
            }
            else
            {
                // Atualiza ou adiciona a sala
                int index = availableRooms.FindIndex(r => r.Name == room.Name);
                if (index >= 0)
                {
                    availableRooms[index] = room;
                }
                else
                {
                    availableRooms.Add(room);
                }
            }
        }

        UnityEngine.Debug.Log($"Cache atualizada! Total de salas: {availableRooms.Count}");

        // Atualiza a UI
        lobbyUI.UpdateRoomList(availableRooms);
    }


    #endregion

    #region Jogadores na Sala

    /// <summary>
    /// Atualiza informações da sala atual
    /// </summary>
    private void UpdateRoomInfo()
    {
        if (!PhotonNetwork.InRoom) return;

        lobbyUI.UpdateRoomInfo(
            PhotonNetwork.CurrentRoom.Name,
            PhotonNetwork.CurrentRoom.PlayerCount,
            PhotonNetwork.CurrentRoom.MaxPlayers
        );

        UpdatePlayerList();
    }

    /// <summary>
    /// Atualiza lista de jogadores na sala
    /// </summary>
    private void UpdatePlayerList()
    {
        if (!PhotonNetwork.InRoom) return;

        List<Player> players = new List<Player>(PhotonNetwork.PlayerList);
        lobbyUI.UpdatePlayerList(players);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UnityEngine.Debug.Log($"✓ {newPlayer.NickName} entrou na sala");
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UnityEngine.Debug.Log($"✗ {otherPlayer.NickName} saiu da sala");
        UpdateUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UnityEngine.Debug.Log($"👑 Novo host: {newMasterClient.NickName}");
        UpdateRoomInfo();
    }

    #endregion

    #region Iniciar Jogo

    /// <summary>
    /// Inicia o jogo (apenas o Master Client pode)
    /// </summary>
    public void StartGame()
    {
        UnityEngine.Debug.Log("=== INICIANDO JOGO ===");

        // Verifica se é o Master Client
        if (!PhotonNetwork.IsMasterClient)
        {
            UnityEngine.Debug.LogWarning("Apenas o host pode iniciar o jogo!");
            lobbyUI.ShowError("Apenas o host pode iniciar o jogo!");
            return;
        }

        // Verifica se está ligado
        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.LogError("Não está ligado ao Photon!");
            lobbyUI.ShowError("Erro de conexão!");
            return;
        }

        // Verifica se está numa sala
        if (!PhotonNetwork.InRoom)
        {
            UnityEngine.Debug.LogError("Não está numa sala!");
            lobbyUI.ShowError("Você não está numa sala!");
            return;
        }

        //LOGS DE DEBUG
        UnityEngine.Debug.Log($"Master Client: {PhotonNetwork.IsMasterClient}");
        UnityEngine.Debug.Log($"ligado: {PhotonNetwork.IsConnected}");
        UnityEngine.Debug.Log($"Na sala: {PhotonNetwork.InRoom}");
        UnityEngine.Debug.Log($"Sala: {PhotonNetwork.CurrentRoom.Name}");
        UnityEngine.Debug.Log($"Jogadores: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UnityEngine.Debug.Log($"AutomaticallySyncScene: {PhotonNetwork.AutomaticallySyncScene}");
        UnityEngine.Debug.Log($"A carregar cena: {gameSceneName}");

        lobbyUI.ShowLoading("A carregar o jogo...");

        //CARREGA A CENA PARA TODOS
        PhotonNetwork.LoadLevel(gameSceneName);

        UnityEngine.Debug.Log("PhotonNetwork.LoadLevel() chamado!");
    }

    #endregion

    #region Métodos Privados de UI

    /// <summary>
    /// Atualiza toda a UI do Lobby
    /// </summary>
    private void UpdateUI()
    {
        if (PhotonNetwork.InRoom)
        {
            UpdateRoomInfo();
        }
    }

    #endregion

    #region Métodos Públicos (Getters)

    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public string GetPlayerName()
    {
        return PhotonNetwork.NickName;
    }

    #endregion
}