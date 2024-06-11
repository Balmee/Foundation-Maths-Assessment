using System;
using JetBrains.Annotations;
using UnityEngine;

public class SpawnScriptWorked : MonoBehaviour
{
    public GameObject spherePrefab;

    void CreateCube(float xOffset,
                    Func<float, float, float, Color> colourFn) {
        for (int x = -5; x <= 5; x++) {
            for (int y = -5; y <= 5; y++) {
                for (int z = -5; z <= 5; z++) {
                    GameObject obj = 
                        Instantiate(spherePrefab,
                                    new Vector3(x + xOffset, y, z),
                                    spherePrefab.transform.rotation);
                    Renderer r = obj.GetComponent<Renderer>();
                    float xn = (x / 10f) + 0.5f;
                    float yn = (y / 10f) + 0.5f;
                    float zn = (z / 10f) + 0.5f;
                    r.material.color = colourFn(xn, yn, zn);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateCube(+10, (x, y, z) => new Color(x, y, z));
        CreateCube(-10, (x, y, z) => Color.HSVToRGB(x, y, z));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
