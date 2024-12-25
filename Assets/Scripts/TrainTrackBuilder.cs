using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TrainTrackBuilder : MonoBehaviour
{
    [Header("Control Points")]
    public List<Transform> controlPoints = new List<Transform>(); // Variable number of control points

    [Header("Track Settings")]
    public int segments = 50; // Number of segments to divide the curve into
    public GameObject prefabToPlace; // Prefab to place along the curve
    public float spacing = 1.0f; // Spacing between objects

    public List<Vector3> bezierPoints = new List<Vector3>();

    private void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Count < 2)
            return;

        // Clear and regenerate the Bézier curve
        bezierPoints.Clear();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 position = CalculateBezierPoint(t, controlPoints);
            bezierPoints.Add(position);

            // Draw the curve in the editor
            if (i > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(bezierPoints[i - 1], position);
            }
        }
    }

    public void PlaceObjectsAlongCurve()
    {
        if (prefabToPlace == null || bezierPoints.Count < 2)
        {
            Debug.LogWarning("Prefab or curve points are missing.");
            return;
        }

        float distanceCovered = 0f;
        for (int i = 1; i < bezierPoints.Count; i++)
        {
            float segmentLength = Vector3.Distance(bezierPoints[i - 1], bezierPoints[i]);

            while (distanceCovered < segmentLength)
            {
                float t = distanceCovered / segmentLength;
                Vector3 position = Vector3.Lerp(bezierPoints[i - 1], bezierPoints[i], t);

                // Place the prefab
                GameObject obj = PrefabUtility.InstantiatePrefab(prefabToPlace) as GameObject;
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.LookRotation(bezierPoints[i] - bezierPoints[i - 1]);
                obj.transform.parent = transform;

                distanceCovered += spacing;
            }

            distanceCovered -= segmentLength;
        }
    }

    public Vector3 CalculateBezierPoint(float t, List<Transform> points)
    {
        if (points.Count == 0) return Vector3.zero;

        List<Vector3> currentPoints = new List<Vector3>();
        foreach (var point in points)
        {
            currentPoints.Add(point.position);
        }

        while (currentPoints.Count > 1)
        {
            List<Vector3> nextPoints = new List<Vector3>();

            for (int i = 0; i < currentPoints.Count - 1; i++)
            {
                Vector3 interpolated = Vector3.Lerp(currentPoints[i], currentPoints[i + 1], t);
                nextPoints.Add(interpolated);
            }

            currentPoints = nextPoints;
        }

        return currentPoints[0];
    }
}

[CustomEditor(typeof(TrainTrackBuilder))]
public class TrainTrackBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrainTrackBuilder builder = (TrainTrackBuilder)target;

        if (GUILayout.Button("Place Objects Along Curve"))
        {
            builder.PlaceObjectsAlongCurve();
        }
    }
}
