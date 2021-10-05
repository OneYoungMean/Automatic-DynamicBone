using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Jobs;
using Unity.Collections;

namespace ADBRuntime
{
    using Mono;
    using Unity.Mathematics;
    public class ADBAvatarReader
    {

        #region Point and parameter


        public const float upperArmWidthAspect = 1f;
        public const float lowerArmWidthAspect = 0.9f;
        public const float endArmWidthAspect = 0.81f;

        public const float upperLegWidthAspect = 1f;
        public const float lowerLegWidthAspect = 0.7f;
        public const float endLegWidthAspect = 0.7f;
        private bool isGenerateFinger;
        private ADBRuntimeController controller;
        public int isGenerateSuccessful { get; private set; } //OYM:-1失败.0未知.1成功

        #endregion


        public ADBAvatarReader(ADBRuntimeController controller)
        {
            this.controller = controller;
        }

        public MinMaxAABB CaculateAABB(ADBConstraintReadAndPointControll[] jointAndPointControlls)
        {
            Transform root = controller.generateTransform;
            MinMaxAABB AABB=new MinMaxAABB();
            if (jointAndPointControlls == null|| jointAndPointControlls.Length==0)
            {
                 AABB = MinMaxAABB.CreateFromCenterAndExtents(root.position, 0);
                Transform[] allTrans = controller.generateTransform.GetComponentsInChildren<Transform>();//OYM:只搜索当前层
                for (int i = 0; i < allTrans.Length; i++)
                {
                    AABB.Encapsulate(allTrans[i].position);
                }

                AABB.Expand(AABB.HalfExtents);
                float3 halfExtents = AABB.HalfExtents;
                float avg = (halfExtents.x + halfExtents.y + halfExtents.z) / 3;
                halfExtents = math.max(halfExtents, avg);
                AABB.HalfExtents = halfExtents;
            }

            else
            {
               
                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    var fixedPoint = jointAndPointControlls[i].fixedNodeList;
                    if (fixedPoint==null|| fixedPoint.Count==0) continue;

                    for (int j = 0; j < fixedPoint.Count; j++)
                    {
                        if (fixedPoint[j]!=null)
                        {
                            MinMaxAABB smallAABB = MinMaxAABB.CreateFromCenterAndHalfExtents(fixedPoint[j].trans.position, GetMaxDeep(fixedPoint[i]));
                            if (math.all(AABB.Min == 0)&& math.all(AABB.Max == 0))
                            {
                                AABB = smallAABB;
                            }
                            else
                            {
                                AABB.Encapsulate(smallAABB);
                            }
                        }
                    }
                }              
            }

            AABB.Center =Quaternion.Inverse( root.rotation)*((Vector3)AABB.Center - controller.generateTransform.position);
            return AABB;
        }

        private float GetMaxDeep(ADBRuntimePoint point)
        {
            if (point == null)
            {
                return 0;
            }
            else if (point.childNode == null|| point.childNode.Count==0)
            {
                return math.length(point.pointRead.initialLocalPosition);
            }
            else
            {
                float max = 0;
                for (int i = 0; i < point.childNode.Count; i++)
                {
                    max =math.max(max, GetMaxDeep(point.childNode[i]));
                }
                if (point.isFixed)
                {
                    return max;
                }
                else
                {
                    return math.length(point.pointRead.initialLocalPosition) + max;
                }

            }
        }

