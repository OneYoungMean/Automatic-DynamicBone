using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;
using Unity.Jobs;
using System;

namespace ADBRuntime
{
    using Internal;
    public unsafe class DataPackage
    {
        //OYM：先把主要功能恢复
        private ADBRunTimeJobsTable ADBRunTimeJobsTable;

        private ADBRunTimeJobsTable.ColliderGetTransform colliderGet;
        private ADBRunTimeJobsTable.PointGetTransform pointGet;
        private ADBRunTimeJobsTable.PointUpdate pointUpdate;
        private ADBRunTimeJobsTable.ColliderUpdate colliderUpdate;
        private ADBRunTimeJobsTable.JobConstraintUpdate[] constraintUpdates;
        private ADBRunTimeJobsTable.JobCollisionPoint pointCollision;
        private ADBRunTimeJobsTable.JobPointToTransform2 pointToTransform;

        private NativeArray<ColliderRead> collidersReadList;
        private NativeArray<ColliderReadWrite> collidersReadWriteList;
        private TransformAccessArray colliderTransformsList;
        private List<ConstraintRead[]> m_constraintList;
        private List<PointRead> m_pointReadList;
        private List<PointReadWrite> m_pointReadWriteList;
        private NativeArray<ConstraintRead>[] constraintReadList;
        private NativeArray<PointRead> pointReadList;
        private NativeArray<PointReadWrite> pointReadWriteList;
        // private NativeArray<PointReadWrite> pointReadWriteListCopy;
        private TransformAccessArray pointTransformsList;

        private const bool isRunning = true;
        private const bool isTryExcute = false;
        private const bool isDebug = false;
        public DataPackage()
        {
            ADBRunTimeJobsTable = ADBRunTimeJobsTable.GetRunTimeJobsTable(isDebug);

            m_constraintList = new List<ConstraintRead[]>();
            m_pointReadList = new List<PointRead>();
            m_pointReadWriteList = new List<PointReadWrite>();
            pointTransformsList = new TransformAccessArray(0);
            colliderTransformsList = new TransformAccessArray(0);
        }
        public void SetColliderPackage(ColliderRead[] collidersReadList, ColliderReadWrite[] collidersReadWriteList, Transform[] collidersTransList)
        {
            this.collidersReadList = new NativeArray<ColliderRead>(collidersReadList, Allocator.Persistent);
            this.collidersReadWriteList = new NativeArray<ColliderReadWrite>(collidersReadWriteList, Allocator.Persistent);
            colliderTransformsList.SetTransforms(collidersTransList);
        }
        public void SetPointAndConstraintpackage(ConstraintRead[][] constraintList, PointRead[] pointReadList, PointReadWrite[] pointReadWriteList, Transform[] pointTransformsList)
        {
            int offset = m_pointReadList.Count;
            for (int i = 0; i < pointReadList.Length; i++)
            {
                if (pointReadList[i].parent != -1)
                {
                    pointReadList[i].parent += offset;
                }
                if (pointReadList[i].childFirstIndex != -1)
                {
                    pointReadList[i].childFirstIndex += offset;
                    pointReadList[i].childLastIndex += offset;
                }
                pointReadList[i].fixedIndex += offset;
            }
            for (int i = 0; i < constraintList.Length; i++)
            {
                for (int j0 = 0; j0 < constraintList[i].Length; j0++)
                {
                    constraintList[i][j0].indexA += offset;
                    constraintList[i][j0].indexB += offset;
                }
            }
            this.m_constraintList.AddRange(constraintList);
            this.m_pointReadList.AddRange(pointReadList);
            this.m_pointReadWriteList.AddRange(pointReadWriteList);
            for (int i = 0; i < pointTransformsList.Length; i++)
            {
                this.pointTransformsList.Add(pointTransformsList[i]);
            }
        }
        public void SetNativeArray()
        {
            pointReadList = new NativeArray<PointRead>(m_pointReadList.ToArray(), Allocator.Persistent);
            pointReadWriteList = new NativeArray<PointReadWrite>(m_pointReadWriteList.ToArray(), Allocator.Persistent);
            constraintReadList = new NativeArray<ConstraintRead>[m_constraintList.Count];
            for (int i = 0; i < m_constraintList.Count; i++)
            {
                constraintReadList[i] = new NativeArray<ConstraintRead>(m_constraintList[i], Allocator.Persistent);
            }

            colliderGet = new ADBRunTimeJobsTable.ColliderGetTransform();
            pointGet = new ADBRunTimeJobsTable.PointGetTransform();
            pointUpdate = new ADBRunTimeJobsTable.PointUpdate();
            colliderUpdate = new ADBRunTimeJobsTable.ColliderUpdate();
            constraintUpdates = new ADBRunTimeJobsTable.JobConstraintUpdate[m_constraintList.Count];
            pointCollision = new ADBRunTimeJobsTable.JobCollisionPoint();
            pointToTransform = new ADBRunTimeJobsTable.JobPointToTransform2();

            colliderGet.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderGet.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            colliderUpdate.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderUpdate.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            pointGet.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointGet.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

            pointUpdate.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointUpdate.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();


            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pConstraintsRead = (ConstraintRead*)constraintReadList[i].GetUnsafePtr();

            }
            pointCollision.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            pointCollision.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
            pointCollision.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            pointCollision.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();

