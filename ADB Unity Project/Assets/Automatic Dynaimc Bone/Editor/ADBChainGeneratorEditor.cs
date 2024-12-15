using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
using static ADBHandleHelper;

namespace ADBRuntime.UntiyEditor
{
    using ADBRuntime.Mono;
    using Mono.Tool;
    public enum ChainGeneratorModeCN
    {
        SelectMode,
        DynamicBoneMode,
        KeyWordMode,
        ClearMode ,
       
    }
    [CustomEditor(typeof(ADBChainGenerateTool))]

    public class ADBChainGeneratorEditor : Editor 
    {
        private const string MANUAL = "Add setting file and click GeneratePhysicsData to make bone dynamic" +
            "\n - SelectMode : Generate physics based on selected bones for visualization." +
            "\n - DynamicBoneMode : Generate physics in a similar way to DynamicBone." +
            "\n - Key Word Mode: Generate physics based on bone naming rules." +
            "\n - ClearMode: Forces to clear all physics bones under this Transform.";

        private ADBChainGenerateTool controller;
        private float pointRadius = 0.03f;
        private bool isFoldout;
        private bool isOpenMonitor;
        private bool[] isFoldouts;
        
        public Dictionary<Object, Editor> containerEditors;
        private bool isOpenPickerWindow;

        public void OnEnable()
        {
            controller = target as ADBChainGenerateTool;
        }

        public void OnDisable()
        {
            if (!Application.isPlaying)
            {
                controller.SaveSelectData();
            }

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Event e = Event.current;
            Titlebar("ADBBoneGenerator", new Color(0.8F, 1, 1),0);
            GUILayout.Space(10);
            ShowManualOnFirstScript();
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
                            if (!controller.generateTransformList[i].gameObject.GetComponentsInParent<Transform>(true).Contains(controller.transform))
                            {
                                Titlebar("Error:Transform " + controller.generateTransformList[i].name + "is not self or child!", new Color(0.7f, 0.3f, 0.3f));
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
                        else if (controller.generateKeyWordWhiteList == null || controller.generateKeyWordWhiteList.Count == 0 && controller.linker.AllKeyWord.Count == 0)
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
                        ADBChainGenerateTool.ClearBoneChain(controller.transform);
                        containerEditors?.Clear();
                    }
                    return;
                case ChainGeneratorModeCN.SelectMode:
                    if (controller.setting == null)
                    {
                        Titlebar("Error:Physics setting cannot be null!", new Color(0.7f, 0.3f, 0.3f));
                    }
                    break;
                default:
                    break;
            }

            Titlebar("=============== GenerateSetting", new Color(0.8f , 1, 1));
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
                case ChainGeneratorModeCN.SelectMode:
                    {

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("setting"), new GUIContent("Phyhsics Setting"), true);

                        //Debug.Log("control id:"+EditorGUIUtility.GetObjectPickerControlID());
                        //Debug.Log("pick object:" + EditorGUIUtility.GetObjectPickerObject().name);
                        //EditorGUILayout.GetControlRect()    EditorGUIUtility.GetControlID()  EditorGUIUtility.ShowObjectPicker