        public List<ADBColliderReader> GenerateBodyCollidersData(List<ADBRuntimePoint> allPointTrans, bool isGenerateFinger,bool isGenerateColliderOpenTrigger)
        {
            isGenerateSuccessful = -1;
            if (controller.generateTransform == null) return null;

            Animator animator = controller.generateTransform.GetComponent<Animator>();//OYM:只搜索当前层
            List<ADBColliderReader> generateColliderList = null;

            if (animator != null && animator.avatar.isHuman)
            {
                if (generateColliderList == null)
                {
                    isGenerateSuccessful = 0;
                    generateColliderList = new List<ADBColliderReader>();
                }
                GenerateCollidersData(ref generateColliderList, allPointTrans, animator, isGenerateFinger, isGenerateColliderOpenTrigger);
            }
            else
            {
                var animators = controller.generateTransform.gameObject.GetComponentsInChildren<Animator>();//OYM:或者是其子物体
                if (animators != null && animators.Length != 0)
                {
                    for (int i = 0; i < animators.Length; i++)
                    {
                        animator = animators[i];
                        if (animator != null && animator.avatar.isHuman)
                        {
                            if (generateColliderList == null)
                            {
                                isGenerateSuccessful = 0;
                                generateColliderList = new List<ADBColliderReader>();
                            }
                            GenerateCollidersData(ref generateColliderList, allPointTrans, animator, isGenerateFinger, isGenerateColliderOpenTrigger);//OYM:尝试生成
                        }
                        else
                        {
                            Debug.Log(controller.generateTransform.name + "'s Avatar is lost or isn't Human!");
                        }
                    }
                }
            }
            isGenerateSuccessful = 1;
            return generateColliderList;
        }
        private void GenerateCollidersData(ref List<ADBColliderReader> runtimeColliders, List<ADBRuntimePoint> allPointTrans, Animator animator, bool isGenerateFinger, bool isGenerateColliderOpenTrigger)
        {//OYM：这坨屎山我连写注释的兴趣都没有,你知道这玩意能大概把你角色圈进去就行
            //OYM：你问我怎么算的?当然是经验(试出来)啦 XD
            Vector3 scaleTemp = animator.transform.localScale;
            Vector3 positionTemp = animator.transform.position;
            Quaternion rotationTemp = animator.transform.rotation;

            animator.transform.position = Vector3.zero;
            animator.transform.rotation = Quaternion.identity;
            animator.transform.localScale = Vector3.one;

            //OYM:这里涉及到unity的一个坑爹特性,当父transform的scale不为1的时候,设置正常的length会被放大/缩小,最好的办法就是预先设置好角色尺寸,然后再还原回去1
            //OYM:这种坑爹的问题真是难死人了,直接重设坐标解决吧

            if (animator.transform.lossyScale != Vector3.one)
            {
                Debug.Log("cannot set scale at one,collider may generate in error peace");
            }

            Vector3 rootPoint;
            Vector3 headStartPoint;
            Vector3 headCenterPoint;
            float headColliderRadiu;

            Vector3 spineStopPoint;
            Vector3 spineStartPoint;
            float spineColliderRadiu;

            float hipsColliderRadiuUp;
            float hipsColliderRadiuDown;

            Vector3 upperArmToHeadCentroid;
            Vector3 upperLegCentroid;

            Vector3 leftHandCenterPoint;
            Vector3 rightHandCenterPoint;

            float torsoWidth;
            float hipsWidth;


            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            var spine = animator.GetBoneTransform(HumanBodyBones.Spine);

            var leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes);

