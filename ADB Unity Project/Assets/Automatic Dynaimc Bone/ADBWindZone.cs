using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime
{
    public class ADBWindZone 
    {
        private static ADBWindZone windZone;
        private float time=0;
        private Vector3 randomVec=Vector3.zero;
        private ADBWindZone()
        {}


        public static Vector3 getWindForce(Vector3 position,float deltaTime)
        {
            if (deltaTime == 0)
            {
                return Vector3.zero;
            }
            if (windZone == null)
            {
                windZone = new ADBWindZone();
            }
            windZone.time += deltaTime;
            windZone.randomVec += Random.insideUnitSphere* deltaTime;
            windZone.randomVec.y = 0;
            windZone.randomVec.Normalize();
            return windZone.randomVec* Mathf.PerlinNoise(position.x+ windZone.time, position.y+ windZone.time) *0.2f;
            
        }
    }
}

