using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ADBRuntime.Mono.Tool
{
    [DisallowMultipleComponent]
    public class ADBColliderGenerateTool : MonoBehaviour
    {
        [SerializeField]
        public bool isGenerateColliderAutomaitc = false;
        [SerializeField]
        public bool isGenerateByAllPoint = true;
        [SerializeField]
        public bool isGenerateFinger = false;
        [SerializeField]
        public bool isGenerateColliderOpenTrigger = true;
        [SerializeField]
        public List<ADBColliderReader> generateColliderList;
        [SerializeField]
        public float colliderSize=1;

        public void initializeCollider()
        {
            if (!isGenerateColliderAutomaitc)
            {
                generateColliderList = new List<ADBColliderReader>();
                generateColliderList.AddRange(gameObject.GetComponentsInChildren<ADBColliderReader>());
            }
            else
            {
                if (generateColliderList != null && generateColliderList.Count > 0)
                {
                    Debug.LogWarning("Pleace delete old generate collider before you want to generate new!");
                    return;
                }

                ADBChainProcessor[] chain = transform.GetComponentsInChildren<ADBChainProcessor>();
                List<ADBRuntimePoint> allNodeList;
                if (isGenerateByAllPoint)
                {
                    allNodeList = chain.SelectMany(x => x.allPointList).ToList();
                }
                else
                {
                    allNodeList = chain.SelectMany(x => x.fixedPointList).ToList();
                }

                if (allNodeList.Count == 0)
                {
                    Debug.Log("You can generate point first to get more accuracy collider");
                }
                generateColliderList = ADBStaticColliderFunc.GenerateBodyCollidersData(transform, allNodeList, isGenerateFinger, isGenerateColliderOpenTrigger, out int isGenerateSuccessful);
                for (int i = 0; i < generateColliderList.Count; i++)
                {
                    generateColliderList[i].transform.localScale *= colliderSize;
                }
                if (isGenerateSuccessful == -1)
                {
                    Debug.LogError("Some bug must be happen");
                }
                else if (isGenerateSuccessful == 0)
                {
                    Debug.LogError("cannot get che character avatar !");
                }
                else
                {
                    isGenerateColliderAutomaitc = false;
                }
            }
        }
    }
}