            var rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            var rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            var rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);

            var leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var leftFinger = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

            var rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var rightFinger = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

            rootPoint = animator.transform.position;
            headStartPoint = head.position;

            upperArmToHeadCentroid = 0.5f * (leftUpperArm.position + rightUpperArm.position);
            torsoWidth = Vector3.Distance(leftUpperArm.position, rightUpperArm.position);

            upperLegCentroid = 0.5f * (leftUpperLeg.position + rightUpperLeg.position);
            hipsWidth = Vector3.Distance(leftUpperLeg.position, rightUpperLeg.position);


            //OYM：Head
            headCenterPoint = headStartPoint + new Vector3(0, 0.5f * torsoWidth, 0);
            headColliderRadiu = CheckNearstPointToSegment(0.5f * torsoWidth, headCenterPoint, Vector3.zero, ColliderChoice.Head, allPointTrans);

            runtimeColliders.Add(CreateUnitySphereCollider(headColliderRadiu, new Vector3(0, 0.5f * torsoWidth, 0), ColliderChoice.Head, head, "Head",isGenerateColliderOpenTrigger));

            // Spine
            spineStartPoint = headCenterPoint + new Vector3(0, -torsoWidth, 0);
            spineStopPoint = upperLegCentroid;
            spineColliderRadiu = CheckNearstPointToSegment(torsoWidth, spineStartPoint, spineStopPoint - spineStartPoint, ColliderChoice.UpperBody, allPointTrans);

            runtimeColliders.Add(CreateUntiyCapsuleCollider(spineColliderRadiu, spineStartPoint, spineStopPoint, ColliderChoice.UpperBody, spine, "Spine1",isGenerateColliderOpenTrigger));

            //Hip

            Vector3 hipColliderCenter = upperLegCentroid;

            hipsColliderRadiuUp = CheckNearstPointToSegment((spineColliderRadiu * 2), hipColliderCenter, Vector3.zero, ColliderChoice.UpperBody, allPointTrans);
            runtimeColliders.Add(CreateUnitySphereCollider(hipsColliderRadiuUp, hipColliderCenter - spine.position, ColliderChoice.UpperBody, spine, "Spine2",isGenerateColliderOpenTrigger));

            Vector3 hipColliderCenterDownA = upperLegCentroid - new Vector3(hipsWidth * 0.5f, 0, 0);
            Vector3 hipColliderCenterDownB = hipColliderCenterDownA + new Vector3(hipsWidth, 0, 0);
            hipsColliderRadiuDown = CheckNearstPointToSegment(hipsWidth, hipColliderCenterDownA, hipColliderCenterDownB - hipColliderCenterDownA, ColliderChoice.LowerBody, allPointTrans);

            runtimeColliders.Add(CreateUntiyCapsuleCollider(hipsColliderRadiuDown, hipColliderCenterDownA, hipColliderCenterDownB, ColliderChoice.LowerBody, pelvis, "Pelvis", isGenerateColliderOpenTrigger, CollideFunc.OutsideNoLimit, true));


            // LeftArms

            float leftArmWidth = Vector3.Distance(leftUpperArm.position, leftLowerArm.position) * 0.3f;
            float leftUpperArmWidth = CheckNearstPointToSegment(leftArmWidth * upperArmWidthAspect, leftUpperArm.position, leftLowerArm.position - leftUpperArm.position, ColliderChoice.UpperArm, allPointTrans);
            float leftLowerArmWidth = CheckNearstPointToSegment(leftArmWidth * lowerArmWidthAspect, leftLowerArm.position, leftHand.position - leftLowerArm.position, ColliderChoice.LowerArm, allPointTrans);
            runtimeColliders.Add(CreateUntiyCapsuleCollider(leftUpperArmWidth, leftUpperArm.position, leftLowerArm.position, ColliderChoice.UpperArm, leftUpperArm, "LeftUpperArm",isGenerateColliderOpenTrigger));
            runtimeColliders.Add(CreateUntiyCapsuleCollider(leftLowerArmWidth, leftLowerArm.position, leftHand.position, ColliderChoice.LowerArm, leftLowerArm, "LeftLowerArm",isGenerateColliderOpenTrigger));

            leftHandCenterPoint = (leftFinger.position + leftHand.position) * 0.5f;




            // LeftLegs
            float leftLegWidth = Vector3.Distance(leftUpperLeg.position, leftLowerLeg.position) * 0.3f;
            float leftUpperLegWidth = CheckNearstPointToSegment(leftLegWidth * upperLegWidthAspect, leftUpperLeg.position, leftLowerLeg.position - leftUpperLeg.position, ColliderChoice.UpperLeg, allPointTrans);
            float leftLowerLegWidth = CheckNearstPointToSegment(leftLegWidth * lowerLegWidthAspect, leftLowerLeg.position, leftHand.position - leftLowerLeg.position, ColliderChoice.LowerLeg, allPointTrans);
            float leftEndLegWidth = leftLegWidth * endLegWidthAspect;
            runtimeColliders.Add(CreateUntiyCapsuleCollider(leftUpperLegWidth, leftUpperLeg.position - new Vector3(0, leftUpperLegWidth, 0), leftLowerLeg.position, ColliderChoice.UpperLeg, leftUpperLeg, "LeftUpperLeg",isGenerateColliderOpenTrigger));
            runtimeColliders.Add(CreateUntiyCapsuleCollider(leftLowerLegWidth, leftLowerLeg.position, leftFoot.position, ColliderChoice.LowerLeg, leftLowerLeg, "LeftLowerLeg",isGenerateColliderOpenTrigger));

            // LeftFoot

            if (leftToes != null)
            {
                runtimeColliders.Add(CreateUntiyCapsuleCollider(leftEndLegWidth, leftFoot.position, leftToes.position, ColliderChoice.Foot, leftFoot, "LeftFoot",isGenerateColliderOpenTrigger));
            }
            else
            {
                Vector3 leftfootStartPoint = leftFoot.position;
                Vector3 leftfootStopPoint = new Vector3(leftfootStartPoint.x, animator.rootPosition.y + leftEndLegWidth, leftfootStartPoint.z) + animator.rootRotation * Vector3.forward * (leftLowerArm.position - leftHand.position).magnitude * endLegWidthAspect;
                runtimeColliders.Add(CreateUntiyCapsuleCollider(leftEndLegWidth, leftfootStartPoint, leftfootStopPoint, ColliderChoice.Foot, leftFoot, "LeftFoot",isGenerateColliderOpenTrigger));
            }

            // rightArms

            float rightArmWidth = Vector3.Distance(rightUpperArm.position, rightLowerArm.position) * 0.3f;
            float rightUpperArmWidth = CheckNearstPointToSegment(rightArmWidth * upperArmWidthAspect, rightUpperArm.position, rightLowerArm.position - rightUpperArm.position, ColliderChoice.UpperArm, allPointTrans);
            float rightLowerArmWidth = CheckNearstPointToSegment(rightArmWidth * lowerArmWidthAspect, rightLowerArm.position, rightHand.position - rightLowerArm.position, ColliderChoice.LowerArm, allPointTrans);

            runtimeColliders.Add(CreateUntiyCapsuleCollider(rightUpperArmWidth, rightUpperArm.position, rightLowerArm.position, ColliderChoice.UpperArm, rightUpperArm, "RightUpperArm",isGenerateColliderOpenTrigger));
            runtimeColliders.Add(CreateUntiyCapsuleCollider(rightLowerArmWidth, rightLowerArm.position, rightHand.position, ColliderChoice.LowerArm, rightLowerArm, "RightLowerArm",isGenerateColliderOpenTrigger));
            rightHandCenterPoint = (rightFinger.position + rightHand.position) * 0.5f;



            // rightLegs
            float rightLegWidth = Vector3.Distance(rightUpperLeg.position, rightLowerLeg.position) * 0.3f;
            float rightUpperLegWidth = CheckNearstPointToSegment(rightLegWidth * upperLegWidthAspect, rightUpperLeg.position, rightLowerLeg.position - rightUpperLeg.position, ColliderChoice.UpperLeg, allPointTrans);
            float rightLowerLegWidth = CheckNearstPointToSegment(rightLegWidth * lowerLegWidthAspect, rightLowerLeg.position, rightHand.position - rightLowerLeg.position, ColliderChoice.LowerLeg, allPointTrans);
            float rightEndLegWidth = rightLegWidth * endLegWidthAspect;
            runtimeColliders.Add(CreateUntiyCapsuleCollider(rightUpperLegWidth, rightUpperLeg.position - new Vector3(0, rightUpperLegWidth, 0), rightLowerLeg.position, ColliderChoice.UpperLeg, rightUpperLeg, "RightUpperLeg",isGenerateColliderOpenTrigger));
            runtimeColliders.Add(CreateUntiyCapsuleCollider(rightLowerLegWidth, rightLowerLeg.position, rightFoot.position, ColliderChoice.LowerLeg, rightLowerLeg, "RightLowerLeg",isGenerateColliderOpenTrigger));

            // rightFoot

            if (rightToes != null)
            {
                runtimeColliders.Add(CreateUntiyCapsuleCollider(rightEndLegWidth, rightFoot.position, rightToes.position, ColliderChoice.Foot, rightFoot, "RightFoot",isGenerateColliderOpenTrigger));
            }
            else
            {
                Vector3 rightfootStartPoint = rightFoot.position;
                Vector3 rightfootStopPoint = new Vector3(rightfootStartPoint.x, animator.rootPosition.y + rightEndLegWidth, rightfootStartPoint.z) + animator.rootRotation * Vector3.forward * (rightLowerArm.position - rightHand.position).magnitude * endLegWidthAspect;
                runtimeColliders.Add(CreateUntiyCapsuleCollider(rightEndLegWidth, rightfootStartPoint, rightfootStopPoint, ColliderChoice.Foot, rightFoot, "RightFoot",isGenerateColliderOpenTrigger));
            }

            if (!isGenerateFinger)
            {
                runtimeColliders.Add(CreateUnitySphereCollider(Vector3.Distance(leftHand.position, leftHandCenterPoint), leftHandCenterPoint - leftHand.position, ColliderChoice.Hand, leftHand, "LeftHand",isGenerateColliderOpenTrigger));//OYM:lefthand
                runtimeColliders.Add(CreateUnitySphereCollider(Vector3.Distance(rightHand.position, rightHandCenterPoint), rightHandCenterPoint - rightHand.position, ColliderChoice.Hand, rightHand, "RightHand",isGenerateColliderOpenTrigger));//OYM:righthand
            }
            else
            {
                //OYM：Left
                var leftThumb1 = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
                var leftThumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
                var leftThumb3 = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
                var leftIndex1 = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                var leftIndex2 = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
                var leftIndex3 = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                var leftMiddle1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                var leftMiddle2 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
                var leftMiddle3 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
                var leftRing1 = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                var leftRing2 = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
                var leftRing3 = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
                var leftLittle1 = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
                var leftLittle2 = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
                var leftLittle3 = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);

                var leftHandLength = (leftMiddle2.position - leftHand.position).magnitude;
                //colliderList.Add(new OBBBoxCollider(leftHandCenterPoint, new Vector3(leftHandLength * 0.25f, leftHandLength, leftHandLength), leftMiddle2.position - leftMiddle1.position, ColliderChoice.Hand, leftHand));
                Quaternion leftHandRotation = Quaternion.LookRotation(leftMiddle2.position - leftHand.position, Vector3.Cross(leftThumb3.position - leftHand.position, leftLittle3.position - leftHand.position));

                runtimeColliders.Add(CreateUntiyBoxCollider(leftHandCenterPoint, leftHandRotation, new Vector3(leftHandLength * 0.75f, leftHandLength * 0.25f, leftHandLength * 0.75f), ColliderChoice.Hand, leftHand, "LeftHand",isGenerateColliderOpenTrigger));

                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftThumb1.position - leftThumb2.position).magnitude / 2, leftThumb1.position, leftThumb2.position, ColliderChoice.Hand, leftThumb1, "LeftThumb1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftThumb2.position - leftThumb3.position).magnitude / 2, leftThumb2.position, leftThumb3.position, ColliderChoice.Hand, leftThumb2, "LeftThumb2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftThumb2.position - leftThumb3.position).magnitude / 2 * 0.8f, leftThumb3.position, leftThumb3.position + (leftThumb3.position - leftThumb2.position) * 0.8f, ColliderChoice.Hand, leftThumb3, "LeftThumb3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftIndex1.position - leftIndex2.position).magnitude / 2, leftIndex1.position, leftIndex2.position, ColliderChoice.Hand, leftIndex1, "LeftIndex1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftIndex2.position - leftIndex3.position).magnitude / 2, leftIndex2.position, leftIndex3.position, ColliderChoice.Hand, leftIndex2, "LeftIndex2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftIndex2.position - leftIndex3.position).magnitude / 2 * 0.8f, leftIndex3.position, leftIndex3.position + (leftIndex3.position - leftIndex2.position) * 0.8f, ColliderChoice.Hand, leftIndex3, "LeftIndex3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftMiddle1.position - leftMiddle2.position).magnitude / 2, leftMiddle1.position, leftMiddle2.position, ColliderChoice.Hand, leftMiddle1, "LeftMiddle1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftMiddle2.position - leftMiddle3.position).magnitude / 2, leftMiddle2.position, leftMiddle3.position, ColliderChoice.Hand, leftMiddle2, "LeftMiddle2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftMiddle2.position - leftMiddle3.position).magnitude / 2 * 0.8f, leftMiddle3.position, leftMiddle3.position + (leftMiddle3.position - leftMiddle2.position) * 0.8f, ColliderChoice.Hand, leftMiddle3, "LeftMiddle3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftRing1.position - leftRing2.position).magnitude / 2, leftRing1.position, leftRing2.position, ColliderChoice.Hand, leftRing1, "LeftRing1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftRing2.position - leftRing3.position).magnitude / 2, leftRing2.position, leftRing3.position, ColliderChoice.Hand, leftRing2, "LeftRing2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftRing2.position - leftRing3.position).magnitude / 2 * 0.8f, leftRing3.position, leftRing3.position + (leftRing3.position - leftRing2.position) * 0.8f, ColliderChoice.Hand, leftRing3, "LeftRing3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftLittle1.position - leftLittle2.position).magnitude / 2, leftLittle1.position, leftLittle2.position, ColliderChoice.Hand, leftLittle1, "LeftLittle1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftLittle2.position - leftLittle3.position).magnitude / 2, leftLittle2.position, leftLittle3.position, ColliderChoice.Hand, leftLittle2, "LeftLittle2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((leftLittle2.position - leftLittle3.position).magnitude / 2 * 0.8f, leftLittle3.position, leftLittle3.position + (leftLittle3.position - leftLittle2.position) * 0.8f, ColliderChoice.Hand, leftLittle3, "LeftLittle3",isGenerateColliderOpenTrigger));
                // right
                var rightThumb1 = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                var rightThumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
                var rightThumb3 = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
                var rightIndex1 = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                var rightIndex2 = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
                var rightIndex3 = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
                var rightMiddle1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                var rightMiddle2 = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
                var rightMiddle3 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
                var rightRing1 = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                var rightRing2 = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
                var rightRing3 = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
                var rightLittle1 = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
                var rightLittle2 = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
                var rightLittle3 = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);

                var rightHandLength = (rightMiddle2.position - rightHand.position).magnitude;
                //colliderList.Add(new OBBBoxCollider(rightHandCenterPoint, new Vector3(rightHandLength * 0.25f, rightHandLength, rightHandLength), rightMiddle2.position - rightMiddle1.position, ColliderChoice.Hand, rightHand));
                Quaternion rightHandRotation = Quaternion.LookRotation(rightMiddle2.position - rightHand.position, Vector3.Cross(rightThumb3.position - rightHand.position, rightLittle3.position - rightHand.position));

                runtimeColliders.Add(CreateUntiyBoxCollider((rightHand.position + rightMiddle1.position) / 2, rightHandRotation, new Vector3(rightHandLength * 0.75f, rightHandLength * 0.25f, rightHandLength * 0.75f), ColliderChoice.Hand, rightHand, "RightHand",isGenerateColliderOpenTrigger));

                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightThumb1.position - rightThumb2.position).magnitude / 2, rightThumb1.position, rightThumb2.position, ColliderChoice.Hand, rightThumb1, "RightThumb1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightThumb2.position - rightThumb3.position).magnitude / 2, rightThumb2.position, rightThumb3.position, ColliderChoice.Hand, rightThumb2, "RightThumb2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightThumb2.position - rightThumb3.position).magnitude / 2 * 0.8f, rightThumb3.position, rightThumb3.position + (rightThumb3.position - rightThumb2.position) * 0.8f, ColliderChoice.Hand, rightThumb3, "RightThumb3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightIndex1.position - rightIndex2.position).magnitude / 2, rightIndex1.position, rightIndex2.position, ColliderChoice.Hand, rightIndex1, "RightIndex1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightIndex2.position - rightIndex3.position).magnitude / 2, rightIndex2.position, rightIndex3.position, ColliderChoice.Hand, rightIndex2, "RightIndex2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightIndex2.position - rightIndex3.position).magnitude / 2 * 0.8f, rightIndex3.position, rightIndex3.position + (rightIndex3.position - rightIndex2.position) * 0.8f, ColliderChoice.Hand, rightIndex3, "RightIndex3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightMiddle1.position - rightMiddle2.position).magnitude / 2, rightMiddle1.position, rightMiddle2.position, ColliderChoice.Hand, rightMiddle1, "RightMiddle1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightMiddle2.position - rightMiddle3.position).magnitude / 2, rightMiddle2.position, rightMiddle3.position, ColliderChoice.Hand, rightMiddle2, "RightMiddle2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightMiddle2.position - rightMiddle3.position).magnitude / 2 * 0.8f, rightMiddle3.position, rightMiddle3.position + (rightMiddle3.position - rightMiddle2.position) * 0.8f, ColliderChoice.Hand, rightMiddle3, "RightMiddle3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightRing1.position - rightRing2.position).magnitude / 2, rightRing1.position, rightRing2.position, ColliderChoice.Hand, rightRing1, "RightRing1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightRing2.position - rightRing3.position).magnitude / 2, rightRing2.position, rightRing3.position, ColliderChoice.Hand, rightRing2, "RightRing2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightRing2.position - rightRing3.position).magnitude / 2 * 0.8f, rightRing3.position, rightRing3.position + (rightRing3.position - rightRing2.position) * 0.8f, ColliderChoice.Hand, rightRing3, "RightRing3",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightLittle1.position - rightLittle2.position).magnitude / 2, rightLittle1.position, rightLittle2.position, ColliderChoice.Hand, rightLittle1, "RightLittle1",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightLittle2.position - rightLittle3.position).magnitude / 2, rightLittle2.position, rightLittle3.position, ColliderChoice.Hand, rightLittle2, "RightLittle2",isGenerateColliderOpenTrigger));
                runtimeColliders.Add(CreateUntiyCapsuleCollider((rightLittle2.position - rightLittle3.position).magnitude / 2 * 0.8f, rightLittle3.position, rightLittle3.position + (rightLittle3.position - rightLittle2.position) * 0.8f, ColliderChoice.Hand, rightLittle3, "RightLittle3",isGenerateColliderOpenTrigger));
            }

            //OYM:恢复坐标
            animator.transform.localScale = scaleTemp;
            animator.transform.position = positionTemp;
            animator.transform.rotation = rotationTemp;
        }
        private static float CheckNearstPointToSegment(float MaxLength, Vector3 position, Vector3 direction, ColliderChoice choice, List<ADBRuntimePoint> pointTrans, bool isInverse = false)
        {
            if (pointTrans == null || pointTrans.Count == 0)
            {
                return MaxLength;
            }
            for (int i = 0; i < pointTrans.Count; i++)
            {
                if ((pointTrans[i].pointRead.colliderChoice & choice) == 0)
                    continue;

                if (direction == Vector3.zero)
                {
                    if (isInverse)
                    {
                        MaxLength = Mathf.Max(MaxLength, (position - pointTrans[i].trans.position).magnitude + pointTrans[i].pointRead.radius);
                    }
                    else
                    {
                        MaxLength = Mathf.Min(MaxLength, (position - pointTrans[i].trans.position).magnitude - pointTrans[i].pointRead.radius);
                    }

                }
                else
                {
                    Vector3 nearstPoint = position + direction * Mathf.Clamp01(Vector3.Dot(pointTrans[i].trans.position - position, direction) / direction.sqrMagnitude);
                    if (isInverse)
                    {
                        MaxLength = Mathf.Max(MaxLength, (nearstPoint - pointTrans[i].trans.position).magnitude + pointTrans[i].pointRead.radius);
                    }
                    else
                    {
                        MaxLength = Mathf.Min(MaxLength, (nearstPoint - pointTrans[i].trans.position).magnitude - pointTrans[i].pointRead.radius);
                    }

                }
            }
            return MaxLength;
        }
        private ADBColliderReader CreateUntiyBoxCollider(Vector3 position, Quaternion rotation, Vector3 size, ColliderChoice colliderChoice, Transform appendTransform, string name, bool isGenerateColliderOpenTrigger = true, CollideFunc collideFunc = CollideFunc.OutsideLimit, bool isOwner = false)
        {
            Transform transform = new GameObject(name + "BoxCollider").transform;
            transform.parent = appendTransform;
            transform.position = position;
            transform.rotation = rotation;


            BoxCollider boxCollider = transform.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = size;
            boxCollider.isTrigger = isGenerateColliderOpenTrigger;

            ADBColliderReader target = transform.gameObject.AddComponent<ADBColliderReader>();
            target.collideFunc = collideFunc;
            target.colliderChoice = colliderChoice;
            if (isOwner)
            {
                target.AddOwner(controller);
            }
            return target;
        }
        private ADBColliderReader CreateUntiyCapsuleCollider(float radius, Vector3 pointHead, Vector3 pointTail, ColliderChoice colliderChoice, Transform appendTransform, string name, bool isGenerateColliderOpenTrigger = true, CollideFunc collideFunc = CollideFunc.OutsideLimit, bool isOwner = false)
        {
            Transform transform = new GameObject(name + "CapsuleCollider").transform;
            transform.parent = appendTransform;
            transform.position = (pointHead + pointTail) / 2;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, pointTail - pointHead);


            CapsuleCollider capsuleCollider = transform.gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = radius;
            capsuleCollider.direction = 1;
            capsuleCollider.height = Vector3.Distance(pointHead, pointTail) + radius * 2;
            capsuleCollider.isTrigger = isGenerateColliderOpenTrigger;

            ADBColliderReader target = transform.gameObject.AddComponent<ADBColliderReader>();
            target.collideFunc = collideFunc;
            target.colliderChoice = colliderChoice;
            if (isOwner)
            {
                target.AddOwner(controller);
            }
            return target;
        }
        private ADBColliderReader CreateUnitySphereCollider(float radius, Vector3 position, ColliderChoice colliderChoice, Transform appendTransform, string name, bool isGenerateColliderOpenTrigger = true, CollideFunc collideFunc = CollideFunc.OutsideLimit, bool isOwner = false)
        {
            Transform transform = new GameObject(name + "SphereCollider").transform;
            transform.parent = appendTransform;
            transform.position = appendTransform.position + position;


            SphereCollider sphereCollider = transform.gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = isGenerateColliderOpenTrigger;
            sphereCollider.radius = radius;

            ADBColliderReader target = transform.gameObject.AddComponent<ADBColliderReader>();
            target.collideFunc = collideFunc;
            target.colliderChoice = colliderChoice;
            if (isOwner)
            {
                target.AddOwner(controller);
            }
            return target;
        }
    }
}
