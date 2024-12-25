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
    public float gravityForce = 9.81f; // Simulated gravity strength
    public float friction = 0.1f; // Friction to gradually reduce velocity

    private float currentT = 0f; // Current position on the curve (0 to 1)
    private List<float> rollingStockT = new List<float>(); // Positions of rolling stock on the curve
    private List<GameObject> rollingStock = new List<GameObject>(); // List of rolling stock objects
    private float velocity = 0f; // Train's current velocity

    private void Start()
    {
        InitializeRollingStock();
    }

    private void Update()
    {
        HandleInput();
        ApplyGravity();
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

    private void ApplyGravity()
    {
        if (trackBuilder.controlPoints.Count < 2) return;

        // Get the tangent at the current position
        Vector3 forward = GetCurveForward(currentT);
        Vector3 up = Vector3.up;
        float incline = Vector3.Dot(forward, up); // Inclination angle (cosine)

        // Apply gravity component along the incline
        velocity -= gravityForce * incline * Time.deltaTime;
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
        // Update each rolling stock's position along the curve based on spacing
        for (int i = 0; i < rollingStock.Count; i++)
        {
            rollingStockT[i] = Mathf.Repeat(currentT - (i + 1) * rollingStockSpacing / trackBuilder.segments, 1f);
            Vector3 position = trackBuilder.CalculateBezierPoint(rollingStockT[i], trackBuilder.controlPoints);
            Vector3 forward = GetCurveForward(rollingStockT[i]);

            rollingStock[i].transform.position = position;
            rollingStock[i].transform.rotation = Quaternion.LookRotation(forward);
        }
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
