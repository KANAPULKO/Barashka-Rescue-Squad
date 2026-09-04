using UnityEngine;

public class JoystickVisual : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Объект рукоятки, которую будем вращать")]
    public Transform joystickHandle;

    [Tooltip("Родительский объект вертолета (чьи углы мы будем считывать)")]
    public Transform helicopterTransform;

    [Tooltip("Максимальный угол наклона рукоятки в градусах (чтобы не выглядело сломанным)")]
    public float maxTiltAngle = 30f;

    [Header("Сглаживание (опционально)")]
    [Tooltip("Скорость анимации джойстика для плавности")]
    public float smoothSpeed = 10f;

    private Quaternion targetLocalRotation;

    void Start()
    {
        if (joystickHandle == null)
        {
            Debug.LogError("Рукоятка джойстика не назначена!");
            enabled = false;
            return;
        }

        if (helicopterTransform == null)
        {
            // Если не назначен, попробуем найти вертолет в родителях
            helicopterTransform = GetComponentInParent<HelicopterController>()?.transform;
            // Или просто предполагаем, что вертолет - это корневой объект
            if (helicopterTransform == null)
                helicopterTransform = transform.root;
        }

        // Запоминаем начальное локальное вращение (обычно 0,0,0)
        targetLocalRotation = joystickHandle.localRotation;
    }

    void LateUpdate()
    {
        if (helicopterTransform == null || joystickHandle == null) return;

        // 1. Получаем углы наклона вертолета в пространстве мира
        // Euler Angles дают понятные нам значения: X (вперед-назад), Z (влево-вправо)
        Vector3 heliEulerAngles = helicopterTransform.eulerAngles;

        // 2. Конвертируем в более удобный диапазон (-180..180), чтобы получить отрицательные углы
        float heliPitch = heliEulerAngles.x; // Тангаж (вперед-назад)
        float heliRoll = heliEulerAngles.z;  // Крен (влево-вправо)

        // Нормализуем углы, чтобы получить значения от -180 до 180
        if (heliPitch > 180) heliPitch -= 360;
        if (heliRoll > 180) heliRoll -= 360;

        // 3. Ограничиваем угол, так как вертолет может висеть вверх ногами, а рукоятка так не должна
        //    И инвертируем некоторые оси, если нужно, чтобы джойстик повторял движения вертолета
        float targetPitch = Mathf.Clamp(-heliPitch, -maxTiltAngle, maxTiltAngle); // Минус для инверсии (наклон вперед = ручка вперед)
        float targetRoll = Mathf.Clamp(-heliRoll, -maxTiltAngle, maxTiltAngle); // <<--- ВОТ ЗДЕСЬ МИНУС (левый/правый наклон)

        // Учтите, что в вертолетах часто ось Y - это рыскание (поворот), но на джойстике нам это не нужно.
        // Вращаем рукоятку только по осям X (вперед-назад) и Z (влево-вправо).

        // 4. Создаем целевой поворот
        //    В Unity: вращение по X — это наклон вперед/назад, по Z — это крен (влево/вправо)
        Quaternion targetRot = Quaternion.Euler(targetPitch, 0, targetRoll);
        // Учтите, что в вертолетах часто ось Y - это рыскание (поворот), но на джойстике нам это не нужно.
        // Вращаем рукоятку только по осям X (вперед-назад) и Z (влево-вправо).

       

        // 5. Применяем поворот с опциональным сглаживанием
        if (smoothSpeed > 0)
        {
            joystickHandle.localRotation = Quaternion.Lerp(joystickHandle.localRotation, targetRot, Time.deltaTime * smoothSpeed);
        }
        else
        {
            joystickHandle.localRotation = targetRot;
        }
    }
}