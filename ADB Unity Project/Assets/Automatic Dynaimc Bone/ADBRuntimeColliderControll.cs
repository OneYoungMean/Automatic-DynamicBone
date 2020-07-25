using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Jobs;
using Unity.Collections;

namespace ADBRuntime
{
    public class ADBRuntimeColliderControll
    {
        public List<ADBRuntimeCollider> runtimeColliderList;

        public bool initialized = false;
        private ColliderRead[] collidersReadTable;
        private ColliderReadWrite[] collidersReadWriteTable;
        private Transform[] colliderTransform;

        #region Point and parameter
        public enum ColliderBody
        {
            Hips = 1,
            Spine = 2,
            Head = 3,
            leftUpperArm = 4,
            leftLowerArm = 5,
            rightUpperArm = 6,
            rightLowerArm = 7,
            leftUpperLeg = 8,
            leftLowerLeg = 9,
            rightUpperLeg = 10,
            rightLowerLeg = 11,
            leftFoot = 12,
            rightFoot = 13,
            leftHand = 14,
            rightHand = 15,

        }

        public const float upperArmWidthAspect = 1f;
        public const float lowerArmWidthAspect = 0.9f;
        public const float endArmWidthAspect = 0.81f;

        public const float upperLegWidthAspect = 1f;
        public const float lowerLegWidthAspect = 0.7f;
        public const float endLegWidthAspect = 0.7f;

        public Vector3 rootPoint;
        public Vector3 headStartPoint;
        public Vector3 headCenterPoint;
        public float headColliderRadiu;

        public Vector3 spineStopPoint;
        public Vector3 spineStartPoint;
        public float spineColliderRadiu;

        public Vector3 hipsStopPoint;
        private float hipsColliderRadiuUp;
        private float hipsColliderRadiuDown;
        public Vector3 hipsStartPoint;

        public Vector3 upperArmToHeadCentroid;
        public Vector3 upperLegCentroid;

        public Vector3 leftHandCenterPoint;
        public Vector3 rightHandCenterPoint;
        public Vector3 leftFootCenterPoint;
        public Vector3 rightFootCenterPoint;

        public float torsoWidth;
        public float hipsWidth;
        public float headToRootHigh;
        #endregion

        public ADBRuntimeColliderControll(GameObject character, List<ADBRuntimePoint> allPointTrans, bool isGenerateBodyRuntimeCollider,bool isGenerateScript, out List<ADBEditorCollider> editorColliderList )
        {
            runtimeColliderList = new List<ADBRuntimeCollider>();
            editorColliderList = new List<ADBEditorCollider>();

            initialized = false;
            bool iniA = false, iniB=false, iniC=false; 
            if (isGenerateBodyRuntimeCollider)
            {
                iniA = GenerateBodyCollidersData(ref runtimeColliderList, character, allPointTrans);
            }
            if (isGenerateScript)
            {
                for (int i = 0; i < runtimeColliderList.Count; i++)
                {
                    if (runtimeColliderList[i].appendTransform == null) continue;
               
                    ADBEditorCollider.RuntimeCollider2Editor(runtimeColliderList[i]);
                }
            }
            iniB = GenerateOtherCollidersData(ref runtimeColliderList,ref editorColliderList, character);
            
            for (int i = 0; i < runtimeColliderList.Count; i++)
            {
                runtimeColliderList[i].colliderRead.isConnectWithBody = true;
            }

            iniC = GenerateGlobalCollidersData(ref runtimeColliderList);

            initialized = iniA || iniB || iniC;

            if (initialized && Application.isPlaying)
            {
                colliderTransform = new Transform[runtimeColliderList.Count];
                collidersReadTable = new ColliderRead[runtimeColliderList.Count];
                collidersReadWriteTable = new ColliderReadWrite[runtimeColliderList.Count];

                for (int i = 0; i < runtimeColliderList.Count; i++)
                {
                    collidersReadTable[i] = runtimeColliderList[i].GetColliderRead();
                    colliderTransform[i] = runtimeColliderList[i].appendTransform;
                }
            }
            if (!initialized)
            {
                Debug.Log("SomeThing in ADBRuntimeColliderControll is wrong....");
            }
        }

