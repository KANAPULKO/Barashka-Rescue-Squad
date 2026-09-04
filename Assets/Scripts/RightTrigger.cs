using Bhaptics.SDK2;
using UnityEngine;

public class SimplerightBhapticsTrigger : MonoBehaviour
{
    public string righteventId = "helrightbreak";

    private void OnCollisionEnter(Collision collision)
    {
        // Простая версия без проверок
        BhapticsLibrary.Play(eventId: righteventId);
        Debug.Log($"Неверный индекс сцены: сработал правый");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Для триггерных коллайдеров
        BhapticsLibrary.Play(eventId: righteventId);
        Debug.Log($"Неверный индекс сцены: сработал правый");
    }
}