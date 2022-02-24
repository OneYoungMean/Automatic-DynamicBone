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

            Titlebar("人形角色碰撞体生成器", new Color(0.5F, 1, 1));
            if (controller.gameObject.GetComponent<Animator>()==null)
            {
                Titlebar("错误: 当前节点上没有检测到Animator!", new Color(0.7f, 0.3f, 0.3f));
            }
            if (controller.gameObject.GetComponentsInChildren<ADBChainProcessor>() == null)
            {
                Titlebar("提示:在生成节点数据之后生成碰撞体会更加精确.", Color.grey);
            }
            if (controller.gameObject.GetComponentsInChildren<ADBColliderReader>() .Length!=0 && (controller.generateColliderList == null || controller.generateColliderList.Count == 0))
            {
                Titlebar("提示:检测到一些潜在的collider,按刷新以显示", Color.grey);
            }
            if (controller.generateColliderList!=null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateColliderList"), new GUIContent("碰撞体列表 :" + controller.generateColliderList.Count), true);
            }



            string key = controller.isGenerateColliderAutomaitc ? "生成" : "刷新";

            if (GUILayout.Button(key + "碰撞体", GUILayout.Height(22.0f)))
            {
                controller.initializeCollider();
            }
            if (controller.generateColliderList == null || controller.generateColliderList.Count == 0)
            {
                controller.isGenerateColliderAutomaitc = EditorGUILayout.Toggle("自动生成全身碰撞体 ", controller.isGenerateColliderAutomaitc);

                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.colliderSize = EditorGUILayout.Slider("  ┗━缩放比例 ", controller.colliderSize, 0.001f, 2f);
                    controller.isGenerateColliderOpenTrigger = EditorGUILayout.Toggle("  ┗━生成的碰撞体为trigger ", controller.isGenerateColliderOpenTrigger);
                    controller.isGenerateByAllPoint = EditorGUILayout.Toggle("  ┗━以所有节点作为参照 ", controller.isGenerateByAllPoint);
                    controller.isGenerateFinger = EditorGUILayout.Toggle("  ┗━生成手指 ", controller.isGenerateFinger);
                }
                if (controller.isGenerateColliderAutomaitc)
                {

                }
                if (controller.isGenerateColliderAutomaitc)
                {

                }
            }
            else
            {
/*                controller.colliderSize = EditorGUILayout.Slider("碰撞体大小", controller. colliderSize, 0, 2);*/
            }
            if (GUILayout.Button("删除所有生成的碰撞体", GUILayout.Height(22.0f)))
            {
                if (EditorUtility.DisplayDialog("你确定需要删除吗?", "该操作不可撤销", "ok", "cancel"))
                {
                    for (int i = 0; i < controller.generateColliderList?.Count; i++)
                    {
                        if (controller.generateColliderList[i] != null)
                        {
                            if (controller.generateColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                            {
                                DestroyImmediate(controller.generateColliderList[i].gameObject);
                            }
                            else
                            {
                                DestroyImmediate(controller.generateColliderList[i]);
                            }

                        }
                    }
                    controller.generateColliderList = null;

                    var overlapsColliderList = controller.transform.GetComponentsInChildren<Collider>();
                    if (isDeleteCollider)
                    {
                        for (int i = 0; i < overlapsColliderList.Length; i++)
                        {
                            if (overlapsColliderList[i] != null)
                            {
                                if (overlapsColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                                {
                                    DestroyImmediate(overlapsColliderList[i].gameObject);
                                }
                                else
                                {
                                    DestroyImmediate(overlapsColliderList[i]);
                                }
                            }
                        }
                        controller.generateColliderList.Clear();
                    }
                }
            }
            isDeleteCollider = EditorGUILayout.Toggle("  ┗━包括不是自动生成的碰撞体 ", isDeleteCollider);
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
