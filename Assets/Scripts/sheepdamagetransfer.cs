using UnityEngine;

public class SheepDamageTransfer : MonoBehaviour
{
    [Header("Настройки передачи урона")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private GameObject targetHelicopter;

    [Header("Настройки автоматического урона")]
    [SerializeField] private bool enableCollisionDamage = false;
    [SerializeField] private float collisionDamageMultiplier = 2f;
    [SerializeField] private float triggerDamage = 10f;

    private HelicopterHealth helicopterHealth;

    private void Start()
    {
        // Автоматически находим вертолет, если не задан вручную
        if (targetHelicopter == null)
        {
            targetHelicopter = GameObject.FindGameObjectWithTag("Player");
        }

        // Получаем компонент здоровья вертолета
        if (targetHelicopter != null)
        {
            helicopterHealth = targetHelicopter.GetComponent<HelicopterHealth>();
        }
    }

    // Основной метод для получения урона
    public void TakeDamage(float damage, GameObject damager = null)
    {
        if (helicopterHealth != null && damage > 0)
        {
            float transferredDamage = damage * damageMultiplier;
            helicopterHealth.TakeDamage(transferredDamage, damager != null ? damager : gameObject);
        }
    }

    // Автоматическая обработка триггеров
    private void OnTriggerEnter(Collider other)
    {
        // <--- НОВОЕ: проверяем тег Finish, если есть - не наносим урон
        if (other.CompareTag("Finish")) return;

        if (!enableCollisionDamage || helicopterHealth == null) return;

        // Простой вариант: передаем фиксированный урон при входе в любой триггер
        TakeDamage(triggerDamage, other.gameObject);
    }

    // Метод для ручной установки цели
    public void SetTargetHelicopter(GameObject helicopter)
    {
        targetHelicopter = helicopter;

        if (targetHelicopter != null)
        {
            helicopterHealth = targetHelicopter.GetComponent<HelicopterHealth>();
        }
    }
}