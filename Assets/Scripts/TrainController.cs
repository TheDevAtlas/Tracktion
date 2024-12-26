using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [Header("Train Settings")]
    public TrainTrackBuilder trackBuilder; // Reference to the track builder
    public GameObject rollingStockPrefab; // Prefab for rolling stock
    public int rollingStockCount = 0; // Number of rolling stock cars
    public float rollingStockSpacing = 2.0f; // Distance between train and rolling stock
    public float speed = 5f; // Base movement speed
    public float friction = 0.1f; // Friction to gradually reduce velocity

    private float currentT = 0f; // Current position on the curve (0 to 1)
    private List<float> rollingStockT = new List<float>(); // Positions of rolling stock on the curve
    private List<GameObject> rollingStock = new List<GameObject>(); // List of rolling stock objects
    private float velocity = 0f; // Train's current velocity

    private void Start()
    {
        InitializeRollingStock();

        currentT = 0.1889279f;
    }

    private void Update()
    {
        HandleInput();
        UpdateTrainPosition();
        UpdateRollingStockPositions();
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            velocity += speed * Time.deltaTime; // Increase velocity forward
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            velocity -= speed * Time.deltaTime; // Increase velocity backward
        }

        // Apply friction to reduce velocity over time
        velocity = Mathf.Lerp(velocity, 0f, friction * Time.deltaTime);
    }

    private void UpdateTrainPosition()
    {
        if (trackBuilder.controlPoints.Count < 2) return;

        // Update position based on velocity
        currentT += velocity * Time.deltaTime / trackBuilder.segments;

        // Wrap currentT to stay within the curve's bounds
        currentT = Mathf.Repeat(currentT, 1f);

        // Get the current position and rotation on the curve
        Vector3 position = trackBuilder.CalculateBezierPoint(currentT, trackBuilder.controlPoints);
        Vector3 forward = GetCurveForward(currentT);

        // Update train's position and rotation
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(forward);
    }

    private void UpdateRollingStockPositions()
    {
        if (trackBuilder.controlPoints.Count < 2) return;

        // Arc length mapping (optional, you can cache this in your track builder for optimization)
        List<float> arcLengths = trackBuilder.GetArcLengths();

        // Train's arc length
        float trainArcLength = GetArcLengthAtT(currentT, arcLengths);

        for (int i = 0; i < rollingStock.Count; i++)
        {
            // Calculate desired arc length for this rolling stock
            float targetArcLength = trainArcLength - (i + 1) * rollingStockSpacing;
            if (targetArcLength < 0)
                targetArcLength += arcLengths[^1]; // Wrap around to the end of the curve

            // Find corresponding T value for the target arc length
            rollingStockT[i] = GetTAtArcLength(targetArcLength, arcLengths);
            Vector3 position = trackBuilder.CalculateBezierPoint(rollingStockT[i], trackBuilder.controlPoints);
            Vector3 forward = GetCurveForward(rollingStockT[i]);

            // Update rolling stock position and rotation
            rollingStock[i].transform.position = position;
            rollingStock[i].transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    private float GetArcLengthAtT(float t, List<float> arcLengths)
    {
        int index = Mathf.FloorToInt(t * (arcLengths.Count - 1));
        return Mathf.Lerp(arcLengths[index], arcLengths[index + 1], t * (arcLengths.Count - 1) - index);
    }

    private float GetTAtArcLength(float arcLength, List<float> arcLengths)
    {
        for (int i = 0; i < arcLengths.Count - 1; i++)
        {
            if (arcLengths[i] <= arcLength && arcLengths[i + 1] >= arcLength)
            {
                float segmentLength = arcLengths[i + 1] - arcLengths[i];
                return (i + (arcLength - arcLengths[i]) / segmentLength) / (arcLengths.Count - 1);
            }
        }
        return 0f; // Fallback in case of rounding errors
    }

    private Vector3 GetCurveForward(float t)
    {
        float delta = 0.01f;
        Vector3 current = trackBuilder.CalculateBezierPoint(t, trackBuilder.controlPoints);
        Vector3 next = trackBuilder.CalculateBezierPoint(Mathf.Repeat(t + delta, 1f), trackBuilder.controlPoints);
        return (next - current).normalized;
    }

    private void InitializeRollingStock()
    {
        rollingStock.Clear();
        rollingStockT.Clear();

        for (int i = 0; i < rollingStockCount; i++)
        {
            GameObject car = Instantiate(rollingStockPrefab, transform.position, Quaternion.identity, transform.parent);
            rollingStock.Add(car);

            // Initialize positions of rolling stock behind the train
            float initialT = Mathf.Repeat(currentT - (i + 1) * rollingStockSpacing / trackBuilder.segments, 1f);
            rollingStockT.Add(initialT);
        }
    }
}
