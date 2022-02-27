using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    [CustomEditor(typeof(ADBPhysicsSetting))]
    public class ADBSettingFileEditor : Editor
    {
        ADBPhysicsSetting controller;
        bool showConstraintValue=false;
        bool showConstrainForce=false;

        public void OnEnable()
        {
            controller = target as ADBPhysicsSetting;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Copy"))
            {
                var copy = Object.Instantiate(controller);
                
                copy.name = controller.name+" copy";
                string path =System.IO.Path.GetDirectoryName( AssetDatabase.GetAssetPath(controller));
                string newPath = EditorUtility.SaveFilePanel("PhysicsSetting Save Path", path, copy.name, "asset");
                if (!string.IsNullOrEmpty(newPath))
                {
                    newPath= System.IO.Path.GetFullPath(newPath);
                    string rootPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                    newPath = newPath.Replace(rootPath+"\\", "");
                    AssetDatabase.CreateAsset(Object.Instantiate(controller), newPath);
                }
            }

/*            if (GUILayout.Button("转换为曲线"))
            {

            }*/

            serializedObject.Update();
            Titlebar("PhysicsSetting", Color.white);

            Titlebar("PointSetting", Color.green);
                DrawValueOrCurve(serializedObject,"gravityScale", "Gravity Scale");
                DrawValueOrCurve(serializedObject,"stiffnessWorld", "Displacement Stiffness Scale");
                DrawValueOrCurve(serializedObject,"stiffnessLocal", "Angle Limit Scale");
                DrawValueOrCurve(serializedObject,"elasticity", "Angle Stiffness Scale");
                DrawValueOrCurve(serializedObject,"elasticityVelocity", "Angle Stiffness Velocity Scale");
                DrawValueOrCurve(serializedObject,"lengthLimitForceScale", "Length Limit Force Scale");
                DrawValueOrCurve(serializedObject,"damping","Damping");
                DrawValueOrCurve(serializedObject,"moveInert", "Move Inert");
                DrawValueOrCurve(serializedObject,"velocityIncrease", "Velocity Increase");
                DrawValueOrCurve(serializedObject,"friction","Friction");
                DrawValueOrCurve(serializedObject,"addForceScale","Add Force Scale");

            GUILayout.Space(5);
            DrawValueOrCurve(serializedObject,"pointRadiu","Point Radius");
            Titlebar("StickSetting", Color.green);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeStructuralVertical"), new GUIContent("Is Open StructuralVertical Stick"), true);
            if (controller.isComputeStructuralVertical)
            {       
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideStructuralVertical"), new GUIContent("┗━Enable Stick Collider"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeStructuralHorizontal"), new GUIContent("Is Open StructuralHorizontal Stick"), true);
            if (controller.isComputeStructuralHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideStructuralHorizontal"), new GUIContent("┗━Enable Stick Collider"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━Enable Loop"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeShear"), new GUIContent("Is Open Shear Stick"), true);
            if (controller.isComputeShear)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideShear"), new GUIContent("┗━Enable Stick Collider"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━Enable Loop"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeBendingVertical"), new GUIContent("Is Open BendingVertical Stick"), true);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeBendingHorizontal"), new GUIContent("Is Open BendingHorizontal Stick"), true);
            if (controller.isComputeBendingHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━Enable Loop"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeCircumference"), new GUIContent("Is Open Circumference Stick"), true);

            GUILayout.Space(10);
            showConstraintValue = EditorGUILayout.Foldout(showConstraintValue, "Stick Force Sitting");
            if (showConstraintValue)
            {
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "structuralShrinkVertical", "structuralStretchVertical", "Range of StretchVertical Stick");
                DrawValueOrCurve(serializedObject, "structuralShrinkVerticalScale", "StretchVertical Shrink Scale");
                DrawValueOrCurve(serializedObject, "structuralStretchVerticalScale", "StretchVertical Stretch Scale");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "structuralShrinkHorizontal", "structuralStretchHorizontal", "Range of StretchHorizontal Stick");
                DrawValueOrCurve(serializedObject, "structuralShrinkHorizontalScale", "StretchHorizontall Shrink Scale");
                DrawValueOrCurve(serializedObject, "structuralStretchHorizontalScale", "StretchHorizontal Stretch Scale");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "shearShrink", "shearStretch", "Range of Shear Stick");
                DrawValueOrCurve(serializedObject, "shearShrinkScale", "Shear Shrink Scale");
                DrawValueOrCurve(serializedObject, "shearStretchScale", "Shear Stretch Scale");
                GUILayout.Space(5);

                DrawMinMaxSlider(serializedObject, "bendingShrinkVertical", "bendingStretchVertical", "Range of BendingVertical Stick");
                DrawValueOrCurve(serializedObject, "bendingShrinkVerticalScale", "BendingVertical Shrink Scale");
                DrawValueOrCurve(serializedObject, "bendingStretchVerticalScale", "BendingVertical Stretch Scale");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "bendingShrinkHorizontal", "bendingStretchHorizontal", "Range of BendingHorizontal Stick");
                DrawValueOrCurve(serializedObject, "bendingShrinkHorizontalScale", "BendingHorizontall Shrink Scale");
                DrawValueOrCurve(serializedObject, "bendingStretchHorizontalScale", "BendingHorizontal Stretch Scale");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "circumferenceShrink", "circumferenceStretch", "Range of Circumference Stick");
                DrawValueOrCurve(serializedObject, "circumferenceShrinkScale", "Circumference Shrink Scale");
                DrawValueOrCurve(serializedObject, "circumferenceStretchScale", "Circumference Stretch Scale");
                GUILayout.Label("Stick Final Force=(Scale)*(Length of outside the Range)");
            }
            GUILayout.Space(10);

            Titlebar("Other Setting", Color.green);

