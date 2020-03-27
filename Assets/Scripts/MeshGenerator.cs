using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

	protected const int verticesPerLayer = 24;
	protected const float minSize = 0.1f;
	
	public float dwidth,ddepth,dlength,dcurvature,ddistribution,dyoffset,dtwist;

    private Mesh mesh;
    private int[] triangles = null;
    private Vector3[] vertices = null;

    void Start () {
		dwidth = ddepth = dlength = dcurvature = 1;
		ddistribution = 0.5f;
		dtwist = dyoffset = 0;
        InvokeRepeating ("inter", 0, 1);
    }

    void inter () {
        mesh = new Mesh ();
        GetComponent<MeshFilter> ().mesh = mesh;
    	Dome dome = new Dome(dwidth,ddepth,dlength,dcurvature,ddistribution,0,dyoffset,0,dtwist,0,0,0,0);
    	mesh.vertices = dome.Vertices;
    	mesh.triangles = dome.Triangles;
        
        mesh.RecalculateNormals ();
    }
}