                        if ( GUILayout.Button((controller.isOpenMonitor?"Close":"Open")+"Monitor"))
                        {
                            if (!controller.isOpenMonitor)
                            {
                                controller.UpdateSelectDictionary(true);
                                var otherControllers = controller.GetComponents<ADBChainGenerateTool>();
                                for (int i = 0; i < otherControllers.Length; i++)
                                {
                                    otherControllers[i].isOpenMonitor = false;
                                }
                            }

                            controller.isOpenMonitor = !controller.isOpenMonitor;
                        }
                        if (controller.isOpenMonitor)
                        {
                            pointRadius = EditorGUILayout.Slider(nameof(pointRadius),pointRadius, 0, 0.2f);
                        }
                        else
                        {

                        }
                    }
                    break;
                default:
                    break;
            }


            if (GUILayout.Button("Generate Physics Data", GUILayout.Height(22.0f)))
                {
                 
                if (controller.setting==null)
                {
                    int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<ADBPhysicsSetting>(null, false, "", controlID);
                     isOpenPickerWindow = true;
                   
                }
                else
                {
                    controller.ClearBoneChain();
                    controller.InitializeChain();
                    containerEditors?.Clear();
                }
                }

            if (GUILayout.Button("Clear Generate Data", GUILayout.Height(22.0f)))
            {
                controller.ClearBoneChain();
                controller.UpdateSelectDictionary(true);
                containerEditors?.Clear();

            }
            if (e.commandName == "ObjectSelectorUpdated")
            {
                if (isOpenPickerWindow)
                {
                    var setting = EditorGUIUtility.GetObjectPickerObject() as ADBPhysicsSetting;
                    if (setting!=null)
                    {
                        controller.setting = setting;
                        controller.UpdateSelectDictionary();
                        controller.ClearBoneChain();
                        controller.InitializeChain();
                        containerEditors?.Clear();
                    }
                }
            }

            if (isOpenPickerWindow && EditorGUIUtility.GetObjectPickerControlID() == 0) //OYM£ºCheck if ObjectWindow was closed.
            {
                isOpenPickerWindow = false;
            }

            if (controller.allChain != null)
            {

                if (isFoldouts == null || isFoldouts.Length != controller.allChain.Count)
                {
                    isFoldouts = new bool[controller.allChain == null ? 0 : controller.allChain.Count];
                }
                isFoldout = EditorGUILayout.Foldout(isFoldout, "  All Transform :" + controller.GetPointCount());
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

        private void ShowManualOnFirstScript()
        {
            if (controller.GetComponent<ADBChainGenerateTool>()==controller)
            {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.TextArea(MANUAL);
                EditorGUI.EndDisabledGroup();
            }

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

        U GetContainerEditor<T,U>(T container) where T : MonoBehaviour where U : Editor
        {
            if (container == null) return default(U);
            if (containerEditors == null) containerEditors = new Dictionary<Object, Editor>();
            if (containerEditors.TryGetValue(container, out Editor editor))
            {
                return editor as U;
            }
            else
            {
                var result = Editor.CreateEditor(container) ;
                containerEditors.Add(container, result);
                return result as U;
            }
        }

        public void OnSceneGUI()
        {


            if (controller.isOpenMonitor)
            {
                SceneView.currentDrawingSceneView.Repaint();
                Event e = Event.current;
                Dictionary<Transform, SelectHelper> selectDictionary = controller.UpdateSelectDictionary();

                Color temp = Handles.color;
                var selectDictionarySort = selectDictionary.OrderBy(x => -(SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(x.Key.position)).z);//OYM£ºGet DeepBuffer and Sort
                foreach (var item in selectDictionarySort)
                {
                    var targetTransform = item.Key;
                    var selectValue = item.Value;
                    var radius = pointRadius;

                    Handles.color = selectValue.GetColor();
                    radius *= selectValue.GetScale();
                    if (Handles.Button(item.Key.position, Quaternion.identity, radius, radius * 0.5f, Handles.SphereHandleCap))
                    {
                        if (e.control )
                        {
                            Selection.activeGameObject = item.Key.gameObject;
                        }
                        else
                        {
                            Transform[] selectChilds, selectParents;
                            switch (selectValue.selectType)
                            {
                                case SelectHelper.SelectType.InvalidBone:
                                    Debug.Log("The bone " + targetTransform.name + "cannot be select, is a Human Bone");
                                    break;
                                case SelectHelper.SelectType.ClothBone:
                                    selectChilds = targetTransform.GetComponentsInChildren<Transform>();
                                    for (int i = 0; i < selectChilds.Length; i++)
                                    {
                                        var child = selectChilds[i];
                                        if (selectDictionary.TryGetValue(child, out SelectHelper value))
                                        {
                                            value.selectType = SelectHelper.SelectType.SelectBone;
                                        }
                                    }
                                    selectParents = targetTransform.GetComponentsInParent<Transform>();
                                    bool isFixedBone = true;
                                    for (int i = 0; i < selectParents.Length; i++)
                                    {
                                        var parent = selectParents[i];
                                        if (selectDictionary.TryGetValue(parent, out SelectHelper value))
                                        {
                                            if (value.selectType==SelectHelper.SelectType.SelectFixedBone)
                                            {
                                                isFixedBone = false;
                                                break;
                                            }
                                        }
                                    }
                                    selectValue.selectType = isFixedBone? SelectHelper.SelectType.SelectFixedBone: SelectHelper.SelectType.SelectBone;
                                    break;
                                case SelectHelper.SelectType.SelectBone:
                                    selectChilds = targetTransform.GetComponentsInChildren<Transform>();
                                    for (int i = 0; i < selectChilds.Length; i++)
                                    {
                                        var child = selectChilds[i];
                                        if (selectDictionary.TryGetValue(child, out SelectHelper value))
                                        {
                                            value.selectType = SelectHelper.SelectType.ClothBone;
                                        }
                                    }
                                    break;
                                case SelectHelper.SelectType.SelectFixedBone:
                                    selectChilds = targetTransform.GetComponentsInChildren<Transform>();
                                    for (int i = 0; i < selectChilds.Length; i++)
                                    {
                                        var child = selectChilds[i];
                                        if (selectDictionary.TryGetValue(child, out SelectHelper value))
                                        {
                                            value.selectType = SelectHelper.SelectType.ClothBone;
                                        }
                                    }
                                    break;
                                case SelectHelper.SelectType.GenerateBone:
                                    selectChilds = targetTransform.GetComponentsInChildren<Transform>();
                                    for (int i = 0; i < selectChilds.Length; i++)
                                    {
                                        var child = selectChilds[i];
                                        if (selectDictionary.TryGetValue(child, out SelectHelper value))
                                        {
                                            value.selectType = SelectHelper.SelectType.ClothBone;
                                        }
                                    }
                                    break;
                                case SelectHelper.SelectType.GenerateFixedBone:
                                    selectChilds = targetTransform.GetComponentsInChildren<Transform>();
                                    for (int i = 0; i < selectChilds.Length; i++)
                                    {
                                        var child = selectChilds[i];
                                        if (selectDictionary.TryGetValue(child, out SelectHelper value))
                                        {
                                            value.selectType = SelectHelper.SelectType.ClothBone;
                                        }
                                    }
                                    break;
                                case SelectHelper.SelectType.OtherBone:
                                    Debug.Log("The bone " + item.Key.name + "cannot be select, is controlled by other script");
                                    break;
                                default:
                                    break;
                            }

                        }
                    }

                    if (selectValue.child.Count!=0)
                    {
                        for (int i = 0; i < selectValue.child.Count; i++)
                        {
                            DrawBone(targetTransform.position, selectValue.child[i].position, 0f);
                        }
                    }
                }
            }
        }
    }
}
