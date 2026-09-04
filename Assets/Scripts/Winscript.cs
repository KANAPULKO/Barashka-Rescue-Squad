using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionOnCollision : MonoBehaviour
{
    [Header("Настройки перехода")]
    [Tooltip("Объект, который должен находиться в коллайдере")]
    public GameObject targetObject;

    [Tooltip("Время в секундах для перехода")]
    public float timeRequired = 5f;

    [Tooltip("Индекс сцены для перехода")]
    public int sceneIndexToLoad;

    [Header("Отладка")]
    [SerializeField] private float currentTimer = 0f;
    [SerializeField] private bool isTargetInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (targetObject != null && other.gameObject == targetObject)
        {
            isTargetInTrigger = true;
            currentTimer = 0f;
            Debug.Log($"Объект {targetObject.name} вошел в триггер");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isTargetInTrigger && targetObject != null && other.gameObject == targetObject)
        {
            currentTimer += Time.deltaTime;

            // Проверяем, достигнуто ли необходимое время
            if (currentTimer >= timeRequired)
            {
                LoadScene();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetObject != null && other.gameObject == targetObject)
        {
            isTargetInTrigger = false;
            currentTimer = 0f;
            Debug.Log($"Объект {targetObject.name} вышел из триггера");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetObject != null && collision.gameObject == targetObject)
        {
            isTargetInTrigger = true;
            currentTimer = 0f;
            Debug.Log($"Объект {targetObject.name} вошел в коллизию");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isTargetInTrigger && targetObject != null && collision.gameObject == targetObject)
        {
            currentTimer += Time.deltaTime;

            // Проверяем, достигнуто ли необходимое время
            if (currentTimer >= timeRequired)
            {
                LoadScene();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (targetObject != null && collision.gameObject == targetObject)
        {
            isTargetInTrigger = false;
            currentTimer = 0f;
            Debug.Log($"Объект {targetObject.name} вышел из коллизии");
        }
    }

    private void LoadScene()
    {
        // Проверяем, существует ли сцена с таким индексом
        if (sceneIndexToLoad >= 0 && sceneIndexToLoad < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Загружаем сцену с индексом: {sceneIndexToLoad}");
            SceneManager.LoadScene(sceneIndexToLoad);
        }
        else
        {
            Debug.LogWarning($"Некорректный индекс сцены: {sceneIndexToLoad}! Доступные индексы: 0-{SceneManager.sceneCountInBuildSettings - 1}");
        }
    }

    private void Update()
    {
        // Опционально: Визуализация таймера в реальном времени
        if (isTargetInTrigger && currentTimer < timeRequired)
        {
            Debug.Log($"Таймер: {currentTimer:F1}/{timeRequired} сек");
        }
    }
}