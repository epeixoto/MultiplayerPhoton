using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestão do spawn e coleta de cubos na rede
/// </summary>
public class CubeManager : MonoBehaviourPunCallbacks
{
    public static CubeManager Instance { get; private set; }

    [Header("Configurações de Spawn")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxCubesInScene = 5;
    [SerializeField] private float spawnInterval = 3f;

    [Header("Área de Spawn Aleatório (se não usar pontos fixos)")]
    [SerializeField] private bool useRandomSpawn = false;
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20, 0, 20);

    private List<GameObject> activeCubes = new List<GameObject>();
    private int playerScore = 0;

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
        // Apenas o MasterClient spawna os cubos
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnCubesRoutine());
        }
    }

    #endregion

    #region Spawn de Cubos

    private IEnumerator SpawnCubesRoutine()
    {
        while (true)
        {
            // Remove cubos nulos da lista
            activeCubes.RemoveAll(cube => cube == null);

            // Spawna novos cubos se necessário
            if (activeCubes.Count < maxCubesInScene)
            {
                SpawnCube();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCube()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 spawnPosition = GetSpawnPosition();

        GameObject cube = PhotonNetwork.InstantiateRoomObject(
            cubePrefab.name,
            spawnPosition,
            Quaternion.identity
        );

        activeCubes.Add(cube);
        UnityEngine.Debug.Log($"Cubo spawnado em {spawnPosition}");
    }

    private Vector3 GetSpawnPosition()
    {
        if (useRandomSpawn)
        {
            // Spawn aleatório dentro da área definida
            float randomX = UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = UnityEngine.Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
            return spawnAreaCenter + new Vector3(randomX, 1f, randomZ);
        }
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Spawn em pontos fixos
            Transform randomPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            return randomPoint.position;
        }
        else
        {
            // Fallback: spawn aleatório básico
            return new Vector3(UnityEngine.Random.Range(-10f, 10f), 1f, UnityEngine.Random.Range(-10f, 10f));
        }
    }

    #endregion

    #region Pontuação

    /// <summary>
    /// Chamado quando o jogador local coleta um cubo
    /// </summary>
    public void OnCubeCollected(int points)
    {
        playerScore += points;
        UnityEngine.Debug.Log($"Pontuação: {playerScore}");

        // Atualiza a UI
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateScore(playerScore);
        }
    }

    public int GetPlayerScore()
    {
        return playerScore;
    }

    #endregion

    #region Debug (Editor)

    private void OnDrawGizmosSelected()
    {
        if (useRandomSpawn)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }

    #endregion
}
