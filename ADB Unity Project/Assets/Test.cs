using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform trans;

    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector3(trans.position.x, -trans.position.y, trans.position.z);
        transform.rotation = new Quaternion( -trans.rotation.x, trans.rotation.y, -trans.rotation.z, trans.rotation.w);
        transform.localScale = trans.localScale;

    }
}
