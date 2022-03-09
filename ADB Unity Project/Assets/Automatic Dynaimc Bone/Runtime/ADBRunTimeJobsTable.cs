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
    public static unsafe class ADBRunTimeJobsTable
    {
        #region Jobs
        /// <summary>
        /// 初始化所有点的位置
        /// </summary>
        [BurstCompile]
        public struct InitiralizePoint1 : IJobParallelForTransform //OYM:先更新fixed节点

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
                    //Debug.Log(pReadPoint->localRotation==quaternion.Inverse(transform.localRotation));这里没问题
                    transform.localRotation = pReadPoint->initialLocalRotation;//OYM：这里改变之后,rotation也会改变

                    pReadWritePoint->rotationNoSelfRotateChange = transform.rotation;
                    //pReadWritePoint->rotationY = pReadPoint->initialRotation;
                    //Debug.Log(pReadWritePoint->rotation+" "+index);
                    pReadWritePoint->position = transform.position / worldScale;

                    //pReadWritePoint->deltaRotationY = pReadWritePoint->deltaRotation = quaternion.identity;
                    pReadWritePoint->deltaPosition = float3.zero;

                }
            }
        }
        [BurstCompile]
        public struct InitiralizePoint2 : IJobParallelForTransform //OYM:再更新普通的点,避免出错
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

                if (pReadPoint->parentIndex != -1)
                {
                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    var pFixReadPoint = pReadPoints + (pReadPoint->fixedIndex);
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    float3 transformPosition = (pFixReadWritePoint->position + math.mul(pFixReadWritePoint->rotationNoSelfRotateChange, pReadPoint->initialPosition));

                    pReadWritePoint->position = transformPosition;
                    transform.position = transformPosition * worldScale;
                    pReadWritePoint->deltaPosition = float3.zero;

                }
            }
        }

        /// <summary>
        /// Collider计算AABB
        /// </summary>
        [BurstCompile]
        public struct ColliderClacAABB : IJobParallelFor
        //OYM：获取collider的deltaPostion
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
                MinMaxAABB AABB, temp1, temp2;
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere://OYM:包含上一帧的位置与这一帧的位置的球体的AABB

                        pReadWriteCollider->size = new float3(pReadCollider->originRadius * colliderScale, 0, 0);


                        AABB = new MinMaxAABB(fromLocalPosition, toLocalPosition);
                        AABB.Expand(pReadCollider->originRadius * colliderScale);


                        break;
                    case ColliderType.Capsule://OYM:包含上一帧的位置与这一帧的位置的胶囊体的AABB
                        //OYM:这儿有点难,需要先判断两个AABB,然后形成一个更大的

                        pReadWriteCollider->direction = pReadCollider->fromDirection;
                        pReadCollider->deltaDirection = (pReadCollider->toDirection - pReadCollider->fromDirection) * oneDivideIteration;
                        pReadWriteCollider->size = new float3(pReadCollider->originRadius * colliderScale, pReadCollider->originHeight * colliderScale, 0);


                        temp1 = new MinMaxAABB(fromLocalPosition, fromLocalPosition + pReadCollider->fromDirection * pReadCollider->originHeight * colliderScale); //OYM:起点形成的AABB
                        temp2 = new MinMaxAABB(toLocalPosition, toLocalPosition + pReadCollider->toDirection * pReadCollider->originHeight * colliderScale); //OYM:终点形成的AABB
                        AABB = new MinMaxAABB(temp1, temp2);
                        AABB.Expand(pReadCollider->originRadius * colliderScale);

                        break;
                    case ColliderType.OBB://OYM:还好它有内置的旋转函数,否则不太好写

                        pReadWriteCollider->rotation = pReadCollider->fromRotation;
                        pReadCollider->deltaRotation = math.slerp(pReadCollider->fromRotation, pReadCollider->toRotation, oneDivideIteration);
                        pReadWriteCollider->size = pReadCollider->originBoxSize * pReadCollider->scale * localScale;

                        temp1 = MinMaxAABB.CreateFromCenterAndHalfExtents(fromLocalPosition, pReadCollider->originBoxSize * colliderScale); //OYM:创建一个与OBB大小一致的AABB
                        temp1 = MinMaxAABB.Rotate(pReadCollider->fromRotation, temp1);//OYM:进行旋转
                        temp2 = MinMaxAABB.CreateFromCenterAndHalfExtents(toLocalPosition, pReadCollider->originBoxSize * colliderScale); //OYM:创建一个与OBB大小一致的AABB
                        temp2 = MinMaxAABB.Rotate(pReadCollider->toRotation, temp2);//OYM:旋转它到OBB的位置,得到一个更大的AABB
                        AABB = new MinMaxAABB(temp1, temp2);//OYM:扩大包围盒
                        break;
                    default:
                        AABB = MinMaxAABB.identity;
                        break;
                }
                pReadCollider->AABB = AABB;
            }
        }


        /// <summary>
        /// 获取点的位置,同时处理速度上的一些调整
        /// </summary>
        [BurstCompile]
        public struct PointGetTransform : IJobParallelForTransform
        {
            [NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float oneDivideIteration;
            [ReadOnly]
            public float worldScale;
            public void Execute(int index, TransformAccess transform)
            {
                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;

                quaternion transformRotation = transform.rotation;
                float3 transformPosition = transform.position / worldScale;//OYM:乘以worldScale是因为缩放到正常的坐标系当中
                quaternion localRotation = transform.localRotation;
                //OYM：做笔记 unity当中 child.rotation =parent.rotation*child.localrotation;
                //OYM:假设子节点的localrotation不变，只有父节点的rotation变了，那么当前子节点的rotation应该是
                //OYM:Rotation*inverse（localRotation）*initialLocalRotation
                quaternion parentRotation = math.mul(transformRotation, math.inverse(localRotation));
                quaternion currentRotationNoSelfRotateChange = math.mul(parentRotation, pReadPoint->initialLocalRotation);
                //quaternion rotationTemp = math.mul(math.mul(transformRotation, math.inverse(localRotation)), pReadPoint->initialLocalRotation);
                if (pReadPoint->parentIndex == -1)//OYM：fixedpoint
                {
                    pReadWritePoint->deltaPosition = (transformPosition - pReadWritePoint->position);
                    pReadWritePoint->deltaRotation = math.slerp(pReadWritePoint->rotationNoSelfRotateChange, currentRotationNoSelfRotateChange, oneDivideIteration);
                    pReadWritePoint->rotationNoSelfRotateChange = currentRotationNoSelfRotateChange;

                }
                else
                {
                    pReadPoint->dampDivIteration = math.exp(math.log(pReadPoint->damping) * oneDivideIteration);//OYM:dampDivIteration^iteration =damping
                    pReadWritePoint->rotationNoSelfRotateChange = currentRotationNoSelfRotateChange;
                }

            }
        }

        [BurstCompile]
        public struct PointUpdate : IJobParallelFor
        {
            const float gravityLimit = 1f;
            /// <summary>
            /// 所有点位置的指针
            /// </summary>
            [NativeDisableUnsafePtrRestriction]
            internal PointReadWrite* pReadWritePoints;
            /// <summary>
            /// 所有点的指针
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal PointRead* pReadPoints;
            /// <summary>
            /// 所有碰撞体的只读指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            /// <summary>
            /// 所有碰撞体的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 碰撞体数量
            /// </summary>
            [ReadOnly]
            public int colliderCount;
            /// <summary>
            /// 风力
            /// </summary>
            [ReadOnly]
            internal float3 addForcePower;
            /// <summary>
            /// 1/迭代次数,为什么不用迭代次数,因为除法比乘法慢
            /// </summary>
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

                    EvaluatePosition(index, pReadPoint, pReadWritePoint, addForcePower, oneDivideIteration, deltaTime, isOptimize);
                    if (isCollision)
                    {
                        for (int i = 0; i < colliderCount; ++i)
                        {
                            ColliderRead* pReadCollider = pReadColliders + i;

                            if (pReadCollider->isOpen && (pReadPoint->colliderMask & pReadCollider->colliderChoice) != 0)
                            {
                                float pointRadius = pReadPoint->radius;
                                bool isColliderInsideMode = (pReadCollider->collideFunc == CollideFunc.InsideLimit || pReadCollider->collideFunc == CollideFunc.InsideNoLimit); //OYM:用于判断是否需要翻转AABB结果
                                //OYM:判断AABB
                                if (pReadCollider->AABB.Overlaps(pReadWritePoint->position - pointRadius, pReadWritePoint->position + pointRadius) ^ isColliderInsideMode)
                                {//OYM:这里有点难以理解，大概的思路是说，如果要求在外/在内的时候判断AABB发现必然在内/在外的情况下，结束判断
                                 //OYM:但是这里其实有个bug，如果你想要将粒子包含在碰撞体内，而粒子却恰好在AABB外，就会出现不判断的情况。
                                 //OYM:如果你发现这种情况一直存在，可以尝试将AABB扩大一倍。
                                    ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;
                                    CollideProcess(pReadPoint, pReadWritePoint, pReadWriteCollider, pointRadius, oneDivideIteration, isColliderInsideMode);
                                }
                            }
                        }
                    }
                }
                pReadWritePoint->position += oneDivideIteration * pReadWritePoint->deltaPosition * deltaTime * 60;//OYM：这里我想了很久,应该是这样,如果是迭代n次的话,那么deltaposition将会被加上n次,正规应该是只加一次

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void EvaluatePosition(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float3 addForcePower, float oneDivideIteration, float deltaTime, bool isOptimize)
            {
                float timeScale = deltaTime * 60;
                pReadWritePointTarget->deltaPosition *= pReadPointTarget->dampDivIteration;
                //OYM：如果你想要添加什么奇怪的力的话,可以在这底下添加

                if (pReadPointTarget->stiffnessLocal != 0 || pReadPointTarget->elasticity != 0 || pReadPointTarget->elasticityVelocity != 0 || pReadPointTarget->lengthLimitForceScale != 0)
                {
                    UpdateDynamicBone(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration, timeScale);
                }
                if (pReadPointTarget->velocityIncrease != 0 || pReadPointTarget->moveInert != 0)
                {
                    UpdateFixedPointChain(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration);//OYM:更新来自fixed节点的力
                }

                if (math.any(pReadPointTarget->gravity))
                {
                    UpdateGravity(pReadPointTarget, pReadWritePointTarget, deltaTime, oneDivideIteration);//OYM:更新重力
                }

                if (pReadPointTarget->stiffnessWorld != 0)
                {
                    UpdateFreeze(index, pReadPointTarget, pReadWritePointTarget, oneDivideIteration, deltaTime);//OYM:更新复位力
                }

                if (math.any(addForcePower))
                {
                    UpdateExternalForce(pReadPointTarget, pReadWritePointTarget, addForcePower, oneDivideIteration); //OYM:更新额外的力
                }


                if (isOptimize)
                {
                    OptimeizeForce(pReadPointTarget, pReadWritePointTarget); //OYM:一些实验性的优化,或许有用?
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateDynamicBone(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float oneDivideIteration, float timeScale)//OYM:刚性，先放这里，修完碰撞体再来整理
            {

                PointReadWrite* pPointReadWriteParent = pReadWritePointTarget + (pReadPointTarget->parentIndex - index);
                //PointRead* pPointReadParernt = pReadPointTarget + (pReadPointTarget->parentIndex - index);

                float3 targetDirection = math.mul(pPointReadWriteParent->rotationNoSelfRotateChange, pReadPointTarget->initialLocalPosition) * pReadPointTarget->initialLocalPositionLength;

                float3 currentDirection = pReadWritePointTarget->position - pPointReadWriteParent->position;

                //OYM:stiffness: 限制原始位置,防止偏转过大

                float3 difficult = currentDirection - targetDirection;//OYM:获取与原始向量之间的差值

                float difficultLength = math.max(math.EPSILON, math.length(difficult));//OYM:获取长度长度,顺便防止0出现

                float stiffnessLength = pReadPointTarget->initialLocalPositionLength * 2 * (1 - pReadPointTarget->stiffnessLocal);//OYM:获取目标约束的差值长度

                float stiffnessForceLength = math.clamp(difficultLength, 0, stiffnessLength) - difficultLength;//OYM:获取在约束外的部分的长度

                currentDirection += difficult / difficultLength * stiffnessForceLength;//OYM:往targetDirection进行移动

                //OYM:elasticity: 弹性力

                float3 lerpDirection = math.lerp(currentDirection, targetDirection, pReadPointTarget->elasticity); //OYM:获取弹性后的向量

                float lerpDirectionLength = math.max(math.EPSILON, math.length(lerpDirection));//OYM:获取向量长度

                lerpDirection *= math.lerp(lerpDirectionLength, pReadPointTarget->initialLocalPositionLength, pReadPointTarget->lengthLimitForceScale) / lerpDirectionLength; //OYM:对长度进行约束,使其位于实际长度和真实长度之间

                float3 move = (pPointReadWriteParent->position + lerpDirection - pReadWritePointTarget->position) * math.min(0.5f, oneDivideIteration * timeScale);

                pReadWritePointTarget->position += move;
                pReadWritePointTarget->deltaPosition += move * pReadPointTarget->elasticityVelocity * oneDivideIteration;
            }
            /*
             *             private static void UpdateDynamicBone(int index,PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget,  float3 worldScale, float oneDivideIteration, float timeScale)//OYM:刚性，先放这里，修完碰撞体再来整理
            {

            }
             */
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateGravity(PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float deltaTime, float oneDivideIteration)
            {
                /*                float oldGravityForce = math.dot(pReadWritePointTarget->deltaPosition, pReadPointTarget->gravity) / math.lengthsq(pReadPointTarget->gravity);
                                if (oldGravityForce < gravityLimit)*/
                {
                    float3 gravity = pReadPointTarget->gravity * (deltaTime * deltaTime);//OYM：重力

                    pReadWritePointTarget->deltaPosition += gravity * oneDivideIteration;
                }
                //OYM：获取归位的向量

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void UpdateFreeze(int index, PointRead* pReadPointTarget, PointReadWrite* pReadWritePointTarget, float oneDivideIteration, float deltatime)
            {
                PointReadWrite* pPointReadWriteFixed = pReadWritePointTarget + (pReadPointTarget->fixedIndex - index);
                PointRead* pPointReadFixed = pReadPointTarget + (pReadPointTarget->fixedIndex - index);

                float3 fixedPointPosition = pPointReadWriteFixed->position;
                float3 direction = pReadWritePointTarget->position - fixedPointPosition;

                quaternion fixedPointRotation = pPointReadWriteFixed->rotationNoSelfRotateChange;
                float3 originDirection = math.mul(fixedPointRotation, pReadPointTarget->initialPosition);

                float3 freezeForce = originDirection - direction;//OYM:因为direction+freezeForce=originDirection，所以freezeforce是这样算的

                float freezeForceLength = math.max(math.EPSILON, math.length(freezeForce));//OYM:这里优化一下,先不开更号
                freezeForceLength = math.sqrt(freezeForceLength);

                float freezeForcelengthLimit = math.clamp(freezeForceLength, -pReadPointTarget->stiffnessWorld * 0.1f, pReadPointTarget->stiffnessWorld * 0.1f);
                freezeForce *= (freezeForcelengthLimit / freezeForceLength);
                freezeForce = oneDivideIteration * deltatime * pReadPointTarget->stiffnessWorld * freezeForce;
                pReadWritePointTarget->deltaPosition += freezeForce;


            }
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

                //OYM:限制速度
                persentage = math.max(math.EPSILON, math.length(pReadWritePointTarget->deltaPosition) / pReadPointTarget->initialLocalPositionLength);
                pReadWritePointTarget->deltaPosition *= math.min(1, persentage) / persentage;
            }
            /*            float3 GetRotateForce(float3 force, float3 direction)//OYM：返回一个不存在的力,使得其向受力方向卷曲,在一些动漫里面会经常出现这种曲线的头发
                        {
                            var result = math.mul(quaternion.Euler(Mathf.Rad2Deg * force.z, 0, Mathf.Rad2Deg * force.x), direction) - direction;
                            return new float3(result.x, 0, result.z);
                        }
                        #endregion*/

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CollideProcess(PointRead* pReadPoint, PointReadWrite* pReadWritePoint, ColliderReadWrite* pReadWriteCollider, float pointRadius, float oneDivideIteration, bool isColliderInsideMode)
            {
                float3 colliderPosition = pReadWriteCollider->position;
                float3 size = pReadWriteCollider->size;
                float3 pushout;
                float radiusSum;

                //OYM:AABB判断在内
                switch (pReadWriteCollider->colliderType)
                {
                    case ColliderType.Sphere: //OYM:球体
                        radiusSum = size.x + pointRadius;
                        pushout = pReadWritePoint->position - colliderPosition;
                        ClacPowerWhenCollision(pushout, radiusSum, pReadPoint, pReadWritePoint, pReadWriteCollider->collideFunc, oneDivideIteration);

                        break;

                    case ColliderType.Capsule: //OYM:胶囊体

                        float3 colliderDirection = pReadWriteCollider->direction;//OYM:渐进朝向
                        radiusSum = pointRadius + size.x;//OYM:选择collidersize3的x与collidersize3.z中最大的那个
                        pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, colliderPosition, colliderDirection * size.y);
                        ClacPowerWhenCollision(pushout, radiusSum, pReadPoint, pReadWritePoint, pReadWriteCollider->collideFunc, oneDivideIteration);

                        break;
                    case ColliderType.OBB: //OYM:OBB

                        quaternion colliderRotation = pReadWriteCollider->rotation;
                        var localPosition = math.mul(math.inverse(colliderRotation), (pReadWritePoint->position - colliderPosition)); //OYM:获取localPosition
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ClacPowerWhenCollision(float3 pushout, float radius, PointRead* pReadPoint, PointReadWrite* pReadWritePoint, CollideFunc collideFunc, float oneDivideIteration)
            {
                float sqrPushout = math.lengthsq(pushout);
                /*                if (sqrPushout == 0)
                                {
                                    return;
                                }*/
                switch (collideFunc)
                {
                    //OYM：整片代码里面最有趣的一块
                    //OYM：反正我现在不想回忆当时怎么想的了XD
                    case CollideFunc.OutsideLimit:
                        if ((sqrPushout > radius * radius) && sqrPushout != 0) //OYM:向外排斥:不允许radius小于Pushout
                        { return; }
                        break;
                    case CollideFunc.InsideLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)//OYM:向内排斥:不允许radius大于Pushout
                        { return; }
                        break;
                    case CollideFunc.OutsideNoLimit://OYM:同向外排斥
                        if ((sqrPushout > radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideNoLimit://OYM:同向内排斥
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;
                    default: { return; }

                }

                pushout = pushout * (radius / math.sqrt(sqrPushout) - 1);//OYM：这里简单解释一下,首先我要计算的是推出的距离,及半径长度减去原始的pushout度之后剩下的值,即pushout/pushout.magnitude*radius-pushout.即pushout*((radius/magnitude -1));
                DistributionPower(pushout, pReadPoint, pReadWritePoint, collideFunc, oneDivideIteration);

            }
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static float3 ConstrainToSegment(float3 tag, float3 pos, float3 dir)
            {
                float t = math.dot(tag - pos, dir) / math.lengthsq(dir);
                return pos + dir * math.clamp(t, 0, 1);
            }
        }

        [BurstCompile]
        public struct ColliderPositionUpdate : IJobParallelFor
        //OYM：把job的点转换成实际的点
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

        [BurstCompile]
        public struct ConstraintUpdate : IJobParallelFor
        {
            /// <summary>
            /// 指向所有可读的点
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            /// <summary>
            /// 指向所有可读写的点
            /// </summary>);
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            /// <summary>
            /// 所有只读的碰撞体
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            /// <summary>
            /// 所有可读写的碰撞体,他们在这里也是只读的
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 所有杆件
            /// 
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ConstraintRead* pConstraintsRead;
            [ReadOnly]
            /// <summary>
            /// 碰撞体序号
            /// </summary>);
            public int colliderCount;
            [ReadOnly]
            public int globalColliderCount;
            [ReadOnly]
            public bool isCollision;
            [ReadOnly]
            internal float oneDivideIteration;

            public void Execute(int index)
            {
                //OYM：获取约束
                ConstraintRead* constraint = pConstraintsRead + index;

                //OYM：获取约束的节点AB
                PointRead* pPointReadA = pReadPoints + constraint->indexA;
                PointRead* pPointReadB = pReadPoints + constraint->indexB;
                if (pPointReadA->parentIndex == -1 && pPointReadB->parentIndex == -1)//OYM:都为fixed节点则不参与运算
                { return; }
                //OYM：任意一点都不能小于极小值
                //OYM：if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                //OYM：获取可读写的点A
                PointReadWrite* pReadWritePointA = pReadWritePoints + constraint->indexA;

                //OYM：获取可读写的点B
                PointReadWrite* pReadWritePointB = pReadWritePoints + constraint->indexB;
                //OYM：获取约束的朝向
                float3 positionA = pReadWritePointA->position;
                float3 positionB = pReadWritePointB->position;

                float WeightProportion = pPointReadB->mass / (pPointReadA->mass + pPointReadB->mass);

                var Direction = positionB - positionA;
                if (math.all(Direction == 0))//OYM:所有的值都为0
                {
                    return;
                }
                float Distance = math.length(Direction);

                //OYM：力度等于距离减去长度除以弹性，这个值可以不存在，可以大于1但是没有什么卵用

                float originDistance = constraint->length;
                float Force = Distance - math.clamp(Distance, originDistance * constraint->shrink, originDistance * constraint->stretch);
                if (Force != 0)
                {
                    bool IsShrink = Force >= 0.0f;
                    float ConstraintPower;//OYM：这个值等于
                    switch (constraint->type)
                    //OYM：这下面都是一个意思，就是确认约束受到的力，然后根据这个获取杆件约束的属性，计算 ConstraintPower
                    //OYM：Shrink为杆件全局值，另外两个值为线性插值获取的值，同理Stretch，所以这里大概可以猜中只是一个简单的不大于1的值
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


                    //OYM：获取AB点重量比值的比值,由于重量越大移动越慢,所以A的值实际上是B的重量的比



                    if (ConstraintPower > 0.0f)//OYM：这里不可能小于0吧（除非有人搞破坏）
                    {
                        float3 Displacement = Direction / Distance * (Force * ConstraintPower);

                        pReadWritePointA->position += Displacement * WeightProportion;
                        pReadWritePointA->deltaPosition += Displacement * WeightProportion;
                        pReadWritePointB->position += -Displacement * (1 - WeightProportion);
                        pReadWritePointB->deltaPosition += -Displacement * (1 - WeightProportion);
                    }

                }
                //float Force = Distance - constraint->length * worldScale;
                //OYM：是否收缩，意味着力大于0

                if (isCollision && constraint->isCollider)
                {
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        ColliderRead* pReadCollider = pReadColliders + i;//OYM：终于到碰撞这里了

                        if (!(pReadCollider->isOpen && (pPointReadA->colliderMask & pReadCollider->colliderChoice) != 0))
                        { continue; }//OYM：collider是否打开,且pPointReadA->colliderChoice是否包含 pReadCollider->colliderChoice的位

                        MinMaxAABB constraintAABB = new MinMaxAABB(positionA, positionB);
                        constraintAABB.Expand(constraint->radius);
                        bool isColliderInsideMode = (pReadCollider->collideFunc == CollideFunc.InsideLimit || pReadCollider->collideFunc == CollideFunc.InsideNoLimit); //OYM:用于判断是否需要翻转AABB结果,参考点碰撞部分  

                        if (pReadCollider->AABB.Overlaps(constraintAABB) ^ isColliderInsideMode) //OYM:overlap为假且isColliderInsideMode为真或者overlap为真且isColliderInsideMode为假
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
            private static void ComputeCollider(ColliderReadWrite* pReadColliderReadWrite,
                PointRead* pReadPointA, PointRead* pReadPointB,
                PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,
                float3 positionA, float3 positionB,
                ConstraintRead* constraint,
                float WeightProportion, float oneDivideIteration, bool isColliderInsideMode)
            {
                float throwTemp;//OYM:丢掉的数据,因为net4.0以下不支持_，为了避免这种情况就写上了
                float t, radius;
                float3 size = pReadColliderReadWrite->size;

                float3 colliderPosition = pReadColliderReadWrite->position;

                switch (pReadColliderReadWrite->colliderType)
                {
                    case ColliderType.Sphere:
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
                    case ColliderType.Capsule:
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
                            //OYM：这个方法可以求出直线与obbbox的两个交点
                            SegmentToOBB(positionA, positionB, colliderPosition, boxSize, math.inverse(colliderRotation), out t1, out t2);

                            t1 = math.saturate(t1);
                            t2 = math.saturate(t2);
                            //OYM：如果存在,那么t2>t1,且至少有一个点不在边界上
                            bool bHit = t1 >= 0f && t2 > t1 && t2 <= 1.0f;

                            if (bHit && !isColliderInsideMode) //OYM:判断杆件是否在胶囊体外
                            {
                                float3 pushout;
                                //OYM：这里不是取最近的点,而是取中点,最近的点效果并不理想
                                t = (t1 + t2) * 0.5f;
                                float3 dir = positionB - positionA;
                                float3 nearestPoint = positionA + dir * t;
                                pushout = math.mul(math.inverse(colliderRotation), (nearestPoint - colliderPosition));
                                float pushoutX = pushout.x > 0 ? boxSize.x - pushout.x : -boxSize.x - pushout.x;
                                float pushoutY = pushout.y > 0 ? boxSize.y - pushout.y : -boxSize.y - pushout.y;
                                float pushoutZ = pushout.z > 0 ? boxSize.z - pushout.z : -boxSize.z - pushout.z;
                                //OYM：这里我自己都不太记得了 XD
                                //OYM：这里是选推出点离的最近的位置,然后推出
                                //OYM：Abas(pushoutZ) < Abs(pushoutY)是错的 ,可能会出现两者都为0的情况
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
                            if (bOutside && isColliderInsideMode) //OYM:判断杆件是否有一部分在OBB边上或者外面
                            {

                                float3 localPositionA = math.mul(math.inverse(colliderRotation), positionA - colliderPosition);
                                float3 localPositionB = math.mul(math.inverse(colliderRotation), positionB - colliderPosition);
                                float3 pushA = math.clamp(localPositionA, -boxSize, boxSize) - localPositionA;
                                float3 pushB = math.clamp(localPositionB, -boxSize, boxSize) - localPositionB;

                                pushA = math.mul(colliderRotation, pushA);
                                pushB = math.mul(colliderRotation, pushB);
                                bool isFixedA = WeightProportion < 1e-6f;
                                if (!isFixedA) //OYM:A点可能固定
                                {
                                    if (pReadColliderReadWrite->collideFunc == CollideFunc.InsideNoLimit)
                                    {
                                        pReadWritePointA->deltaPosition += 0.01f * oneDivideIteration * pReadPointA->addForceScale * pushA;
                                    }
                                    else
                                    {
                                        pReadWritePointA->deltaPosition += pushA;
                                        pReadWritePointA->deltaPosition *= (1 - pReadPointA->friction);//OYM:增加摩擦力,同时避免摩擦力过大

                                        positionA += pushA;

                                    }
                                }

                                if (pReadColliderReadWrite->collideFunc == CollideFunc.InsideNoLimit) //OYM:B点一般不固定
                                {
                                    pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * pReadPointB->addForceScale * pushB;
                                }
                                else
                                {
                                    pReadWritePointB->deltaPosition += pushB;
                                    pReadWritePointB->deltaPosition *= (1 - pReadPointB->friction);//OYM:增加摩擦力,同时避免摩擦力过大

                                    positionB += pushB;

                                }

                            }

                            break;
                        }
                    default:
                        return;

                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void ClacPowerWhenCollision(float3 pushout, float radius,
                 PointRead* pReadPointA, PointRead* pReadPointB,
                PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,
                float WeightProportion, float lengthPropotion, float oneDivideIteration,
                CollideFunc collideFunc)
            {
                float sqrPushout = math.lengthsq(pushout);
                /*                if (sqrPushout==0) //加了一个随机的微弱力简单解决这个问题
                                {
                                    return;
                                }*/
                switch (collideFunc)
                {
                    //OYM：整片代码里面最有趣的一块
                    //OYM：反正我现在不想回忆当时怎么想的了XD
                    case CollideFunc.Freeze:
                        break;//OYM：猜猜为啥这样写
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
                        pReadWritePointA->deltaPosition *= (1 - pReadPointA->friction);//OYM:增加摩擦力,同时避免摩擦力过大

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
                    pReadWritePointB->deltaPosition *= (1 - pReadPointB->friction);//OYM:增加摩擦力,同时避免摩擦力过大

                    pReadWritePointB->position += (pushout * lengthPropotion);

                }

            }
            //OYM：https://zalo.github.io/blog/closest-point-between-segments/#line-segments
            //OYM：目前是我见过最快的方法

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float SqrComputeNearestPoints(
                float3 posP,//OYM：碰撞体的位置起点位置
                float3 dirP,//OYM：碰撞体的朝向
                float3 posQ,//OYM：约束的起点坐标
                float3 dirQ,//OYM：约束的起点朝向
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
        public struct ConstraintForceUpdateByPoint : IJobParallelFor //OYM:一个能避免粒子过于颤抖的 ConstraintForceUpdate,但是移除了 Constraint碰撞计算
        {
            /// <summary>
            /// 指向所有可读的点
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            /// <summary>
            /// 指向所有可读写的点
            /// </summary>);
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            /// <summary>
            /// 所有杆件
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public NativeMultiHashMap<int, ConstraintRead> constraintsRead;
            [ReadOnly]
            internal float oneDivideIteration;
            public void Execute(int index)
            {
                PointRead* pPointReadA = pReadPoints + index;
                if (pPointReadA->parentIndex < 0)//OYM:fixed节点不考虑受力
                {
                    return;
                }

                NativeMultiHashMapIterator<int> iterator;
                ConstraintRead constraint;
                float3 move = float3.zero;

                if (!constraintsRead.TryGetFirstValue(index, out constraint, out iterator)) //OYM:  在这里获取约束与迭代器
                {
                    return;
                }
                PointReadWrite* pReadWritePointA = pReadWritePoints + index;
                float3 positionA = pReadWritePointA->position;

                int count = 0;
                do
                {
                    count++;
                    //OYM：获取约束的节点AB
                    PointRead* pPointReadB = pReadPoints + constraint.indexB;
                    //OYM：任意一点都不能小于极小值
                    //OYM：if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                    //OYM：获取可读写的点B
                    PointReadWrite* pReadWritePointB = pReadWritePoints + constraint.indexB;

                    float3 positionB = pReadWritePointB->position;
                    //OYM：获取约束的朝向
                    var Direction = positionB - positionA;
                    if (math.all(Direction == 0))//OYM:所有的值都为0
                    {
                        continue;
                    }

                    float Distance = math.length(Direction);

                    //OYM：力度等于距离减去长度除以弹性，这个值可以不存在，可以大于1但是没有什么卵用

                    float originDistance = constraint.length;
                    float Force = Distance - math.clamp(Distance, originDistance * constraint.shrink, originDistance * constraint.stretch);
                    if (Force != 0)
                    {
                        bool IsShrink = Force >= 0.0f;
                        float ConstraintPower;//OYM：这个值等于
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

                    //OYM：获取AB点重量比值的比值,由于重量越大移动越慢,所以A的值实际上是B的重量的比



                } while (constraintsRead.TryGetNextValue(out constraint, ref iterator));
                if (count != 0)
                {
                    pReadWritePointA->deltaPosition += move / count;
                    pReadWritePointA->position += move / count;
                }
            }
        }
        [BurstCompile]
        public struct JobPointToTransform : IJobParallelForTransform
        //OYM：把job的点转换成实际的点
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
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;//OYM：获取每个读写点
                PointRead* pReadPoint = pReadPoints + index;//OYM：获取每个只读点

                float3 writePosition = pReadWritePoint->position * worldScale;
                if (pReadPoint->parentIndex != -1)//OYM：不是fix点
                {
                    transform.position = writePosition;
                }
                //OYM:  旋转节点
                //OYM:  这里有个bug,当初考虑的时候是存在多个子节点的,但是实际上并没有


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
                        FromDirection += math.normalize(math.mul((quaternion)transform.rotation, targetChildRead->initialLocalPosition));//OYM：将BoneAxis按照transform.rotation进行旋转
                        ToDirection += math.normalize(targetChild->position * worldScale - math.lerp(transform.position, writePosition, startDampTime));//OYM：朝向等于面向子节点的方向

                    }

                    Quaternion AimRotation = FromToRotation(FromDirection, ToDirection);
                    transform.rotation = AimRotation * transform.rotation;
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
                else if (math.abs(1.0f - cos) < 1e-06f)
                {
                    //angle = 0.0f;
                    //axis = new float3(1, 0, 0);
                    return quaternion.identity;
                }
                return quaternion.AxisAngle(math.normalize(axis), angle * t);
            }
        }
        #endregion
    }
}




/*        [BurstCompile]
        public struct ColliderGetTransform : IJobParallelForTransform
        //OYM：获取collider的deltaPostion
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            public float oneDivideIteration;
            [ReadOnly]
            public float worldScale;
            public void Execute(int index, TransformAccess transform)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;
                float colliderScale = pReadCollider->isConnectWithBody ? worldScale : 1;

                MinMaxAABB AABB;
                float3 currentPosition = (float3)transform.position + math.mul((quaternion)transform.rotation, pReadCollider->positionOffset);
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere://OYM:包含上一帧的位置与这一帧的位置的球体的AABB

                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        AABB = new MinMaxAABB(currentPosition, pReadWriteCollider->position);
                        AABB.Expand(pReadCollider->radius * colliderScale);
                        break;
                    case ColliderType.Capsule://OYM:包含上一帧的位置与这一帧的位置的胶囊体的AABB
                        //OYM:这儿有点难,需要先判断两个AABB,然后形成一个更大的
                        float3 currentDirection = math.mul((quaternion)transform.rotation, pReadCollider->staticDirection);
                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        pReadWriteCollider->deltaDirection = oneDivideIteration * (currentDirection - pReadWriteCollider->direction);

                        MinMaxAABB temp1 = new MinMaxAABB(currentPosition, pReadWriteCollider->position); //OYM:起点形成的AABB
                        MinMaxAABB temp2 = new MinMaxAABB(currentPosition + currentDirection * pReadCollider->length, pReadWriteCollider->position + pReadWriteCollider->direction * pReadCollider->length); //OYM:终点形成的AABB
                        AABB = new MinMaxAABB(temp1, temp2);
                        AABB.Expand(pReadCollider->radius * colliderScale);

                        break;
                    case ColliderType.OBB://OYM:还好它有内置的旋转函数,否则不太好写
                        quaternion currentRotation = (transform.rotation * pReadCollider->staticRotation);
                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        pReadWriteCollider->deltaRotation = math.nlerp(quaternion.identity, math.mul(currentRotation, math.inverse(pReadWriteCollider->rotation)), oneDivideIteration);

                        MinMaxAABB temp3 = MinMaxAABB.CreateFromCenterAndHalfExtents(currentPosition, pReadCollider->boxSize * colliderScale); //OYM:创建一个与OBB大小一致的AABB
                        MinMaxAABB temp4 = MinMaxAABB.Rotate(currentRotation, temp3);//OYM:旋转它到OBB的位置,得到一个更大的AABB
                        temp3 = MinMaxAABB.Rotate(pReadWriteCollider->rotation, temp3); //OYM:重复利用一下temp3,得到上一次位置的AABB
                        AABB = new MinMaxAABB(temp3, temp4);//OYM:扩大包围盒

                        break;
                    default:
                        AABB = new MinMaxAABB();
                        break;
                }
                pReadWriteCollider->AABB = AABB;
            }
        }*/