using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomVolumeCheck))]
public class RoomVolumeCheckEditor : Editor
{
    private bool showEncapsulationBox = true; // Toggle for visualization

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Keeps default Inspector fields

        RoomVolumeCheck script = (RoomVolumeCheck)target;

        GUILayout.Space(10);
        showEncapsulationBox = EditorGUILayout.Toggle("Show Encapsulation Box", showEncapsulationBox);

        if (GUILayout.Button("Calculate Encapsulating Box"))
        {
            CalculateEncapsulatingBox(script);
            EditorUtility.SetDirty(script); // Marks script as changed
        }

        SceneView.RepaintAll(); // Refresh Scene view to update Gizmos
    }

    private void CalculateEncapsulatingBox(RoomVolumeCheck script)
    {
        Collider[] colliders = script.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found in children of " + script.gameObject.name);
            return;
        }

        List<Vector3> worldPoints = new List<Vector3>();

        foreach (Collider col in colliders)
        {
            Bounds bounds = col.bounds; // World-space bounds
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // Get all 8 corner points
            worldPoints.Add(new Vector3(min.x, min.y, min.z));
            worldPoints.Add(new Vector3(min.x, min.y, max.z));
            worldPoints.Add(new Vector3(min.x, max.y, min.z));
            worldPoints.Add(new Vector3(min.x, max.y, max.z));
            worldPoints.Add(new Vector3(max.x, min.y, min.z));
            worldPoints.Add(new Vector3(max.x, min.y, max.z));
            worldPoints.Add(new Vector3(max.x, max.y, min.z));
            worldPoints.Add(new Vector3(max.x, max.y, max.z));
        }

        // Compute min/max in world space
        Vector3 minPoint = worldPoints[0];
        Vector3 maxPoint = worldPoints[0];

        foreach (Vector3 point in worldPoints)
        {
            minPoint = Vector3.Min(minPoint, point);
            maxPoint = Vector3.Max(maxPoint, point);
        }

        // Compute encapsulating box
        Vector3 center = (minPoint + maxPoint) / 2f;
        Vector3 size = maxPoint - minPoint;

        // Convert world position to local space
        script.CheckPosition = script.transform.InverseTransformPoint(center);
        script.currentSize = size;

        Debug.Log($"Encapsulating Box Updated! Local Position: {script.CheckPosition}, Size: {size}");
    }

    private void OnSceneGUI()
    {
        if (!showEncapsulationBox) return;

        RoomVolumeCheck script = (RoomVolumeCheck)target;

        if (script == null) return;

        Handles.color = Color.magenta;
        Handles.DrawWireCube(script.gameObject.transform.position + script.CheckPosition, script.currentSize);
    }
}
