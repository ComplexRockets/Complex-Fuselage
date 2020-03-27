namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System;
    using ModApi.Craft.Parts;
    using ModApi.GameLoop.Interfaces;
    using UnityEngine;
    public class DomeGeneratorScript : PartModifierScript<DomeGeneratorData> {

        private float width, depth, length, curvature, distribution, twist, yoffset;
        private float xoffset, zoffset, corner1, corner2, corner3, corner4; // Unused

        private const int Layers = 12;
        private const int totalVertices = Layers * MeshGeneratorScript.verticesPerLayer;
        private Vector3[] vertices = new Vector3[totalVertices + 1];
        private int[] triangles = new int[totalVertices * 6 + MeshGeneratorScript.verticesPerLayer * 3];
        public int[] Triangles => triangles;
        public Vector3[] Vertices => vertices;

        public DomeGeneratorScript () {
            width = depth = length = corner1 = corner2 = corner3 = corner4 = 1;
            distribution = 0.5f;
            curvature = 0;
            xoffset = yoffset = zoffset = twist = 0;
            recalculateMesh ();
            Debug.Log("Dome Created");
        }

        public DomeGeneratorScript (float width, float depth, float length, float curvature, float distribution) {
            setDimensions (width, depth, length, curvature, distribution);
            corner1 = corner2 = corner3 = corner4 = 1;
            xoffset = yoffset = zoffset = twist = 0;
            recalculateMesh ();
            Debug.Log("Dome Created");
        }

        public DomeGeneratorScript (float width, float depth, float length, float curvature, float distribution, float xoffset, float yoffset, float zoffset, float twist, float corner1, float corner2, float corner3, float corner4) {
            setDimensions (width, depth, length, curvature, distribution);
            setOffsets (xoffset, yoffset, zoffset, twist);
            setCorners (corner1, corner2, corner3, corner4);
            recalculateMesh ();
            Debug.Log("Dome Created");
        }

        public void recalculateMesh () {
            int verticeIndex, triangle, previousLayers;
            verticeIndex = triangle = previousLayers = 0;

            // Calculate the geometry
            float angleFactor = 2 * Mathf.PI / MeshGeneratorScript.verticesPerLayer;
            for (int layer = 0; layer < Layers; layer++) {
                float layerz = curvature * (1 - Mathf.Cos (Mathf.PI * layer / Layers / 2)) +
                    (1 - curvature) * Mathf.Sin (Mathf.PI * layer / Layers / 2);
                float layerro = Mathf.Pow (curvature * (1 - Mathf.Sin (Mathf.PI * layer / Layers / 2)) +
                    (1 - curvature) * Mathf.Cos (Mathf.PI * layer / Layers / 2), Mathf.Pow (1f * layer / Layers, distribution));
                float layerTwist = 2 * Mathf.PI * twist * layer / Layers;
                for (int vertice = 0; vertice < MeshGeneratorScript.verticesPerLayer; vertice++, verticeIndex++)
                    vertices[verticeIndex] = new Vector3 (
                        width * layerro * Mathf.Cos (angleFactor * vertice + layerTwist),
                        length * layerz + yoffset,
                        depth * layerro * Mathf.Sin (angleFactor * vertice + layerTwist));
            }
            vertices[verticeIndex] = new Vector3 (0, length + yoffset, 0);

            // Connect every layer to the next
            if (twist > 0)
                for (int layer = 0; layer < DomeGeneratorScript.Layers - 1; layer++, triangle += 6, previousLayers += MeshGeneratorScript.verticesPerLayer) {
                    for (verticeIndex = 0; verticeIndex < MeshGeneratorScript.verticesPerLayer - 1; verticeIndex++, triangle += 6) {
                        triangles[triangle + 2] = triangles[triangle + 3] = 1 + (triangles[triangle] = verticeIndex + previousLayers);
                        triangles[triangle + 5] = 1 + (triangles[triangle + 1] = triangles[triangle + 4] = verticeIndex + MeshGeneratorScript.verticesPerLayer + previousLayers);
                    }
                    triangles[triangle] = verticeIndex + (triangles[triangle + 2] = triangles[triangle + 3] = previousLayers);
                    triangles[triangle + 1] = triangles[triangle + 4] = verticeIndex + (triangles[triangle + 5] = MeshGeneratorScript.verticesPerLayer + previousLayers);
                }
            else // To prevent too much squishing with the twists invert the triangles
                for (int layer = 0; layer < DomeGeneratorScript.Layers - 1; layer++, triangle += 6, previousLayers += MeshGeneratorScript.verticesPerLayer) {
                    for (verticeIndex = 0; verticeIndex < MeshGeneratorScript.verticesPerLayer - 1; verticeIndex++, triangle += 6) {
                        triangles[triangle] = 1 + (triangles[triangle + 1] = triangles[triangle + 4] = verticeIndex + previousLayers);
                        triangles[triangle + 2] = triangles[triangle + 3] = 1 + (triangles[triangle + 5] = verticeIndex + MeshGeneratorScript.verticesPerLayer + previousLayers);
                    }
                    triangles[triangle + 1] = triangles[triangle + 4] = verticeIndex + (triangles[triangle] = previousLayers);
                    triangles[triangle + 5] = verticeIndex + (triangles[triangle + 2] = triangles[triangle + 3] = MeshGeneratorScript.verticesPerLayer + previousLayers);
                }

            // Connect the last layer to the center point
            for (verticeIndex = 0; verticeIndex < MeshGeneratorScript.verticesPerLayer - 1; verticeIndex++, triangle += 6) {
                triangles[triangle] = verticeIndex + 1 + previousLayers;
                triangles[triangle + 1] = verticeIndex + previousLayers;
                triangles[triangle + 2] = totalVertices;
            }
            triangles[triangle] = previousLayers;
            triangles[triangle + 1] = verticeIndex + previousLayers;
            triangles[triangle + 2] = totalVertices;
        }

        public void setDimensions (float width, float depth, float length, float curvature, float distribution) {
            this.length = length;
            this.width = (width < MeshGeneratorScript.minSize) ? MeshGeneratorScript.minSize : width;
            this.depth = (depth < MeshGeneratorScript.minSize) ? MeshGeneratorScript.minSize : depth;
            this.curvature = (curvature < -1) ? 1 : ((curvature > 1) ? 0 : ((curvature - 1) / -2));
            this.distribution = distribution;
            recalculateMesh ();
        }

        public void setOffsets (float xoffset, float yoffset, float zoffset, float twist) {
            this.xoffset = xoffset;
            this.yoffset = yoffset;
            this.zoffset = zoffset;
            this.twist = (twist < -0.5f) ? -0.5f : ((twist > 0.5f) ? 0.5f : twist);
            recalculateMesh ();
        }

        public void setCorners (float corner1, float corner2, float corner3, float corner4) {
            this.corner1 = (corner1 < 0) ? 0 : ((corner1 > 1) ? 1 : corner1);
            this.corner2 = (corner2 < 0) ? 0 : ((corner2 > 1) ? 1 : corner2);
            this.corner3 = (corner3 < 0) ? 0 : ((corner3 > 1) ? 1 : corner3);
            this.corner4 = (corner4 < 0) ? 0 : ((corner4 > 1) ? 1 : corner4);
            recalculateMesh ();
        }
    }
}