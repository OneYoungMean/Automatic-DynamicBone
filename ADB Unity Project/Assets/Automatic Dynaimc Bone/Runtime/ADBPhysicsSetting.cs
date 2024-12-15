using System.Collections.Generic;
using UnityEngine;
using System;
using ADBRuntime.Mono;
namespace ADBRuntime
{

    [CreateAssetMenu(fileName = "ADBSettingFile", menuName = "ADB/ADBSettingFile")]
    public class ADBPhysicsSetting : ScriptableObject, IEquatable<ADBPhysicsSetting>
    {
        //高级情况下用这一套
        public AnimationCurve frictionCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f)});
        public   float frictionMin = 0;
        public   float frictionMax = 1;
        public float frictionValue = 0f;//OYM:摩擦力比值
        public bool isfrictionCurve = false;

        public AnimationCurve addForceScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float addForceScaleMin = 0;
        public   float addForceScaleMax = 2;
        public float addForceScaleValue = 1f;//OYM:力量比
        public bool isaddForceScaleCurve = false;


        public AnimationCurve gravityScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float gravityScaleMin = 0;
        public   float gravityScaleMax = 10;
        public float gravityScaleValue = 1f;//OYM:重力比值
        public bool isgravityScaleCurve = false;

        public AnimationCurve moveInertCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float moveInertMin = 0;
        public   float moveInertMax = 1;
        public float moveInertValue = 0f;//OYM:fixed节点传递下来的速度
        public bool ismoveInertCurve = false;

        public AnimationCurve dampingCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float dampingMin = 0;
        public   float dampingMax = 1;
        public float dampingValue = 0.99f;//OYM:怠速
        public bool isdampingCurve = false;

        public AnimationCurve elasticityCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float elasticityMin = 0;
        public   float elasticityMax = 1;
        public float elasticityValue = 0f;//OYM:parent节点传递下来的速度
        public bool iselasticityCurve = false;

        public AnimationCurve velocityIncreaseCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float velocityIncreaseMin = 0;
        public   float velocityIncreaseMax = 10;
        public float velocityIncreaseValue = 0f;//OYM:位移距离压缩
        public bool isvelocityIncreaseCurve = false;


        public AnimationCurve stiffnessWorldCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float stiffnessWorldMin = 0;
        public   float stiffnessWorldMax = 10;
        public float stiffnessWorldValue = 0f;//OYM:世界刚性(位置刚性)
        public bool isstiffnessWorldCurve = false;

        public AnimationCurve stiffnessLocalCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float stiffnessLocalMin = 0;
        public   float stiffnessLocalMax = 1;
        public float stiffnessLocalValue;//OYM:局部刚性(角度刚性)
        public bool isstiffnessLocalCurve = false;

        public AnimationCurve lengthLimitForceScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float lengthLimitForceScaleMin = 0;
        public   float lengthLimitForceScaleMax = 1;
        public float lengthLimitForceScaleValue = 0;//OYM:父节点对子节点的拉力产生的力
        public bool islengthLimitForceScaleCurve = false;

        public AnimationCurve elasticityVelocityCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f) });
        public   float elasticityVelocityMin = 0;
        public   float elasticityVelocityMax =1;
        public float elasticityVelocityValue = 0;//OYM:弹性所产生的速度
        public bool iselasticityVelocityCurve = false;

        public AnimationCurve structuralShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 1.0f) });
        public   float structuralShrinkVerticalScaleMin = 0;
        public   float structuralShrinkVerticalScaleMax = 1;
        public float structuralShrinkVerticalScaleValue = 1.0f;
        public bool isstructuralShrinkVerticalScaleCurve = false;

        public AnimationCurve structuralStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f)});
        public   float structuralStretchVerticalScaleMin = 0;
        public   float structuralStretchVerticalScaleMax = 1;
        public float structuralStretchVerticalScaleValue = 1.0f;
        public bool isstructuralStretchVerticalScaleCurve = false;

        public AnimationCurve structuralShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float structuralShrinkHorizontalScaleMin = 0;
        public   float structuralShrinkHorizontalScaleMax = 1;
        public float structuralShrinkHorizontalScaleValue = 1.0f;
        public bool isstructuralShrinkHorizontalScaleCurve = false;

        public AnimationCurve structuralStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float structuralStretchHorizontalScaleMin = 0;
        public   float structuralStretchHorizontalScaleMax = 1;
        public float structuralStretchHorizontalScaleValue = 1.0f;
        public bool isstructuralStretchHorizontalScaleCurve = false;

        public AnimationCurve shearShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float shearShrinkScaleMin = 0;
        public   float shearShrinkScaleMax = 1;
        public float shearShrinkScaleValue = 1.0f;
        public bool isshearShrinkScaleCurve = false;

        public AnimationCurve shearStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float shearStretchScaleMin = 0;
        public   float shearStretchScaleMax = 1;
        public float shearStretchScaleValue = 1.0f;
        public bool isshearStretchScaleCurve = false;

        public AnimationCurve bendingShrinkVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float bendingShrinkVerticalScaleMin = 0;
        public   float bendingShrinkVerticalScaleMax = 1;
        public float bendingShrinkVerticalScaleValue = 1.0f;
        public bool isbendingShrinkVerticalScaleCurve = false;


        public AnimationCurve bendingStretchVerticalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float bendingStretchVerticalScaleMin = 0;
        public   float bendingStretchVerticalScaleMax = 1;
        public float bendingStretchVerticalScaleValue = 1.0f;
        public bool isbendingStretchVerticalScaleCurve = false;

        public AnimationCurve bendingShrinkHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float bendingShrinkHorizontalScaleMin = 0;
        public   float bendingShrinkHorizontalScaleMax = 1;
        public float bendingShrinkHorizontalScaleValue = 1.0f;
        public bool isbendingShrinkHorizontalScaleCurve = false;

        public AnimationCurve bendingStretchHorizontalScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float bendingStretchHorizontalScaleMin = 0;
        public   float bendingStretchHorizontalScaleMax = 1;
        public float bendingStretchHorizontalScaleValue = 1.0f;
        public bool isbendingStretchHorizontalScaleCurve = false;

        public AnimationCurve circumferenceShrinkScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float circumferenceShrinkScaleMin = 0;
        public   float circumferenceShrinkScaleMax = 1;
        public float circumferenceShrinkScaleValue = 1.0f;
        public bool iscircumferenceShrinkScaleCurve = false;

        public AnimationCurve circumferenceStretchScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(1.0f, 1.0f) });
        public   float circumferenceStretchScaleMin = 0;
        public   float circumferenceStretchScaleMax = 1;
        public float circumferenceStretchScaleValue = 1.0f;
        public bool iscircumferenceStretchScaleCurve = false;

        public AnimationCurve pointRadiuCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f) });
        public   float pointRadiuMin = 0;
        public   float pointRadiuMax = 1;
        public float pointRadiuValue = 0;
        public bool ispointRadiuCurve = false;

        //OYM:闲置值,以后会用上
        public AnimationCurve value2Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value2Value = 0;
        public AnimationCurve value3Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value3Value = 0;
        public AnimationCurve value4Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value4Value = 0;
        public AnimationCurve value5Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value5Value = 0;
        public AnimationCurve value6Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value6Value = 0;
        public AnimationCurve value7Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value7Value = 0;
        public AnimationCurve value8Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value8Value = 0;
        public AnimationCurve value9Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0f), new Keyframe(1.0f, 0f) });
        public float value9Value = 0;



        //OYM:闲置值,以后会用上

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
        public bool isAllowComputeOtherConstraint = false;
        public float virtualPointAxisLength = 0.1f;
        public bool ForceLookDown = false;//OYM:强制朝下
        public bool isFixedPointFreezeRotation;//OYM:fixed节点固定旋转,用来解决一些坑爹的头发
        //质量
        public bool isAutoComputeWeight = true;//OYM：算质量
        public AnimationCurve weightCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 10.0f) });

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

        public bool isDebugDraw = true;//OYM:debug绘制,,废弃
        public bool isFixGravityAxis = true;//OYM:废弃
        public Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);//OYM：重力(注意会跟随角色旋转而旋转)
        public ColliderChoice colliderChoice = (ColliderChoice)(1 << 10 - 1);//OYM:collider选择

        public void Deserialize(GameObject go, string keyword)
        {
            /*            var chain = go.AddComponent<ADBChainProcessor>();
                        chain.aDBSetting = this;*/
            ADBChainProcessor.CreateADBChainProcessor(go.transform, keyword, this);

        }

        #region Interface
        public bool Equals(ADBPhysicsSetting other)
        {
            //OYM:这段代码是用正则表达式写的
            //OYM:public \w+ (?<repeated>\w+) .*
            //OYM:${repeated}.Equals(other.${repeated})&& \n
            //OYM:感谢正则救我狗命

            do
            {
                //OYM:
                bool result = name.Equals(other.name) &&
                isComputeStructuralVertical.Equals(other.isComputeStructuralVertical) &&
                isComputeStructuralHorizontal.Equals(other.isComputeStructuralHorizontal) &&
                isComputeShear.Equals(other.isComputeShear) &&
                isComputeBendingVertical.Equals(other.isComputeBendingVertical) &&
                isComputeBendingHorizontal.Equals(other.isComputeBendingHorizontal) &&
                isComputeCircumference.Equals(other.isComputeCircumference) &&
                isCollideStructuralVertical.Equals(other.isCollideStructuralVertical) &&
                isCollideStructuralHorizontal.Equals(other.isCollideStructuralHorizontal) &&
                isCollideShear.Equals(other.isCollideShear) &&
                isLoopRootPoints.Equals(other.isLoopRootPoints);

                if (!result) break;
                result &=
                frictionCurve.Equals(other.frictionCurve) &&
                addForceScaleCurve.Equals(other.addForceScaleCurve) &&
                gravityScaleCurve.Equals(other.gravityScaleCurve) &&
                moveInertCurve.Equals(other.moveInertCurve) &&
                dampingCurve.Equals(other.dampingCurve) &&
                elasticityCurve.Equals(other.elasticityCurve) &&
                velocityIncreaseCurve.Equals(other.velocityIncreaseCurve) &&
                stiffnessWorldCurve.Equals(other.stiffnessWorldCurve) &&
                stiffnessLocalCurve.Equals(other.stiffnessLocalCurve);

                if (!result) break;
                result = frictionValue.Equals(other.frictionValue) &&
                addForceScaleValue.Equals(other.addForceScaleValue) &&
                gravityScaleValue.Equals(other.gravityScaleValue) &&
                moveInertValue.Equals(other.moveInertValue) &&
                velocityIncreaseValue.Equals(other.velocityIncreaseValue) &&
                dampingValue.Equals(other.dampingValue) &&
                elasticityValue.Equals(other.elasticityValue) &&
                stiffnessWorldValue.Equals(other.stiffnessWorldValue) &&
                stiffnessLocalValue.Equals(other.stiffnessLocalValue);

                if (!result) break;
                //OYM:杆件约束
                result = structuralShrinkVerticalScaleCurve.Equals(other.structuralShrinkVerticalScaleCurve) &&
                structuralStretchVerticalScaleCurve.Equals(other.structuralStretchVerticalScaleCurve) &&
                structuralShrinkHorizontalScaleCurve.Equals(other.structuralShrinkHorizontalScaleCurve) &&
                structuralStretchHorizontalScaleCurve.Equals(other.structuralStretchHorizontalScaleCurve) &&
                shearShrinkScaleCurve.Equals(other.shearShrinkScaleCurve) &&
                shearStretchScaleCurve.Equals(other.shearStretchScaleCurve) &&
                bendingShrinkVerticalScaleCurve.Equals(other.bendingShrinkVerticalScaleCurve) &&
                bendingStretchVerticalScaleCurve.Equals(other.bendingStretchVerticalScaleCurve) &&
                bendingShrinkHorizontalScaleCurve.Equals(other.bendingShrinkHorizontalScaleCurve) &&
                bendingStretchHorizontalScaleCurve.Equals(other.bendingStretchHorizontalScaleCurve) &&
                circumferenceShrinkScaleCurve.Equals(other.circumferenceShrinkScaleCurve) &&
                circumferenceStretchScaleCurve.Equals(other.circumferenceStretchScaleCurve) &&
                pointRadiuCurve.Equals(other.pointRadiuCurve) &&
                structuralShrinkVerticalScaleValue.Equals(other.structuralShrinkVerticalScaleValue) &&
                structuralStretchVerticalScaleValue.Equals(other.structuralStretchVerticalScaleValue) &&
                structuralShrinkHorizontalScaleValue.Equals(other.structuralShrinkHorizontalScaleValue) &&
                structuralStretchHorizontalScaleValue.Equals(other.structuralStretchHorizontalScaleValue) &&
                shearShrinkScaleValue.Equals(other.shearShrinkScaleValue) &&
                shearStretchScaleValue.Equals(other.shearStretchScaleValue) &&
                bendingShrinkVerticalScaleValue.Equals(other.bendingShrinkVerticalScaleValue) &&
                bendingStretchVerticalScaleValue.Equals(other.bendingStretchVerticalScaleValue) &&
                bendingShrinkHorizontalScaleValue.Equals(other.bendingShrinkHorizontalScaleValue) &&
                bendingStretchHorizontalScaleValue.Equals(other.bendingStretchHorizontalScaleValue) &&
                circumferenceShrinkScaleValue.Equals(other.circumferenceShrinkScaleValue) &&
                circumferenceStretchScaleValue.Equals(other.circumferenceStretchScaleValue) &&

                //这是杆件基础值

                structuralShrinkVertical.Equals(other.structuralShrinkVertical) &&
                structuralStretchVertical.Equals(other.structuralStretchVertical) &&
                structuralShrinkHorizontal.Equals(other.structuralShrinkHorizontal) &&
                structuralStretchHorizontal.Equals(other.structuralStretchHorizontal) &&
                shearShrink.Equals(other.shearShrink) &&
                shearStretch.Equals(other.shearStretch) &&
                bendingShrinkVertical.Equals(other.bendingShrinkVertical) &&
                bendingStretchVertical.Equals(other.bendingStretchVertical) &&
                bendingShrinkHorizontal.Equals(other.bendingShrinkHorizontal) &&
                bendingStretchHorizontal.Equals(other.bendingStretchHorizontal) &&
                circumferenceShrink.Equals(other.circumferenceShrink) &&
                circumferenceStretch.Equals(other.circumferenceStretch) &&

                //各种设定
                isComputeVirtual.Equals(other.isComputeVirtual) &&
                isAllowComputeOtherConstraint.Equals(other.isAllowComputeOtherConstraint) &&
                virtualPointAxisLength.Equals(other.virtualPointAxisLength) &&
                ForceLookDown.Equals(other.ForceLookDown) &&
                isFixedPointFreezeRotation.Equals(other.isFixedPointFreezeRotation) &&
                //质量
                isAutoComputeWeight.Equals(other.isAutoComputeWeight) &&
                weightCurve.Equals(other.weightCurve) &&

                //OYM:其他
                //isDebugDraw.Equals(other.isDebugDraw) &&
                isFixGravityAxis.Equals(other.isFixGravityAxis) &&
                gravity.Equals(other.gravity) &&
                colliderChoice.Equals(other.colliderChoice);

                if (!result) break;

                return true;
            } while (false);

            return false;


        }
        #endregion
    }
    public enum ColliderChoice
    {
        Head = 1 << 0,
        UpperBody = 1 << 1,
        LowerBody = 1 << 2,
        UpperLeg = 1 << 3,
        LowerLeg = 1 << 4,
        UpperArm = 1 << 5,
        LowerArm = 1 << 6,
        Hand = 1 << 7,
        Foot = 1 << 8,
        Other = 1 << 9,
    }
}