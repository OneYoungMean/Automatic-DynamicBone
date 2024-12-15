using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    using Mono.Tool;


    [CustomEditor(typeof(ADBColliderGenerateTool))]
    public class ADBColliderGeneratorEditor : Editor
    {
        public const string MANUAL = "Add to Animator and Transform, then click GenerateCollider";
        private bool isDeleteCollider;
        private bool isGenerateColliderOpenTrigger;
        private ADBColliderGenerateTool controller;
        public void OnEnable()
        {
            controller = target as ADBColliderGenerateTool;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Titlebar("Human Avatar Collider Generate Tool", new Color(0.5F, 1, 1));
            ShowManualOnFirstScript();
            if (controller.gameObject.GetComponent<Animator>() == null)
            {
                Titlebar("Error: No Animator Component or animator is not human!", new Color(0.7f, 0.3f, 0.3f));
            }
            if (controller.gameObject.GetComponentsInChildren<ADBChainProcessor>().Length==0)
            {
                Titlebar("Warning:Tool did not find the physics bone, will generate default range .", Color.yellow);
            }
            if (controller.gameObject.GetComponentsInChildren<ADBColliderReader>().Length != 0 && (controller.generateColliderList == null || controller.generateColliderList.Count == 0))
            {
                Titlebar("Tips:There some collider has been generated ,use refresh button to show it", Color.grey);
            }
            if (controller.generateColliderList != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateColliderList"), new GUIContent("Collider list :" + controller.generateColliderList.Count), true);
            }



            string key = controller.isGenerateColliderAutomaitc ? "Generate" : "Refresh";

            if (GUILayout.Button(key + "Colllider", GUILayout.Height(22.0f)))
            {
                controller.initializeCollider();
            }
            if (controller.generateColliderList == null || controller.generateColliderList.Count == 0)
            {
                controller.isGenerateColliderAutomaitc = EditorGUILayout.Toggle("Generate Human Collider ", controller.isGenerateColliderAutomaitc);

                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.colliderSize = EditorGUILayout.Slider("  ©»©¥Global Scale ", controller.colliderSize, 0.001f, 2f);
                    controller.isGenerateColliderOpenTrigger = EditorGUILayout.Toggle("  ©»©¥Collider isTrigger ", controller.isGenerateColliderOpenTrigger);
                    controller.isGenerateByAllPoint = EditorGUILayout.Toggle("  ©»©¥Use Fixed Transform to fitting collider size ", controller.isGenerateByAllPoint);
                    controller.isGenerateFinger = EditorGUILayout.Toggle("  ©»©¥Generate Finger ", controller.isGenerateFinger);
                    controller.isColliderAllMask = EditorGUILayout.Toggle("  ©»©¥Collider Mask is All ", controller.isColliderAllMask);
                }
            }
            else
            {
                /*                controller.colliderSize = EditorGUILayout.Slider("Åö×²Ìå´óÐ¡", controller. colliderSize, 0, 2);*/
            }
            if (GUILayout.Button("Delete all collider", GUILayout.Height(22.0f)))
            {

                if (isDeleteCollider)
                {
                    var overlapsColliderList = controller.transform.GetComponentsInChildren<Collider>();
                    for (int i = 0; i < overlapsColliderList.Length; i++)
                    {
                        if (overlapsColliderList[i] != null)
                        {
                            if (overlapsColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                            {
                                try
                                {
                                    Undo.DestroyObjectImmediate(overlapsColliderList[i].gameObject);
                                }
                                catch (Exception)
                                {
                                    if (overlapsColliderList[i].gameObject.TryGetComponent<ADBColliderReader>(out ADBColliderReader reader))
                                    {
                                        Undo.DestroyObjectImmediate(reader);
                                        Undo.DestroyObjectImmediate(overlapsColliderList[i]);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                if (overlapsColliderList[i].gameObject.TryGetComponent<ADBColliderReader>(out ADBColliderReader reader))
                                {
                                    Undo.DestroyObjectImmediate(reader);
                                    Undo.DestroyObjectImmediate(overlapsColliderList[i]);
                                }
                            }
                        }
                    }
                    controller.generateColliderList.Clear();
                }
                else
                {
                    for (int i = 0; i < controller.generateColliderList?.Count; i++)
                    {
                        if (controller.generateColliderList[i] != null)
                        {
                            if (controller.generateColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                            {

                                try
                                {
                                    Undo.DestroyObjectImmediate(controller.generateColliderList[i].gameObject);
                                }
                                catch (Exception)
                                {
                                    Undo.DestroyObjectImmediate(controller.generateColliderList[i].gameObject.GetComponent<ADBColliderReader>());
                                    Undo.DestroyObjectImmediate(controller.generateColliderList[i]);
       
                                    continue;
                                }
                            }
                            else
                            {
                                Undo.DestroyObjectImmediate(controller.generateColliderList[i]);
                            }

                        }
                    }
                }
                controller.generateColliderList = null;
            }
            isDeleteCollider = EditorGUILayout.Toggle("  ©»©¥Include the collider which is not generate by tool ", isDeleteCollider);
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

        private void ShowManualOnFirstScript()
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextArea(MANUAL);
            EditorGUI.EndDisabledGroup();

        }
    }
}
