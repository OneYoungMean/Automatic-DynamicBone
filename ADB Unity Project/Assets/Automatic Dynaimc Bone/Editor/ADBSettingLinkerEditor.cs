using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    [CustomEditor(typeof(ADBSettingLinker))]
    public class ADBSettingLinkerEditor : Editor
    {
        ADBSettingLinker controller;

        public void OnEnable()
        {
            controller = target as ADBSettingLinker;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Titlebar("Physics Setting Linker", Color.white);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("Physics Setting"), true);
            GUILayout.Space(12);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSetting"), new GUIContent("Default Physics Setting"), true);
            serializedObject.ApplyModifiedProperties();
        }

        void Titlebar(string text, Color color)
        {
            GUILayout.Space(12);

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(text);
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = backgroundColor;

            GUILayout.Space(3);
        }
    }
}


