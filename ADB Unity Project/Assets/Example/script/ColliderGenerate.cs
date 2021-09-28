using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ADBRuntime.Mono;
public class ColliderGenerate : MonoBehaviour
{
    // Start is called before the first frame update

    public  float interTime=3;
    private float innertime=0;
    int seed = 0;
    Random random;
    void Start()
    {
        Random.InitState(seed);
    }
    private void Update()
    {
        innertime += Time.deltaTime;
        if (innertime>interTime)
        {
            innertime -= interTime;
            CreateCollider(transform);
        }
    }
    void CreateCollider(Transform transform)
    {
        int target = (int)(Random.value * 3);
        GameObject collider;
        switch (target)
        {
            case 0:
                collider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case 1:
                collider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            default:
                collider = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }
        collider.transform.position = transform.position;
        collider.transform.rotation = transform.rotation;
        collider.transform.localScale = transform.localScale;

        collider.AddComponent<Rigidbody>();
        collider.AddComponent<ADBColliderReader>();
    }
}