/*            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isDebugDraw"), new GUIContent("绘制杆件"), true);*/
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeVirtual"), new GUIContent("Generate Virtual Transform"), true);
            
            if (controller.isComputeVirtual)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isAllowComputeOtherConstraint"), new GUIContent("┗━Allow Virtual Transform Use Other Stick "), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "virtualPointAxisLength"), new GUIContent("┗━Virtual Transform's Stick Length"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "ForceLookDown"), new GUIContent("┗━Virtual Transform Stick's Direction is Down"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isAutoComputeWeight"), new GUIContent("Is Auto Compute Weight"), true);
            if (!controller.isAutoComputeWeight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "weightCurve"), new GUIContent("┗━WeightCurve"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "gravity"), new GUIContent("Gravity Direction"), true);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty( "isFixGravityAxis"), new GUIContent("重力轴随角色旋转而旋转"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isFixedPointFreezeRotation"), new GUIContent("Is Freeze Fixed Transform's Rotation"), true);
            controller.colliderChoice =(ColliderChoice) EditorGUILayout.EnumFlagsField("ColliderMask",(ColliderChoiceZh)controller.colliderChoice);
            EditorUtility.SetDirty(controller);
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

        static void DrawValueOrCurve(SerializedObject serializedObject, string baseName,string baseNameCN)
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                string boolName = "is" + baseName + "Curve";
                var boolField = serializedObject.FindProperty(boolName);
                EditorGUILayout.PropertyField(boolField, new GUIContent(""), false, GUILayout.MaxWidth(30));
                if (boolField.boolValue)
                {
                    var cruveField = serializedObject.FindProperty(baseName + "Curve");
                    EditorGUILayout.PropertyField(cruveField, new GUIContent(baseNameCN + "Curve"), true, GUILayout.MinWidth(250));
                }
                else
                {
                    var valueField = serializedObject.FindProperty(baseName + "Value");
                    var valueMin = serializedObject.FindProperty(baseName + "Min");
                    var valueMax = serializedObject.FindProperty(baseName + "Max");
                    valueField.floatValue = EditorGUILayout.Slider(baseNameCN + "Value", valueField.floatValue, valueMin.floatValue, valueMax.floatValue);
                    //EditorGUILayout.PropertyField(valueField, new GUIContent(baseNameCN + "值"), false);
                }

                EditorGUILayout.EndHorizontal();
            }
            catch (System.Exception)
            {

                throw;
            }

        }

        static void DrawMinMaxSlider(SerializedObject serializedObject, string baseNameMin, string baseNameMax, string baseNameCN)
        {
            EditorGUILayout.BeginHorizontal();
            var fieldMin = serializedObject.FindProperty(baseNameMin);
            var fieldMax = serializedObject.FindProperty(baseNameMax);
            var valueMin = fieldMin.floatValue;
            var valueMax = fieldMax.floatValue;
            valueMin= EditorGUILayout.FloatField(valueMin, GUILayout.MaxWidth(30));
            valueMax = EditorGUILayout.FloatField(valueMax, GUILayout.MaxWidth(30));
            EditorGUILayout.MinMaxSlider(baseNameCN+" "+valueMin.ToString("f2")+"-"+ valueMax.ToString("f2"), ref valueMin, ref valueMax, 0, 2);

            fieldMin.floatValue = valueMin;
            fieldMax.floatValue = valueMax;
            EditorGUILayout.EndHorizontal();
        }
    }
}


