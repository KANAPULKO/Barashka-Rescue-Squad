using Bhaptics.SDK2;
using UnityEngine;
using UnityEngine.XR; // Добавляем пространство имен для OpenXR

public class HelicopterController : MonoBehaviour
{
    [Header("Винты")]
    public GameObject mainRotor;
    public GameObject tailRotor;

    [Header("Передвижение")]
    public float liftForce = 15f;
    public float moveSpeed = 10f;
    public float strafeSpeed = 8f;
    public float mainRotorSpeed = 1000f;
    public float tailRotorSpeed = 500f;

    [Header("Плавность")]
    public float liftSmoothness = 2f;
    public float moveSmoothness = 2f;

    private Rigidbody rb;
    private float currentLift;
    private float targetLift;
    private Vector3 currentMove;
    private Vector3 targetMove;

    void Start()
    {
        BhapticsLibrary.Play(eventId: "helon");
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody!");
        }
        else
        {
            // Запрещаем вращение по оси Y
            rb.constraints = RigidbodyConstraints.FreezeRotationY;
        }

        currentLift = 0f;
        targetLift = 0f;
        currentMove = Vector3.zero;
        targetMove = Vector3.zero;
    }

    void Update()
    {
        HandleInput();
        RotateRotors();

        // Дополнительная защита от вращения по Y
        LockYRotation();
    }

    void LockYRotation()
    {
        // Принудительно фиксируем вращение по оси Y
        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentEuler.x, 0, currentEuler.z);

        // Также обнуляем угловую скорость по Y
        if (rb != null)
        {
            Vector3 angularVelocity = rb.angularVelocity;
            angularVelocity.y = 0;
            rb.angularVelocity = angularVelocity;
        }
    }

    void HandleInput()
    {
        // Получаем устройства контроллеров
        var leftHandDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

        // Подъем и спуск через правый контроллер
        if (rightHandDevices.Count > 0)
        {
            var rightController = rightHandDevices[0];

            // Проверяем кнопку триггера для подъема
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
            {
                targetLift = liftForce; // Подъем при нажатии триггера
            }
            // Проверяем кнопку A для спуска
            else if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool aButtonPressed) && aButtonPressed)
            {
                targetLift = -liftForce; // Спуск при нажатии кнопки A
            }
            else
            {
                targetLift = 0f;
            }
        }
        else
        {
            targetLift = 0f; // Если контроллер не найден
        }

        // Движение вперед/назад через левый джойстик
        float forwardInput = 0f;
        if (leftHandDevices.Count > 0)
        {
            var leftController = leftHandDevices[0];

            // Получаем значение джойстика
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 leftStick))
            {
                forwardInput = leftStick.y;

                // Движение влево/вправо
                float strafeInput = leftStick.x;

                // Рассчитываем целевое движение
                targetMove = Vector3.zero;
                if (Mathf.Abs(forwardInput) > 0.1f)
                {
                    targetMove += transform.forward * forwardInput * moveSpeed;
                }
                if (Mathf.Abs(strafeInput) > 0.1f)
                {
                    targetMove += transform.right * strafeInput * strafeSpeed;
                }
            }
            else
            {
                targetMove = Vector3.zero;
            }
        }
        else
        {
            targetMove = Vector3.zero; // Если контроллер не найден
        }

        // Плавная интерполяция
        currentLift = Mathf.Lerp(currentLift, targetLift, liftSmoothness * Time.deltaTime);
        currentMove = Vector3.Lerp(currentMove, targetMove, moveSmoothness * Time.deltaTime);
    }

    void RotateRotors()
    {
        // Вращение винтов с учетом скорости движения и подъема
        float movementMultiplier = 1f + (currentMove.magnitude / moveSpeed) * 0.5f;
        float liftMultiplier = 1f + Mathf.Abs(currentLift) / liftForce * 0.3f;

        float currentMainRotorSpeed = mainRotorSpeed * movementMultiplier * liftMultiplier;
        float currentTailRotorSpeed = tailRotorSpeed * movementMultiplier * liftMultiplier;

        if (mainRotor != null)
        {
            mainRotor.transform.Rotate(0f, currentMainRotorSpeed * Time.deltaTime, 0f);
        }

        if (tailRotor != null)
        {
            tailRotor.transform.Rotate(currentTailRotorSpeed * Time.deltaTime, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    void ApplyPhysics()
    {
        if (rb == null) return;

        // Применяем подъемную силу
        Vector3 liftVector = transform.up * currentLift;
        rb.AddForce(liftVector, ForceMode.Force);

        // Применяем движение
        rb.AddForce(currentMove, ForceMode.Force);

        // Стабилизация вертолета
        StabilizeHelicopter();

        // Демпфирование
        ApplyDamping();
    }

    void StabilizeHelicopter()
    {
        // Стабилизация только когда нет активного управления
        if (currentMove.magnitude < 0.1f && Mathf.Abs(currentLift) < 0.1f)
        {
            float stabilizationForce = 2f;

            // Снижаем колебания (только по X и Z, Y уже заблокирована)
            Vector3 angularVelocity = rb.angularVelocity;
            angularVelocity.x *= 0.95f;
            angularVelocity.z *= 0.95f;
            rb.angularVelocity = angularVelocity;

            // Стабилизируем наклоны, но сохраняем Y = 0
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, stabilizationForce * Time.deltaTime);
        }
    }

    void ApplyDamping()
    {
        // Демпфирование при отсутствии управления
        if (currentMove.magnitude < 0.1f && Mathf.Abs(currentLift) < 0.1f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 0.5f * Time.deltaTime);

            // Демпфируем только X и Z вращение, Y уже заблокирована
            Vector3 angularVelocity = rb.angularVelocity;
            angularVelocity.x = Mathf.Lerp(angularVelocity.x, 0, 0.5f * Time.deltaTime);
            angularVelocity.z = Mathf.Lerp(angularVelocity.z, 0, 0.5f * Time.deltaTime);
            rb.angularVelocity = angularVelocity;
        }
    }

    // Публичные методы для доступа к данным
    public float GetCurrentLift()
    {
        return currentLift;
    }

    public Vector3 GetCurrentMove()
    {
        return currentMove;
    }
}