using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
namespace ADBRuntime
{
    /// <summary>
    /// The physics bone's ReadOnly data.
    /// </summary>
    [Serializable]
    public struct PointRead
    {
        /// <summary>
        /// Fixed point's Index
        /// </summary>
        public int fixedIndex;
        /// <summary>
        /// Parent point's Index
        /// </summary>
        public int parentIndex;
        /// <summary>
        /// Child begin Index
        /// </summary>
        public int childFirstIndex;
        /// <summary>
        /// Child end Index
        /// </summary>
        public int childLastIndex;
        /// <summary>
        /// the point's mass
        /// </summary>);
        public float mass;
        /// <summary>
        /// Linked constraint number
        /// </summary>
        public int constraintCount; 
        /// <summary>
        /// Collider mask
        /// </summary>
        public int colliderMask;
        /// <summary>
        ///  Freeze fixed point's Rotation
        /// </summary>
        public bool isFixedPointFreezeRotation;
        /// <summary>
        /// Damp£ºHow much the points slowed down.
        /// </summary>);
        public float damping;
        /// <summary>
        /// Friction between point and collider 
        /// </summary>);
        public float friction;
        /// <summary>
        /// The world's rigidity brings the bones back to the initial worldPosition 
        /// </summary>
        public float stiffnessWorld;
        /// </summary>
        ///The local rigidity brings the bones back to the initial localPosition,
        /// </summary>
        public float stiffnessLocal;
        /// <summary>
        /// Elasticity:how much the force applied to return each bone to original orientation.
        /// </summary>
        public float elasticity;
        /// <summary>
        /// moveInert:How much character's position change is ignored in physics simulation.
        /// </summary>);
        public float moveInert;
        /// <summary>
        ///VelocityIncrease How much character's position velocity is increase in physics simulation.
        /// </summary>
        public float velocityIncrease;
        /// <summary>
        /// Gravity
        /// </summary>
        public float3 gravity;

        //Constraint parameter
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
        /// Collision radius
        /// </summary>
        public float radius;

        /// <summary>
        /// Point's localPosition when initial
        /// </summary>
        public float3 initialLocalPosition;
        /// <summary>
        /// localPosition's length
        /// </summary>
        public float initialLocalPositionLength;
        /// <summary>
        /// Position under its fixedpoint coordinate when initial
        /// </summary>
        public float3 initialPosition;
        /// <summary>
        /// Point's localPosition when initial
        /// </summary>
        public quaternion initialLocalRotation;
        /// <summary>
        ///  Rotation under its fixedpoint coordinate when initial
        /// </summary>
        public quaternion initialRotation;
        /// <summary>
        /// Damping for each iteration
        /// </summary>
        internal float dampDivIteration;
        /// <summary>
        /// Extenral force's Scale
        /// </summary>
        internal float addForceScale;
        /// <summary>
        /// Forces that make to parent's distance return to the initial LocalizationPositionLength
        /// </summary>
        public float lengthLimitForceScale;
        /// <summary>
        ///  ElasticityVelocity:how much elasticity force applied in velocity.
        /// </summary>
        public float elasticityVelocity;
        internal float vrmstiffnessForce;


        /*        public float value2;
                public float value3;
                public float value4;
                public float value5;
                public float value6;
                public float value7;
                public float value8;
                public float value9;*/
    }
    /// <summary>
    /// The physics bone's readwrite frequency data.
    /// </summary>
    public struct PointReadWrite
    {
        public float3 position;
        /// <summary>
        /// Fixed point's rotation or point's parent rotation
        /// </summary>
        public quaternion Rotation;
        /// <summary>
        /// velocity
        /// </summary>
        public float3 deltaPosition;

        /// <summary>
        /// Angle velocity
        /// </summary>
        public quaternion deltaRotation;
        internal float3 oldPosition;
        public quaternion LoacalRotation;
        internal quaternion rotationNoSelfRotateChange;

        //public quaternion deltaRotationY;
    }
}
