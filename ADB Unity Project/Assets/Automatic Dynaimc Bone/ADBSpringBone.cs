using ADBRuntime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ADBSpringBone : MonoBehaviour
{
    //OYM：啊...怎么写才好
    private ADBSpringBone parent;
    private ADBSpringBone child;
    private List<ADBSpringBone> springboneChain;
    public ADBRuntimePoint runtimePoint;

    private void OnEnable()
    {
        if (parent == null)
        {
            springboneChain = new List<ADBSpringBone>();
            runtimePoint = new ADBRuntimePoint(transform,0);
        }
        if (child == null)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                //child = (transform.GetChild(i).GetComponent<ADBSpringBone>() )?? (transform.GetChild(i).gameObject.AddComponent<ADBSpringBone>()); 
                //上述写法存在潜在bug.要小心
                child = transform.GetChild(i).GetComponent<ADBSpringBone>();
                if (child == null)
                {
                    child = transform.GetChild(i).gameObject.AddComponent<ADBSpringBone>();
                }
                child.parent = this;
            }
            runtimePoint = new ADBRuntimePoint(transform, parent.runtimePoint.depth + 1);

        }

    }
}
