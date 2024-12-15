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
        DynamicBoneMode= 0,
        KeyWordMode = 1,
        ClearMode=2,
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

            Titlebar("ADBBoneGenerator", new Color(0.8F, 1, 1),0);
            controller.generatorMode = (ChainGeneratorMode)EditorGUILayout.EnumPopup("GenerateMode", (ChainGeneratorModeCN)controller.generatorMode);
            switch ((ChainGeneratorModeCN)controller.generatorMode)
            {
                case ChainGeneratorModeCN.DynamicBoneMode:
                    {
                        if (controller.setting == null)
                        {
                            Titlebar("Error:Physics setting cannot be null!", new Color(0.7f, 0.3f, 0.3f));
                        }
                        for (int i = 0; i < controller.generateTransformList?.Count; i++)
                        {
                            if (controller.generateTransformList[i]==null)
                            {
                                continue;
                            }
                            if (!controller.generateTransformList[i].gameObject.GetComponentsInParent<Transform>(true).Contains(controller.transform))//OYM:搜索节点必须为挂载节点的子节点或本身
                            {
                                Titlebar("Error:Transform "+ controller.generateTransformList[i] .name+ "is not self or child!", new Color(0.7f, 0.3f, 0.3f));
                            }
                        }
                    }
                    break;
                case ChainGeneratorModeCN.KeyWordMode:
                    {
                        if (controller.linker == null)
                        {
                            Titlebar("Error:setting linker cannot be null!", new Color(0.7f, 0.3f, 0.3f));
                        }
                        if (controller.generateKeyWordWhiteList == null || controller.generateKeyWordWhiteList.Count == 0 && controller.linker.AllKeyWord.Count == 0)
                        {
                            Titlebar("Tips:Lost keyword,will be use linker's keyword", Color.gray);
                        }
                        else if (controller.linker != null)
                        {
                            for (int i = 0; i < controller.generateKeyWordWhiteList.Count; i++)
                            {
                                if (!controller.linker.isContain(controller.generateKeyWordWhiteList[i]))
                                {
                                    Titlebar("Warning:Keyword: " + controller.generateKeyWordWhiteList[i] + "doesn't exist on Linker's keyword", Color.yellow);
                                }
                            }
                        }
                    }
                    break;
                case ChainGeneratorModeCN.ClearMode:
                    if (GUILayout.Button("Clear all physics data", GUILayout.Height(22.0f)))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete?", "ok", "cancel"))
                        {
                            ADBChainGenerateTool.ClearBoneChain(controller.transform);
                            containerEditors?.Clear();
                        }
                    }
                    return;
                default:
                    break;
            }

            Titlebar("=============== GenerateSetting", new Color(0.8f
                , 1, 1));
            switch ((ChainGeneratorModeCN)controller.generatorMode)
            {
                case ChainGeneratorModeCN.DynamicBoneMode:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("setting"), new GUIContent("Phyhsics Setting"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateTransformList"), new GUIContent("Search Start Transform"), true);
                    }
                    break;
                case ChainGeneratorModeCN.KeyWordMode:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("linker"), new GUIContent("Physics Setting Linker"), true);
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("White Keyword List"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("Black Transform List"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("Black Keyword List"), true);
                    }
                    break;
                default:
                    break;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Physics Data", GUILayout.Height(22.0f)))
            {
                controller.InitializeChain();
                containerEditors?.Clear();
            }
            if (GUILayout.Button("Clear Generate Data", GUILayout.Height(22.0f)))
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
                isFoldout = EditorGUILayout.Foldout(isFoldout, " All Transform :" + controller.GetPointCount());
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