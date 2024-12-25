using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TrainTrackBuilder : MonoBehaviour
{
    [Header("Control Points")]
    public Transform point0;
    public Transform point1;
    public Transform point2;
    public Transform point3;

    [Header("Track Settings")]
    public int segments = 50; // Number of segments to divide the curve into
    public GameObject prefabToPlace; // Prefab to place along the curve
    public float spacing = 1.0f; // Spacing between objects

    private List<Vector3> bezierPoints = new List<Vector3>();

    private void OnDrawGizmos()
    {
        if (point0 == null || point1 == null || point2 == null || point3 == null)
            return;

        // Clear and regenerate the Bézier curve
        bezierPoints.Clear();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 position = CalculateBezierPoint(t, point0.position, point1.position, point2.position, point3.position);
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

        // Clear existing objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            //DestroyImmediate(transform.GetChild(i).gameObject);
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

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1-t)^3 * P0
        point += 3 * uu * t * p1; // 3(1-t)^2 * t * P1
        point += 3 * u * tt * p2; // 3(1-t) * t^2 * P2
        point += ttt * p3;        // t^3 * P3

        return point;
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
