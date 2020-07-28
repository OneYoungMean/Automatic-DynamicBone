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
    using Mono;
    public unsafe class DataPackage
    {
        //private const bool isRunning = true;
        private const bool isTryExcute = false;
        private const bool isDebug = false;

        //OYM：先把主要功能恢复
        private ADBRunTimeJobsTable ADBRunTimeJobsTable;

        private ADBRunTimeJobsTable.ColliderGetTransform colliderGet;
        private ADBRunTimeJobsTable.PointGetTransform pointGet;
        private ADBRunTimeJobsTable.PointUpdate pointUpdate;
        private ADBRunTimeJobsTable.ColliderUpdate colliderUpdate;
        private ADBRunTimeJobsTable.ConstraintUpdate[] constraintUpdates;
        private ADBRunTimeJobsTable.ConstraintUpdate constraintUpdates1;
        private ADBRunTimeJobsTable.JobPointToTransform pointToTransform;

        private NativeArray<ColliderRead> collidersReadList;
        private NativeArray<ColliderReadWrite> collidersReadWriteList;
        private TransformAccessArray colliderTransformsList;
        private List<ConstraintRead[]> m_constraintList;
        private List<PointRead> m_pointReadList;
        private List<PointReadWrite> m_pointReadWriteList;
        private NativeArray<ConstraintRead>[] constraintReadList;
        private NativeArray<ConstraintRead> constraintReadList1;
        private NativeArray<PointRead> pointReadList;
        private NativeArray<PointReadWrite> pointReadWriteList;
        // private NativeArray<PointReadWrite> pointReadWriteListCopy;
        private TransformAccessArray pointTransformsList;


        public DataPackage()
        {
            ADBRunTimeJobsTable = ADBRunTimeJobsTable.GetRunTimeJobsTable(isDebug);

            m_constraintList = new List<ConstraintRead[]>();
            m_pointReadList = new List<PointRead>();
            m_pointReadWriteList = new List<PointReadWrite>();
            pointTransformsList = new TransformAccessArray(0);
            colliderTransformsList = new TransformAccessArray(0);
        }
        internal bool SetRuntimeData(float deltaTime, float scale, int iteration, Vector3 addForceForce, ColliderCollisionType colliderCollisionType, bool isOptimize)
        {
            int batchLength = isTryExcute ? 1 : 64;
            iteration = isTryExcute ? 1 : iteration;

            JobHandle Hjob = ADBRunTimeJobsTable.returnHJob;
            if (!Hjob.IsCompleted)
            {
                return false;
            }

            constraintUpdates1.oneDivideIteration = pointUpdate.oneDivideIteration = colliderGet.oneDivideIteration = pointGet.oneDivideIteration = 1.0f / iteration;

            pointUpdate.deltaTime = deltaTime;
            pointUpdate.globalScale = scale;
            pointUpdate.isOptimize = isOptimize;
            pointUpdate.addForcePower = addForceForce;
            pointUpdate.isCollision = (colliderCollisionType == ColliderCollisionType.Fast);

            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].globalScale = scale;
                constraintUpdates[i].isCollision = (colliderCollisionType == ColliderCollisionType.Accuate);
            }
            constraintUpdates1.globalScale = scale;
          constraintUpdates1.isCollision = (colliderCollisionType == ColliderCollisionType.Accuate);

            #region LifeCycle
            Hjob = colliderGet.Schedule(colliderTransformsList);
            Hjob = pointGet.Schedule(pointTransformsList);

            //pointGet.TryExecute(pointTransformsList, Hjob);

            for (int i = 0; i < iteration; i++)
            {
                if (isTryExcute)
                {
                    colliderUpdate.TryExecute(collidersReadList.Length, batchLength, Hjob);

                    pointUpdate.TryExecute(pointReadList.Length, batchLength, Hjob);
                    for (int j0 = 0; j0 < constraintUpdates.Length; j0++)
                    {
                        constraintUpdates[j0].TryExecute(constraintReadList[j0].Length, batchLength, Hjob);
                    }
                }
                else
                {
                    Hjob = colliderUpdate.Schedule(collidersReadList.Length, batchLength);
                    //Hjob = fixedPointUpdate.Schedule(pointReadList.Length, batchLength);
                    Hjob =pointUpdate.Schedule(pointReadList.Length, batchLength);
                    Hjob = constraintUpdates1.Schedule(constraintReadList1.Length, batchLength);
                }
            }

            Hjob = pointToTransform.Schedule(pointTransformsList);
            #endregion
            // pointToTransform.TryExecute(pointTransformsList, Hjob);

            return true;
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
                if (pointReadList[i].parentIndex != -1)
                {
                    pointReadList[i].parentIndex += offset;
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
            List<ConstraintRead> constraintReadList1Target = new List<ConstraintRead>();
            for (int i = 0; i < m_constraintList.Count; i++)
            {
                constraintReadList1Target.AddRange(m_constraintList[i]);
                constraintReadList[i] = new NativeArray<ConstraintRead>(m_constraintList[i], Allocator.Persistent);
            }
            constraintReadList1 = new NativeArray<ConstraintRead>(constraintReadList1Target.ToArray(), Allocator.Persistent);

            colliderGet = new ADBRunTimeJobsTable.ColliderGetTransform();
            pointGet = new ADBRunTimeJobsTable.PointGetTransform();
            pointUpdate = new ADBRunTimeJobsTable.PointUpdate();
            colliderUpdate = new ADBRunTimeJobsTable.ColliderUpdate();
            constraintUpdates = new ADBRunTimeJobsTable.ConstraintUpdate[m_constraintList.Count];
            constraintUpdates1 = new ADBRunTimeJobsTable.ConstraintUpdate();
            pointToTransform = new ADBRunTimeJobsTable.JobPointToTransform();

            colliderGet.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderGet.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            colliderUpdate.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderUpdate.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            pointGet.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointGet.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

            pointUpdate.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointUpdate.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            pointUpdate.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            pointUpdate.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
            pointUpdate.colliderCount = collidersReadList.Length;

            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pConstraintsRead = (ConstraintRead*)constraintReadList[i].GetUnsafePtr();
                constraintUpdates[i].colliderCount = collidersReadList.Length;

            }
            constraintUpdates1.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            constraintUpdates1.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
            constraintUpdates1.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            constraintUpdates1.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            constraintUpdates1.pConstraintsRead = (ConstraintRead*)constraintReadList1.GetUnsafePtr();
            constraintUpdates1.colliderCount = collidersReadList.Length;



            pointToTransform.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointToTransform.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

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

        }

        public void Dispose(bool isReset)
        {
            ADBRunTimeJobsTable.returnHJob.Complete();
            pointReadList.Dispose();
            pointReadWriteList.Dispose();
            pointTransformsList.Dispose();
            constraintReadList1.Dispose();
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
