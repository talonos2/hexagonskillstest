using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonMaker : MonoBehaviour
{
    //These are basically constants. In other languages, these would be defines or static finals, etc,
    //but in Unity, anything that's public can be accessed by the inspector, so we make constants public
    //so a designer can frob these variables without needing to edit code to do so.

    [Tooltip("How far to the sides the corners of the depth faces are. Affects how 'rounded' the corners look.")]
    public float offsetOfDepthFaces = .25f;
    [Tooltip("The strength of the 3-D effect once you reach level 4.")]
    public float depthOfDepthFaces = .25f;
    [Tooltip("The strength of the bevel once you reach level 4.")]
    public float lengthOfDepthFaces = 1.2f;
    [Tooltip("How rounded the main face of the polygon looks. High values might make it look like a pyramid.")]
    public float middlePokeyOutiness = .1f;
    [Tooltip("Which material should the polygon clone to make its own?")]
    public Material referenceMaterial;
    [Tooltip("How small the polygon gets when you click it once. 1 = 100% size.")]
    public float pulseSize = .8f;
    [Tooltip("How long the 'pulse' effect from a polygon lasts when you click on it, in seconds.")]
    public float shapePulseTime = .4f;
    [Tooltip("How close together must two click be to count as a double-click, in seconds.")]
    public float doubleClickTime = .2f;

    //Public variables not intended as constants:
    [Tooltip("True if this is the reference polygon that you double-click on. False if it's the user-controlled polygon.")]
    public bool isReferencePolygon = false;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private float timeSinceLastClick = Mathf.Infinity;
    private float shapePulseAmount = 0;

    //Reference Code obtained from https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html. You'll note
    //it's basically unrecognizable now. ;)

    void Start()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(referenceMaterial);
        meshFilter = gameObject.AddComponent<MeshFilter>();

        if (isReferencePolygon)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        MakeARegularPolygon(6);

        if (!isReferencePolygon)
        {
            //this.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        //Used to detect double-clicks.
        timeSinceLastClick += Time.deltaTime;

        //When you click once, the hexagon pulses a little. This is because a normal user, if given a single
        //component on the screen, will click it. If it does something, they'll click it again. The hope is
        //that they will shortly begin mashing the click button and discover they must double-click it on their
        //own. If not, we fall back on an icon, which you'll see below.
        if (shapePulseAmount > 0)
        {
            float t = shapePulseAmount / shapePulseTime;
            float scale = Mathf.Lerp(1.0f, pulseSize, t);
            this.transform.localScale = new Vector3(scale, scale, scale);
            shapePulseAmount -= Time.deltaTime;
        }
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
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; //Disables shadow acne.

        if (isReferencePolygon)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    //If I had to implement click detection on a regular n-gon manually, I could. I'd do it by decomposing the shape into
    //n triangles, with each triangle being the center vert to the two ajacent corners. First, I'd convert all the verts to screen
    //space. Then, I'd find the area of one of the triangles (using Heron's/Hero's formula) and cache it. (Because this is a
    //regular n-gon, the areas of all n triangles are identical)

    //For each triangle: Imagine breaking it into three triangles. Each of those three triangles is comprised of two verts from the
    //original triangle, and the third point is the location of the mouse click. Calculate and add up the areas of all three of these
    //sub triangles. If the sum of the areas is within a floating-point rounding error of the original, it means the point is within
    //the triangle; you can register the hit test as true and short circuit out. If it's larger, it means the point is outside
    //the triangle; move onto the next triangle in the NGon and check that one.

    //I choose not to implement this below. One of the things that differentiates a good Unity programmer from a novice is knowing what
    //Unity gives to you "for free". As I was learning Unity in university, *far* too often I'd re-invent the wheel and spend hours
    //coding clever solutions to problems that Unity had already solved for me. Given that the original
    //skills test you sent was extremely simple, I can only assume you're planning on judging based on the "extra credit" you
    //suggested. Because of this, I choose to save tons of time by using Unity's out-of-the box solution so I can spend more time
    //on polish. This is in line with what I would do in a corporate environment: In the real world, you don't get bonus points for
    //making things harder on yourself.

    //So at any rate: OnMouseDown is called automatically by the mesh collider when a ray from the main camera through the clicked
    //point intersects with the collider. Only ever called by the reference polygon, because the user polygon lacks a collider.
    void OnMouseDown()
    {
        if (timeSinceLastClick < doubleClickTime)
        {
            SubmitMatch();
            timeSinceLastClick = Mathf.Infinity;
        }
        else
        {
            timeSinceLastClick = 0;
            shapePulseAmount = shapePulseTime;
        }
    }

    private void SubmitMatch()
    {
        GameController.Instance.Submit();
    }
}
