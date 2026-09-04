using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private bool isTrigger = false; // Если коллайдер - триггер

    

    private void OnTriggerEnter(Collider other)
    {
        if (isTrigger)
            ProcessHit(other.gameObject);
    }

    private void ProcessHit(GameObject hitObject)
    {
        Debug.Log($"{gameObject.name} попал в {hitObject.name}");

        HelicopterHealth health = hitObject.GetComponent<HelicopterHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, gameObject);
            Debug.Log($"Нанесен урон {damage} вертолету {hitObject.name}");
        }

       

        Destroy(gameObject);
    }
}