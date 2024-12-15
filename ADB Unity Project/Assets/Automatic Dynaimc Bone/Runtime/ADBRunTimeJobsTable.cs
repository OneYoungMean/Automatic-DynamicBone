//#define ADB_DEBUG

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ADBRuntime.Internal
{
    /// <summary>
    /// The physical core driver code is stored in here.
    /// Please DO NOT MODIFY IT before you save all data.
    /// Due to the unsafe code, any secondary compilation 
    /// may cause untiy's crash.
    /// </summary>
    public static unsafe class ADBRunTimeJobsTable
    {
        #region Jobs
        /// <summary>
        /// Read fixed point's data and refresh old data.
        /// </summary>
        [BurstCompile]
        public struct InitiralizePoint1 : IJobParallelForTransform

        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            internal float worldScale;

            public void Execute(int index, TransformAccess transform)
            {
                var pReadWritePoint = pReadWritePoints + index;
                var pReadPoint = pReadPoints + index;

                if (pReadPoint->parentIndex == -1)
                {
                    transform.localRotation = pReadPoint->initialLocalRotation;// transform.rotation will be change too 

                    pReadWritePoint->Rotation = transform.rotation;
                    pReadWritePoint->position = transform.position / worldScale;
                    pReadWritePoint->deltaPosition = float3.zero;

                }
            }
        }
        /// <summary>
        /// Restore it transform according to its localPosition, localRotation and fixed point's Transform. 
        /// </summary>
        [BurstCompile]
        public struct InitiralizePoint2 : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [NativeDisableUnsafePtrRestriction]
            public PositionKalmanFilter* pPositionFliter;
            internal float worldScale;

            public void Execute(int index, TransformAccess transform)
            {
                var pReadWritePoint = pReadWritePoints + index;
                var pReadPoint = pReadPoints + index;

                if (pReadPoint->parentIndex != -1)
                {
                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    var pFixReadPoint = pReadPoints + (pReadPoint->fixedIndex);
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    float3 transformPosition = (pFixReadWritePoint->position + math.mul(pFixReadWritePoint->Rotation, pReadPoint->initialPosition));

                    pReadWritePoint->oldPosition = pReadWritePoint->position = transformPosition;
                    transform.position = transformPosition * worldScale;
                    pReadWritePoint->deltaPosition = float3.zero;

                }
                pPositionFliter[index] = new PositionKalmanFilter(pReadWritePoint->position, pReadWritePoint->deltaPosition);
            }

        }

        /// <summary>
        /// Claculate Collider's MinMaxAABB
        /// </summary>
        [BurstCompile]
        public struct ColliderClacAABB : IJobParallelFor
        //Get collider deltaPostion
        {
            [NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            public float oneDivideIteration;
            [ReadOnly]
            public float localScale;
            [ReadOnly]
            public float maxPointRadius;
            public void Execute(int index)
            {

                ColliderRead* pReadCollider = pReadColliders + index;

                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                pReadWriteCollider->collideFunc = pReadCollider->collideFunc;
                pReadWriteCollider->colliderType = pReadCollider->colliderType;
                float colliderScale = math.cmax(pReadCollider->scale) * localScale;
                float3 fromLocalPosition = pReadCollider->fromPosition * localScale;
                float3 toLocalPosition = pReadCollider->toPosition * localScale;

                pReadWriteCollider->position = pReadCollider->fromPosition * localScale;
                pReadCollider->deltaPosition = (pReadCollider->toPosition - pReadCollider->fromPosition) * localScale * oneDivideIteration;
                MinMaxAABB AABB, temp1, temp2;//AABB will contains the position of the previous update and current update. 

                switch (pReadCollider->colliderType)//Sphere collider's AABB
                {
                    case ColliderType.Sphere:

                        pReadWriteCollider->size = new float3(pReadCollider->originRadius * colliderScale, 0, 0);
                        AABB = new MinMaxAABB(fromLocalPosition, toLocalPosition);
                        AABB.Expand(pReadCollider->originRadius * colliderScale);

                        break;
                    case ColliderType.Capsule://Capsule collider's AABB

                        pReadWriteCollider->direction = pReadCollider->fromDirection;
                        pReadCollider->deltaDirection = (pReadCollider->toDirection - pReadCollider->fromDirection) * oneDivideIteration;
                        pReadWriteCollider->size = new float3(pReadCollider->originRadius * colliderScale, pReadCollider->originHeight * colliderScale, 0);

                        temp1 = new MinMaxAABB(fromLocalPosition, fromLocalPosition + pReadCollider->fromDirection * pReadCollider->originHeight * colliderScale);
                        temp2 = new MinMaxAABB(toLocalPosition, toLocalPosition + pReadCollider->toDirection * pReadCollider->originHeight * colliderScale);
                        AABB = new MinMaxAABB(temp1, temp2);
                        AABB.Expand(pReadCollider->originRadius * colliderScale);

                        break;
                    case ColliderType.OBB://Box collider's AABB

                        pReadWriteCollider->rotation = pReadCollider->fromRotation;
                        pReadCollider->deltaRotation = math.slerp(pReadCollider->fromRotation, pReadCollider->toRotation, oneDivideIteration);
                        pReadWriteCollider->size = pReadCollider->originBoxSize * pReadCollider->scale * localScale;

                        temp1 = MinMaxAABB.CreateFromCenterAndHalfExtents(fromLocalPosition, pReadCollider->originBoxSize * colliderScale); //Create a AABB which has the same size and position with target OBB 
                        temp1 = MinMaxAABB.Rotate(pReadCollider->fromRotation, temp1);//Reclaculate its size after is rotate.
                        temp2 = MinMaxAABB.CreateFromCenterAndHalfExtents(toLocalPosition, pReadCollider->originBoxSize * colliderScale);
                        temp2 = MinMaxAABB.Rotate(pReadCollider->toRotation, temp2);
                        AABB = new MinMaxAABB(temp1, temp2);
                        break;
                    default:
                        AABB = MinMaxAABB.identity;
                        break;
                }
                pReadCollider->AABB = AABB;
            }
        }


        /// <summary>
        /// Collection Fixed poition's position and rotation.
        /// Change other poition's rotation.
        /// Change every iteration's damp by iterationCount.
        /// </summary>
        [BurstCompile]
        public struct PointGetTransform : IJobParallelForTransform
        {
            [NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [NativeDisableUnsafePtrRestriction]
            public PositionKalmanFilter* pPositionFliters;
            [ReadOnly]
            public float oneDivideIteration;
            [ReadOnly]
            public float worldScale;
            [ReadOnly]
            public float deltaTime;

            public void Execute(int index, TransformAccess transform)
            {
                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                PositionKalmanFilter* pPositionFliter = pPositionFliters + index;

                quaternion transformRotation = transform.rotation;
                float3 transformPosition = transform.position / worldScale;//Standardize Scale 
                //transformPosition = pPositionFliter->Update(transformPosition, deltaTime);
                quaternion localRotation = transform.localRotation;

                quaternion parentRotation = math.mul(transformRotation, math.inverse(localRotation));
                quaternion currentRotationNoSelfRotateChange = math.mul(parentRotation, pReadPoint->initialLocalRotation);
                if (pReadPoint->parentIndex == -1)//fixed point
                {
                    //pReadWritePoint->deltaPosition = (transformPosition - pReadWritePoint->position);
                    pReadWritePoint->position = transformPosition;
                    pReadWritePoint->deltaRotation = math.slerp(pReadWritePoint->Rotation, currentRotationNoSelfRotateChange, oneDivideIteration);//dampDivIteration^iteration =damping
                }
                pReadWritePoint->Rotation = transformRotation;
                pReadWritePoint->LoacalRotation = localRotation;
                pReadWritePoint->rotationNoSelfRotateChange = currentRotationNoSelfRotateChange;
                pReadPoint->dampDivIteration = pReadPoint->damping == 0 ? 0 : math.exp(math.log(pReadPoint->damping) * oneDivideIteration);//dampDivIteration^iteration =damping
            }
        }
        /// <summary>
        /// Update point position each iteration
        /// </summary>
        [BurstCompile]
        public struct PointUpdate : IJobParallelFor
        {
            const float gravityLimit = 1f;

            [NativeDisableUnsafePtrRestriction]
            internal PointReadWrite* pReadWritePoints;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal PointRead* pReadPoints;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            [ReadOnly]
            public int colliderCount;
            [ReadOnly]
            internal float3 addForcePower;
            [ReadOnly]
            internal float oneDivideIteration;
            [ReadOnly]
            internal float deltaTime;
            [ReadOnly]
            internal bool isCollision;
            [ReadOnly]
            internal bool isOptimize;
            public void Execute(int index)
            {
                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                if (pReadPoint->fixedIndex != index)
                {

                    EvaluatePosition(index, pReadPoint, pReadWritePoint, addForcePower, oneDivideIteration, deltaTime, isOptimize);//Update point physics move.
                    pReadWritePoint->position += oneDivideIteration * pReadWritePoint->deltaPosition;
                    if (isCollision) //OYM：Update point collision move.
                    {
                        for (int i = 0; i < colliderCount; ++i)
                        {
                            ColliderRead* pReadCollider = pReadColliders + i;

                            if (pReadCollider->isOpen && (pReadPoint->colliderMask & pReadCollider->colliderChoice) != 0)
                            {
                                float pointRadius = pReadPoint->radius;
                                bool isColliderInsideMode = (pReadCollider->collideFunc == CollideFunc.InsideLimit || pReadCollider->collideFunc == CollideFunc.InsideNoLimit); //Used to determine if AABB overlaps results need to be flipped
                                if (pReadCollider->AABB.Overlaps(pReadWritePoint->position - pointRadius, pReadWritePoint->position + pointRadius) ^ isColliderInsideMode)
                                {
                                    ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;
                                    CollideProcess(pReadPoint, pReadWritePoint, pReadWriteCollider, pointRadius, oneDivideIteration, isColliderInsideMode);
                                }
                            }
                        }
                    }
                }



            }
            /// <summary>
            /// Update point physics move.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="addForcePower"></param>
            /// <param name="oneDivideIteration"></param>
            /// <param name="deltaTime"></param>
            /// <param name="isOptimize"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void EvaluatePosition(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float3 addForcePower, float oneDivideIteration, float deltaTime, bool isOptimize)
            {

                float timeScale = deltaTime * 60;
                pReadWritePointTarget->deltaPosition *= pReadPointTarget->dampDivIteration;

                if (pReadPointTarget->stiffnessLocal != 0 || pReadPointTarget->elasticity != 0 || pReadPointTarget->elasticityVelocity != 0 || pReadPointTarget->lengthLimitForceScale != 0)//Updates the forces from dynamic bone's schematic
                {
                    UpdateDynamicBone(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration, deltaTime, timeScale);
                }
                if (pReadPointTarget->velocityIncrease != 0 || pReadPointTarget->moveInert != 0)
                {
                    UpdateFixedPointChain(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration);//Updates the forces from the fixed point .
                }

                if (math.any(pReadPointTarget->gravity))
                {
                    UpdateGravity(pReadPointTarget, pReadWritePointTarget, deltaTime, oneDivideIteration);//Updates the forces from gravity .
                }

                if (pReadPointTarget->stiffnessWorld != 0)
                {
                    UpdateFreeze(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration, deltaTime);//Update the force from reset origin position's force.
                }

                if (math.any(addForcePower))
                {
                    UpdateExternalForce(pReadPointTarget, pReadWritePointTarget, addForcePower, oneDivideIteration); //Update the force from the external force .
                }


                if (isOptimize)
                {
                    OptimeizeForce(pReadPointTarget, pReadWritePointTarget); // (Experimental) some optimize Function
                }
            }
            /// <summary>
            /// Updates the forces from dynamic bone's schematic
            /// </summary>
            /// <param name="index"></param>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="oneDivideIteration"></param>
            /// <param name="timeScale"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateDynamicBone(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float oneDivideIteration, float deltaTime, float timeScale)
            {

                PointReadWrite* pPointReadWriteParent = pReadWritePointTarget + (pReadPointTarget->parentIndex - index);
                //PointRead* pPointReadParernt = pReadPointTarget + (pReadPointTarget->parentIndex - index);

                float3 targetDirection = math.mul(pPointReadWriteParent->rotationNoSelfRotateChange, pReadPointTarget->initialLocalPosition) * pReadPointTarget->initialLocalPositionLength;

                float3 currentDirection = pReadWritePointTarget->position - pPointReadWriteParent->position;
                //stiffness: Prevent excessive deflection and force back into the original position 

                float3 difficult = currentDirection - targetDirection;

                float difficultLength = math.max(math.EPSILON, math.length(difficult));

                float stiffnessLength = pReadPointTarget->initialLocalPositionLength * 2 * (1 - pReadPointTarget->stiffnessLocal);

                float stiffnessForceLength = math.clamp(difficultLength, 0, stiffnessLength) - difficultLength;

                currentDirection += difficult / difficultLength * stiffnessForceLength;

                //elasticity: Handles the progressive forces between the current localPosition and original localPosition

                float3 lerpDirection = math.lerp(currentDirection, targetDirection, pReadPointTarget->elasticity);

                float lerpDirectionLength = math.max(math.EPSILON, math.length(lerpDirection));

                lerpDirection *= math.lerp(lerpDirectionLength, pReadPointTarget->initialLocalPositionLength, pReadPointTarget->lengthLimitForceScale) / lerpDirectionLength;

                float3 move = (pPointReadWriteParent->position + lerpDirection - pReadWritePointTarget->position) * math.min(0.5f, oneDivideIteration * timeScale);

                pReadWritePointTarget->position += move;
                pReadWritePointTarget->deltaPosition += move * pReadPointTarget->elasticityVelocity;
            }
            /// <summary>
            /// Updates the forces from the fixed point .
            /// </summary>
            /// <param name="index"></param>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateFixedPointChain(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float oneDivideIteration)
            {
                PointReadWrite* pPointReadWriteFixed = pReadWritePointTarget + (pReadPointTarget->fixedIndex - index);
                PointRead* pPointReadFixed = pReadPointTarget + (pReadPointTarget->fixedIndex - index);
                float3 fixedPointdeltaPosition = pPointReadWriteFixed->deltaPosition;
                pReadWritePointTarget->position += fixedPointdeltaPosition * pReadPointTarget->moveInert * oneDivideIteration;//OYM:计算速度补偿
                //OYM：计算以fixed位移进行为参考进行速度补偿
                pReadWritePointTarget->deltaPosition -= fixedPointdeltaPosition * pReadPointTarget->velocityIncrease * oneDivideIteration;
            }
            /// <summary>
            /// Updates the forces from gravity
            /// </summary>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="deltaTime"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateGravity(PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float deltaTime, float oneDivideIteration)
            {
               
                float3 gravity = pReadPointTarget->gravity * (deltaTime * deltaTime);
                pReadWritePointTarget->deltaPosition += gravity * oneDivideIteration;
            }
            /// <summary>
            /// Update the force from reset origin position's force.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="oneDivideIteration"></param>
            /// <param name="deltatime"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateFreeze(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float oneDivideIteration, float deltatime)
            {
                PointReadWrite* pPointReadWriteFixed = pReadWritePointTarget + (pReadPointTarget->fixedIndex - index);
                PointRead* pPointReadFixed = pReadPointTarget + (pReadPointTarget->fixedIndex - index);

                float3 fixedPointPosition = pPointReadWriteFixed->position;
                float3 direction = pReadWritePointTarget->position - fixedPointPosition;

                quaternion fixedPointRotation = pPointReadWriteFixed->rotationNoSelfRotateChange;
                float3 originDirection = math.mul(fixedPointRotation, pReadPointTarget->initialPosition);

                float3 freezeForce = originDirection - direction;

                float freezeForceLength = math.max(math.EPSILON, math.length(freezeForce));
                freezeForceLength = math.sqrt(freezeForceLength);

                float freezeForcelengthLimit = math.clamp(freezeForceLength, -pReadPointTarget->stiffnessWorld * 0.1f, pReadPointTarget->stiffnessWorld * 0.1f);
                freezeForce *= (freezeForcelengthLimit / freezeForceLength);
                freezeForce = oneDivideIteration * deltatime * pReadPointTarget->stiffnessWorld * freezeForce;
                pReadWritePointTarget->deltaPosition += freezeForce;
            }

            /// <summary>
            /// Update the force from the external force
            /// </summary>
            /// <param name="pReadPointTarget"></param>
            /// <param name="pReadWritePointTarget"></param>
            /// <param name="addForcePower"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateExternalForce(PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float3 addForcePower, float oneDivideIteration)
            {
                float3 addForce = oneDivideIteration * addForcePower * pReadPointTarget->addForceScale / pReadPointTarget->mass;
                pReadWritePointTarget->deltaPosition += addForce;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void OptimeizeForce(PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget)
            {
                float persentage;

                persentage = math.max(math.EPSILON, math.length(pReadWritePointTarget->deltaPosition) / pReadPointTarget->initialLocalPositionLength);
                pReadWritePointTarget->deltaPosition *= math.min(1, persentage) / persentage;
            }

            /// <summary>
            ///  Handles the bones collision move
            /// </summary>
            /// <param name="pReadPoint"></param>
            /// <param name="pReadWritePoint"></param>
            /// <param name="pReadWriteCollider"></param>
            /// <param name="pointRadius"></param>
            /// <param name="oneDivideIteration"></param>
            /// <param name="isColliderInsideMode"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CollideProcess(PointRead* pReadPoint, PointReadWrite* pReadWritePoint, ColliderReadWrite* pReadWriteCollider, float pointRadius, float oneDivideIteration, bool isColliderInsideMode)
            {
                float3 colliderPosition = pReadWriteCollider->position;
                float3 size = pReadWriteCollider->size;
                float3 pushout;
                float radiusSum;

                switch (pReadWriteCollider->colliderType)
                {
                    case ColliderType.Sphere: //Sphere
                        radiusSum = size.x + pointRadius;
                        pushout = pReadWritePoint->position - colliderPosition;
                        ClacPowerWhenCollision(pushout, radiusSum, pReadPoint, pReadWritePoint, pReadWriteCollider->collideFunc, oneDivideIteration);

                        break;

                    case ColliderType.Capsule: //Capsule

                        float3 colliderDirection = pReadWriteCollider->direction;
                        radiusSum = pointRadius + size.x;
                        pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, colliderPosition, colliderDirection * size.y);
                        ClacPowerWhenCollision(pushout, radiusSum, pReadPoint, pReadWritePoint, pReadWriteCollider->collideFunc, oneDivideIteration);

                        break;
                    case ColliderType.OBB: //OBB

                        quaternion colliderRotation = pReadWriteCollider->rotation;
                        var localPosition = math.mul(math.inverse(colliderRotation), (pReadWritePoint->position - colliderPosition));
                        MinMaxAABB localOBB = MinMaxAABB.CreateFromCenterAndHalfExtents(0, size + pointRadius);

                        if (localOBB.Contains(localPosition) ^ isColliderInsideMode)
                        {
                            if (isColliderInsideMode)
                            {
                                pushout = math.clamp(localPosition, localOBB.Min, localOBB.Max) - localPosition;
                            }
                            else
                            {
                                float3 toMax = localOBB.Max - localPosition;
                                float3 toMin = localOBB.Min - localPosition;
                                float3 min3 = new float3
                                (
                                math.abs(toMax.x) < math.abs(toMin.x) ? toMax.x : toMin.x,
                                math.abs(toMax.y) < math.abs(toMin.y) ? toMax.y : toMin.y,
                                math.abs(toMax.z) < math.abs(toMin.z) ? toMax.z : toMin.z
                                );
                                float3 min3Abs = math.abs(min3);
                                if (min3Abs.x <= min3Abs.y && min3Abs.x <= min3Abs.z)
                                {
                                    pushout = new float3(min3.x, 0, 0);
                                }
                                else if (min3Abs.y <= min3Abs.x && min3Abs.y <= min3Abs.z)
                                {
                                    pushout = new float3(0, min3.y, 0);
                                }
                                else
                                {
                                    pushout = new float3(0, 0, min3.z);
                                }
                            }

                            pushout = math.mul(colliderRotation, pushout);

                            DistributionPower(pushout, pReadPoint, pReadWritePoint, pReadWriteCollider->collideFunc, oneDivideIteration);

                        }
                        break;
                    default:
                        return;
                }
            }

            /// <summary>
            /// Calculate pushshout when there is a collision
            /// </summary>
            /// <param name="pushout"></param>
            /// <param name="radius"></param>
            /// <param name="pReadPoint"></param>
            /// <param name="pReadWritePoint"></param>
            /// <param name="collideFunc"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ClacPowerWhenCollision(float3 pushout, float radius, PointRead* pReadPoint, PointReadWrite* pReadWritePoint, CollideFunc collideFunc, float oneDivideIteration)
            {
                float sqrPushout = math.lengthsq(pushout);
                if (sqrPushout == 0)
                {
                    return;
                }
                switch (collideFunc)
                {
                    case CollideFunc.OutsideLimit:
                        if ((sqrPushout > radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.OutsideNoLimit:
                        if ((sqrPushout > radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideNoLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;
                    default: { return; }

                }

                pushout = pushout * (radius / math.sqrt(sqrPushout) - 1);

                DistributionPower(pushout, pReadPoint, pReadWritePoint, collideFunc, oneDivideIteration);

            }
            /// <summary>
            /// Distribution the Power of the ejection force
            /// </summary>
            /// <param name="pushout"></param>
            /// <param name="pReadPoint"></param>
            /// <param name="pReadWritePoint"></param>
            /// <param name="collideFunc"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void DistributionPower(float3 pushout, PointRead* pReadPoint, PointReadWrite* pReadWritePoint, CollideFunc collideFunc, float oneDivideIteration)
            {
                float sqrPushout = math.lengthsq(pushout);
                if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                {
                    pReadWritePoint->deltaPosition += 0.01f * oneDivideIteration * pReadPoint->addForceScale * pushout;
                }
                else
                {
                    pReadWritePoint->deltaPosition += pushout;
                    pReadWritePoint->deltaPosition *= (1 - pReadPoint->friction);
                    pReadWritePoint->position += pushout;

                }
            }
            /// <summary>
            /// Calculates the nearest distance between the point and the line segment
            /// </summary>
            /// <param name="tag"></param>
            /// <param name="pos"></param>
            /// <param name="dir"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static float3 ConstrainToSegment(float3 tag, float3 pos, float3 dir)
            {
                float t = math.dot(tag - pos, dir) / math.lengthsq(dir);
                return pos + dir * math.clamp(t, 0, 1);
            }
        }

        /// <summary>
        ///  Update collider position each iteration
        /// </summary>
        [BurstCompile]
        public struct ColliderPositionUpdate : IJobParallelFor
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            public float oneDivideIteration;
            public void Execute(int index)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->position += pReadCollider->deltaPosition;
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->position += pReadCollider->deltaPosition;
                        pReadWriteCollider->direction += pReadCollider->deltaDirection;
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->position += pReadCollider->deltaPosition;
                        pReadWriteCollider->rotation = math.mul(pReadCollider->deltaRotation, pReadWriteCollider->rotation);
                        break;
                    default:
                        break;

                }
            }
        }
        /// <summary>
        /// Update constraint position each iteration.
        /// Includes the location of the start and end points
        /// </summary>
        [BurstCompile]
        public struct ConstraintUpdate : IJobParallelFor
        {

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;

            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ConstraintRead* pConstraintsRead;

            [ReadOnly]
            public int colliderCount;
            [ReadOnly]
            public int globalColliderCount;
            [ReadOnly]
            public bool isCollision;
            [ReadOnly]
            internal float oneDivideIteration;

            public void Execute(int index)
            {

                ConstraintRead* constraint = pConstraintsRead + index;

                PointRead* pPointReadA = pReadPoints + constraint->indexA;
                PointRead* pPointReadB = pReadPoints + constraint->indexB;
                if (pPointReadA->parentIndex == -1 && pPointReadB->parentIndex == -1)//exclude Fixed point
                { return; }

                PointReadWrite* pReadWritePointA = pReadWritePoints + constraint->indexA;

                PointReadWrite* pReadWritePointB = pReadWritePoints + constraint->indexB;

                float3 positionA = pReadWritePointA->position;
                float3 positionB = pReadWritePointB->position;

                float WeightProportion = pPointReadB->mass / (pPointReadA->mass + pPointReadB->mass);//Get weight proportion between A and B

                var Direction = positionB - positionA;
                if (math.all(Direction == 0))
                {
                    return;
                }

                float Distance = math.length(Direction);

                float originDistance = constraint->length;
                float Force = Distance - math.clamp(Distance, originDistance * constraint->shrink, originDistance * constraint->stretch);

                if (Force != 0)
                {
                    bool IsShrink = Force >= 0.0f;
                    float ConstraintPower;
                    switch (constraint->type)
                    {
                        case ConstraintType.Structural_Vertical:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->structuralShrinkVertical + pPointReadB->structuralShrinkVertical)
                                : (pPointReadA->structuralStretchVertical + pPointReadB->structuralStretchVertical);
                            break;
                        case ConstraintType.Structural_Horizontal:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->structuralShrinkHorizontal + pPointReadB->structuralShrinkHorizontal)
                                : (pPointReadA->structuralStretchHorizontal + pPointReadB->structuralStretchHorizontal);
                            break;
                        case ConstraintType.Shear:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->shearShrink + pPointReadB->shearShrink)
                                : (pPointReadA->shearStretch + pPointReadB->shearStretch);
                            break;
                        case ConstraintType.Bending_Vertical:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->bendingShrinkVertical + pPointReadB->bendingShrinkVertical)
                                : (pPointReadA->bendingStretchVertical + pPointReadB->bendingStretchVertical);
                            break;
                        case ConstraintType.Bending_Horizontal:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->bendingShrinkHorizontal + pPointReadB->bendingShrinkHorizontal)
                                : (pPointReadA->bendingStretchHorizontal + pPointReadB->bendingStretchHorizontal);
                            break;
                        case ConstraintType.Circumference:
                            ConstraintPower = IsShrink
                                ? (pPointReadA->circumferenceShrink + pPointReadB->circumferenceShrink)
                                : (pPointReadA->circumferenceStretch + pPointReadB->circumferenceStretch);
                            break;
                        default:
                            ConstraintPower = 0.0f;
                            break;
                    }

                    if (ConstraintPower > 0.0f)
                    {
                        float3 Displacement = Direction / Distance * (Force * ConstraintPower);
                        // distribution power by weright proportion
                        pReadWritePointA->position += Displacement * WeightProportion;
                        pReadWritePointA->deltaPosition += Displacement * WeightProportion;
                        pReadWritePointB->position += -Displacement * (1 - WeightProportion);
                        pReadWritePointB->deltaPosition += -Displacement * (1 - WeightProportion);
                    }

                }


                if (isCollision && constraint->isCollider)
                {
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        ColliderRead* pReadCollider = pReadColliders + i;

                        if (!(pReadCollider->isOpen && (pPointReadA->colliderMask & pReadCollider->colliderChoice) != 0))
                        { continue; }//OYM：Whether collider is open and whether pPointReadA-> colliderChoice contains the bit of pReadCollider->colliderChoice

                        MinMaxAABB constraintAABB = new MinMaxAABB(positionA, positionB);
                        constraintAABB.Expand(constraint->radius);
                        bool isColliderInsideMode = (pReadCollider->collideFunc == CollideFunc.InsideLimit || pReadCollider->collideFunc == CollideFunc.InsideNoLimit);

                        if (pReadCollider->AABB.Overlaps(constraintAABB) ^ isColliderInsideMode) // Used to determine if the AABB results need to be flipped, refer to the point collision section  
                        {
                            ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;
                            ComputeCollider(
                                pReadWriteCollider,
                                pPointReadA, pPointReadB,
                                pReadWritePointA, pReadWritePointB, positionA, positionB,
                                constraint,
                                WeightProportion, oneDivideIteration, isColliderInsideMode
                                );
                        }


                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ComputeCollider(ColliderReadWrite* pReadColliderReadWrite,
                PointRead* pReadPointA, PointRead* pReadPointB,
                PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,
                float3 positionA, float3 positionB,
                ConstraintRead* constraint,
                float WeightProportion, float oneDivideIteration, bool isColliderInsideMode)
            {
                float throwTemp;//OYM：droped data 
                float t, radius;
                float3 size = pReadColliderReadWrite->size;

                float3 colliderPosition = pReadColliderReadWrite->position;

                switch (pReadColliderReadWrite->colliderType)
                {
                    case ColliderType.Sphere: //segment overlap sphere
                        {
                            radius = size.x + constraint->radius;

                            {
                                float3 pointOnLine = ConstrainToSegment(colliderPosition, positionA, positionB - positionA, out t);
                                ClacPowerWhenCollision(pointOnLine - colliderPosition, radius,
                                    pReadPointA, pReadPointB, pReadWritePointA, pReadWritePointB,
                                    WeightProportion, t, oneDivideIteration,
                                    pReadColliderReadWrite->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.Capsule://segment overlap capsule
                        {
                            radius = size.x + constraint->radius;
                            float3 colliderDirection = pReadColliderReadWrite->direction;

                            {
                                float3 pointOnCollider, pointOnLine;
                                SqrComputeNearestPoints(colliderPosition, colliderDirection * size.y, positionA, positionB - positionA, out throwTemp, out t, out pointOnCollider, out pointOnLine);
                                ClacPowerWhenCollision(pointOnLine - pointOnCollider, radius,
                                    pReadPointA, pReadPointB, pReadWritePointA, pReadWritePointB,
                                    WeightProportion, t, oneDivideIteration,
                                    pReadColliderReadWrite->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.OBB:
                        {
                            quaternion colliderRotation = pReadColliderReadWrite->rotation;
                            float3 boxSize = size + new float3(constraint->radius);

                            float t1, t2;
                            // Calculate the intersection point of the line segment with the OBB
                            SegmentToOBB(positionA, positionB, colliderPosition, boxSize, math.inverse(colliderRotation), out t1, out t2);

                            t1 = math.saturate(t1);
                            t2 = math.saturate(t2);
                            //If present, then t2 > t1, and at least one point is not on the boundary
                            bool bHit = t1 >= 0f && t2 > t1 && t2 <= 1.0f;

                            if (bHit && !isColliderInsideMode) //Determine if the member is outside the OBB
                            {
                                float3 pushout;
                                //Here is not to take the nearest point, but to take the midpoint, the nearest point effect is not ideal
                                t = (t1 + t2) * 0.5f;
                                float3 dir = positionB - positionA;
                                float3 nearestPoint = positionA + dir * t;
                                pushout = math.mul(math.inverse(colliderRotation), (nearestPoint - colliderPosition));
                                float pushoutX = pushout.x > 0 ? boxSize.x - pushout.x : -boxSize.x - pushout.x;
                                float pushoutY = pushout.y > 0 ? boxSize.y - pushout.y : -boxSize.y - pushout.y;
                                float pushoutZ = pushout.z > 0 ? boxSize.z - pushout.z : -boxSize.z - pushout.z;

                                //Get the closest location to the pushout position on OBB and push out

                                if (math.abs(pushoutZ) <= math.abs(pushoutY) && math.abs(pushoutZ) <= math.abs(pushoutX))
                                {
                                    pushout = math.mul(colliderRotation, new float3(0, 0, pushoutZ));

                                }
                                else if (math.abs(pushoutY) <= math.abs(pushoutX) && math.abs(pushoutY) <= math.abs(pushoutZ))
                                {
                                    pushout = math.mul(colliderRotation, new float3(0, pushoutY, 0));
                                }
                                else
                                {
                                    pushout = math.mul(colliderRotation, new float3(pushoutX, 0, 0));
                                }
                                DistributionPower(pushout,
                                pReadPointA, pReadPointB, pReadWritePointA, pReadWritePointB,
                                WeightProportion, t, oneDivideIteration,
                                pReadColliderReadWrite->collideFunc);

                            }
                            bool bOutside = t1 <= 0f || t1 >= 1f || t2 <= 0 || t2 >= 1f;
                            if (bOutside && isColliderInsideMode) //Determine if part of the member is on the side or outside of the OBB
                            {

                                float3 localPositionA = math.mul(math.inverse(colliderRotation), positionA - colliderPosition);
                                float3 localPositionB = math.mul(math.inverse(colliderRotation), positionB - colliderPosition);
                                float3 pushA = math.clamp(localPositionA, -boxSize, boxSize) - localPositionA;
                                float3 pushB = math.clamp(localPositionB, -boxSize, boxSize) - localPositionB;

                                pushA = math.mul(colliderRotation, pushA);
                                pushB = math.mul(colliderRotation, pushB);
                                bool isFixedA = WeightProportion < 1e-6f;
                                if (!isFixedA) //Point A may be fixed
                                {
                                    if (pReadColliderReadWrite->collideFunc == CollideFunc.InsideNoLimit)
                                    {
                                        pReadWritePointA->deltaPosition += 0.01f * oneDivideIteration * pReadPointA->addForceScale * pushA;
                                    }
                                    else
                                    {
                                        pReadWritePointA->deltaPosition += pushA;
                                        pReadWritePointA->deltaPosition *= (1 - pReadPointA->friction);

                                        positionA += pushA;

                                    }
                                }

                                if (pReadColliderReadWrite->collideFunc == CollideFunc.InsideNoLimit)
                                {
                                    pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * pReadPointB->addForceScale * pushB;
                                }
                                else
                                {
                                    pReadWritePointB->deltaPosition += pushB;
                                    pReadWritePointB->deltaPosition *= (1 - pReadPointB->friction);

                                    positionB += pushB;

                                }

                            }

                            break;
                        }
                    default:
                        return;

                }
            }

            /// <summary>
            /// Calculate pushshout when there is a collision
            /// </summary>
            /// <param name="pushout"></param>
            /// <param name="radius"></param>
            /// <param name="pReadPoint"></param>
            /// <param name="pReadWritePoint"></param>
            /// <param name="collideFunc"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]

            static void ClacPowerWhenCollision(float3 pushout, float radius,
                 PointRead* pReadPointA, PointRead* pReadPointB,
                PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,
                float WeightProportion, float lengthPropotion, float oneDivideIteration,
                CollideFunc collideFunc)
            {
                float sqrPushout = math.lengthsq(pushout);
                if (sqrPushout == 0)
                {
                    return;
                }

                switch (collideFunc)
                {

                    case CollideFunc.Freeze:
                        break;
                    case CollideFunc.OutsideLimit:
                        if (!(sqrPushout < radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.OutsideNoLimit:
                        if (!(sqrPushout < radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideNoLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;

                }
                pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);
                DistributionPower(pushout,
                    pReadPointA, pReadPointB, pReadWritePointA, pReadWritePointB,
                    WeightProportion, lengthPropotion, oneDivideIteration,
                    collideFunc);
            }

            /// <summary>
            /// Distribution the Power of the ejection force
            /// </summary>
            /// <param name="pushout"></param>
            /// <param name="radius"></param>
            /// <param name="pReadPoint"></param>
            /// <param name="pReadWritePoint"></param>
            /// <param name="collideFunc"></param>
            /// <param name="oneDivideIteration"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void DistributionPower(float3 pushout,
                 PointRead* pReadPointA, PointRead* pReadPointB, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,
                float WeightProportion, float lengthPropotion, float oneDivideIteration,
                CollideFunc collideFunc)
            {
                float sqrPushout = math.lengthsq(pushout);
                if (WeightProportion > 1e-6f)
                {
                    if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                    {
                        pReadWritePointA->deltaPosition += 0.01f * oneDivideIteration * (1 - lengthPropotion) * pReadPointA->addForceScale * pushout;
                    }
                    else
                    {
                        pReadWritePointA->deltaPosition += (pushout * (1 - lengthPropotion));
                        pReadWritePointA->deltaPosition *= (1 - pReadPointA->friction);

                        pReadWritePointA->position += (pushout * (1 - lengthPropotion));

                    }
                }
                else
                {
                    lengthPropotion = 1;
                }

                if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                {
                    pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * (lengthPropotion) * pReadPointB->addForceScale * pushout;
                }
                else
                {
                    pReadWritePointB->deltaPosition += (pushout * lengthPropotion);
                    pReadWritePointB->deltaPosition *= (1 - pReadPointB->friction);

                    pReadWritePointB->position += (pushout * lengthPropotion);

                }

            }
            //https://zalo.github.io/blog/closest-point-between-segments/#line-segments

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float SqrComputeNearestPoints(
                float3 posP,
                float3 dirP,
                float3 posQ,
                float3 dirQ,
out float tP, out float tQ, out float3 pointOnP, out float3 pointOnQ)
            {
                float lineDirSqrMag = math.lengthsq(dirQ);
                float3 inPlaneA = posP - ((math.dot(posP - posQ, dirQ) / lineDirSqrMag) * dirQ);
                float3 inPlaneB = posP + dirP - ((math.dot(posP + dirP - posQ, dirQ) / lineDirSqrMag) * dirQ);
                float3 inPlaneBA = inPlaneB - inPlaneA;

                float t1 = math.dot(posQ - inPlaneA, inPlaneBA) / math.lengthsq(inPlaneBA);
                t1 = math.all(inPlaneA != inPlaneB) ? t1 : 0f; // Zero's t if parallel
                float3 L1ToL2Line = posP + dirP * math.saturate(t1);

                pointOnQ = ConstrainToSegment(L1ToL2Line, posQ, dirQ, out tQ);
                pointOnP = ConstrainToSegment(pointOnQ, posP, dirP, out tP);
                return math.lengthsq(pointOnP - pointOnQ);
            }

            static float3 ConstrainToSegment(float3 tag, float3 pos, float3 dir, out float t)
            {
                t = math.dot(tag - pos, dir) / math.lengthsq(dir);
                t = math.saturate(t);
                return pos + dir * t;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void SegmentToOBB(float3 start, float3 end, float3 center, float3 size, quaternion InverseNormal, out float t1, out float t2)
            {
                float3 startP = math.mul(InverseNormal, (center - start));
                float3 endP = math.mul(InverseNormal, (center - end));
                SegmentToAABB(startP, endP, center, -size, size, out t1, out t2);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void SegmentToAABB(float3 start, float3 end, float3 center, float3 min, float3 max, out float t1, out float t2)
            {
                float3 dir = end - start;
                t1 = math.cmax(math.min((min - start) / dir, (max - start) / dir));
                t2 = math.cmin(math.max((min - start) / dir, (max - start) / dir));
            }

        }

        [BurstCompile]
        public struct ConstraintForceUpdateByPoint : IJobParallelFor //A ConstraintForceUpdate that prevents particles from shaking too much, but removes the Constraint collision calculation
        {

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;

            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly, NativeDisableUnsafePtrRestriction]
#if UNITY_2022_OR_NEWER
          public NativeParallelMultiHashMap<int, ConstraintRead> constraintsRead;
#else
            public NativeMultiHashMap<int, ConstraintRead> constraintsRead;
#endif

            internal float oneDivideIteration;
            public void Execute(int index)
            {
                PointRead* pPointReadA = pReadPoints + index;
                if (pPointReadA->parentIndex < 0)
                {
                    return;
                }
#if UNITY_2022_OR_NEWER
                 NativeParallelMultiHashMapIterator<int> iterator;
#else
                NativeMultiHashMapIterator<int> iterator;
#endif

                ConstraintRead constraint;
                float3 move = float3.zero;

                if (!constraintsRead.TryGetFirstValue(index, out constraint, out iterator)) //  get iterator
                {
                    return;
                }
                PointReadWrite* pReadWritePointA = pReadWritePoints + index;
                float3 positionA = pReadWritePointA->position;

                int count = 0;
                do
                {
                    count++;
                    PointRead* pPointReadB = pReadPoints + constraint.indexB;

                    PointReadWrite* pReadWritePointB = pReadWritePoints + constraint.indexB;

                    float3 positionB = pReadWritePointB->position;

                    var Direction = positionB - positionA;

                    if (math.all(Direction == 0))
                    {
                        continue;
                    }

                    float Distance = math.length(Direction);


                    float originDistance = constraint.length;
                    float Force = Distance - math.clamp(Distance, originDistance * constraint.shrink, originDistance * constraint.stretch);
                    if (Force != 0)
                    {
                        bool IsShrink = Force >= 0.0f;
                        float ConstraintPower;
                        switch (constraint.type)
                        {
                            case ConstraintType.Structural_Vertical:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->structuralShrinkVertical + pPointReadB->structuralShrinkVertical)
                                    : (pPointReadA->structuralStretchVertical + pPointReadB->structuralStretchVertical);
                                break;
                            case ConstraintType.Structural_Horizontal:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->structuralShrinkHorizontal + pPointReadB->structuralShrinkHorizontal)
                                    : (pPointReadA->structuralStretchHorizontal + pPointReadB->structuralStretchHorizontal);
                                break;
                            case ConstraintType.Shear:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->shearShrink + pPointReadB->shearShrink)
                                    : (pPointReadA->shearStretch + pPointReadB->shearStretch);
                                break;
                            case ConstraintType.Bending_Vertical:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->bendingShrinkVertical + pPointReadB->bendingShrinkVertical)
                                    : (pPointReadA->bendingStretchVertical + pPointReadB->bendingStretchVertical);
                                break;
                            case ConstraintType.Bending_Horizontal:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->bendingShrinkHorizontal + pPointReadB->bendingShrinkHorizontal)
                                    : (pPointReadA->bendingStretchHorizontal + pPointReadB->bendingStretchHorizontal);
                                break;
                            case ConstraintType.Circumference:
                                ConstraintPower = IsShrink
                                    ? (pPointReadA->circumferenceShrink + pPointReadB->circumferenceShrink)
                                    : (pPointReadA->circumferenceStretch + pPointReadB->circumferenceStretch);
                                break;
                            default:
                                ConstraintPower = 0.0f;
                                break;
                        }

                        float WeightProportion = pPointReadB->mass / (pPointReadA->mass + pPointReadB->mass);

                        float3 Displacement = Direction / Distance * (Force * ConstraintPower);
                        move += Displacement * WeightProportion;
                    }

                } while (constraintsRead.TryGetNextValue(out constraint, ref iterator));
                if (count != 0)
                {
                    pReadWritePointA->deltaPosition += move / count;
                    pReadWritePointA->position += move / count;
                }
            }
        }
        [BurstCompile]
        public struct ClacSpringBonePhysics : IJobFor
        {
            [NativeDisableUnsafePtrRestriction]
            internal PointReadWrite* pReadWritePoints;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal PointRead* pReadPoints;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            public float deltaTime;

            public void Execute(int index)
            {
                PointRead* pReadPointTarget = pReadPoints + index;
                PointReadWrite* pReadWritePointTarget = pReadWritePoints + index;

                int childIndex = pReadPointTarget->childFirstIndex;
                int parentIndex = pReadPointTarget->parentIndex;
                if (pReadPointTarget->vrmstiffnessForce == 0 || childIndex == -1)
                {
                    return;
                }
                PointRead* pReadPointChild = pReadPoints + (childIndex);
                PointReadWrite* pReadWritePointChild = pReadWritePoints + (childIndex);
                quaternion currentRotationNoSelfRotateChange = quaternion.identity;
                if (parentIndex == -1)
                {
                    currentRotationNoSelfRotateChange = pReadWritePointTarget->rotationNoSelfRotateChange;
                }
                else
                {
                    quaternion parentRotation = (pReadWritePoints + (parentIndex))->Rotation;
                    currentRotationNoSelfRotateChange = math.mul(parentRotation, pReadPointTarget->initialLocalRotation);
                }
                float3 axis = math.mul(currentRotationNoSelfRotateChange, pReadPointChild->initialLocalPosition);
                float3 childPosition = pReadWritePointChild->position;
                float3 oldChildPosition = pReadWritePointChild->oldPosition;
                pReadWritePointChild->oldPosition = childPosition;

                float3 nextChildPosition = childPosition + (childPosition - oldChildPosition) * pReadPointTarget->damping + axis * pReadPointTarget->vrmstiffnessForce * deltaTime;
                //float3 nextChildPosition = childPosition;
                float3 direction = nextChildPosition - pReadWritePointTarget->position;
                if (math.all(direction == 0))
                {
                    return;
                }


                direction = math.normalizesafe(direction);
                nextChildPosition = pReadWritePointTarget->position + direction * pReadPointChild->initialLocalPositionLength;
                pReadWritePointChild->position = nextChildPosition;

                var ftrotation = FromToRotation(axis, direction);

                quaternion currentRotation = math.mul(ftrotation, currentRotationNoSelfRotateChange);
                pReadWritePointTarget->Rotation = currentRotation;
                pReadWritePointTarget->LoacalRotation = math.mul(math.inverse(currentRotationNoSelfRotateChange), currentRotation);
            }


            public static quaternion FromToRotation(float3 from, float3 to, float t = 1.0f)
            {
                from = math.normalize(from);
                to = math.normalize(to);

                float cos = math.dot(from, to);
                float angle = math.acos(cos);
                float3 axis = math.cross(from, to);

                if (math.abs(1.0f + cos) < 1e-06f)
                {
                    angle = (float)math.PI;

                    if (from.x > from.y && from.x > from.z)
                    {
                        axis = math.cross(from, new float3(0, 1, 0));
                    }
                    else
                    {
                        axis = math.cross(from, new float3(1, 0, 0));
                    }
                }
                if (math.abs(1.0f - cos) < 1e-06f)
                {
                    //angle = 0.0f;
                    //axis = new float3(1, 0, 0);
                    return quaternion.identity;
                }
                return quaternion.AxisAngle(math.normalize(axis), angle * t);
            }
        }


        /// <summary>
        /// Convert the point to the actual Transform
        /// </summary>
        [BurstCompile]
        public struct JobPointToTransform : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float deltaTime;
            [ReadOnly]
            public float startDampTime;
            [ReadOnly]
            public float worldScale;
            public void Execute(int index, TransformAccess transform)
            {
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                PointRead* pReadPoint = pReadPoints + index;

                float3 writePosition = pReadWritePoint->position * worldScale;
/*                if (pReadPoint->parentIndex != -1)
                {
                    transform.position = writePosition;
                }*/


                if (pReadPoint->childFirstIndex > -1 &&
                   !(pReadPoint->isFixedPointFreezeRotation && pReadPoint->parentIndex == -1))
                {
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    int childCount = pReadPoint->childLastIndex - pReadPoint->childFirstIndex;
                    if (childCount > 1) return;

                    float3 ToDirection = 0;
                    float3 FromDirection = 0;
                    for (int i = pReadPoint->childFirstIndex; i < pReadPoint->childLastIndex; i++)
                    {
                        var targetChild = pReadWritePoints + i;
                        var targetChildRead = pReadPoints + i;
                        FromDirection += math.normalize(math.mul((quaternion)transform.rotation, targetChildRead->initialLocalPosition));
                        ToDirection += math.normalize(targetChild->position * worldScale - math.lerp(transform.position, writePosition, startDampTime));

                    }

                    Quaternion AimRotation = FromToRotation(FromDirection, ToDirection);
                    transform.rotation = AimRotation * transform.rotation;

                    //transform.rotation = pReadWritePoint->Rotation;

                }

            }
            public static quaternion FromToRotation(float3 from, float3 to, float t = 1.0f)
            {
                from = math.normalize(from);
                to = math.normalize(to);

                float cos = math.dot(from, to);
                float angle = math.acos(cos);
                float3 axis = math.cross(from, to);

                if (math.abs(1.0f + cos) < 1e-06f)
                {
                    angle = (float)math.PI;

                    if (from.x > from.y && from.x > from.z)
                    {
                        axis = math.cross(from, new float3(0, 1, 0));
                    }
                    else
                    {
                        axis = math.cross(from, new float3(1, 0, 0));
                    }
                }
                if (math.abs(1.0f - cos) < 1e-06f)
                {
                    //angle = 0.0f;
                    //axis = new float3(1, 0, 0);
                    return quaternion.identity;
                }
                return quaternion.AxisAngle(math.normalize(axis), angle * t);
            }
            #endregion
        }
    }
}