        private bool GenerateGlobalCollidersData(ref List<ADBRuntimeCollider> runtimeColliderList)
        {
            if (Application.isPlaying&&ADBEditorCollider.globalColliderList != null)
            {
                runtimeColliderList.AddRange(ADBEditorCollider.globalColliderList);
                return true;
            }
            return false;
        }

        internal void GetData(ref DataPackage dataPackage)
        {
            if (!initialized) return;
            dataPackage.SetColliderPackage(collidersReadTable, collidersReadWriteTable, colliderTransform);
        }

        private bool GenerateBodyCollidersData(ref List<ADBRuntimeCollider> runtimeColliderList, GameObject character, List<ADBRuntimePoint> allPointTrans)
        {
            if (!character) return false;

            var animator = character.GetComponent<Animator>();
            if (animator != null && animator.avatar.isHuman)
            {
                GenerateCollidersData(ref runtimeColliderList, allPointTrans, animator);
                return true;

            }
            else
            {
                var animators = character.GetComponentsInChildren<Animator>();
                if (animators != null && animators.Length != 0)
                {
                    bool isFind = false;
                    for (int i = 0; i < animators.Length; i++)
                    {
                        isFind = isFind || GenerateBodyCollidersData(ref runtimeColliderList, animators[i].gameObject, allPointTrans);
                    }
                }
                else
                {
                    Debug.Log(character.name + "'s Avatar is lost or isn't Human!");
                }
            }
            return false;
        }

        private bool GenerateOtherCollidersData(ref List<ADBRuntimeCollider> runtimeColliderList, ref List<ADBEditorCollider> editorColliderList, GameObject character)
        {
            ADBEditorCollider[] colliderList = character.GetComponentsInChildren<ADBEditorCollider>(true);
            foreach (var collider in colliderList)
            {
                var co = collider.GetCollider();

                if (co != null)
                {
                    runtimeColliderList.Add(co);
                    collider.isDraw = false;
                }
            }
            if (editorColliderList != null)
            {
                editorColliderList.AddRange(colliderList);
            }
            return true;
        }

        public void OnDrawGizmos()
        {
            if (!initialized) return;

            for (int i = 0; i < runtimeColliderList.Count; i++)
            {
                if (runtimeColliderList[i].appendTransform)
                {
                    runtimeColliderList[i].OnDrawGizmos();
                }
            }
        }


