using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime
{
    /// <summary>
    /// A static windZone that provides wind power to the bones
    /// </summary>
    public class ADBWindZone 
    {
        private static ADBWindZone windZone;
        private float time=0;
        private Vector3 randomVec=Vector3.zero;
        private const float granularity = 0.005f;
        private ADBWindZone()
        {}

        /// <summary>
        /// Get force by space
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 getaddForceForce(Vector3 position)
        {
            if (windZone == null)
            {
                windZone = new ADBWindZone();
            }
            windZone.time = Time.time;
            windZone.randomVec += Random.insideUnitSphere* granularity;
            windZone.randomVec.y = 0;
            windZone.randomVec.Normalize();
            return windZone.randomVec.normalized * ( Mathf.Sin(windZone.time+ position.magnitude) * 0.25f+0.25f);
            
        }
    }
}

