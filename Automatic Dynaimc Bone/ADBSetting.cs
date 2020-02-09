using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime
{
    [CreateAssetMenu(fileName = "ADBSettingFile",menuName = "ADB/SettingFile")]
    public class ADBSetting : ScriptableObject
    {
        public bool useGlobal = false;
        //高级情况下用这一套
            public AnimationCurve frictionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });
        public AnimationCurve gravityScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve airResistanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve massCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.9f), new Keyframe(1.0f, 1f) });
        public AnimationCurve lazyCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.2f) });
        public AnimationCurve freezeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve shearShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve shearStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve bendingShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve bendingStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve bendingShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve bendingStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralCircumferenceShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        public AnimationCurve structuralCircumferenceStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f) });
        //OYM：调试情况用这一套

        public float lazyGlobal = 0f;
        public float freezeGlobal = 0f;
        public float frictionGlobal = 0f;
        public float massGlobal=1f;
        public float airResistanceGlobal = 1f;
        public float structuralShrinkVerticalScaleGlobal = 1.0f;
        public float structuralStretchVerticalScaleGlobal = 1.0f;
        public float structuralShrinkHorizontalScaleGlobal = 1.0f;
        public float structuralStretchHorizontalScaleGlobal = 1.0f;
        public float shearShrinkScaleGlobal = 1.0f;
        public float shearStretchScaleGlobal = 1.0f;
        public float bendingShrinkVerticalScaleGlobal = 1.0f;
        public float bendingStretchVerticalScaleGlobal = 1.0f;
        public float bendingShrinkHorizontalScaleGlobal = 1.0f;
        public float bendingStretchHorizontalScaleGlobal = 1.0f;
        public float structuralCircumferenceShrinkScaleGlobal = 1.0f;
        public float structuralCircumferenceStretchScaleGlobal = 1.0f;
        //这是基础值

        public float structuralShrinkVertical = 1.0f;//OYM：垂直结构收缩
        public float structuralStretchVertical = 1.0f;//OYM：垂直结构拉伸
        public float structuralShrinkHorizontal = 1.0f;//OYM：水平结构收缩
        public float structuralStretchHorizontal = 1.0f;//OYM：水平结构拉伸
        public float shearShrink = 1.0f;//OYM：剪切力收缩
        public float shearStretch = 1.0f;//OYM：剪切力拉伸
        public float bendingShrinkVertical = 1.0f;//OYM：垂直弯曲应力收缩
        public float bendingStretchVertical = 1.0f;//OYM：垂直弯曲应力拉伸
        public float bendingShrinkHorizontal = 1.0f;//OYM：水平弯曲应力收缩
        public float bendingStretchHorizontal = 1.0f;//OYM：水平弯曲应力拉伸
        public float circumferenceShrink = 1.0f;
        public float circumferenceStretch = 1.0f;

        //各种设定
        public bool isComputeVirtual = true;//OYM：计算虚拟
        public bool isComputeStructuralVertical = true;//OYM：要计算垂直
        public bool isComputeStructuralHorizontal = false;//OYM：要计算水平
        public bool isComputeShear = false;//OYM：要计算剪切
        public bool isComputeBendingVertical = false;//OYM：要计算垂直弯曲
        public bool isComputeBendingHorizontal = false;//OYM：要计算水平弯曲
        public bool isComputeCircumference = false;//OYM：计算fixPoint与point
        public bool isCollideStructuralVertical = true;
        public bool isCollideStructuralHorizontal = true;
        public bool isCollideShear = true;
        public bool isLoopRootPoints = true;//OYM：与首节点循环链接（非刚体尽量别点

        public bool isDebugDraw;
        public Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);//OYM：重力
        public bool isComputeQuantityByArea = false;

    }
}