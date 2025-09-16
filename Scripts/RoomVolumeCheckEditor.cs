using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomVolumeCheck))]
public class RoomVolumeCheckEditor : Editor
{
    private bool showEncapsulationBox = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        showEncapsulationBox = EditorGUILayout.Toggle("Show Encapsulation Box", showEncapsulationBox);

        if (GUILayout.Button("Calculate Encapsulating Box"))
        {
            RoomVolumeCheck script = (RoomVolumeCheck)target;
            CalculateEncapsulatingBox(script);
            EditorUtility.SetDirty(script);
        }

        SceneView.RepaintAll();
    }

    private void CalculateEncapsulatingBox(RoomVolumeCheck script)
    {
        Collider[] colliders = script.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found in " + script.gameObject.name);
            return;
        }

        List<Vector3> points = new List<Vector3>();
        Transform root = script.transform;

        foreach (Collider col in colliders)
        {
            if (col.isTrigger) continue;

            // Handle different collider types
            if (col is BoxCollider box)
            {
                // BoxCollider is defined in local space of the collider's transform
                Vector3 center = box.center;
                Vector3 size = box.size * 0.5f;

                Vector3[] localCorners = new Vector3[]
                {
                    center + new Vector3(-size.x, -size.y, -size.z),
                    center + new Vector3(-size.x, -size.y,  size.z),
                    center + new Vector3(-size.x,  size.y, -size.z),
                    center + new Vector3(-size.x,  size.y,  size.z),
                    center + new Vector3( size.x, -size.y, -size.z),
                    center + new Vector3( size.x, -size.y,  size.z),
                    center + new Vector3( size.x,  size.y, -size.z),
                    center + new Vector3( size.x,  size.y,  size.z)
                };

                foreach (var c in localCorners)
                {
                    // Convert collider-local → world → root-local
                    Vector3 world = box.transform.TransformPoint(c);
                    Vector3 rootLocal = root.InverseTransformPoint(world);
                    points.Add(rootLocal);
                }
            }
            else
            {
                // Fallback for non-BoxColliders: use world-space bounds
                Bounds bounds = col.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;

                Vector3[] corners = new Vector3[]
                {
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z)
                };

                foreach (var c in corners)
                {
                    Vector3 rootLocal = root.InverseTransformPoint(c);
                    points.Add(rootLocal);
                }
            }
        }

        if (points.Count == 0)
        {
            Debug.LogWarning("No valid colliders to calculate bounds.");
            return;
        }

        // Find local AABB relative to root
        Vector3 minLocal = points[0];
        Vector3 maxLocal = points[0];
        foreach (var p in points)
        {
            minLocal = Vector3.Min(minLocal, p);
            maxLocal = Vector3.Max(maxLocal, p);
        }

        Vector3 centerLocal = (minLocal + maxLocal) * 0.5f;
        Vector3 sizeLocal = maxLocal - minLocal;

        script.CheckPosition = centerLocal;
        script.currentSize = sizeLocal;

        Debug.Log($"Encapsulation Box Updated! Local Center: {centerLocal}, Size: {sizeLocal}");
    }

    private void OnSceneGUI()
    {
        if (!showEncapsulationBox) return;

        RoomVolumeCheck script = (RoomVolumeCheck)target;
        if (script == null) return;

        Handles.color = Color.magenta;

        // Save the original Handles matrix
        Matrix4x4 oldMatrix = Handles.matrix;

        // Apply the root's localToWorldMatrix so the box respects rotation and position
        Handles.matrix = script.transform.localToWorldMatrix;

        Handles.DrawWireCube(script.CheckPosition, script.currentSize);

        // Restore the original Handles matrix
        Handles.matrix = oldMatrix;
    }
}
