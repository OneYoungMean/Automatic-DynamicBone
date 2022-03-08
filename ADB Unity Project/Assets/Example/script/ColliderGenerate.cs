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
    public PhysicMaterial material;

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
        GameObject collider;
        
        switch (seed)
        {
            case 0:
                collider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                seed++;
                break;
            case 1:
                collider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                seed++;
                break;
            default:
                collider = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seed = 0;
                break;
        }
        collider.transform.position = transform.position;
        collider.transform.rotation = transform.rotation;
        collider.transform.localScale = transform.localScale;
        collider.GetComponent<Collider>().material = material;
        collider.AddComponent<Rigidbody>();
        collider.AddComponent<ADBColliderReader>();
    }
}
