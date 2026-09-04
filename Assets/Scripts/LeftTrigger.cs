using Bhaptics.SDK2;
using UnityEngine;

public class SimpleBhapticsTrigger : MonoBehaviour
{
    public string eventId = "helleftbreak";

    private void OnCollisionEnter(Collision collision)
    {
        // Простая версия без проверок
        BhapticsLibrary.Play(eventId: eventId);
        Debug.Log($"Неверный индекс сцены: сработал левый");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Для триггерных коллайдеров
        BhapticsLibrary.Play(eventId: eventId);
        Debug.Log($"Неверный индекс сцены: сработал левый");
    }
}