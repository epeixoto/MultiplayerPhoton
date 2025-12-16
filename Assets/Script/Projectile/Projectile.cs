using System.Diagnostics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float lifetime = 5f; // Tempo antes de destruir
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject hitEffect; // Opcional: efeito de impacto

    private void Start()
    {
        // Destroi o projétil após X segundos
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verifica o que atingiu
        UnityEngine.Debug.Log("Projétil atingiu: " + collision.gameObject.name);

        // Se atingir um inimigo (exemplo)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Aqui podes adicionar lógica de dano
            UnityEngine.Debug.Log("Inimigo atingido!");
        }

        // Efeito de impacto (opcional)
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Destroi o projétil ao colidir
        Destroy(gameObject);
    }
}
