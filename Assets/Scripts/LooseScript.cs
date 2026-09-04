using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Bhaptics.SDK2;
public class HelicopterHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Настройки разрушения")]
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private int targetSceneIndex = 0;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private float fallingSpeed = 5f;

    private bool isDestroyed = false;
    private HelicopterController helicopterController;
    private Rigidbody rb;

    private void Start()
    {
        currentHealth = maxHealth;
        helicopterController = GetComponent<HelicopterController>();
        rb = GetComponent<Rigidbody>();

        if (fireEffect != null)
            fireEffect.SetActive(false);

        Debug.Log($"HelicopterHealth инициализирован на {gameObject.name}. Здоровье: {currentHealth}");
    }

    // Вызывайте этот метод из других скриптов при попадании
    public void TakeDamage(float damage, GameObject damager)
    {
        // <--- НОВОЕ: проверяем тег Finish, если есть - не наносим урон
        if (damager != null && damager.CompareTag("Finish")) return;

        if (isDestroyed) return;

        currentHealth -= damage;
        Debug.Log($"Вертолет получил {damage} урона от {damager.name}. Здоровье: {currentHealth}");

        if (currentHealth <= 0)
        {
            StartDestruction();
        }
    }

    // Разрушение от триггера
    public void StartDestructionFromTrigger(GameObject triggerObject)
    {
        // <--- НОВОЕ: проверяем тег Finish, если есть - не вызываем разрушение
        if (triggerObject != null && triggerObject.CompareTag("Finish")) return;

        if (isDestroyed) return;

        Debug.Log($"Вертолет уничтожен триггером {triggerObject.name}");
        StartDestruction();
    }

    private void StartDestruction()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        StartCoroutine(DestructionSequence());
    }

    private IEnumerator DestructionSequence()
    {
        // Отключаем управление
        if (helicopterController != null)
        {
            helicopterController.enabled = false;
        }
        BhapticsLibrary.Play(eventId: "helbreak", duration: destroyDelay);
        // Включаем эффект
        if (fireEffect != null)
        {
            fireEffect.SetActive(true);
        }

        // Плавное разрушение без вращения
        float timer = 0f;
        while (timer < destroyDelay)
        {
            if (rb != null)
            {
                // Плавно уменьшаем горизонтальную скорость
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 targetHorizontalVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 2f * Time.deltaTime);

                // Применяем постоянную силу падения вниз
                float currentFallSpeed = fallingSpeed;
                rb.linearVelocity = new Vector3(
                    targetHorizontalVelocity.x,
                    Mathf.Min(currentVelocity.y, -currentFallSpeed), // Гарантируем падение вниз
                    targetHorizontalVelocity.z
                );
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Загрузка сцены
        if (targetSceneIndex >= 0 && targetSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError($"Неверный индекс сцены: {targetSceneIndex}");
        }
    }
}