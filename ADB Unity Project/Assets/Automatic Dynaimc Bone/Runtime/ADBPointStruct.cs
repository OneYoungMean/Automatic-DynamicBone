using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
namespace ADBRuntime
{
    [Serializable]
    public struct PointRead
    {
        //public bool isFixGravityAxis;
        public int fixedIndex;
        /// <summary>
        /// ���ڵ����
        /// </summary>
        public int parentIndex;
        /// <summary>
        /// �ӽڵ㿪ͷ��� 
        /// </summary>
        public int childFirstIndex;
        /// <summary>
        /// �ӽڵ��β��ŵĺ�һ�����
        /// </summary>
        public int childLastIndex;
        /// <summary>
        ///����,�˼�����ʱ������໥������ʹ��
        /// </summary>);
        public float mass;

        /// <summary>
        /// Colliderѡ���Զ�ײ
        /// </summary>
        public int colliderMask;
        /// <summary>
        /// �̶��ڵ��Ƿ�����ת?
        /// </summary>
        public bool isFixedPointFreezeRotation;
        /// <summary>
        /// ���᣺How much the bones slowed down.
        /// </summary>);
        public float damping;

        /// <summary>
        /// Ħ������С,����colliderʱ�����ٶȽ��м���ʱ���õ�,
        /// </summary>);
        public float friction;
        /// <summary>
        /// �������,ʹ�����ص���ʼ��worldPosition�����ȵĴ�С
        /// </summary>
        public float stiffnessWorld;
        /// ��������,ʹ�����ص���ʼ��worldPosition�����ȵĴ�С����
        /// </summary>
        //public float stiffnessWorldLimit;
        ///�ֲ�����,ǿ�ƽڵ�ص���ʼ��localPosition�����Ĵ�С,
        /// </summary>
        public float stiffnessLocal;
        /// <summary>
        /// ���ԣ�How much the force applied to return each bone to original orientation.
        /// </summary>
        public float elasticity;
        /// <summary>
        /// ���ԣ�How much character's position change is ignored in physics simulation.
        /// </summary>);
        public float moveInert;
        /// <summary>
        /// ����ģ��,fixed�ڵ㷢��λ��,����ӽڵ����λ�Ʋ����ı���,���Լ���λ�ƹ�����������
        /// </summary>
        public float velocityIncrease;
        /// <summary>
        /// ����
        /// </summary>
        public float3 gravity;



        //����һ��������˼������
        public float structuralShrinkVertical;
        public float structuralStretchVertical;
        public float structuralShrinkHorizontal;
        public float structuralStretchHorizontal;
        public float shearShrink;
        public float shearStretch;
        public float bendingShrinkVertical;
        public float bendingStretchVertical;
        public float bendingShrinkHorizontal;
        public float bendingStretchHorizontal;
        public float circumferenceShrink;
        public float circumferenceStretch;
        /// <summary>
        /// �뾶
        /// </summary>
        public float radius;

        public float3 initialLocalPosition;//OYM:����ڸ����ӵĽڵ�
        internal float initialLocalPositionLength;

        public float3 initialPosition;//OYM��������ֱ�ӵ�position ,���������fixed���position;

        internal quaternion initialLocalRotation;
        internal quaternion initialRotation;

        internal float dampDivIteration;
        internal float addForceScale;
        public float lengthLimitForceScale;
        public float elasticityVelocity;


        /*        public float value2;
                public float value3;
                public float value4;
                public float value5;
                public float value6;
                public float value7;
                public float value8;
                public float value9;*/
    }

    //OYM��д��ϵͳ
    public struct PointReadWrite
    {
        public float3 position;
        /// <summary>
        /// ��Ϊfixed�ڵ��ʱ��������rotation����Ϊfixed�ڵ��ʱ���������ڵ�ĸ��ڵ��rotation
        /// </summary>
        public quaternion rotationNoSelfRotateChange;
        /// <summary>
        /// �ٶ�
        /// </summary>
        public float3 deltaPosition;

        /// <summary>
        /// ���ٶ�,Ŀǰ��û��ȫ����
        /// </summary>
        public quaternion deltaRotation;

        public bool isCollide;
        //public quaternion deltaRotationY;
    }
}
