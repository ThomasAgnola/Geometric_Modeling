using System;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeMesh
{
    
    public List<WingedEdge> edges = new List<WingedEdge>();
    public List<Face> faces = new List<Face>();
    public List<Vertex> vertices = new List<Vertex>();

    public HalfEdgeMesh()
    {
        WingedEdge edge;
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
        edge = new WingedEdge(0, startVertex, endVertex, face);
        edges.Add(edge);
        face.edge = edge;
        // ( ¯ )
        startVertex = vertices[1];
        endVertex = vertices[2];
        edge = new WingedEdge(1, startVertex, endVertex, face);
        edges.Add(edge);
        // (  |)
        startVertex = vertices[2];
        endVertex = vertices[3];
        WingedEdge otheredge = new WingedEdge(2, startVertex, endVertex, face);
        edge = new WingedEdge(2, startVertex, endVertex, face);
        edges.Add(edge);
        // ( _ )
        startVertex = vertices[3];
        endVertex = vertices[0];
        edge = new WingedEdge(3, startVertex, endVertex, face);
        edges.Add(edge);
        faces.Add(face);

        face = new Face(2);
        face.edge = otheredge;

        startVertex = vertices[2];
        endVertex = vertices[4];
        edge = new WingedEdge(4, startVertex, endVertex, face);
        edges.Add(edge);

        startVertex = vertices[4];
        endVertex = vertices[5];
        edge = new WingedEdge(5, startVertex, endVertex, face);
        edges.Add(edge);

        startVertex = vertices[5];
        endVertex = vertices[3];
        edge = new WingedEdge(6, startVertex, endVertex, face);
        edges.Add(edge);
        faces.Add(face);

        for (int i = 0; i < faces.Count; i++)
        {
            face = faces[i];
            for (int k = 0; k < edges.Count; k++)
            {
                WingedEdge currentEdge = edges[k];
                WingedEdge nextEdge = edges[ ( k + 1 ) % 4 ];
                WingedEdge prevEdge = edges[ ( k + 3 ) % 4 ];
                // traitement en fonction de l'orientation de l'arrête
                if(currentEdge.rightFace == face)
                {
                    currentEdge.startCW = prevEdge;
                    currentEdge.endCCW = nextEdge;
                }
                else
                {
                    currentEdge.startCCW = nextEdge;
                    currentEdge.endCW = prevEdge;
                }
            }
        }
        
    }
    
    
    public HalfEdgeMesh(Mesh mesh)
    {
        Vector3[] vfVertices = mesh.vertices; // Vertex Face vertices
        int[] vfQuads = mesh.GetIndices(0); // Vertex Face quads

        Dictionary<ulong, WingedEdge> dictionnaryWinged = new Dictionary<ulong, WingedEdge>();
            
        List<WingedEdge> faceEdges = new List<WingedEdge>();

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
                WingedEdge edge;
                if ( !dictionnaryWinged.TryGetValue(key, out edge) )
                {
                    edge = new WingedEdge(edges.Count, startVertex, endVertex, face);
                    edges.Add(edge);
                    dictionnaryWinged.Add(key, edge);
                    if( j == 0) face.edge = edge;
                }
                else
                {
                    face.edge = edge;
                    edge.leftFace = face;
                }
                faceEdges.Add(edge);
            }

            for (int k = 0; k < faceEdges.Count; k++)
            {
                WingedEdge currentEdge = faceEdges[k];
                WingedEdge nextEdge = faceEdges[ ( k + 1 ) % 4 ];
                WingedEdge prevEdge = faceEdges[ (k - 1 + faceEdges.Count) % faceEdges.Count ];
                // traitement en fonction de l'orientation de l'arrête
                if(currentEdge.rightFace == face)
                {
                    currentEdge.startCW = prevEdge;
                    currentEdge.endCCW = nextEdge;
                }
                else
                {
                    currentEdge.startCCW = nextEdge;
                    currentEdge.endCW = prevEdge;
                }
            }

        }

        // A NE PAS SUPPRIMER !!!
        // FONCTIONNE SI LE RESTE DU CODE FONCTIONNE !!
        for (int i = 0; i < 4; i++)
        {
            WingedEdge currentEdge = faceEdges[i];
            //if (currentEdge.leftFace == null)
            //{
                try
                {
                    Debug.Log("Next List");
                    Debug.Log("start : " + currentEdge.startVertex.GetPos() + " end : " + currentEdge.endVertex.GetPos());
                    List<WingedEdge> tempList = new List<WingedEdge>();
                    tempList = currentEdge.getFanCCW(currentEdge.endVertex);
                    currentEdge.endCW = tempList[tempList.Count - 1];
                    tempList = currentEdge.getFanCW(currentEdge.startVertex);
                    currentEdge.startCCW = tempList[tempList.Count - 1];
                    Debug.Log("startCCW : " + currentEdge.startCCW.index);
                }
                catch (System.Exception)
                {
                    throw;
                }
            //}
            /*if (currentEdge.rightFace == null)
            {
                Debug.Log("Next List");
                List<WingedEdge> tempList = new List<WingedEdge>();
                tempList = currentEdge.getFanCW(currentEdge.endVertex);
                currentEdge.endCCW = tempList[tempList.Count - 1];
                tempList = currentEdge.getFanCCW(currentEdge.startVertex);
                currentEdge.startCW = tempList[tempList.Count - 1];
            }*/
            Debug.Log(i.ToString());
        }
        
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
        quads[index++] = 0;
        quads[index++] = 1;
        quads[index++] = 3;
        quads[index++] = 2;
        for (int i = 4; i < (faces.Count*4); i+=4)
        {
            quads[index++] = i-2;
            quads[index++] = i-1;
            quads[index++] = i+1;
            quads[index++] = i;
        }

        /*for (int i = 0; i < edges.Count; i++)
        {
            quads[index++] = i;
            
        }*/

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


public class HalfEdge : IComparable<WingedEdge>
{

    public HalfEdge nextEdge;
    public HalfEdge previousEdge;
    public int index;

    public Vertex startVertex { get; set; }

    public Vertex endVertex { get; set; }


    public HalfEdge(int index, Vertex startVertex, Vertex endVertex)
    {
        this.index = index;
        this.startVertex = startVertex;
        this.endVertex = endVertex;
    }

    public int CompareTo(WingedEdge other)
    {
        if (other == null)
        {
            return 1;
        }

        //Return the difference in index.
        return index - other.index;
    }
}