            pointToTransform.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointToTransform.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

        }

        internal void SetRuntimeData(float deltaTime, float scale, int iteration, Vector3 windForce, ColliderCollisionType colliderCollisionType)
        {
            int batchLength = isTryExcute ? 1 :64;

            JobHandle Hjob = ADBRunTimeJobsTable.returnHJob;

            pointGet.iteration = iteration;
            pointUpdate.deltaTime = deltaTime;
            pointUpdate.scale = scale;
            pointUpdate.iteration = iteration;
            pointUpdate.windForcePower = windForce;
            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].scale = scale;
               constraintUpdates[i].colliderCount = collidersReadList.Length;
                constraintUpdates[i].isCollision = (colliderCollisionType == ColliderCollisionType.Accuate);
            }
            pointCollision.colliderCount = collidersReadList.Length;
            pointCollision.isCollider = (colliderCollisionType == ColliderCollisionType.Fast);
            Hjob = colliderGet.Schedule(colliderTransformsList);
            Hjob = pointGet.Schedule(pointTransformsList);

            if (isRunning)
            {
                float step;
                for (int i = 0; i < iteration; i++)
                {
                    step = 1 / ((float)iteration - i);
                    if (isTryExcute)
                    {
                        pointUpdate.TryExecute(pointReadList.Length, batchLength, Hjob);
                    }
                    else 
                    {
                        Hjob = pointUpdate.Schedule(pointReadList.Length, batchLength);
                    } 

                    colliderUpdate.step = step;
                    Hjob = colliderUpdate.Schedule(collidersReadList.Length, batchLength);

                    for (int j0 = 0; j0 < constraintUpdates.Length; j0++)
                    {
                        if (isTryExcute)
                        {
                            constraintUpdates[j0].TryExecute(constraintReadList[j0].Length, batchLength, Hjob);
                        }
                        else
                        {
                            Hjob = constraintUpdates[j0].Schedule(constraintReadList[j0].Length, batchLength);
                        }
                    }
                    if (isTryExcute)
                    {
                        pointCollision.TryExecute(pointReadList.Length, batchLength, Hjob);
                    }
                    else
                    {
                        Hjob = pointCollision.Schedule(pointReadList.Length, batchLength);
                    }
                }
                 Hjob = pointToTransform.Schedule(pointTransformsList);
                //pointToTransform.TryExecute(pointTransformsList, Hjob);
            }
        }

        public void restorePoint()
        {
            
            ADBRunTimeJobsTable.InitiralizePoint initialpoint = new ADBRunTimeJobsTable.InitiralizePoint
            {
                pReadPoints = (PointRead*)pointReadList.GetUnsafePtr(),
                pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr(),
            };
            ADBRunTimeJobsTable.returnHJob = initialpoint.Schedule(pointTransformsList, ADBRunTimeJobsTable.returnHJob);
            ADBRunTimeJobsTable.InitiralizeCollider initialCollider = new ADBRunTimeJobsTable.InitiralizeCollider
            {
                pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr(),
                pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr()
            };
            ADBRunTimeJobsTable.returnHJob = initialCollider.Schedule(colliderTransformsList, ADBRunTimeJobsTable.returnHJob);
            ADBRunTimeJobsTable.returnHJob = pointToTransform.Schedule(pointTransformsList, ADBRunTimeJobsTable.returnHJob);
        }

        public void Dispose(bool isReset)
        {
            ADBRunTimeJobsTable.returnHJob.Complete();
            pointReadList.Dispose();
            pointReadWriteList.Dispose();
            pointTransformsList.Dispose();
            for (int i = 0; i < constraintReadList.Length; i++)
            {
                constraintReadList[i].Dispose();
            }
            if (isReset)
            {
                pointTransformsList = new TransformAccessArray(0);
                m_constraintList = new List<ConstraintRead[]>();
                m_pointReadList = new List<PointRead>();
                m_pointReadWriteList = new List<PointReadWrite>();
            }
            else
            {
                collidersReadList.Dispose();
                collidersReadWriteList.Dispose();
                colliderTransformsList.Dispose();
            }
            

        }
    }
}