        private void GenerateCollidersData(ref List<ADBRuntimeCollider> runtimeColliders, List<ADBRuntimePoint> allPointTrans, Animator animator)
        {//OYM：这坨屎山我连写注释的兴趣都没有,你知道这玩意能大概把你角色圈进去就行
            //OYM：你问我怎么算的?当然是经验(试出来)啦 XD
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
            runtimeColliders.Add(new SphereCollider(headColliderRadiu, head.InverseTransformPoint(head.position+ new Vector3(0, 0.5f * torsoWidth, 0)),ColliderChoice.Head, head));

            // Spine
            spineStartPoint = headCenterPoint + new Vector3(0,  - torsoWidth, 0);
            spineStopPoint = upperLegCentroid;
            spineColliderRadiu = CheckNearstPointToSegment(torsoWidth, spineStartPoint, spineStopPoint - spineStartPoint, ColliderChoice.UpperBody, allPointTrans);

            runtimeColliders.Add(new CapsuleCollider(spineColliderRadiu, spineStartPoint, spineStopPoint, ColliderChoice.UpperBody, spine));

            //Hip

            Vector3 hipColliderCenter =upperLegCentroid;

            hipsColliderRadiuUp = CheckNearstPointToSegment((spineColliderRadiu *2), hipColliderCenter, Vector3.zero, ColliderChoice.UpperBody, allPointTrans);
            runtimeColliders.Add(new SphereCollider(hipsColliderRadiuUp,spine.InverseTransformPoint(hipColliderCenter) , ColliderChoice.UpperBody, spine));
            Vector3 hipColliderCenterDownA = upperLegCentroid- new Vector3(hipsWidth*0.5f, 0, 0);
            Vector3 hipColliderCenterDownB = hipColliderCenterDownA + new Vector3(hipsWidth, 0,0);
            hipsColliderRadiuDown = CheckNearstPointToSegment(hipsWidth, hipColliderCenterDownA, hipColliderCenterDownB- hipColliderCenterDownA, ColliderChoice.LowerBody, allPointTrans);

            runtimeColliders.Add(new CapsuleCollider(hipsColliderRadiuDown, hipColliderCenterDownA, hipColliderCenterDownB, ColliderChoice.LowerBody, pelvis));


            // LeftArms

            float leftArmWidth = Vector3.Distance(leftUpperArm.position, leftLowerArm.position) * 0.3f;
            float leftUpperArmWidth = CheckNearstPointToSegment(leftArmWidth * upperArmWidthAspect, leftUpperArm.position, leftLowerArm.position - leftUpperArm.position, ColliderChoice.UpperArm, allPointTrans);
            float leftLowerArmWidth = CheckNearstPointToSegment(leftArmWidth * lowerArmWidthAspect, leftLowerArm.position, leftHand.position - leftLowerArm.position, ColliderChoice.LowerArm, allPointTrans);

            runtimeColliders.Add(new CapsuleCollider(leftUpperArmWidth, leftUpperArm.position, leftLowerArm.position, ColliderChoice.UpperArm, leftUpperArm));
            runtimeColliders.Add(new CapsuleCollider(leftLowerArmWidth, leftLowerArm.position, leftHand.position, ColliderChoice.LowerArm, leftLowerArm));
            var leftHandCenterPoint = (leftFinger.position + leftHand.position) * 0.5f;

            runtimeColliders.Add(new SphereCollider(Vector3.Distance(leftHand.position, leftHandCenterPoint),  leftHand.InverseTransformPoint(leftHandCenterPoint), ColliderChoice.Hand,leftHand));

            // LeftLegs
            float leftLegWidth = Vector3.Distance(leftUpperLeg.position, leftLowerLeg.position) * 0.3f;
            float leftUpperLegWidth = CheckNearstPointToSegment(leftLegWidth * upperLegWidthAspect, leftUpperLeg.position, leftLowerLeg.position - leftUpperLeg.position, ColliderChoice.UpperLeg, allPointTrans);
            float leftLowerLegWidth = CheckNearstPointToSegment(leftLegWidth * lowerLegWidthAspect, leftLowerLeg.position, leftHand.position - leftLowerLeg.position, ColliderChoice.LowerLeg, allPointTrans);
            float leftEndLegWidth = leftLegWidth * endLegWidthAspect;

            runtimeColliders.Add(new CapsuleCollider(leftUpperLegWidth, leftUpperLeg.position - new Vector3(0, leftUpperLegWidth, 0), leftLowerLeg.position,ColliderChoice.UpperLeg, leftUpperLeg));

            runtimeColliders.Add(new CapsuleCollider(leftLowerLegWidth, leftLowerLeg.position, leftFoot.position, ColliderChoice.LowerLeg, leftLowerLeg));
            // LeftFoot

            if (leftToes != null)
            {
                runtimeColliders.Add(new CapsuleCollider(leftEndLegWidth, leftFoot.position, leftToes.position, ColliderChoice.Foot,leftFoot));
            }
            else
            {
                Vector3 leftfootStartPoint = leftFoot.position;
                Vector3 leftfootStopPoint = new Vector3(leftfootStartPoint.x, animator.rootPosition.y + leftEndLegWidth, leftfootStartPoint.z) + animator.rootRotation * Vector3.forward * (leftLowerArm.position - leftHand.position).magnitude * endLegWidthAspect;
                runtimeColliders.Add(new CapsuleCollider(leftEndLegWidth, leftfootStartPoint, leftfootStopPoint, ColliderChoice.Foot, leftFoot));
            }

            // rightArms

            float rightArmWidth = Vector3.Distance(rightUpperArm.position, rightLowerArm.position) * 0.3f;
            float rightUpperArmWidth = CheckNearstPointToSegment(rightArmWidth * upperArmWidthAspect, rightUpperArm.position, rightLowerArm.position - rightUpperArm.position, ColliderChoice.UpperArm, allPointTrans);
            float rightLowerArmWidth = CheckNearstPointToSegment(rightArmWidth * lowerArmWidthAspect, rightLowerArm.position, rightHand.position - rightLowerArm.position, ColliderChoice.LowerArm, allPointTrans);

            runtimeColliders.Add(new CapsuleCollider(rightUpperArmWidth, rightUpperArm.position, rightLowerArm.position, ColliderChoice.UpperArm, rightUpperArm));
            runtimeColliders.Add(new CapsuleCollider(rightLowerArmWidth, rightLowerArm.position, rightHand.position, ColliderChoice.LowerArm, rightLowerArm));
            var rightHandCenterPoint = (rightFinger.position + rightHand.position) * 0.5f;

            runtimeColliders.Add(new SphereCollider(Vector3.Distance(rightHand.position, rightHandCenterPoint),  rightHand.InverseTransformPoint(rightHandCenterPoint), ColliderChoice.Hand, rightHand));

            // rightLegs
            float rightLegWidth = Vector3.Distance(rightUpperLeg.position, rightLowerLeg.position) * 0.3f;
            float rightUpperLegWidth = CheckNearstPointToSegment(rightLegWidth * upperLegWidthAspect, rightUpperLeg.position, rightLowerLeg.position - rightUpperLeg.position, ColliderChoice.UpperLeg, allPointTrans);
            float rightLowerLegWidth = CheckNearstPointToSegment(rightLegWidth * lowerLegWidthAspect, rightLowerLeg.position, rightHand.position - rightLowerLeg.position, ColliderChoice.LowerLeg, allPointTrans);
            float rightEndLegWidth = rightLegWidth * endLegWidthAspect;

            runtimeColliders.Add(new CapsuleCollider(rightUpperLegWidth, rightUpperLeg.position - new Vector3(0, rightUpperLegWidth, 0), rightLowerLeg.position, ColliderChoice.UpperLeg, rightUpperLeg));

            runtimeColliders.Add(new CapsuleCollider(rightLowerLegWidth, rightLowerLeg.position, rightFoot.position, ColliderChoice.LowerLeg, rightLowerLeg));
            // rightFoot

            if (rightToes != null)
            {
                runtimeColliders.Add(new CapsuleCollider(rightEndLegWidth, rightFoot.position, rightToes.position, ColliderChoice.Foot, rightFoot));
            }
            else
            {
                Vector3 rightfootStartPoint = rightFoot.position;
                Vector3 rightfootStopPoint = new Vector3(rightfootStartPoint.x, animator.rootPosition.y + rightEndLegWidth, rightfootStartPoint.z) + animator.rootRotation * Vector3.forward * (rightLowerArm.position - rightHand.position).magnitude * endLegWidthAspect;
                runtimeColliders.Add(new CapsuleCollider(rightEndLegWidth, rightfootStartPoint, rightfootStopPoint, ColliderChoice.Foot, rightFoot));
            }

            // fingre and other
            /*
            //OYM：Left
            var leftThumb1= animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
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

            var leftHandLength = (leftMiddle2.position - leftHand.position).sqrMagnitude;
            runtimeColliders.Add(new OBBBox( leftHandCenterPoint,new Vector3(leftHandLength*0.25f, leftHandLength, leftHandLength) , leftFinger.position - leftHand.position, leftHand));

            runtimeColliders.Add(new CapsuleCollider((leftThumb1.position - leftThumb2.position).magnitude, leftThumb1.position, leftThumb2.position, leftThumb1));
            runtimeColliders.Add(new CapsuleCollider((leftThumb2.position - leftThumb3.position).magnitude, leftThumb2.position, leftThumb3.position, leftThumb2));
            runtimeColliders.Add(new CapsuleCollider((leftThumb2.position - leftThumb3.position).magnitude*0.8f, leftThumb3.position, leftThumb3.position+(leftThumb2.position - leftThumb3.position)*0.8f, leftThumb3));
            runtimeColliders.Add(new CapsuleCollider((leftIndex1.position - leftIndex2.position).magnitude, leftIndex1.position, leftIndex2.position, leftIndex1));
            runtimeColliders.Add(new CapsuleCollider((leftIndex2.position - leftIndex3.position).magnitude, leftIndex2.position, leftIndex3.position, leftIndex2));
            runtimeColliders.Add(new CapsuleCollider((leftIndex2.position - leftIndex3.position).magnitude * 0.8f, leftIndex3.position, leftIndex3.position + (leftIndex2.position - leftIndex3.position) * 0.8f, leftIndex3));
            runtimeColliders.Add(new CapsuleCollider((leftMiddle1.position - leftMiddle2.position).magnitude, leftMiddle1.position, leftMiddle2.position, leftMiddle1));
            runtimeColliders.Add(new CapsuleCollider((leftMiddle2.position - leftMiddle3.position).magnitude, leftMiddle2.position, leftMiddle3.position, leftMiddle2));
            runtimeColliders.Add(new CapsuleCollider((leftMiddle2.position - leftMiddle3.position).magnitude * 0.8f, leftMiddle3.position, leftMiddle3.position + (leftMiddle2.position - leftMiddle3.position) * 0.8f, leftMiddle3));
            runtimeColliders.Add(new CapsuleCollider((leftRing1.position - leftRing2.position).magnitude, leftRing1.position, leftRing2.position, leftRing1));
            runtimeColliders.Add(new CapsuleCollider((leftRing2.position - leftRing3.position).magnitude, leftRing2.position, leftRing3.position, leftRing2));
            runtimeColliders.Add(new CapsuleCollider((leftRing2.position - leftRing3.position).magnitude * 0.8f, leftRing3.position, leftRing3.position + (leftRing2.position - leftRing3.position) * 0.8f, leftRing3));
            runtimeColliders.Add(new CapsuleCollider((leftLittle1.position - leftLittle2.position).magnitude, leftLittle1.position, leftLittle2.position, leftLittle1));
            runtimeColliders.Add(new CapsuleCollider((leftLittle2.position - leftLittle3.position).magnitude, leftLittle2.position, leftLittle3.position, leftLittle2));
            runtimeColliders.Add(new CapsuleCollider((leftLittle2.position - leftLittle3.position).magnitude * 0.8f, leftLittle3.position, leftLittle3.position + (leftLittle2.position - leftLittle3.position) * 0.8f, leftLittle3));
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

            var rightHandLength = (rightMiddle2.position - rightHand.position).sqrMagnitude;
            runtimeColliders.Add(new OBBBox(rightHandCenterPoint, new Vector3(rightHandLength * 0.25f, rightHandLength, rightHandLength), rightFinger.position - rightHand.position, rightHand));

            runtimeColliders.Add(new CapsuleCollider((rightThumb1.position - rightThumb2.position).magnitude, rightThumb1.position, rightThumb2.position, rightThumb1));
            runtimeColliders.Add(new CapsuleCollider((rightThumb2.position - rightThumb3.position).magnitude, rightThumb2.position, rightThumb3.position, rightThumb2));
            runtimeColliders.Add(new CapsuleCollider((rightThumb2.position - rightThumb3.position).magnitude * 0.8f, rightThumb3.position, rightThumb3.position + (rightThumb2.position - rightThumb3.position) * 0.8f, rightThumb3));
            runtimeColliders.Add(new CapsuleCollider((rightIndex1.position - rightIndex2.position).magnitude, rightIndex1.position, rightIndex2.position, rightIndex1));
            runtimeColliders.Add(new CapsuleCollider((rightIndex2.position - rightIndex3.position).magnitude, rightIndex2.position, rightIndex3.position, rightIndex2));
            runtimeColliders.Add(new CapsuleCollider((rightIndex2.position - rightIndex3.position).magnitude * 0.8f, rightIndex3.position, rightIndex3.position + (rightIndex2.position - rightIndex3.position) * 0.8f, rightIndex3));
            runtimeColliders.Add(new CapsuleCollider((rightMiddle1.position - rightMiddle2.position).magnitude, rightMiddle1.position, rightMiddle2.position, rightMiddle1));
            runtimeColliders.Add(new CapsuleCollider((rightMiddle2.position - rightMiddle3.position).magnitude, rightMiddle2.position, rightMiddle3.position, rightMiddle2));
            runtimeColliders.Add(new CapsuleCollider((rightMiddle2.position - rightMiddle3.position).magnitude * 0.8f, rightMiddle3.position, rightMiddle3.position + (rightMiddle2.position - rightMiddle3.position) * 0.8f, rightMiddle3));
            runtimeColliders.Add(new CapsuleCollider((rightRing1.position - rightRing2.position).magnitude, rightRing1.position, rightRing2.position, rightRing1));
            runtimeColliders.Add(new CapsuleCollider((rightRing2.position - rightRing3.position).magnitude, rightRing2.position, rightRing3.position, rightRing2));
            runtimeColliders.Add(new CapsuleCollider((rightRing2.position - rightRing3.position).magnitude * 0.8f, rightRing3.position, rightRing3.position + (rightRing2.position - rightRing3.position) * 0.8f, rightRing3));
            runtimeColliders.Add(new CapsuleCollider((rightLittle1.position - rightLittle2.position).magnitude, rightLittle1.position, rightLittle2.position, rightLittle1));
            runtimeColliders.Add(new CapsuleCollider((rightLittle2.position - rightLittle3.position).magnitude, rightLittle2.position, rightLittle3.position, rightLittle2));
            runtimeColliders.Add(new CapsuleCollider((rightLittle2.position - rightLittle3.position).magnitude * 0.8f, rightLittle3.position, rightLittle3.position + (rightLittle2.position - rightLittle3.position) * 0.8f, rightLittle3));

            for (int i =1; i <= 30; i++)
            {
                runtimeColliders[runtimeColliders.Count - i].colliderRead.isOpen = false;
            }
            */
        }
        public static float CheckNearstPointToSegment(float MaxLength, Vector3 position, Vector3 direction,ColliderChoice choice ,List<ADBRuntimePoint> pointTrans)
        {
            if (pointTrans==null||pointTrans.Count == 0)
            {
                return MaxLength;
            }
            for (int i = 0; i < pointTrans.Count; i++)
            {
                if ((pointTrans[i].pointRead.colliderChoice & choice) == 0) 
                    continue;

                if (direction == Vector3.zero)
                {
                    MaxLength = Mathf.Min(MaxLength, (position - pointTrans[i].trans.position).magnitude);
                }
                else
                {
                    Vector3 nearstPoint = position + direction * Mathf.Clamp01(Vector3.Dot(pointTrans[i].trans.position - position, direction) / direction.sqrMagnitude);
                    MaxLength = Mathf.Min(MaxLength, (nearstPoint - pointTrans[i].trans.position).magnitude-0.001f);
                }
            }
            return MaxLength;
        }
    }
}
