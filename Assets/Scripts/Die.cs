using UnityEngine;

public sealed class Die : MonoBehaviour
{
    private static readonly Vector3[] Normals =
    {
        Vector3.up, 
        Vector3.down, 
        Vector3.right,
        Vector3.left, 
        Vector3.forward, 
        Vector3.back
    };

    [SerializeField] private Rigidbody body;
        
    [SerializeField] private Vector2 impulse = new(12f, 15f);
    [SerializeField] private Vector2 elevation = new(65f, 85f);
    [SerializeField] private Vector2 torque = new(5f, 7f);
    [SerializeField] private float maxAngularVelocity = 50f;
        
    [SerializeField] private int[] faceValues = { 2, 5, 4, 3, 1, 6 };
    [SerializeField] private float minFaceAlignment = 0.95f;
        
    private Vector3 startPosition;
    private Quaternion startRotation;

    public bool IsSleeping => isActiveAndEnabled && body.IsSleeping();

    private void Awake()
    {
        startPosition = body.position;
        startRotation = body.rotation;
        body.maxAngularVelocity = maxAngularVelocity;
    }

    public void Roll()
    {
        ResetPosition();
        body.rotation = Random.rotationUniform;

        var yaw = Random.Range(0f, 2f * Mathf.PI);
        var pitch = RandomInRange(elevation) * Mathf.Deg2Rad;
        var direction = new Vector3(
            Mathf.Cos(yaw) * Mathf.Cos(pitch),
            Mathf.Sin(pitch),
            Mathf.Sin(yaw) * Mathf.Cos(pitch));

        body.AddForce(direction * RandomInRange(impulse), ForceMode.Impulse);
        body.AddTorque(Random.onUnitSphere * RandomInRange(torque), ForceMode.Impulse);
    }
        
    public bool TryReadValue(out int value)
    {
        value = 0;
        if (faceValues.Length != Normals.Length)
            return false;

        var bestAlignment = float.NegativeInfinity;
        for (var i = 0; i < Normals.Length; i++)
        {
            var alignment = Vector3.Dot(body.rotation * Normals[i], Vector3.up);
            if (alignment <= bestAlignment)
                continue;

            bestAlignment = alignment;
            value = faceValues[i];
        }

        return bestAlignment >= minFaceAlignment;
    }

    public void ResetPosition()
    {
        body.position = startPosition;
        body.rotation = startRotation;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.WakeUp();
    }

    private static float RandomInRange(Vector2 range) =>
        Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
}