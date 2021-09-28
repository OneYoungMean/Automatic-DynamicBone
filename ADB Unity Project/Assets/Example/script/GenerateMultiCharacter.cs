using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ADBRuntime.Mono;
[DefaultExecutionOrder(-10000)]
public class GenerateMultiCharacter : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject character;
    public int generateCount;

    private int sqrCount;
   private void Start()
    {
        character.transform.parent = transform;

        if (generateCount < 0)
        {
            Debug.Log("You must be a debugger,right?  :P");
            return;
        }
        sqrCount = Mathf.CeilToInt(Mathf.Sqrt(generateCount));
        int k = 1;
        for (int i = 0; i < sqrCount; i++)
        {
            for (int j = 0; j < sqrCount; j++)
            {
                if (k == generateCount)
                {
                    character.transform.position = new Vector3(i, 0, j);
                    return;
                }

                GameObject clone=  Instantiate(character, transform);
                Destroy(clone.GetComponent<ADBRuntimeController>());
                clone.transform.position = new Vector3(i, 0, j);
                k++;
            }
        }
    }
}
