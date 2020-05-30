using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs.LowLevel;
using Unity.Profiling;

namespace ADBRuntime.Internal
{
    public class ADBRuntimeJobsTableMono : MonoBehaviour
    {
        private ADBRunTimeJobsTable aDBRunTimeJobsTable;
        public bool jobsDebug;
        public int computeCount;
        void Start()
        {
            aDBRunTimeJobsTable = ADBRunTimeJobsTable.GetRunTimeJobsTable();
            DontDestroyOnLoad(gameObject);
        }
        private void Update()
        {
            if (jobsDebug )
            {
                Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobDebuggerEnabled = jobsDebug;
            }
            computeCount = aDBRunTimeJobsTable.computeCount;
            aDBRunTimeJobsTable.returnHJob.Complete();
        }
    }
}

