namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System;
    using ModApi.Craft.Parts;
    using ModApi.GameLoop.Interfaces;
    using UnityEngine;

    public class MeshGeneratorScript : PartModifierScript<MeshGeneratorData> {
        private GameObject Prefab;
        private GameObject Collider;
        public const int verticesPerLayer = 24;
        public const float minSize = 0.1f;
        public float dwidth,
        ddepth,
        dlength,
        dcurvature,
        ddistribution,
        dyoffset,
        dtwist;

        private Mesh mesh;
        private Mesh mesh2;
        private int[] triangles = null;
        private Vector3[] vertices = null;

        protected override void OnInitialized () {
            Prefab = transform.Find ("MeshGenerator").gameObject;
            Collider = Prefab.transform.Find ("MeshGeneratorCollider").gameObject;

            UpdateMesh ();
        }

        public void UpdateMesh () {
            dwidth = Data.width;
            ddepth = Data.depth;
            dlength = Data.length;
            dcurvature = Data.curvature;
            ddistribution = Data.distribution;
            dtwist = Data.twist;

            mesh = new Mesh ();

            DomeGeneratorScript dome = new DomeGeneratorScript (dwidth, ddepth, dlength, dcurvature, ddistribution, 0, dyoffset, 0, dtwist, 0, 0, 0, 0);

            mesh.Clear();
            mesh.vertices = dome.Vertices;
            mesh.triangles = dome.Triangles;
            mesh.RecalculateNormals ();

            Prefab.GetComponent<MeshFilter> ().mesh = mesh;
            Collider.GetComponent<MeshCollider> ().sharedMesh = mesh;
        }
    }
}