using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

/// <summary>
/// Gere a interface do jogo
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Lista de Jogadores")]
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListItemPrefab;

    private Dictionary<int, GameObject> playerListItems = new Dictionary<int, GameObject>();

    public static GameUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    #region Inicialização

    private void Start()
    {
        UpdatePlayerName();
        UpdatePlayerList();
    }

    #endregion

    #region HUD

    /// <summary>
    /// Atualiza o nome do jogador
    /// </summary>
    public void UpdatePlayerName()
    {
        if (playerNameText != null)
        {
            playerNameText.text = $"Jogador: {PhotonNetwork.NickName}";
        }
    }

    /// <summary>
    /// Atualiza a vida do jogador
    /// </summary>
    public void UpdateHealth(int health)
    {
        if (healthText != null)
        {
            healthText.text = $"Vida: {health}";

            // Muda a cor baseado na vida
            if (health > 70)
                healthText.color = Color.green;
            else if (health > 30)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }

    #endregion

    #region Lista de Jogadores

    /// <summary>
    /// Atualiza a lista de jogadores
    /// </summary>
    public void UpdatePlayerList()
    {
        if (!PhotonNetwork.InRoom) return;

        // Limpa lista antiga
        ClearPlayerList();

        // Adiciona todos os jogadores
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddPlayerToList(player);
        }

        UnityEngine.Debug.Log($"Lista de jogadores atualizada: {playerListItems.Count} jogadores");
    }

    /// <summary>
    /// Adiciona um jogador à lista
    /// </summary>
    public void AddPlayerToList(Player player)
    {
        if (playerListContent == null || playerListItemPrefab == null)
        {
            UnityEngine.Debug.LogWarning("PlayerListContent ou PlayerListItemPrefab não está atribuído!");
            return;
        }

        // Verifica se já existe
        if (playerListItems.ContainsKey(player.ActorNumber))
        {
            UnityEngine.Debug.LogWarning($"Jogador {player.NickName} já está na lista!");
            return;
        }

        // Cria o item
        GameObject item = Instantiate(playerListItemPrefab, playerListContent);
        playerListItems.Add(player.ActorNumber, item);

        // Configura o texto
        TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            string displayName = player.NickName;

            if (player.IsMasterClient)
                displayName += " [HOST]";

            if (player.IsLocal)
                displayName += " (Tu)";

            nameText.text = displayName;
        }

        UnityEngine.Debug.Log($"✓ Jogador {player.NickName} adicionado à lista");
    }

    /// <summary>
    /// Remove um jogador da lista
    /// </summary>
    public void RemovePlayerFromList(Player player)
    {
        if (playerListItems.ContainsKey(player.ActorNumber))
        {
            Destroy(playerListItems[player.ActorNumber]);
            playerListItems.Remove(player.ActorNumber);
            UnityEngine.Debug.Log($"✗ Jogador {player.NickName} removido da lista");
        }
    }

    /// <summary>
    /// Limpa toda a lista
    /// </summary>
    private void ClearPlayerList()
    {
        foreach (var item in playerListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Mostra uma mensagem no ecrã
    /// </summary>
    public void ShowMessage(string message)
    {
        UnityEngine.Debug.Log($"💬 Mensagem: {message}");
        // TODO: Implementar sistema de mensagens no ecrã
    }

    #endregion

    [Header("Pontuação")]
    [SerializeField] private TextMeshProUGUI scoreText;

    /// <summary>
    /// Atualiza a pontuação do jogador
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Pontos: {score}";
        }
    }

}