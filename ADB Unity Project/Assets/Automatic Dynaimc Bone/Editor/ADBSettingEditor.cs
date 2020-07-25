using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime
{
    [CustomEditor(typeof(ADBSetting))]
    public class ADBSettingEditor : Editor
    {
        ADBSetting controller;
        bool showConstraintGlobal=false;
        bool showConstrainForce=false;
        public void OnEnable()
        {
            controller = target as ADBSetting;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Titlebar("Point Setting", Color.green);
            controller.useGlobal=! EditorGUILayout.Toggle("AdvanceMode", !controller.useGlobal);
            if (!controller.useGlobal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleCurve"), new GUIContent("GravityScale Curve"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("freezeCurve"), new GUIContent("Freeze Curve"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("massCurve"), new GUIContent("Mass Curve"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByFixedPointCurve"), new GUIContent("Move By Fixed PointCurve"), true);
               // EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByPrePointCurve"), new GUIContent("Move By PrePoint Curve"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceCompensationCurve"), new GUIContent("Distance Compensation Curve"), true); 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionCurve"), new GUIContent("Friction Curve"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addForceScaleCurve"), new GUIContent("add Force Scale Curve"), true);
                GUILayout.Space(10);
                showConstraintGlobal = EditorGUILayout.Foldout(showConstraintGlobal,"Point-Constraint Scale Curve");
                if (showConstraintGlobal)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVerticalScaleCurve"), new GUIContent("Structural Vertical Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVerticalScaleCurve"), new GUIContent("Structural Vertical Stretch Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontalScaleCurve"), new GUIContent("Structural Horizontal Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontalScaleCurve"), new GUIContent("Structural Horizontal Stretch Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrinkScaleCurve"), new GUIContent("Shear Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretchScaleCurve"), new GUIContent("Shear Stretch Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVerticalScaleCurve"), new GUIContent("Bengding Vertical Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVerticalScaleCurve"), new GUIContent("Bengding Vertical Stretch Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontalScaleCurve"), new GUIContent("Bengding Horizontal Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontalScaleCurve"), new GUIContent("Bengding Horizontal Stretch Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrinkScaleCurve"), new GUIContent("Circumference Shrink Scale Curve"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretchScaleCurve"), new GUIContent("Circumference Stretch Scale Curve"), true);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleGlobal"), new GUIContent("GravityScale Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("freezeGlobal"), new GUIContent("Freeze Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("massGlobal"), new GUIContent("Mass Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByFixedPointGlobal"), new GUIContent("Move By Fixed Point Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceCompensationGlobal"), new GUIContent("Distance Compensation Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionGlobal"), new GUIContent("Friction Float"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addForceScaleGlobal"), new GUIContent("add Force Scale Float"), true);
              //  EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByPrePointGlobal"), new GUIContent("Move By PrePoint Float"), true);


                GUILayout.Space(10);
                showConstraintGlobal = EditorGUILayout.Foldout(showConstraintGlobal, "Point-Constraint Scale Float");
                if (showConstraintGlobal)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVerticalScaleGlobal"), new GUIContent("Structural Vertical Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVerticalScaleGlobal"), new GUIContent("Structural Vertical Stretch Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontalScaleGlobal"), new GUIContent("Structural Horizontal Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontalScaleGlobal"), new GUIContent("Structural Horizontal Stretch Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrinkScaleGlobal"), new GUIContent("Shear Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretchScaleGlobal"), new GUIContent("Shear Stretch Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVerticalScaleGlobal"), new GUIContent("Bengding Vertical Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVerticalScaleGlobal"), new GUIContent("Bengding Vertical Stretch Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontalScaleGlobal"), new GUIContent("Bengding Horizontal Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontalScaleGlobal"), new GUIContent("Bengding Horizontal Stretch Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrinkScaleGlobal"), new GUIContent("Circumference Shrink Scale Float"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretchScaleGlobal"), new GUIContent("Circumference Stretch Scale Float"), true);
                }
            }
            Titlebar("Constraint Setting", Color.green);
            showConstrainForce = EditorGUILayout.Foldout(showConstrainForce, "Constraint Force");
            if (showConstrainForce)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVertical"), new GUIContent("Structural Vertical Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVertical"), new GUIContent("Structural Vertical Stretch"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontal"), new GUIContent("Structural Horizontal Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontal"), new GUIContent("Structural Horizontal Stretch"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrink"), new GUIContent("Shear Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretch"), new GUIContent("Shear Stretch"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVertical"), new GUIContent("Bengding Vertical Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVertical"), new GUIContent("Bengding Vertical Stretch"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontal"), new GUIContent("Bengding Horizontal Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontal"), new GUIContent("Bengding Horizontal Stretch"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrink"), new GUIContent("Circumference Shrink"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretch"), new GUIContent("Circumference Stretch"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeStructuralVertical"), new GUIContent("Is Compute Structural Vertical"), true);
            if (controller.isComputeStructuralVertical)
            {       
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideStructuralVertical"), new GUIContent("┗━Is Collide Structural Vertical"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeBendingVertical"), new GUIContent("Is Compute Bending Vertical"), true);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeStructuralHorizontal"), new GUIContent("Is Compute Structural Horizontal"), true);
            if(controller.isComputeStructuralHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideStructuralHorizontal"), new GUIContent("┗━Is Collide StructuralHorizontal"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━isLoopRootPoints"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeBendingHorizontal"), new GUIContent("Is Compute Bending Horizontal"), true);
            if (controller.isComputeBendingHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━isLoopRootPoints"), true);
            }
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeShear"), new GUIContent("Is Compute Shear"), true);
            if (controller.isComputeShear)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideShear"), new GUIContent("┗━Is Collide Shear"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━isLoopRootPoints"), true);
            }
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeCircumference"), new GUIContent("Is Compute Circumference"), true);
            GUILayout.Space(10);







            Titlebar("Other Setting", Color.green);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isDebugDraw"), new GUIContent("isDebugDraw"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeVirtual"), new GUIContent("Is Compute Virtual"), true);
            
            if (controller.isComputeVirtual)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualPointRate"), new GUIContent("┗━Virtual Point Rate"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isAutoComputeWeight"), new GUIContent("is Auto Compute Weight"), true);
            if (!controller.isAutoComputeWeight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weightCurve"), new GUIContent("┗━weight Curve"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"), new GUIContent("Gravity"), true);
            controller.colliderChoice =(ColliderChoice) EditorGUILayout.EnumFlagsField("Colider Choice",controller.colliderChoice);
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


