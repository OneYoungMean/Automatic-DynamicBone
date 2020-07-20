using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime
{
    [CustomEditor(typeof(BoneSubdivision))]
    public class BoneSubdivisionEditor : Editor
    {
        // Start is called before the first frame update
        BoneSubdivision controller;
        public void OnEnable()
        {
            controller = target as BoneSubdivision;
            controller.runtimeController = controller.gameObject.GetComponent<ADBRuntimeController>();
            if (controller.runtimeController == null)
            {
                controller.runtimeController = controller.gameObject.GetComponentInParent<ADBRuntimeController>();
                if (controller.runtimeController == null)
                {
                    Debug.LogError("Cant find the ADBRuntimeContoller!");
                    controller.runtimeController = controller.gameObject.AddComponent<ADBRuntimeController>();
                }
                controller.subdivisionKey = "";

            }
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("test", GUILayout.Height(22.0f)))
            {
                controller.MakeBoneSubdivision();
            }
            if (GUILayout.Button("test2", GUILayout.Height(22.0f)))
            {
                controller.MeshTest();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subdivisionKey"), new GUIContent("SubdivisionKey"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isSubdivisionhorizontal"), new GUIContent("is Subdivision Horizontal"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isSubdivisionvertical"), new GUIContent("is Subdivision Vertical"), true);
            serializedObject.ApplyModifiedProperties();
        }


    }
}