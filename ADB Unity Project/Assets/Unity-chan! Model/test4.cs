using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test4 : MonoBehaviour
{
    // Start is called before the first frame update

    public Vector3 forward1;
    public Vector3 forward2;
    public float value=0.1f;
    public float lerp;

    // Update is called once per frame
    void Update()
    {
        lerp += Time.deltaTime*value;
        if (lerp < 1)
        {
            transform.localPosition = Vector3.Lerp(forward1, forward2, lerp);
        }
        else if (lerp < 2)
        {
            transform.localPosition = Vector3.Lerp(forward1, forward2, 2-lerp);
        }
        else
        {
            lerp = 0;
        }
    }
}
