using ADBRuntime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime.Mono
{
    public class ADBSpringBone : MonoBehaviour
    {
        [SerializeField]
        public List<string> generateKeyWordBlackList = new List<string> { "ik" };
        [SerializeField]
        public List<Transform> blackListOfGenerateTransform = new List<Transform>();
        [SerializeField]
        public ADBSetting aDBSetting;
        [SerializeField]
        public Transform fixedPointTransform;
        [SerializeField]
        public List<Transform> allTransfromList = new List<Transform>();

        public ADBRuntimePoint fixedNode; 
        private void Start()
        {
            var runtimeController = gameObject.GetComponentsInParent<ADBRuntimeController>();
            if (runtimeController == null || runtimeController.Length == 0)
            {
                Debug.Log(transform.name+" cannot find the ADB Runtime Controller ");
            }
        }

    }

}
