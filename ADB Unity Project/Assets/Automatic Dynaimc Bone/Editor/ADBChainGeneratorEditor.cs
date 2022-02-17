using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ADBRuntime.UntiyEditor
{
    using Mono.Tool;
    public enum ChainGeneratorModeCN
    {
        //自动模式,
        DynamicBone模式 = 0,
        关键词模式 = 1,
        清除模式=2,
    }
    [CustomEditor(typeof(ADBChainGenerateTool))]

    public class ADBChainGeneratorEditor : Editor //OYM:我晚点再来修Editor
    {
        ADBChainGenerateTool controller;
        private bool isFoldout;
        private bool[] isFoldouts;
        private Dictionary<Object, Editor> containerEditors;

        public void OnEnable()
        {
            controller = target as ADBChainGenerateTool;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Titlebar("ADB骨骼生成器", new Color(0.8F, 1, 1),0);
            controller.generatorMode = (ChainGeneratorMode)EditorGUILayout.EnumPopup("生成器模式", (ChainGeneratorModeCN)controller.generatorMode);
            switch ((ChainGeneratorModeCN)controller.generatorMode)
            {
                case ChainGeneratorModeCN.DynamicBone模式:
                    {
                        if (controller.setting == null)
                        {
                            Titlebar("错误:物理设置不能为空!", new Color(0.7f, 0.3f, 0.3f));
                        }
                        for (int i = 0; i < controller.generateTransformList?.Count; i++)
                        {
                            if (controller.generateTransformList[i]==null)
                            {
                                continue;
                            }
                            if (!controller.generateTransformList[i].gameObject.GetComponentsInParent<Transform>(true).Contains(controller.transform))//OYM:搜索节点必须为挂载节点的子节点或本身
                            {
                                Titlebar("错误:节点 "+ controller.generateTransformList[i] .name+ "不是挂载节点的子节点或本身!", new Color(0.7f, 0.3f, 0.3f));
                            }
                        }
                    }
                    break;
                case ChainGeneratorModeCN.关键词模式:
                    {
                        if (controller.linker == null)
                        {
                            Titlebar("错误:全局关联设置不能为空!", new Color(0.7f, 0.3f, 0.3f));
                        }
                        if (controller.generateKeyWordWhiteList == null || controller.generateKeyWordWhiteList.Count == 0 && controller.linker.AllKeyWord.Count == 0)
                        {
                            Titlebar("警告:识别关键词缺失", Color.yellow);
                        }
                        else if (controller.linker != null)
                        {
                            for (int i = 0; i < controller.generateKeyWordWhiteList.Count; i++)
                            {
                                if (!controller.linker.isContain(controller.generateKeyWordWhiteList[i]))
                                {
                                    Titlebar("警告:关键词: " + controller.generateKeyWordWhiteList[i] + "不在全局关联设置内!", Color.yellow);
                                }
                            }
                        }
                    }
                    break;
                case ChainGeneratorModeCN.清除模式:
                    if (GUILayout.Button("清除所有节点数据", GUILayout.Height(22.0f)))
                    {
                        ADBChainGenerateTool.ClearBoneChain(controller.transform);
                        containerEditors?.Clear();

                    }
                    return;
                default:
                    break;
            }

            Titlebar("=============== 节点设置", new Color(0.8f
                , 1, 1));
            switch ((ChainGeneratorModeCN)controller.generatorMode)
            {
                case ChainGeneratorModeCN.DynamicBone模式:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("setting"), new GUIContent("物理效果设置"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateTransformList"), new GUIContent("搜索起始点"), true);
                    }
                    break;
                case ChainGeneratorModeCN.关键词模式:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("linker"), new GUIContent("全局关联设置"), true);
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("识别关键词"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("节点黑名单"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("关键词黑名单"), true);
                    }
                    break;
                default:
                    break;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成节点数据", GUILayout.Height(22.0f)))
            {
                controller.InitializeChain();
                containerEditors?.Clear();
            }
            if (GUILayout.Button("清除生成的节点数据", GUILayout.Height(22.0f)))
            {
                controller.ClearBoneChain();
                containerEditors?.Clear();

            }
            EditorGUILayout.EndHorizontal();
            if (controller.allChain != null)
            {

                if (isFoldouts == null || isFoldouts.Length != controller.allChain.Count)
                {
                    isFoldouts = new bool[controller.allChain == null ? 0 : controller.allChain.Count];
                }
                isFoldout = EditorGUILayout.Foldout(isFoldout, "  所有节点坐标 :" + controller.GetPointCount());
                if (isFoldout)
                {
                    for (int i = 0; i < controller.allChain?.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.BeginVertical();
                        isFoldouts[i] = EditorGUILayout.Foldout(isFoldouts[i], "Element " + i + ": Count:" + (controller.allChain[i].allPointTransforms == null ? 0 : controller.allChain[i].allPointTransforms.Length));
                        if (isFoldouts[i])
                        {
                            ShowContainerGUI(controller.allChain[i]);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.Space(20);
            serializedObject.ApplyModifiedProperties();
        }
        void Titlebar(string text, Color color,int space =12)
        {
            GUILayout.Space(space);

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(text);
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = backgroundColor;

            GUILayout.Space(3);
        }

        void ShowContainerGUI<T>(T container) where T : MonoBehaviour
        {
            if (container == null) return;
            if (containerEditors == null) containerEditors = new Dictionary<Object, Editor>();
            if (containerEditors.TryGetValue(container, out Editor editor))
            {
                editor.OnInspectorGUI();
            }
            else
            {
                containerEditors.Add(container, Editor.CreateEditor(container));
            }
        }
    }

}