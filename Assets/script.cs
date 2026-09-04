using Bhaptics.SDK2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator;

public class ActivatorOnX : MonoBehaviour
{
    [Header("Скрипты (MonoBehaviour) для активации")]
    [Tooltip("Перетащи сюда скрипты, которые нужно включить")]
    public MonoBehaviour[] scriptsToEnable;

    [Header("Звуки (AudioSource) для воспроизведения")]
    [Tooltip("Перетащи сюда объекты со звуками, которые нужно проиграть")]
    public AudioSource[] soundsToPlay;

    [Header("Надписи (GameObject) для уничтожения")]
    [Tooltip("Перетащи сюда первый объект с надписью, который исчезнет")]
    public GameObject textToDestroy;

    [Tooltip("Перетащи сюда второй объект с надписью, который исчезнет")]
    public GameObject secondTextToDestroy;

    [Header("Настройки")]
    [Tooltip("Клавиша для активации")]
    // Было:
    // public KeyCode activationKey = KeyCode.X;

    // Стало (для левой кнопки X):
    public InputDeviceRole deviceRole = InputDeviceRole.LeftHanded; // Или укажите в инспекторе
    public InputFeatureUsage<bool> activationButton = CommonUsages.primaryButton; // primaryButton = X (слева) или A (справа)

    [Tooltip("Уничтожить ли этот скрипт после срабатывания? (чтобы не срабатывал повторно)")]
    public bool destroyThisAfterTrigger = true;

    [Tooltip("Задержка перед уничтожением надписей (в секундах)")]
    public float destroyDelay = 0f;

    [Header("bhaptics настройки")]
    [Tooltip("ID события bhaptics для воспроизведения")]
    public string bhapticsEventId = "testevent1";

    

    private bool isActivated = false;

    void Update()
    {
        if (!isActivated)
        {
            // Ищем устройство (левую руку)
            var inputDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithRole(deviceRole, inputDevices);

            foreach (var device in inputDevices)
            {
                if (device.TryGetFeatureValue(activationButton, out bool buttonValue) && buttonValue)
                {
                    Activate();
                    break;
                }
            }
        }
    }

    void Activate()
    {
        isActivated = true;

        // 1. Активируем скрипты (ставим enabled = true)
        if (scriptsToEnable != null)
        {
            foreach (MonoBehaviour script in scriptsToEnable)
            {
                if (script != null)
                {
                    script.enabled = true;
                    Debug.Log($"Скрипт {script.name} активирован.");
                }
            }
        }

        // 2. Воспроизводим звуки
        if (soundsToPlay != null)
        {
            foreach (AudioSource sound in soundsToPlay)
            {
                if (sound != null)
                {
                    sound.Play();
                    Debug.Log($"Звук {sound.name} воспроизводится.");
                }
            }
        }

        // 3. Воспроизводим bhaptics событие
        try
        {
           
                BhapticsLibrary.Play(eventId: bhapticsEventId);
                Debug.Log($"bhaptics событие {bhapticsEventId} воспроизведено.");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при воспроизведении bhaptics: {e.Message}");
        }

        // 4. Уничтожаем первую надпись
        if (textToDestroy != null)
        {
            if (destroyDelay > 0)
            {
                Destroy(textToDestroy, destroyDelay);
                Debug.Log($"Надпись {textToDestroy.name} будет уничтожена через {destroyDelay} сек.");
            }
            else
            {
                Destroy(textToDestroy);
                Debug.Log($"Надпись {textToDestroy.name} уничтожена.");
            }
        }

        // 5. Уничтожаем вторую надпись
        if (secondTextToDestroy != null)
        {
            if (destroyDelay > 0)
            {
                Destroy(secondTextToDestroy, destroyDelay);
                Debug.Log($"Вторая надпись {secondTextToDestroy.name} будет уничтожена через {destroyDelay} сек.");
            }
            else
            {
                Destroy(secondTextToDestroy);
                Debug.Log($"Вторая надпись {secondTextToDestroy.name} уничтожена.");
            }
        }

        // 6. Опционально: уничтожаем этот скрипт, чтобы он больше не срабатывал
        if (destroyThisAfterTrigger)
        {
            Destroy(this);
            Debug.Log("Скрипт ActivatorOnX удалён.");
        }
    }
}