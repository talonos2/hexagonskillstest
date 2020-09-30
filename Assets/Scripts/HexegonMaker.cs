using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexegonMaker : MonoBehaviour
{
    public float offsetOfDepthFaces = .25f;
    public float depthOfDepthFaces = .25f;
    public float lengthOfDepthFaces = 1.2f;
    public float middlePokeyOutiness = .1f;
    public Material referenceMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    //Reference Code obtained from https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html

    void Start()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(referenceMaterial);

        meshFilter = gameObject.AddComponent<MeshFilter>();

        MakeARegularPolygon(6);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Normally, I'd ask the artists to make meshes and reference them as prefabs, but I think it'd be easier
    //to make my own mesh here than to find them online, plus it lets you see me do some of the trig I skipped
    //during colision detection.
    private void MakeARegularPolygon (int sides)
    {
        Mesh mesh = new Mesh();
        int vertsNeeded = 1 + (3 * sides);
        Vector3[] verts = new Vector3[vertsNeeded];
        verts[vertsNeeded-1] = new Vector3(0,0,-middlePokeyOutiness);
        float degrees = 0;
        float aLittleBitOffset = (2 * Mathf.PI / sides) * offsetOfDepthFaces;

        for (int x = 0; x < sides; x++)
        {
            verts[x] = new Vector3(Mathf.Sin(degrees), Mathf.Cos(degrees), 0);
            verts[x + sides] = new Vector3(Mathf.Sin(degrees - aLittleBitOffset) * lengthOfDepthFaces,
                                           Mathf.Cos(degrees - aLittleBitOffset) * lengthOfDepthFaces,
                                           depthOfDepthFaces);
            verts[x + sides*2] = new Vector3(Mathf.Sin(degrees + aLittleBitOffset) * lengthOfDepthFaces,
                                             Mathf.Cos(degrees + aLittleBitOffset) * lengthOfDepthFaces,
                                             depthOfDepthFaces);
            degrees += (2 * Mathf.PI / sides);
        }
        //All verts in. Make Triangles.
        int[] tris = new int[4 * 3 * sides];
        for (int x = 0; x < sides; x++)
        {
            int offsetInTriArray = x * 4 * 3;
            //The top face for the side:
            tris[offsetInTriArray] = vertsNeeded - 1;  //Center.
            tris[offsetInTriArray+1] = x; //This side's primary corner.
            tris[offsetInTriArray+2] = (x==sides-1?0:x+1); //This side's secondary corner, can wrap around.
            //The face for the primary corner's corner face
            tris[offsetInTriArray+3] = x; //This side's primary corner.
            tris[offsetInTriArray + 4] = x + sides;
            tris[offsetInTriArray + 5] = x + sides*2;
            //The face for the first tri between the primary and secondary corner
            tris[offsetInTriArray + 6] = x; //This side's primary corner.
            tris[offsetInTriArray + 7] = x + sides * 2;
            tris[offsetInTriArray + 8] = (x == sides - 1 ? 0 : x + 1); //This side's secondary corner, can wrap around.
            //The face for the second tri between the primary and secondary corner
            tris[offsetInTriArray + 9] = x + sides * 2;
            tris[offsetInTriArray + 10] = (x == sides - 1 ? 0 : x + 1) + sides;
            tris[offsetInTriArray + 11] = (x == sides - 1 ? 0 : x + 1); //This side's secondary corner, can wrap around.
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}
