using System;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeMesh
{
    
    public List<HalfEdge> edges = new List<HalfEdge>();
    public List<Face> faces = new List<Face>();
    public List<Vertex> vertices = new List<Vertex>();

    public HalfEdgeMesh()
    {
        HalfEdge edge;
        var startVertex = new Vertex(0, new Vector3(0.0f, 0.0f, 0.0f));
        var endVertex = new Vertex(0, new Vector3(0.0f, 0.0f, 0.0f));

        // set Vectices 
        vertices.Add(new Vertex(0, new Vector3(-1.0f, 0.0f, -1.0f)));
        vertices.Add(new Vertex(1, new Vector3(-1.0f, 0.0f, 1.0f)));
        vertices.Add(new Vertex(2, new Vector3(0.0f, 0.0f, 1.0f)));
        vertices.Add(new Vertex(3, new Vector3(0.0f, 0.0f, -1.0f)));
        vertices.Add(new Vertex(4, new Vector3(1.0f, 0.0f, 1.0f)));
        vertices.Add(new Vertex(5, new Vector3(1.0f, 0.0f, -1.0f)));

        // Simple Dual Quads
        Face face = new Face(1);
        // (|  )
        startVertex = vertices[0];
        endVertex = vertices[1];
        edge = new HalfEdge(0, startVertex, endVertex, face);
        edges.Add(edge);
        // ( ¯ )
        startVertex = vertices[1];
        endVertex = vertices[2];
        edge = new HalfEdge(1, startVertex, endVertex, face);
        edges.Add(edge);
        // (  |)
        startVertex = vertices[2];
        endVertex = vertices[3];
        edge = new HalfEdge(2, startVertex, endVertex, face);
        edges.Add(edge);
        // ( _ )
        startVertex = vertices[3];
        endVertex = vertices[0];
        edge = new HalfEdge(3, startVertex, endVertex, face);
        edges.Add(edge);
        faces.Add(face);

        face = new Face(2);

        startVertex = vertices[2];
        endVertex = vertices[4];
        edge = new HalfEdge(4, startVertex, endVertex, face);
        edges.Add(edge);

        startVertex = vertices[4];
        endVertex = vertices[5];
        edge = new HalfEdge(5, startVertex, endVertex, face);
        edges.Add(edge);

        startVertex = vertices[5];
        endVertex = vertices[3];
        edge = new HalfEdge(6, startVertex, endVertex, face);
        edges.Add(edge);
        faces.Add(face);

        for (int i = 0; i < faces.Count; i++)
        {
            face = faces[i];
            for (int k = 0; k < edges.Count; k++)
            {
                HalfEdge currentEdge = edges[k];
                HalfEdge nextEdge = edges[ ( k + 1 ) % 4 ];
                HalfEdge prevEdge = edges[ ( k + 3 ) % 4 ];
                currentEdge.nextEdge = nextEdge;
                currentEdge.previousEdge = prevEdge;
            }
        }
        
    }
    
    
    public HalfEdgeMesh(Mesh mesh)
    {
        Vector3[] vfVertices = mesh.vertices; // Vertex Face vertices
        int[] vfQuads = mesh.GetIndices(0); // Vertex Face quads

        Dictionary<ulong, HalfEdge> dictionnaryWinged = new Dictionary<ulong, HalfEdge>();
            
        List<HalfEdge> faceEdges = new List<HalfEdge>();

        //vertices
        for (int i = 0; i < vfVertices.Length; i++)
        {
            vertices.Add(new Vertex(i, vfVertices[i]));
        }

        // Faces & edges
        for (int i = 0; i < vfQuads.Length / 4; i++)
        {
            Face face = new Face(i);
            faces.Add(face);

            //edges
            for (int j = 0; j < 4; j++)
            {
                var startVertex = vertices[vfQuads[ 4 * i + j]];
                var endVertex = vertices[vfQuads[ 4 * i + (j + 1) % 4]];
                ulong key = (UInt32) Mathf.Max(startVertex.getIndex(), endVertex.getIndex()) + ( (ulong) Mathf.Min(startVertex.getIndex(), endVertex.getIndex()) << 32 );
                HalfEdge edge;
                HalfEdge twinedge;
                if ( !dictionnaryWinged.TryGetValue(key, out edge) )
                {
                    edge = new HalfEdge(edges.Count, startVertex, endVertex, face);
                    edges.Add(edge);
                    dictionnaryWinged.Add(key, edge);
                }
                else
                {
                    edge.Twin = new HalfEdge(edges.Count, startVertex, endVertex, face);
                    twinedge = new HalfEdge(edges.Count, startVertex, endVertex, face);
                    twinedge.Twin = edge;
                    edges.Add(twinedge);
                }
                faceEdges.Add(edge);
            }

            for (int k = 0; k < faceEdges.Count; k++)
            {
                HalfEdge currentEdge = faceEdges[k];
                HalfEdge nextEdge = faceEdges[ ( k + 1 ) % 4 ];
                HalfEdge prevEdge = faceEdges[ (k + 3) % 4 ];
                currentEdge.nextEdge = nextEdge;
                currentEdge.previousEdge = prevEdge;
            }

        }
        
    }

    public void CatGenerator()
    {
        CatmullClark catmul = new CatmullClark(this);
        faces = catmul.GetFaces();
        edges = catmul.GetHalfEdges();
        vertices = catmul.GetVertices();
    }

    public Mesh getMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] tabVertices = new Vector3[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            tabVertices[i] = vertices[i].GetPos();
        }

        int[] quads = new int[faces.Count * 4];
        Debug.Log("edges nbr : " + edges.Count);
        Debug.Log("vertices nbr : " + vertices.Count);
        Debug.Log("faces nbr : " + faces.Count);

        int index = 0;
        int face_jump = 0;
        for (int i = 0; i < faces.Count; i++)
        {
            quads[index++] = edges[face_jump].startVertex.index;
            quads[index++] = edges[face_jump+1].startVertex.index;
            quads[index++] = edges[face_jump+2].startVertex.index;
            quads[index++] = edges[face_jump+3].startVertex.index;
            face_jump = 4 * (i+1) ;
        }

        mesh.vertices = tabVertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    public Mesh SimpleMesh(){
        Mesh mesh = new Mesh();

        Vector3[] tabVertices = new Vector3[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            tabVertices[i] = vertices[i].GetPos();
        }

        int[] quads = new int[edges.Count + 1];

        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        quads[4] = 3;
        quads[5] = 2;
        quads[6] = 4;
        quads[7] = 5;

        mesh.vertices = tabVertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

}


public class HalfEdge : IComparable<HalfEdge>
{

    public HalfEdge nextEdge;
    public HalfEdge previousEdge;
    public HalfEdge Twin;
    Face face;
    public int index;

    public Vertex startVertex { get; set; }

    public Vertex endVertex { get; set; }


    public HalfEdge(int index, Vertex startVertex, Vertex endVertex, Face face)
    {
        this.index = index;
        this.startVertex = startVertex;
        this.endVertex = endVertex;
        this.face = face;
    }

    public int CompareTo(HalfEdge other)
    {
        if (other == null)
        {
            return 1;
        }

        //Return the difference in index.
        return index - other.index;
    }
}
