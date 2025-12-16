using Photon.Pun; 
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    private Camera playerCamera;

    [Header("Configurações")]
    [SerializeField] private float launchForce = 10f; 
    [SerializeField] private float fireRate = 0.5f;

    private float nextFireTime = 0f;
    private PhotonView photonView; 

    private void Start()
    {
        // Se usas Photon, descomenta:
        photonView = GetComponent<PhotonView>();

        // Encontra a câmera automaticamente
        if (photonView != null && photonView.IsMine)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
        else if (photonView == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Se usas Photon, descomenta:
        if (photonView != null && !photonView.IsMine)
            return;

        // Input de disparo
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Câmera não encontrada!");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile Prefab não atribuído!");
            return;
        }

        // Cria o projétil
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        // Ignora colisão com o jogador
        Collider playerCollider = GetComponent<Collider>();
        Collider projectileCollider = projectile.GetComponent<Collider>();

        if (playerCollider != null && projectileCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, projectileCollider);
        }

        // Aplica velocidade
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 shootDirection = playerCamera.transform.forward;
            rb.velocity = shootDirection * launchForce;
        }

        Debug.Log("Projétil lançado!");
    }
}
