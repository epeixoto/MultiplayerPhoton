using Photon.Pun;
using UnityEngine;

/// <summary>
/// Cubo colecionável sincronizado pela rede
/// </summary>
public class CollectibleCube : MonoBehaviourPun
{
    [Header("Configurações")]
    [SerializeField] private int pointsValue = 10;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Efeitos")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        // Rotação contínua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Movimento de "bobbing" (subir e descer)
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é o jogador local
        if (isCollected) return;

        PhotonView playerPhotonView = other.GetComponent<PhotonView>();
        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            // Coleta o cubo via RPC
            photonView.RPC("CollectCube", RpcTarget.AllBuffered, playerPhotonView.Owner.ActorNumber);
        }
    }

    [PunRPC]
    private void CollectCube(int playerActorNumber)
    {
        if (isCollected) return;
        isCollected = true;

        UnityEngine.Debug.Log($"Cubo coletado pelo jogador {playerActorNumber}");

        // Efeitos visuais/sonoros
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null && AudioSource.FindObjectOfType<AudioSource>() != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Notifica o sistema de pontuação (apenas no jogador que coletou)
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber)
        {
            CubeManager.Instance?.OnCubeCollected(pointsValue);
        }

        // Destroi o cubo para todos
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}