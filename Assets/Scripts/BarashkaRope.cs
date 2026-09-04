using UnityEngine;

public class PhysicsRopeConstraint : MonoBehaviour
{
    [Header("Rope Settings")]
    public Transform target;
    public float maxDistance = 3f;
    public float pullForce = 80f;
    public float damping = 4f;

    [Header("Physics Behavior")]
    public bool maintainGravity = true;
    public float gravityScale = 1f;
    public bool allowCollisions = true;

    [Header("Smoothing")]
    public float responseSpeed = 8f;
    public float predictionFactor = 0.3f;
    public float smoothTime = 0.1f;

    private Rigidbody rb;
    private Rigidbody targetRb;
    private Vector3 smoothVelocity;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    private bool isOverstretched = false;

    // ��� �������� �������
    private Vector3 calculatedForce;
    private float lastDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }

        // ��������� ������ ��� ������������� ��������
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            // ��������� �������� ��������� �������������
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
        }

        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // ��������� �������� ����
        UpdateTargetVelocity();

        // ��������� ����������� �������
        ApplyRopeConstraint();

        // ��������� ������� ��� ���������� �����
        lastTargetPosition = target.position;
    }

    void UpdateTargetVelocity()
    {
        // ������ �������� ���� � ������������
        Vector3 currentTargetVelocity = (target.position - lastTargetPosition) / Time.fixedDeltaTime;
        targetVelocity = Vector3.Lerp(targetVelocity, currentTargetVelocity, 0.5f);
    }

    void ApplyRopeConstraint()
    {
        Vector3 toTarget = target.position - transform.position;
        float currentDistance = toTarget.magnitude;
        lastDistance = currentDistance;

        // ���������, �� ��������� �� ������������ ���������
        if (currentDistance > maxDistance)
        {
            isOverstretched = true;

            // ������ ���� ���������� � ����������
            Vector3 force = CalculatePullForce(toTarget, currentDistance);

            // ��������� ����
            rb.AddForce(force, ForceMode.Acceleration);

            // ������������ � ���������
            Debug.DrawLine(transform.position, target.position, Color.red);
        }
        else
        {
            isOverstretched = false;

            // ������ ���������� ���� ����� � �������� ���������
            ApplyGentleFollow(toTarget, currentDistance);

            // ������������ � ���������
            Debug.DrawLine(transform.position, target.position, Color.green);
        }

        // ��������� ������ ��� �������
        calculatedForce = CalculatePullForce(toTarget, currentDistance);
    }

    Vector3 CalculatePullForce(Vector3 toTarget, float currentDistance)
    {
        // ������ ���������� ���������
        float overstretchAmount = currentDistance - maxDistance;
        float normalizedOverstretch = overstretchAmount / maxDistance;

        // ������� ����������� ����
        Vector3 forceDirection = toTarget.normalized;

        // ���� �������� ���� ��� ������������
        if (predictionFactor > 0 && targetRb != null)
        {
            Vector3 predictedPosition = target.position + targetVelocity * predictionFactor;
            Vector3 toPredicted = predictedPosition - transform.position;
            forceDirection = Vector3.Slerp(forceDirection, toPredicted.normalized, 0.3f);
        }

        // ������ ���� � ���������� ��������������� (������� ��� ������� ����������)
        float forceMagnitude = pullForce * (1f + normalizedOverstretch * 2f);

        // ��������� ����������� � ����
        Vector3 targetForce = forceDirection * forceMagnitude;
        Vector3 smoothForce = Vector3.SmoothDamp(
            calculatedForce,
            targetForce,
            ref smoothVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );

        // ����������� �������������
        Vector3 dampingForce = -rb.linearVelocity * damping;

        return smoothForce + dampingForce;
    }

    void ApplyGentleFollow(Vector3 toTarget, float currentDistance)
    {
        // ������ ���� ���������� ����� ������� �� ��������
        float followStrength = 0.2f; // ������ ������

        // ������ ���� ���� ��������
        if (targetVelocity.magnitude > 0.5f)
        {
            // ������������� ��������� ����
            Vector3 predictedPosition = target.position + targetVelocity * 0.2f;
            Vector3 toPredicted = predictedPosition - transform.position;

            // ������ �������������� ����
            Vector3 followForce = toPredicted.normalized * followStrength * targetVelocity.magnitude;
            rb.AddForce(followForce, ForceMode.Acceleration);
        }

        // ������ ������������� ���������
        if (rb.linearVelocity.magnitude > 2f)
        {
            rb.AddForce(-rb.linearVelocity * 0.5f, ForceMode.Acceleration);
        }
    }

    // ��������� ����������
    void HandleGravity()
    {
        if (!maintainGravity) return;

        // �������� ���������� �� ����������� ������
        Vector3 gravity = Physics.gravity * gravityScale;

        // ��������� ����������, ���� ��� �� ������������ ��������� �������
        if (isOverstretched)
        {
            // ��������� ���������� ����� ������� �������� (�����������)
            rb.AddForce(gravity * 0.7f, ForceMode.Acceleration);
        }
        else
        {
            // ������ ���������� ����� ������� ��������
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    void Update()
    {
        // ��������� ���������� � Update ��� ���������
        HandleGravity();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!allowCollisions) return;

        // �������������� ������������� ��� ������������� ��� ���������� ��������
        rb.linearVelocity *= 0.8f;
        rb.angularVelocity *= 0.8f;
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // ������������ �������
            float distance = Vector3.Distance(transform.position, target.position);
            Gizmos.color = distance > maxDistance ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, target.position);

            // ����� ���������� ����
            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawWireSphere(target.position, maxDistance);

            // ������������ ���� (������ � Play mode)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, calculatedForce * 0.01f);
            }
        }
    }

    // ������ ��� ������������ ���������
    public void SetMaxDistance(float newDistance)
    {
        maxDistance = newDistance;
    }

    public void SetPullForce(float newForce)
    {
        pullForce = newForce;
    }

    public float GetCurrentTension()
    {
        return Mathf.Max(0, lastDistance - maxDistance);
    }
}