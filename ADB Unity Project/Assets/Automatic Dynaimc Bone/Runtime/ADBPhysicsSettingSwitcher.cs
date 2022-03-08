using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime.Mono
{
    [RequireComponent(typeof(ADBRuntimeController))]
    public class ADBPhysicsSettingSwitcher : MonoBehaviour
    {
        [SerializeField]
        private ADBRuntimeController runtimeController;
        [SerializeField]
        public ADBSettingLinker currentLinker;
        [SerializeField]
        public List<ADBSettingLinker> targetLinkers =new List<ADBSettingLinker>();
        int index = 0;
        public void Awake()
        {
            runtimeController= gameObject.GetComponent<ADBRuntimeController>(); 
        }
        public void Switch()
        {
            if (targetLinkers==null&&targetLinkers.Count==0)
            {
                return;
            }

            currentLinker = targetLinkers[index];
            for (int i = 0; i < runtimeController.allChain.Length; i++)
            {
                ADBChainProcessor chain = runtimeController.allChain[i];
                string keyword = chain.keyWord;
                ADBPhysicsSetting setting = currentLinker.GetSetting(keyword);
                chain.SetADBSetting(setting);
            }
            runtimeController.ResetData();

            index = index + 1 <targetLinkers.Count ? index + 1 : 0;
            
        }
    }
}