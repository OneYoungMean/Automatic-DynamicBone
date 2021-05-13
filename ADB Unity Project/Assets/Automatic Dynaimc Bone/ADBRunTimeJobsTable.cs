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

namespace ADBRuntime.Internal
{
    public unsafe class ADBRunTimeJobsTable
    {
        private const float EPSILON = 0.001f;
        private const float SQRT_2 = 1.41421356f;

        #region Jobs
        /// <summary>
        /// 初始化所有点的位置
        /// </summary>
        [BurstCompile]
        public struct InitiralizePoint : IJobParallelForTransform

        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }
            void Execute(int index, Transform transform)
            {
#endif

                var pReadWritePoint = pReadWritePoints + index;
                var pReadPoint = pReadPoints + index;


                if (pReadPoint->fixedIndex == index)
                {
                    //Debug.Log(pReadPoint->localRotation==quaternion.Inverse(transform.localRotation));这里没问题
                    transform.localRotation = pReadPoint->initialLocalRotation;//OYM：这里改变之后,rotation也会改变

                    pReadWritePoint->rotation = transform.rotation;
                    pReadWritePoint->rotationY = pReadPoint->initialRotation;
                    //Debug.Log(pReadWritePoint->rotation+" "+index);
                    pReadWritePoint->position = transform.position;

                    pReadWritePoint->deltaRotationY = pReadWritePoint->deltaRotation = quaternion.identity;
                    pReadWritePoint->deltaPosition = float3.zero;

                }
                else
                {
                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    var pFixReadPoint = pReadPoints + (pReadPoint->fixedIndex);
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    pReadWritePoint->position = (float3)pFixReadWritePoint->position + math.mul(pFixReadWritePoint->rotation, pReadPoint->initialPosition);

                    transform.position = pReadWritePoint->position;
                    pReadWritePoint->deltaPosition = float3.zero;
                }
            }
        }
        /// <summary>
        /// 初始化所有的colldier
        /// </summary>
        [BurstCompile]
        public struct InitiralizeCollider : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }
            public void Execute(int index, Transform transform)
            {
#endif
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                pReadWriteCollider->position = transform.position + transform.rotation * pReadCollider->positionOffset;
                pReadWriteCollider->direction = transform.rotation * pReadCollider->staticDirection;
                pReadWriteCollider->rotation = transform.rotation * pReadCollider->staticRotation;
                pReadWriteCollider->deltaPosition = float3.zero;
                pReadWriteCollider->deltaDirection = float3.zero;
                pReadWriteCollider->deltaRotation = quaternion.identity;
            }
        }

        [BurstCompile]
        public struct ColliderGetTransform : IJobParallelForTransform
        //OYM：获取collider的deltaPostion
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            public float oneDivideIteration;
            public void Execute(int index, TransformAccess transform)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;
                MinMaxAABB AABB;
                float3 currentPosition = (float3)transform.position + math.mul((quaternion)transform.rotation, pReadCollider->positionOffset);
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:

                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        AABB = new MinMaxAABB(currentPosition, pReadWriteCollider->position);
                        AABB.Expand(pReadCollider->radius);
                        break;
                    case ColliderType.Capsule:
                        float3 currentDirection = math.mul((quaternion)transform.rotation, pReadCollider->staticDirection);
                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        pReadWriteCollider->deltaDirection = oneDivideIteration * (currentDirection - pReadWriteCollider->direction);

                        MinMaxAABB temp1 = new MinMaxAABB(currentPosition, pReadWriteCollider->position); //OYM:起点形成的AABB
                        MinMaxAABB temp2 = new MinMaxAABB(currentPosition + currentDirection * pReadCollider->length, pReadWriteCollider->position + pReadWriteCollider->direction * pReadCollider->length); //OYM:终点形成的AABB
                        AABB = new MinMaxAABB(temp1, temp2);
                        AABB.Expand(pReadCollider->radius);

                        break;
                    case ColliderType.OBB:
                        quaternion currentRotation = (transform.rotation * pReadCollider->staticRotation);
                        pReadWriteCollider->deltaPosition = oneDivideIteration * (currentPosition - pReadWriteCollider->position);
                        pReadWriteCollider->deltaRotation = math.nlerp(quaternion.identity, math.mul(currentRotation, math.inverse(pReadWriteCollider->rotation)), oneDivideIteration);

                        MinMaxAABB temp3 = MinMaxAABB.CreateFromCenterAndHalfExtents(currentPosition, pReadCollider->boxSize);
                        MinMaxAABB temp4 = MinMaxAABB.Rotate(currentRotation, temp3);
                        temp3 = MinMaxAABB.Rotate(pReadWriteCollider->rotation, temp3);
                        AABB = new MinMaxAABB(temp3, temp4);

                        break;
                    default:
                        AABB = new MinMaxAABB();
                        break;
                }
                pReadWriteCollider->AABB = AABB;
            }
        }
        [BurstCompile]
        public struct ColliderUpdate : IJobParallelFor
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            public void TryExecute(int index, int _, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
            public void Execute(int index)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->position += pReadWriteCollider->deltaPosition;
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->position += pReadWriteCollider->deltaPosition;
                        pReadWriteCollider->direction += pReadWriteCollider->deltaDirection;
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->position += pReadWriteCollider->deltaPosition;
                        pReadWriteCollider->rotation = math.mul(pReadWriteCollider->deltaRotation, pReadWriteCollider->rotation);
                        break;
                    default:
                        break;

                }
            }
        }

        /// <summary>
        /// 获取点的位置,同时处理速度上的一些调整
        /// </summary>
        [BurstCompile]
        public struct PointGetTransform : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float oneDivideIteration;
            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }

            public void Execute(int index, Transform transform)//OYM：注意,这里只获取delta,不获取真实的坐标
            {
#endif

                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;

                if (pReadPoint->fixedIndex == index)//OYM：fixedpoint
                {
                    pReadWritePoint->deltaPosition = oneDivideIteration * ((float3)transform.position - pReadWritePoint->position);


                    //OYM：做笔记 unity当中 child.rotation =parent.rotation*child.localrotation;
                    quaternion rotationTemp = (transform.rotation * math.inverse(transform.localRotation)) * pReadPoint->initialLocalRotation;
                    pReadWritePoint->deltaRotation = math.nlerp(quaternion.identity,math.mul( rotationTemp , math.inverse(pReadWritePoint->rotation)), oneDivideIteration);
                    Quaternion q = (pReadWritePoint->deltaRotation);
                     pReadWritePoint->deltaRotationY = quaternion.AxisAngle(new float3(0,1,0),q.eulerAngles.y);

                }
                else
                {
                    pReadWritePoint->deltaPosition *= (0.8f + pReadPoint->mass);
                }
            }
        }

        [BurstCompile]
        public struct PointUpdate : IJobParallelFor
        {
            const float gravityLimit = 0.1f;
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
            /// 所有碰撞体坐标的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 所有碰撞体的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
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
            /// 大小
            /// </summary>
            [ReadOnly]
            internal float globalScale;
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
#if ADB_DEBUG
            public void TryExecute(int index, int _, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
#endif
            public void Execute(int index)
            {
                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                if (pReadPoint->fixedIndex != index)
                {
                    EvaluatePosition(index, pReadPoint, pReadWritePoint);
                    if (isCollision)
                    {
                        for (int i = 0; i < colliderCount; ++i)
                        {
                            ColliderRead* pReadCollider = pReadColliders + i;
                            ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;

                            if (pReadCollider->isOpen && (pReadPoint->colliderChoice & pReadCollider->colliderChoice) != 0)
                            {
                                ColliderCheck(pReadPoint, pReadWritePoint, pReadCollider, pReadWriteCollider);
                            }
                        }
                    }
                }
                else
                {
                    //OYM：计算渐进的fixed点坐标
                    pReadWritePoint->position += pReadWritePoint->deltaPosition;
                    pReadWritePoint->rotationY = math.mul(pReadWritePoint->deltaRotationY, pReadWritePoint->rotationY);
                    pReadWritePoint->rotation = math.mul(pReadWritePoint->deltaRotation, pReadWritePoint->rotation);
                }

            }
            private void EvaluatePosition(int index, PointRead* pReadPoint, PointReadWrite* pReadWritePoint)
            {
                //OYM：如果你想要添加什么奇怪的力的话,可以在这底下添加

                float3 deltaPosition = pReadWritePoint->deltaPosition;
                var temp = deltaPosition;
                //OYM：获取固定点的信息
                PointReadWrite* pFixedPointReadWrite = (pReadWritePoints + pReadPoint->fixedIndex);
                PointRead* pFixedPointRead = (pReadPoints + pReadPoint->fixedIndex);
                //OYM：获取以fixed位移进行为参考进行距离补偿(这里的delta已经乘以过onedivideItertation了)

                pReadWritePoint->position += pFixedPointReadWrite->deltaPosition * pReadPoint->distanceCompensation;


                //OYM：获取当前相对fixed的向量
                float3 direction = pReadWritePoint->position - pFixedPointReadWrite->position;
                float3 gravity = float3.zero;
                //OYM：获取归位的向量
                if (pReadPoint->isFixGravityAxis)//OYM:  固定重力方向,这个值会受到fix节点初始rotation的影响
                {
                    gravity = math.mul(math.mul(pFixedPointReadWrite->rotation , math.inverse(pFixedPointRead->initialRotation)) , pReadPoint->gravity) * (deltaTime * deltaTime) * globalScale;//OYM：重力
                }
                else
                {
                    gravity = pReadPoint->gravity * (deltaTime * deltaTime) * globalScale;//OYM：重力
                }

                deltaPosition += gravity * oneDivideIteration;

                float3 addForce = oneDivideIteration * addForcePower * pReadPoint->addForceScale / pReadPoint->weight;
                deltaPosition += GetRotateForce(addForce, direction) + addForce;
                //OYM：计算回到原始位置的力,当freeze很小的时候设置上限,很大的时候乘以系数
                float3 back = float3.zero;
                if (pReadPoint->freeze > EPSILON)
                {
                    if (pReadPoint->isFixGravityAxis)//OYM:  固定重力方向,这个值会受到fix节点初始rotation的影响
                    {
                        back = math.mul(pFixedPointReadWrite->rotation, pReadPoint->initialPosition) * globalScale - direction;
                    }
                    else
                    {
                        back = math.mul(pFixedPointReadWrite->rotationY, pReadPoint->initialPosition) * globalScale - direction;
                    }
                    back = oneDivideIteration * deltaTime * pReadPoint->freeze * math.clamp(back, -pReadPoint->freeze * 0.1f, pReadPoint->freeze * 0.1f);
                }
                deltaPosition += back;

                //OYM：计算以fixed位移进行为参考进行速度补偿
                deltaPosition -= pFixedPointReadWrite->deltaPosition * pReadPoint->moveByFixedPoint * 0.2f;//OYM：测试了一下,0.2是个恰到好处的值,不会显得太大也不会太小

                //OYM：计算离心力(理想状态
                deltaPosition += deltaTime * (math.mul(pFixedPointReadWrite->deltaRotation, direction) - direction);

                //OYM：传回
                pReadWritePoint->deltaPosition = deltaPosition; //OYM:   
                if (isOptimize)
                {
                    //OYM：减少对于骨骼的拉长
                    deltaPosition -= (math.dot(deltaPosition, direction) / math.lengthsq(direction)) * direction * 0.1f;
                    //OYM:  注意,这个操作会让你的骨骼看上去更加的卷,更加的带有动漫风
                    deltaPosition += GetRotateForce(deltaPosition *0.015f, direction) ;


                }

                pReadWritePoint->position += oneDivideIteration * deltaPosition;//OYM：这里我想了很久,应该是这样,如果是迭代n次的话,那么deltaposition将会被加上n次,正规应该是只加一次


                // if (index == 19) {}

                //Debug.Log(index + " : " + pFixedPointReadWrite->position + " " + positionA + " " + pFixedPointReadWrite->deltaPosition * pReadPoint->distanceCompensation + "  " + positionB);
            }
            float3 GetRotateForce(float3 force, float3 direction)//OYM：返回一个不存在的力,使得其向受力方向卷曲,在一些动漫里面会经常出现这种曲线的头发
            {
                var result = math.mul(quaternion.Euler( Mathf.Rad2Deg* force.z, 0, Mathf.Rad2Deg * force.x), direction) - direction;
                return new float3( result.x,0,result.z);
            }
            private void ColliderCheck(PointRead* pPointRead, PointReadWrite* pReadWritePoint, ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider)
            {
                float3 pushout;
                float radiusSum;
                float colliderScale = pReadCollider->isConnectWithBody ? globalScale : 1;
                float pointRadius = pPointRead->radius * colliderScale;
                bool isColliderInsideMode =! (pReadCollider->collideFunc == CollideFunc.OutsideLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit); //OYM:用于判断是否需要翻转AABB结果
                //OYM:判断AABB
                MinMaxAABB AABB = pReadWriteCollider->AABB;
                AABB.Expand(pointRadius);
                if (!AABB.Contains(pReadWritePoint->position)^ isColliderInsideMode)
                {
                    return;
                }
                //OYM:AABB判断在内
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere: //OYM:球体
                        radiusSum = pReadCollider->radius * colliderScale+ pointRadius;
                        pushout = pReadWritePoint->position - pReadWriteCollider->position;
                        DistributionPower(pushout, radiusSum, pReadWritePoint, pReadCollider->collideFunc);

                        break;

                    case ColliderType.Capsule: //OYM:胶囊体
                        radiusSum =pointRadius+ pReadCollider->radius * colliderScale;

                        pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, pReadWriteCollider->position, pReadWriteCollider->direction * pReadCollider->length * colliderScale);
                        DistributionPower(pushout, radiusSum, pReadWritePoint, pReadCollider->collideFunc);

                        break;
                    case ColliderType.OBB: //OYM:OBB

                        var localPosition =math.mul( math.inverse(pReadWriteCollider->rotation) ,(pReadWritePoint->position - pReadWriteCollider->position)); //OYM:获取localPosition
                        MinMaxAABB localOBB = MinMaxAABB.CreateFromCenterAndHalfExtents(0, colliderScale * pReadCollider->boxSize + pointRadius);

                        if (localOBB.Contains(localPosition))
                        {
                            float3 toMax = localOBB.Max - localPosition;
                            float3 toMin = localOBB.Min - localPosition;

                            float3 min3 = new float3
                                (
                                math.abs(toMax.x) < math.abs(toMin.x) ? toMax.x : toMin.x,
                                math.abs(toMax.y) < math.abs(toMin.y) ? toMax.y : toMin.y,
                                math.abs(toMax.z) < math.abs(toMin.z) ? toMax.z : toMin.z
                                ) ;
                            float3 min3Abs = math.abs(min3);
                            if (min3Abs.x<= min3Abs.y&& min3Abs.x <= min3Abs.z)
                            {
                                pushout = new float3(min3.x, 0, 0);
                            }
                            else if (min3Abs.y <= min3Abs.x && min3Abs.y<= min3Abs.z)
                            {
                                pushout = new float3(0, min3.y, 0);
                            }
                            else
                            {
                                pushout = new float3(0, 0 , min3.z);
                            }
                            pushout =math.mul( pReadWriteCollider->rotation, pushout);

                            if (pReadCollider->collideFunc == CollideFunc.InsideNoLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit)
                            {
                                pReadWritePoint->deltaPosition += 0.001f * oneDivideIteration * pushout;
                            }
                            else
                            {
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }

                        }
                        break;
                    default:
                        return;
                }
            }
            void DistributionPower(float3 pushout, float radius, PointReadWrite* pReadWritePoint, CollideFunc collideFunc)
            {
                float sqrPushout = math.lengthsq(pushout);
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

                if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                {
                     pReadWritePoint->deltaPosition += 0.001f * oneDivideIteration * pushout;
                }
                else
                {
                    pReadWritePoint->position += pushout;
                    pReadWritePoint->deltaPosition += pushout;
                }
              

            }
            
                float3 ConstrainToSegment(float3 tag, float3 pos, float3 dir)
            {
                float t = math.dot(tag - pos, dir) / math.lengthsq(dir);
                return pos + dir * math.clamp(t, 0, 1);
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
            /// 所有可读的碰撞体
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            /// <summary>
            /// 所有读写碰撞体
            /// </summary>
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
            public float globalScale;
            [ReadOnly]
            public int globalColliderCount;
            [ReadOnly]
            public bool isCollision;
            [ReadOnly]
            internal float oneDivideIteration;
#if ADB_DEBUG
            public void TryExecute(int index, int temp, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
#endif
            public void Execute(int index)
            {
                //OYM：获取约束
                ConstraintRead* constraint = pConstraintsRead + index;

                //OYM：获取约束的节点AB
                PointRead* pPointReadA = pReadPoints + constraint->indexA;
                PointRead* pPointReadB = pReadPoints + constraint->indexB;

                //OYM：任意一点都不能小于极小值
                //OYM：if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                //OYM：获取可读写的点A
                PointReadWrite* pReadWritePointA = pReadWritePoints + constraint->indexA;

                //OYM：获取可读写的点B
                PointReadWrite* pReadWritePointB = pReadWritePoints + constraint->indexB;
                //OYM：获取约束的朝向
                var Direction = pReadWritePointB->position - pReadWritePointA->position;


                float Distance = math.length(Direction);
                //OYM：力度等于距离减去长度除以弹性，这个值可以不存在，可以大于1但是没有什么卵用
                float Force = Distance - constraint->length * globalScale;
                //OYM：是否收缩，意味着力大于0
                bool IsShrink = Force >= 0.0f;
                float ConstraintPower;//OYM：这个值等于
                switch (constraint->type)
                //OYM：这下面都是一个意思，就是确认约束受到的力，然后根据这个获取杆件约束的属性，计算 ConstraintPower
                //OYM：Shrink为杆件全局值，另外两个值为线性插值获取的值，同理Stretch，所以这里大概可以猜中只是一个简单的不大于1的值
                {
                    case ConstraintType.Structural_Vertical:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->structuralShrinkVertical + pPointReadB->structuralShrinkVertical)
                            : constraint->stretch * (pPointReadA->structuralStretchVertical + pPointReadB->structuralStretchVertical);
                        break;
                    case ConstraintType.Structural_Horizontal:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->structuralShrinkHorizontal + pPointReadB->structuralShrinkHorizontal)
                            : constraint->stretch * (pPointReadA->structuralStretchHorizontal + pPointReadB->structuralStretchHorizontal);
                        break;
                    case ConstraintType.Shear:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->shearShrink + pPointReadB->shearShrink)
                            : constraint->stretch * (pPointReadA->shearStretch + pPointReadB->shearStretch);
                        break;
                    case ConstraintType.Bending_Vertical:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->bendingShrinkVertical + pPointReadB->bendingShrinkVertical)
                            : constraint->stretch * (pPointReadA->bendingStretchVertical + pPointReadB->bendingStretchVertical);
                        break;
                    case ConstraintType.Bending_Horizontal:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->bendingShrinkHorizontal + pPointReadB->bendingShrinkHorizontal)
                            : constraint->stretch * (pPointReadA->bendingStretchHorizontal + pPointReadB->bendingStretchHorizontal);
                        break;
                    case ConstraintType.Circumference:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->circumferenceShrink + pPointReadB->circumferenceShrink)
                            : constraint->stretch * (pPointReadA->circumferenceStretch + pPointReadB->circumferenceStretch);
                        break;
                    default:
                        ConstraintPower = 0.0f;
                        break;
                }


                //OYM：获取AB点重量比值的比值,由于重量越大移动越慢,所以A的值实际上是B的重量的比

                float WeightProportion = pPointReadB->weight / (pPointReadA->weight + pPointReadB->weight);

                if (ConstraintPower > 0.0f)//OYM：这里不可能小于0吧（除非有人搞破坏）
                {
                    float3 Displacement = math.normalize(Direction) * (Force * ConstraintPower);

                    pReadWritePointA->position += Displacement * WeightProportion * oneDivideIteration;
                    pReadWritePointA->deltaPosition += Displacement * WeightProportion * oneDivideIteration;
                    pReadWritePointB->position += -Displacement * (1 - WeightProportion) * oneDivideIteration;
                    pReadWritePointB->deltaPosition += -Displacement * (1 - WeightProportion) * oneDivideIteration;
                }

                if (isCollision && constraint->isCollider)
                {
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        ColliderRead* pReadCollider = pReadColliders + i;//OYM：终于到碰撞这里了

                        if (pReadCollider->isOpen && (pPointReadA->colliderChoice & pReadCollider->colliderChoice) != 0)
                        {//OYM：collider是否打开,且pPointReadA->colliderChoice是否包含 pReadCollider->colliderChoice的位
                            ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;
                            ComputeCollider(
                                pReadCollider, pReadWriteCollider,
                                pReadWritePointA, pReadWritePointB,
                                constraint,
                                WeightProportion,
                                pPointReadA->friction, pPointReadB->friction
                                );
                        }
                    }
                }
            }
            private void ComputeCollider(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, ConstraintRead* constraint, float WeightProportion,
                float frictionA, float frictionB)
            {
                float throwTemp;//OYM:丢掉的数据
                float t, radius;
                float scale = pReadCollider->isConnectWithBody ? globalScale : 1;

                bool isColliderInsideMode = !(pReadCollider->collideFunc == CollideFunc.OutsideLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit); //OYM:用于判断是否需要翻转AABB结果

                MinMaxAABB colliderAABB = pReadWriteCollider->AABB;
                MinMaxAABB constraintAABB = new MinMaxAABB(pReadWritePointA->position, pReadWritePointB->position);
                constraintAABB.Expand(constraint->radius);

                if (!colliderAABB.Overlaps(constraintAABB) ^ isColliderInsideMode) //OYM:overlap为假且isColliderInsideMode为真或者overlap为真且isColliderInsideMode为假
                {
                    return;
                }
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            radius = scale * pReadCollider->radius + globalScale * constraint->radius;

                            {
                                float3 pointOnLine = ConstrainToSegment(pReadWriteCollider->position, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out t);
                                DistributionPower(pointOnLine - pReadWriteCollider->position, radius, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.Capsule:
                        {
                            radius = scale * pReadCollider->radius + globalScale * constraint->radius;

                            {
                                float3 pointOnCollider, pointOnLine;
                                SqrComputeNearestPoints(pReadWriteCollider->position, pReadWriteCollider->direction * scale, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out throwTemp, out t, out pointOnCollider, out pointOnLine);
                                DistributionPower(pointOnLine - pointOnCollider, radius, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.OBB:
                        {

                            {
                                float3 boxSize = scale * pReadCollider->boxSize + new float3(globalScale * constraint->radius, globalScale * constraint->radius, globalScale * constraint->radius);
                                float t1, t2;
                                //OYM：这个方法可以求出直线与obbbox的两个交点
                                SegmentToOBB(pReadWritePointA->position, pReadWritePointB->position, pReadWriteCollider->position, boxSize, math.inverse(pReadWriteCollider->rotation), out t1, out t2);

                                t1 = Clamp01(t1);
                                t2 = Clamp01(t2);
                                //OYM：如果存在,那么t2>t1,且至少有一个点不在边界上
                                bool bHit = t1 >= 0f && t2 > t1 && t2 <= 1.0f;
                                if (bHit)
                                {
                                    //OYM：这里不是取最近的点,而是取中点,最近的点效果并不理想
                                    t = (t1 + t2) * 0.5f;
                                    float3 dir = pReadWritePointB->position - pReadWritePointA->position;
                                    float3 nearestPoint = pReadWritePointA->position + dir * t;
                                    float3 pushout =math.mul( math.inverse(pReadWriteCollider->rotation) ,(nearestPoint - pReadWriteCollider->position));
                                    float pushoutX = pushout.x > 0 ? boxSize.x - pushout.x : -boxSize.x - pushout.x;
                                    float pushoutY = pushout.y > 0 ? boxSize.y - pushout.y : -boxSize.y - pushout.y;
                                    float pushoutZ = pushout.z > 0 ? boxSize.z - pushout.z : -boxSize.z - pushout.z;
                                    //OYM：这里我自己都不太记得了 XD
                                    //OYM：这里是选推出点离的最近的位置,然后推出
                                    //OYM：Abs(pushoutZ) < Abs(pushoutY)是错的 ,可能会出现两者都为0的情况
                                    if (Abs(pushoutZ) <= Abs(pushoutY) && Abs(pushoutZ) <= Abs(pushoutX))
                                    {
                                        pushout = math.mul(pReadWriteCollider->rotation, new float3(0, 0, pushoutZ));

                                    }
                                    else if (Abs(pushoutY) <= Abs(pushoutX) && Abs(pushoutY) <= Abs(pushoutZ))
                                    {
                                        pushout = math.mul(pReadWriteCollider->rotation, new float3(0, pushoutY, 0));
                                    }
                                    else
                                    {
                                        pushout = math.mul(pReadWriteCollider->rotation, new float3(pushoutX, 0, 0));
                                    }
                                    if (math.lengthsq(pushout) != 0)
                                    {
                                        //float inverse1Velocity = float3.Dot(pushout, pReadWritePointA->velocity) / pushout.sqrMagnitude;
                                        //pReadWritePointA->velocity -= pushout * inverse1Velocity;
                                        //pReadWritePointB->velocity -= pushout * inverse1Velocity;
                                        pReadWritePointA->deltaPosition *= (1 - frictionA);
                                        pReadWritePointB->deltaPosition *= (1 - frictionB);

                                        //float Propotion = WeightProportion * t / (1 - WeightProportion - t + 2 * WeightProportion * t);
                                        if (WeightProportion > EPSILON)
                                        {
                                            if (pReadCollider->collideFunc == CollideFunc.InsideNoLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit)
                                            {
                                                pReadWritePointA->deltaPosition += 0.01f * oneDivideIteration * (pushout * (1 - t));
                                            }
                                            else
                                            {
                                                pReadWritePointA->position += (pushout * (1 - t));
                                                pReadWritePointA->deltaPosition += (pushout * (1 - t));
                                            }

                                        }
                                        else
                                        {
                                            t = 1;
                                        }
                                        if (pReadCollider->collideFunc == CollideFunc.InsideNoLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit)
                                        {
                                            pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * (pushout * t);
                                        }
                                        else
                                        {
                                            pReadWritePointB->position += (pushout * t);
                                            pReadWritePointB->deltaPosition += (pushout * t);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        return;

                }
            }

            void DistributionPower(float3 pushout, float radius, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float WeightProportion, float lengthPropotion, float frictionA, float frictionB, CollideFunc collideFunc)
            {

                float sqrPushout = math.lengthsq(pushout);
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
                //OYM：把pushout方向多余的力给减掉
                //OYM：没有也不需要
                // pReadWritePointA->velocity -= pushout * (float3.Dot(pushout, pReadWritePointA->velocity) / sqrPushout);
                //pReadWritePointB->velocity -= pushout * (float3.Dot(pushout, pReadWritePointB->velocity) / sqrPushout);
                pReadWritePointA->deltaPosition *= (1 - frictionA);
                pReadWritePointB->deltaPosition *= (1 - frictionB);

                pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);//OYM：这里简单解释一下,首先我要计算的是推出的距离,及半径长度减去原始的pushout度之后剩下的值,即pushout/pushout.magnitude*radius-pushout.即pushout*((radius/magnitude -1));

                //  float Propotion = WeightProportion * lengthPropotion / (1 - WeightProportion - lengthPropotion + 2 * WeightProportion * lengthPropotion);

                if (WeightProportion > EPSILON)
                {
                    if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                    {
                        pReadWritePointA->deltaPosition += 0.001f * oneDivideIteration * (1 - lengthPropotion) * pushout;
                    }
                    else
                    {
                        pReadWritePointA->position += (pushout * (1 - lengthPropotion));
                        pReadWritePointA->deltaPosition += (pushout * (1 - lengthPropotion));
                    }

                }
                else
                {
                    lengthPropotion = 1;
                }

                if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                {
                    pReadWritePointB->deltaPosition += 0.001f * oneDivideIteration * (lengthPropotion) * pushout;
                }
                else
                {
                    pReadWritePointB->position += (pushout * lengthPropotion);
                    pReadWritePointB->deltaPosition += (pushout * lengthPropotion);
                }

            }
            //OYM：https://zalo.github.io/blog/closest-point-between-segments/#line-segments
            //OYM：目前是我见过最快的方法
            float SqrComputeNearestPoints(
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
                float3 L1ToL2Line = posP + dirP * Clamp01(t1);

                pointOnQ = ConstrainToSegment(L1ToL2Line, posQ, dirQ, out tQ);
                pointOnP = ConstrainToSegment(pointOnQ, posP, dirP, out tP);
                return math.lengthsq(pointOnP - pointOnQ);
            }

            float3 ConstrainToSegment(float3 tag, float3 pos, float3 dir, out float t)
            {
                t = math.dot(tag - pos, dir) / math.lengthsq(dir);
                t = Clamp01(t);
                return pos + dir * t;
            }
            void SegmentToOBB(float3 start, float3 end, float3 center, float3 size, quaternion InverseNormal, out float t1, out float t2)
            {
                float3 startP =math.mul( InverseNormal , (center - start));
                float3 endP = math.mul(InverseNormal , (center - end));
                SegmentToAABB(startP, endP, center, -size, size, out t1, out t2);
            }

            void SegmentToAABB(float3 start, float3 end, float3 center, float3 min, float3 max, out float t1, out float t2)
            {
                float3 dir = end - start;
                t1 = Max(
                                Min(
                                    (min.x - start.x) / dir.x,
                                    (max.x - start.x) / dir.x),
                                Min(
                                    (min.y - start.y) / dir.y,
                                    (max.y - start.y) / dir.y),
                                Min(
                                    (min.z - start.z) / dir.z,
                                    (max.z - start.z) / dir.z));
                t2 = Min(
                                Max(
                                    (min.x - start.x) / dir.x,
                                    (max.x - start.x) / dir.x),
                                Max(
                                    (min.y - start.y) / dir.y,
                                    (max.y - start.y) / dir.y),
                                Max(
                                    (min.z - start.z) / dir.z,
                                    (max.z - start.z) / dir.z));
            }
            float Abs(float A)
            {
                return A > 0 ? A : -A;
            }
            float Clamp01(float A)
            {
                return A > 0 ? (A < 1 ? A : 1) : 0;
            }
            float Min(float A, float B, float C)
            {
                return A < B ? (A < C ? A : C) : (B < C ? B : C);
            }
            float Min(float A, float B)
            {
                return A > B ? B : A;
            }
            float Max(float A, float B, float C)
            {
                return A > B ? (A > C ? A : C) : (B > C ? B : C);
            }
            float Max(float A, float B)
            {
                return A > B ? A : B;
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
            public float globalScale;
            [ReadOnly]
            internal float oneDivideIteration;
#if! ADB_DEBUG
            public JobHandle TryExecute(int index, int temp, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
                return job;
            }
#endif
            public void Execute(int index)
            {
                PointRead* pPointReadA = pReadPoints + index;
                if (pPointReadA->parentIndex == -1)
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
                int count = 0;
                do
                {
                    count++;
                    //OYM：获取约束的节点AB
                    PointRead* pPointReadB = pReadPoints + constraint.indexB;

                    //OYM：任意一点都不能小于极小值
                    //OYM：if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                    //OYM：获取可读写的点A


                    //OYM：获取可读写的点B
                    PointReadWrite* pReadWritePointB = pReadWritePoints + constraint.indexB;
                    //OYM：获取约束的朝向
                    var Direction = pReadWritePointB->position - (pReadWritePointA->position);


                    float Distance = math.length(Direction);
                    //OYM：力度等于距离减去长度除以弹性，这个值可以不存在，可以大于1但是没有什么卵用
                    float Force = Distance - constraint.length * globalScale;
                    //OYM：是否收缩，意味着力大于0
                    bool IsShrink = Force >= 0.0f;
                    float ConstraintPower;//OYM：这个值等于
                    switch (constraint.type)
                    //OYM：这下面都是一个意思，就是确认约束受到的力，然后根据这个获取杆件约束的属性，计算 ConstraintPower
                    //OYM：Shrink为杆件全局值，另外两个值为线性插值获取的值，同理Stretch，所以这里大概可以猜中只是一个简单的不大于1的值
                    {
                        case ConstraintType.Structural_Vertical:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->structuralShrinkVertical + pPointReadB->structuralShrinkVertical)
                                : constraint.stretch * (pPointReadA->structuralStretchVertical + pPointReadB->structuralStretchVertical);
                            break;
                        case ConstraintType.Structural_Horizontal:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->structuralShrinkHorizontal + pPointReadB->structuralShrinkHorizontal)
                                : constraint.stretch * (pPointReadA->structuralStretchHorizontal + pPointReadB->structuralStretchHorizontal);
                            break;
                        case ConstraintType.Shear:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->shearShrink + pPointReadB->shearShrink)
                                : constraint.stretch * (pPointReadA->shearStretch + pPointReadB->shearStretch);
                            break;
                        case ConstraintType.Bending_Vertical:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->bendingShrinkVertical + pPointReadB->bendingShrinkVertical)
                                : constraint.stretch * (pPointReadA->bendingStretchVertical + pPointReadB->bendingStretchVertical);
                            break;
                        case ConstraintType.Bending_Horizontal:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->bendingShrinkHorizontal + pPointReadB->bendingShrinkHorizontal)
                                : constraint.stretch * (pPointReadA->bendingStretchHorizontal + pPointReadB->bendingStretchHorizontal);
                            break;
                        case ConstraintType.Circumference:
                            ConstraintPower = IsShrink
                                ? constraint.shrink * (pPointReadA->circumferenceShrink + pPointReadB->circumferenceShrink)
                                : constraint.stretch * (pPointReadA->circumferenceStretch + pPointReadB->circumferenceStretch);
                            break;
                        default:
                            ConstraintPower = 0.0f;
                            break;
                    }


                    //OYM：获取AB点重量比值的比值,由于重量越大移动越慢,所以A的值实际上是B的重量的比

                    float WeightProportion = pPointReadB->weight / (pPointReadA->weight + pPointReadB->weight);

                    if (ConstraintPower > 0.0f)//OYM：这里不可能小于0吧（除非有人搞破坏）
                    {
                        float3 Displacement = math.normalize(Direction) * (Force * ConstraintPower);
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
        public struct JobPointToTransform : IJobParallelForTransform
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float deltaTime;

            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
             }
             public void TryExecute(TransformAccessArray transforms, JobHandle job)
             {
                 if (!job.IsCompleted)
                 {
                     job.Complete();
                 }
                 for (int i = 0; i < transforms.length; i++)
                 {
                     Execute(i, transforms[i]);
                 }
             }
             public void Execute(int index, Transform transform)
             {
#endif
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;//OYM：获取每个读写点
                PointRead* pReadPoint = pReadPoints + index;//OYM：获取每个只读点

                if (pReadPoint->fixedIndex != index)//OYM：不是fix点
                {
                    transform.position = pReadWritePoint->position;
                }

                //OYM:  旋转节点
                //OYM:  这里有个bug,当初考虑的时候是存在多个子节点的,但是实际上并没有
                if (pReadPoint->childFirstIndex > -1)
                {
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    var child = pReadWritePoints + pReadPoint->childFirstIndex;
                    var parent = pReadWritePoints + pReadPoint->parentIndex;
                    var childRead = pReadPoints + pReadPoint->childFirstIndex;

                    float3 ToDirection = child->position - pReadWritePoint->position;//OYM：朝向等于面向子节点的方向
                    float3 FixedDirection = parent->position - pReadWritePoint->position;
                    if (math.lengthsq(ToDirection) > EPSILON * EPSILON)//OYM：两点不再一起
                    {
                        float3 FromDirection = transform.rotation * childRead->initialLocalPosition;//OYM：将BoneAxis按照transform.rotation进行旋转

                        Quaternion AimRotation = Quaternion.FromToRotation(FromDirection, ToDirection);//OYM：我仔细考虑了下,fromto用在这里不一定是最好,但是一定是最快

                        transform.rotation = AimRotation * transform.rotation;
                    }
                }
            }
        }
        #endregion
    }
}



