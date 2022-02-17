using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*namespace ADBRuntime
{
    using Mono;
    [CustomEditor(typeof(ADBBoneChainImporter))]
    public class ADBSpringBoneEditor : Editor
    {

        ADBRuntimeController controller;

        public void OnEnable()
        {
            controller = target as ADBRuntimeController;

        }
        public override void OnInspectorGUI()
        {

            Titlebar("=====================节点", Color.yellow);
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aDBSetting"), new GUIContent("效果设置(必填)"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("关键词白名单"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("关键词黑名单"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("节点黑名单"), true);

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
}*/
