using System;
using System.Data;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Video;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class SpawnScript : MonoBehaviour
{
    private const int RESOLUTION = 11;      //  Number of spheres per side.
    private const int EXTENT = 4;           //  Cube size: half of size length.

    public float stopAfterSecs = 0;

    public float strobeTime = 0;

    public GameObject spherePrefab;

    readonly System.Func<float, float, float, float, float, float> map =
        (v, from1, to1, from2, to2) =>
            Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v));

    //  Could make an array of these, but for simplicity:
    private GameObject[,,] cube1, cube2, cube3, cube4;

    /*  Create cube out of spheres - except there's no size, position, colour
        etc. at this point: it's nascent spheres at the origin. */

    private GameObject[,,] CreateCube() {
        GameObject[,,] result = new GameObject[RESOLUTION, RESOLUTION, RESOLUTION];

        for (int x = 0; x < RESOLUTION; x++) {
            for (int y = 0; y < RESOLUTION; y++) {
                for (int z = 0; z < RESOLUTION; z++) {
                    result[x, y, z] = 
                        Instantiate(spherePrefab,
                                    Vector3.zero,
                                    spherePrefab.transform.rotation);
                }
            }
        }

        return result;
    }

    private void UpdateCube(GameObject[,,] cube,
                            float xCentre,
                            float yCentre,
                            Func<Vector4, Vector4> colour,
                            Func<Vector4, Vector4> size,
                            Func<Vector4, Vector4> coords) {
        float t = 0;//map(Time.time % strobeTime, 0, strobeTime, -1, 1);               // takes 10 seconds to reach -1 to 1

        for (int x = 0; x < RESOLUTION; x++) {
            for (int y = 0; y < RESOLUTION; y++) {
                for (int z = 0; z < RESOLUTION; z++) {
                
                    // Normalised (-1..1) values for x, y, z ranges.
                    float normalX = map(x, 0, RESOLUTION - 1, -1, 1);
                    float normalY = map(y, 0, RESOLUTION - 1, -1, 1);
                    float normalZ = map(z, 0, RESOLUTION - 1, -1, 1);

                    Vector4 normals = new Vector4(normalX, normalY, normalZ, t);
                    Vector4 vCoords = coords(normals);

                    float xPos = map(vCoords.x, -1, 1, -EXTENT, EXTENT);
                    float yPos = map(vCoords.y, -1, 1, -EXTENT, EXTENT);
                    float zPos = map(vCoords.z, -1, 1, -EXTENT, EXTENT);

                if (cube == cube1)
                {
                    Vector4 pos = new Vector4(xPos, yPos, zPos, t);
                    pos.x += (pos.y + pos.z) * 0.5f;
                    xPos = pos.x; // Update xPos with the altered value
                }
                    GameObject obj = cube[x, y, z];
                    obj.transform.position = new Vector3(xCentre + xPos, yCentre + yPos, zPos);
                    Vector4 thisSize =
                        size(new Vector4(normalX, normalY, normalZ, t));
                    obj.transform.localScale =
                        new Vector3(map(thisSize.x, -1, 1, 0, 1),
                                    map(thisSize.y, -1, 1, 0, 1),
                                    map(thisSize.z, -1, 1, 0, 1));
                        ;
                    Renderer r = obj.GetComponent<Renderer>();
                    Vector4 v = colour(new Vector4(normalX, normalY, normalZ, t));
                    r.material.color = new Color(map(v.x, -1, 1, 0, 1),
                                                 map(v.y, -1, 1, 0, 1),
                                                 map(v.z, -1, 1, 0, 1));
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        cube1 = CreateCube();
        cube2 = CreateCube();
        cube3 = CreateCube();
        cube4 = CreateCube();
    }

    // Update is called once per frame
    void Update() {
        if (Time.time <= stopAfterSecs) {
            // Unipolar normalisation here (x/y/z args from -1 to 1):
            UpdateCube(cube1, -10, -10,
                        colour: (pos) => {
                            pos.x += (pos.y + pos.z) * 0.5f; 
                             return new Vector4(1, 1, 1); // Return the color
                        },
                        size: (_) => new Vector4(1, 1, 1),
                        coords: (v) => v     
                      );

// Colour Function:
// The color function takes a Vector4 position (pos) and modifies it as follows:
// pos.x is incremented by the average of pos.y and pos.z. This adjustment means that the x-coordinate of the position is dynamically altered based on the y and z coordinates, creating a dependence of the x-coordinate on the other two coordinates.
// The function returns a Vector4 with all components set to 1 (new Vector4(1, 1, 1)), which represents the color white. This indicates that regardless of the position, the cube will always be colored white.

// Size Function:
// The size function takes an input (ignored here, represented by _) and returns a Vector4 with all components set to 1 (new Vector4(1, 1, 1)). This means the cube will have a uniform size of 1 unit in all three dimensions (x, y, and z), resulting in a consistent cubic shape.

// Coords Function:
// The coordinates function takes a vector v and returns it unchanged (coords: (v) => v). This means the original coordinates of the cube are retained without any transformation.

            UpdateCube(cube2, +10, -10,
                        colour: (pos) =>{
                                if (pos.x < 0) {
                                    return new Vector4(-1, -1, -1);
                                }
                                else {
                                    return new Vector4(1, 1, 1);
                                }
                        },
                        size: (_) => new Vector4(1, 1, 1),
                        coords: (v) => v                  
                      );

// Colour Function:
// The color function checks the x-coordinate of the position:
// If pos.x is negative (pos.x < 0), it returns new Vector4(-1, -1, -1), which represents the color black.
// Otherwise, it returns new Vector4(1, 1, 1), representing the color white. This creates a binary color scheme where the cube's color is black if it's on the negative x side and white otherwise.

// Size Function:
// Similar to cube1, the size function returns a uniform size of 1 unit in all dimensions (new Vector4(1, 1, 1)), maintaining a consistent cubic shape.

// Coords Function:
// The coordinates function returns the input vector v unchanged, so the original coordinates are preserved.

            UpdateCube(cube3, -10, +10,
                        colour: (pos) => {
                                if ((Math.Abs(pos.x) == 1 && Math.Abs(pos.y) == 1 && Math.Abs(pos.z) == 1)) {     // && means AND
                                    return new Vector4(-1, -1, -1);
                                } else {
                                    return new Vector4(1, 1, 1);
                                }
                        },
                        size: (pos) => {
                                if (pos.magnitude > 1.5) {             // THIS IS WHAT MAKES THE CUBE INTO THE SPHERE SHAPE
                                    float f = 5;
                                    return new Vector4(f, 10, f);      // THE SPHERES OUTSIDE THE GIVEN MAGNITUDE RANGE ARE AFFECTED BY THIS CODE, MAKING THEM ZERO
                                } else {
                                    return new Vector4(-1, -1, -1);    // RETURNS THE SPHERE TO NOTHING, MEANING THAT THE SPHERES OUTSIDE MAGNITUDE ARE SO SMALL THEY ARE UNSEEN AS IT IS '-1'
                                }
                        },
                        coords: (v) => v
                      );

// Colour Function:
// The color function checks the absolute values of the x, y, and z coordinates of the position:
// If the absolute values of all three coordinates are 1 (Math.Abs(pos.x) == 1 && Math.Abs(pos.y) == 1 && Math.Abs(pos.z) == 1), it returns new Vector4(-1, -1, -1), representing black.
// Otherwise, it returns new Vector4(1, 1, 1), representing white. This results in a cube that changes color to black only if it's at a specific coordinate set (1, 1, 1) in all dimensions.

// Size Function:
// The size function checks the magnitude of the position vector:
// If the magnitude is greater than 1.5, it returns a size vector of new Vector4(5, 10, 5), significantly resizing the cube, making it larger and elongated on the y-axis. Now creating an inverse sphere shape in the cube.
// If the magnitude is 1.5 or less, it returns new Vector4(-1, -1, -1), shrinking the cube to a size so small that it becomes effectively invisible. This magnitude-based resizing introduces a dynamic size variation based on the cube's position.

// Coords Function:
// The coordinates function returns the input vector v unchanged.

            UpdateCube(cube4, +10, +10,
                        colour: (_) => Random.value < 0.1f ? new Vector4(1, -1, -1) : new Vector4(0f, 0f, 0f),
                        size: (_) => new Vector4(1, -0.6f, 1),
                        coords: (v) => new Vector4(v.x + Random.Range(-0.1f, 0.1f), v.y, v.z + Random.Range(-0.1f, 0.1f))
                      );

// Colour Function:
// The color function generates a random value and checks if it is less than 0.1 (Random.value < 0.1f):
// If true, it returns new Vector4(1, -1, -1), giving red.
// If false, it returns new Vector4(0f, 0f, 0f), giving the grey wanted. This introduces randomness in the cube's color, making it occasionally change to a specific color/effect.

// Size Function:
// The size function returns a non-uniform size vector (new Vector4(1, -0.6f, 1)) where the y-component is -0.6. This effectively flips the cube along the y-axis and shrinks it.

// Coords Function:
// The coordinates function introduces slight random displacements to the x and z coordinates:
// v.x is adjusted by a random value between -0.1 and 0.1 (Random.Range(-0.1f, 0.1f)).
// v.z is similarly adjusted by a random value between -0.1 and 0.1. This adds variability to the cube's position, showing the random movement shown in the wanted outcome for cube 4.
                      
        }
    }
}
