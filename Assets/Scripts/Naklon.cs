using UnityEngine;
using Futurift;
using Futurift.DataSenders;
using Futurift.Options;

public class HelicopterTilt : MonoBehaviour
{
    public HelicopterController helicopterController;
    public float maxTiltAngle = 15f;
    public float tiltSpeed = 2f;

    // Настройки для FutuRift
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 6065;
    [SerializeField] private int dataInterval = 100;

    // Контроллер FutuRift
    private FutuRiftController _futuRiftController;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("HelicopterTilt: Rigidbody not found!");
        }

        // Инициализация FutuRiftController
        var udpOptions = new UdpOptions
        {
            ip = ipAddress,
            port = port
        };

        var futuRiftOptions = new FutuRiftOptions
        {
            interval = dataInterval
        };

        _futuRiftController = new FutuRiftController(
            dataSender: new UdpPortSender(udpOptions),
            futuRiftOptions: futuRiftOptions
        );
    }

    private void OnEnable()
    {
        // Запускаем контроллер при активации
        if (_futuRiftController != null)
        {
            _futuRiftController.Start();
        }
    }

    private void OnDisable()
    {
        // Останавливаем контроллер при деактивации
        if (_futuRiftController != null)
        {
            _futuRiftController.Stop();
        }
    }

    private void Update()
    {
        if (helicopterController == null || rb == null) return;

        // Получаем текущие углы вертолета
        Vector3 currentEuler = transform.eulerAngles;

        // Нормализуем углы
        float currentPitch = NormalizeAngle(currentEuler.x); // Наклон вперед/назад
        float currentRoll = NormalizeAngle(currentEuler.z);  // Реальный крен вертолета!

        // Ограничиваем максимальным углом
        currentPitch = Mathf.Clamp(currentPitch, -maxTiltAngle, maxTiltAngle);
        currentRoll = Mathf.Clamp(currentRoll, -maxTiltAngle, maxTiltAngle);

        // Применяем наклон к вертолету (оставляем как есть)
        Vector3 move = helicopterController.GetCurrentMove();
        float targetTiltX = move.z * maxTiltAngle;
        float targetTiltZ = -move.x * maxTiltAngle;
        targetTiltX = Mathf.Clamp(targetTiltX, -maxTiltAngle, maxTiltAngle);
        targetTiltZ = Mathf.Clamp(targetTiltZ, -maxTiltAngle, maxTiltAngle);
        ApplyTilt(targetTiltX, targetTiltZ);

        // НО в капсулу передаем РЕАЛЬНЫЕ углы наклона вертолета!
        SendTiltToFutuRift(currentPitch, currentRoll);
    }

    private void ApplyTilt(float targetTiltX, float targetTiltZ)
    {
        Vector3 currentEuler = transform.eulerAngles;
        float currentX = NormalizeAngle(currentEuler.x);
        float currentZ = NormalizeAngle(currentEuler.z);

        float newTiltX = Mathf.LerpAngle(currentX, targetTiltX, tiltSpeed * Time.deltaTime);
        float newTiltZ = Mathf.LerpAngle(currentZ, targetTiltZ, tiltSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(newTiltX, currentEuler.y, newTiltZ);
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180)
            angle -= 360;
        return angle;
    }

    private void SendTiltToFutuRift(float pitch, float roll)
    {
        if (_futuRiftController == null) return;

        float normalizedPitch = Mathf.InverseLerp(-maxTiltAngle, maxTiltAngle, pitch);
        float futuRiftPitch = Mathf.Lerp(-15f, 21f, normalizedPitch);

        float normalizedRoll = Mathf.InverseLerp(-maxTiltAngle, maxTiltAngle, roll);
        float futuRiftRoll = Mathf.Lerp(-18f, 18f, normalizedRoll);

        _futuRiftController.Pitch = futuRiftPitch;
        _futuRiftController.Roll = futuRiftRoll;

        //Debug.Log($"Реальный крен вертолета: Pitch={pitch:F1}°, Roll={roll:F1}° => Капсула: Pitch={futuRiftPitch:F1}°, Roll={futuRiftRoll:F1}°");
    }
}