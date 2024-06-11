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
    private const int RESOLUTION = 15;      // Number of spheres per side.
    private const int EXTENT = 5;           // Cube size: half of size length.

    public float stopAfterSecs = 0;
    public float strobeTime = 10f;
    public GameObject spherePrefab;

    readonly System.Func<float, float, float, float, float, float> map =
        (v, from1, to1, from2, to2) =>
            Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v));

    private GameObject[,,] cube1, cube2, cube3, cube4;

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
        float t = map(Time.time % strobeTime, 0, strobeTime, -1, 1);

        for (int x = 0; x < RESOLUTION; x++) {
            for (int y = 0; y < RESOLUTION; y++) {
                for (int z = 0; z < RESOLUTION; z++) {
                    float normalX = map(x, 0, RESOLUTION - 1, -1, 1);
                    float normalY = map(y, 0, RESOLUTION - 1, -1, 1);
                    float normalZ = map(z, 0, RESOLUTION - 1, -1, 1);

                    Vector4 normals = new Vector4(normalX, normalY, normalZ, t);
                    Vector4 vCoords = coords(normals);

                    float xPos = map(vCoords.x, -1, 1, -EXTENT, EXTENT);
                    float yPos = map(vCoords.y, -1, 1, -EXTENT, EXTENT);
                    float zPos = map(vCoords.z, -1, 1, -EXTENT, EXTENT);

                    GameObject obj = cube[x, y, z];
                    obj.transform.position = new Vector3(xCentre + xPos, yCentre + yPos, zPos);

                    Vector4 thisSize = size(new Vector4(normalX, normalY, normalZ, t));
                    obj.transform.localScale = new Vector3(
                        map(thisSize.x, -1, 1, 0, 1),
                        map(thisSize.y, -1, 1, 0, 1),
                        map(thisSize.z, -1, 1, 0, 1)
                    );

                    Renderer r = obj.GetComponent<Renderer>();
                    Vector4 v = colour(new Vector4(normalX, normalY, normalZ, t));
                    r.material.color = new Color(
                        map(v.x, -1, 1, 0, 1),
                        map(v.y, -1, 1, 0, 1),
                        map(v.z, -1, 1, 0, 1)
                    );
                }
            }
        }
    }

    void Start() {
        cube1 = CreateCube();
        cube2 = CreateCube();
        cube3 = CreateCube();
    }

    void Update()
    {
        if (Time.time <= stopAfterSecs)
        {
            // Coords
            UpdateCube(cube1, -10, -10,
                        colour: (v) => new Vector4(-1, -1, 1),  // blue
                        size: (v) => new Vector4(0.5f, 0.5f, 0.5f),  
                        coords: (v) => new Vector4(
                            Mathf.Sin(v.x * v.w * Time.time),   // use of trig functions give the unique movement for each individual sphere against time for this cube
                            Mathf.Cos(v.y * v.w * Time.time),
                            Mathf.Sin(v.z * v.w * Time.time),
                            v.w
                        )
                      );

            // Colors
            UpdateCube(cube2, +0, +10,
                        colour: (v) => new Vector4(
                            Random.Range(-1f, 1f),         // creates a random point for the colours to be selected from allowing for a range of colours to be created through RGB
                            Random.Range(-1f, 1f),
                            Random.Range(-1f, 1f),
                            1
                        ),
                        size: (pos) =>
                        {
                            if (pos.magnitude < 1)    // used to determine the size of a cube based on its distance from the origin
                            {
                                return new Vector4(Random.Range(-1f, 1f),
                                            Random.Range(-1f, 1f),
                                            Random.Range(-1f, 1f));    // THE SPHERES OUTSIDE THE GIVEN MAGNITUDE RANGE ARE AFFECTED BY THIS CODE, MAKING THEM ZERO
                            }
                            else
                            {
                                return new Vector4(-1, -1, -1);  // RETURNS THE SPHERE TO NOTHING, MEANING THAT THE SPHERES OUTSIDE MAGNITUDE ARE SO SMALL THEY ARE UNSEEN AS IT IS '-1'
                            }
                        },
                        coords: (v) => v
                      );

            // Size 
            UpdateCube(cube3, +10, -10,
                        colour: (v) => v,
                        size: (v) =>
                        {
                            float size = map(v.magnitude,   // cubes closer to the origin appear larger, while those farther away appear smaller
                                            2, (float)Math.Sqrt(1), 2, -2);   // Returns the square root of 1, the whole line being higher than this (2) allows the size range to be increased, which is shown
                            return new Vector4(size, size, size);
                        },
                        coords: (v) => v  // Static position
            );
        }
    }
}

// Cube 1: Coords Cube

// The first call to UpdateCube modifies cube1 with specific color, size, and coordinate functions. 
// The color function, colour: (v) => new Vector4(-1, -1, 1), sets the color of the cube to a fixed blue, the size function, size: (v) => new Vector4(0.5f, 0.5f, 0.5f), assigns a fixed size to the cube. Each dimension (x, y, z) of the cube is set to 0.5f, reducing the cube to half its original size, ensuring a uniform scaling effect.
// The coordinate function is more dynamic. The line coords: (v) => new Vector4(Mathf.Sin(v.x * v.w * Time.time), Mathf.Cos(v.y * v.w * Time.time), Mathf.Sin(v.z * v.w * Time.time), v.w) creates an oscillating effect. It uses the sine and cosine functions in combination with Time.time to generate periodic movement. The v.w component remains constant, ensuring that the oscillation affects only the spatial coordinates. As time progresses, the cube's position changes sinusoidally, creating a smooth, continuous motion.


// Cube 2: Colour Cube

// The color function for this cube, colour: (v) => new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f), 1), assigns a random color each time the function is called. Each color component (red, green, blue) is randomly selected within the range of -1f to 1f, resulting in a wide variety of possible colors, and the alpha component is fixed at 1, making the cube fully opaque.
// The size function, size: (pos) => { if (pos.magnitude < 1) { return new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)); } else { return new Vector4(-1, -1, -1); } }, introduces conditional logic based on the cube’s position magnitude. If the distance from the origin (pos.magnitude) is less than 1, the size is set to random values between -1f and 1f for each dimension, resulting in the bigger sphere appearance when if the magnitude is 1 or greater as the size is set to (-1, -1, -1), effectively making the cube so small that it becomes invisible. The shape of the spheres is also random, giving different shapes as a result also.
// The coordinate function, coords: (v) => v, ensures that the cube's position remains unchanged. The position vector v is directly returned without modification.


// Cube 3: Size Cube

// For the color, the function colour: (v) => v simply retains the original color of the cube, meaning the color is unaffected by this update and remains whatever it was initially set to, which happens to make a rainbow like pattern.
// The size function is defined as size: (v) => { float size = map(v.magnitude, 2, (float)Math.Sqrt(1), 2, -2); return new Vector4(size, size, size); }. This function uses a custom map function to adjust the cube's size based on its position magnitude. The magnitude of the position vector v is remapped from the range 2 to sqrt(1) (which is 1) to a new range 2 to -2. This means that as the cube moves further from or closer to the origin, its size scales proportionally. The result is a size vector (size, size, size) where each dimension is scaled uniformly. Having the scale above 1 allowed for the size affect to be more prominent throughout the cube.
// Finally, the coordinate function, coords: (v) => v, ensures that the cube's position remains static. The input vector v is returned unchanged, keeping the cube in its current position; so no spheres will leave formation.


