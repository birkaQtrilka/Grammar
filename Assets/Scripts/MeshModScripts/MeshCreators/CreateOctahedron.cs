using Demo;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctahedron : Shape
{
    [SerializeField] int Width;
    [SerializeField] int Depth;
    [SerializeField] float height = 1;
    [SerializeField] int numOfPoints = 4;
    List<Vector3> poss = new();
    public void Initialize(int Width, int Depth)
    {
        this.Width = Width;
        this.Depth = Depth;
    }

    protected override void Execute()
    {
		poss.Clear();
        MeshBuilder builder = new MeshBuilder();
        /*
		// V1, bad winding:
		int v1 = builder.AddVertex(new Vector3(1, 0, 0));
		int v2 = builder.AddVertex(new Vector3(0, 0,-1));
		int v3 = builder.AddVertex(new Vector3(-1,0, 0));
		int v4 = builder.AddVertex(new Vector3(0, 0, 1));
		int v5 = builder.AddVertex(new Vector3(0, 1, 0));
		int v6 = builder.AddVertex(new Vector3(0,-1, 0));

		// top:
		builder.AddTriangle(v1, v2, v5);
		builder.AddTriangle(v2, v3, v5);
		builder.AddTriangle(v3, v4, v5);
		builder.AddTriangle(v4, v1, v5);

		// bottom:
		builder.AddTriangle(v1, v2, v6);
		builder.AddTriangle(v2, v3, v6);
		builder.AddTriangle(v3, v4, v6);
		builder.AddTriangle(v4, v1, v6);

		/**/
        float halfWidth = Width * .5f;
        float halfDepth = Depth * .5f;
        // V2, correct winding:
  //      int v1 = builder.AddVertex(new Vector3(halfWidth, 0, halfDepth));
  //      int v2 = builder.AddVertex(new Vector3(halfWidth, 0, -halfDepth));
  //      int v3 = builder.AddVertex(new Vector3(-halfWidth, 0, -halfDepth));
  //      int v4 = builder.AddVertex(new Vector3(-halfWidth, 0, halfDepth));
		//builder.AddTriangle(v1,v2,v3);
		//builder.AddTriangle(v3, v4, v1);

		var increment = 360.0f / numOfPoints;

        float radius = Mathf.Max(halfWidth, halfDepth); // Match scale nicely
        List<int> topVerts = new();

        for (int i = 0; i < numOfPoints; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, increment * i, 0);
            Vector3 pos = rotation * Vector3.forward * radius;
            pos.y = height; // Raise to top
            topVerts.Add(builder.AddVertex(pos));
			poss.Add(pos);
        }


        for (int i = 1; i < numOfPoints - 1; i++)
            builder.AddTriangle(topVerts[0], topVerts[i], topVerts[i + 1]);

        //      var center = new Vector3(0, height, 0);
        //poss.Add(center);
        var bottomRect = new List<Vector3>
        {
            new (halfWidth, 0, halfDepth),
            new (halfWidth, 0, -halfDepth),
            new (-halfWidth, 0, -halfDepth),
            new (-halfWidth, 0, halfDepth),
        };

        // Add bottom rectangle vertices to the mesh builder
        List<int> bottomVerts = new();
        foreach (var v in bottomRect)
        {
            bottomVerts.Add(builder.AddVertex(v));
            poss.Add(v);
        }
        builder.AddTriangle(bottomVerts[0], bottomVerts[1], bottomVerts[2]);
        builder.AddTriangle(bottomVerts[2], bottomVerts[3], bottomVerts[0]);

        List<float> edgeLengths = new();
        float totalLength = 0;

        // Build a flat list of rectangle perimeter vertices (looped)
        for (int i = 0; i < 4; i++)
        {
            Vector3 from = bottomRect[i];
            Vector3 to = bottomRect[(i + 1) % 4];
            float len = Vector3.Distance(from, to);
            edgeLengths.Add(len);
            totalLength += len;
        }

        // Now walk the perimeter and add points at equal intervals
        List<Vector3> resampledRect = new();
        for (int i = 0; i < numOfPoints; i++)
        {
            float t = (float)i / numOfPoints * totalLength;
            float distSoFar = 0;
            for (int edge = 0; edge < 4; edge++)
            {
                if (t <= distSoFar + edgeLengths[edge])
                {
                    float localT = (t - distSoFar) / edgeLengths[edge];
                    Vector3 from = bottomRect[edge];
                    Vector3 to = bottomRect[(edge + 1) % 4];
                    Vector3 p = Vector3.Lerp(from, to, localT);
                    resampledRect.Add(p);
                    break;
                }
                distSoFar += edgeLengths[edge];
            }
        }

        // Add resampled rectangle vertices to the mesh
        List<int> resampledBottomVerts = new();
        foreach (var v in resampledRect)
        {
            resampledBottomVerts.Add(builder.AddVertex(v));
            poss.Add(v);
        }

        for (int i = 0; i < numOfPoints; i++)
        {
            int next = (i + 1) % numOfPoints;

            int b0 = resampledBottomVerts[i];
            int b1 = resampledBottomVerts[next];
            int t0 = topVerts[i];
            int t1 = topVerts[next];

            // Fixed triangle winding for correct surface normals
            builder.AddTriangle(b0, b1, t1);
            builder.AddTriangle(t1, t0, b0);
        }

        GetComponent<MeshFilter>().mesh = builder.CreateMesh();
    }


    void OnDrawGizmos()
    {
		for (int i = poss.Count-1; i >= 0; i--)
		{
			Gizmos.DrawSphere(transform.TransformPoint(poss[i]),.1f);
		}
    }
}
