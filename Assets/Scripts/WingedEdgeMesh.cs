using System;
using System.Collections.Generic;
using UnityEngine;

public class WingedEdgeMesh
{
    
    public List<WingedEdge> edges = new List<WingedEdge>();
    public List<Face> faces = new List<Face>();
    public List<Vertex> vertices = new List<Vertex>();
    
    
    public WingedEdgeMesh(Mesh mesh)
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
                    edge.leftFace = face;
                }
                faceEdges.Add(edge);
            }

            for (int k = 0; k < faceEdges.Count; k++)
            {
                WingedEdge currentEdge = faceEdges[k];
                WingedEdge nextEdge = faceEdges[ ( k + 1 ) % 4 ];
                WingedEdge prevEdge = faceEdges[ ( k - 1 + faceEdges.Count) % faceEdges.Count ];
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

        /*for (int i = 0; i < 2; i++)
        {
            WingedEdge currentEdge = faceEdges[i];
            if (currentEdge.leftFace == null)
            {
                try
                {
                    Debug.Log("Next List");
                    List<WingedEdge> tempList = new List<WingedEdge>();
                    tempList = currentEdge.getFanCCW(currentEdge.endVertex);
                    currentEdge.endCW = tempList[tempList.Count - 1];
                    tempList = currentEdge.getFanCW(currentEdge.startVertex);
                    currentEdge.startCCW = tempList[tempList.Count - 1];
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            if (currentEdge.rightFace == null)
            {
                Debug.Log("Next List");
                List<WingedEdge> tempList = new List<WingedEdge>();
                tempList = currentEdge.getFanCW(currentEdge.endVertex);
                currentEdge.endCCW = tempList[tempList.Count - 1];
                tempList = currentEdge.getFanCCW(currentEdge.startVertex);
                currentEdge.startCW = tempList[tempList.Count - 1];
            }
            Debug.Log(i.ToString());
        }*/
        
    }

    public Mesh getMesh(){
        Mesh mesh = new Mesh();

        Vector3[] tabVertices = new Vector3[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            tabVertices[i] = vertices[i].GetPos();
        }

        int[] quads = new int[edges.Count];

        for (int i = 0; i < edges.Count; i+=4)
        {
            for (int j = 0; j < 4; j++)
            {
                quads[j] = j+i;
            }
        }

        mesh.vertices = tabVertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

}


public class WingedEdge : IComparable<WingedEdge>
{

    public WingedEdge startCW = null;
    public WingedEdge startCCW = null;
    public WingedEdge endCW = null;
    public WingedEdge endCCW = null;
    public Face rightFace;
    public Face leftFace;
    public int index;

    public Vertex startVertex { get; set; }

    public Vertex endVertex { get; set; }


    public WingedEdge(int index, Vertex startVertex, Vertex endVertex, Face rightface)
    {
        this.index = index;
        this.startVertex = startVertex;
        this.endVertex = endVertex;
        this.rightFace = rightface;
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

    public List<WingedEdge> getFanCCW(Vertex positionVertex){
        List<WingedEdge> FanCCW = new List<WingedEdge>();
        FanCCW.Add(this);
        bool isDone = false;
        if(positionVertex == endVertex) 
        {
            FanCCW.Add(this.endCCW);
            while (!isDone)
            {
                int position = FanCCW.Count -1 ;
                WingedEdge lastEdge = FanCCW[position];
                WingedEdge beforeLasteEdge = FanCCW[position - 1];
                if(lastEdge.leftFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.rightFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.endCCW == this)  {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCW == beforeLasteEdge) {
                    FanCCW.Add(FanCCW[position].startCCW);
                }
                if(lastEdge.endCW == beforeLasteEdge) {
                    FanCCW.Add(FanCCW[position].endCCW);
                }
            }
        }
        else if (positionVertex == startVertex)
        {
            FanCCW.Add(this.startCCW);
            while (!isDone)
            {
                int position = FanCCW.Count -1 ;
                WingedEdge lastEdge = FanCCW[position];
                WingedEdge beforeLasteEdge = FanCCW[position - 1];
                if(lastEdge.leftFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.rightFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.endCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCCW == beforeLasteEdge) {
                    FanCCW.Add(FanCCW[position].startCW);
                }
                if(lastEdge.endCCW == beforeLasteEdge) {
                    FanCCW.Add(FanCCW[position].endCW);
                }
            }
        }
        return FanCCW;
    }

    public List<WingedEdge> getFanCW(Vertex positionVertex){
        List<WingedEdge> FanCW = new List<WingedEdge>();
        FanCW.Add(this);
        bool isDone = false;
        if(positionVertex == endVertex) 
        {
            FanCW.Add(this.endCW);
            while (!isDone)
            {
                int position = FanCW.Count -1 ;
                WingedEdge lastEdge = FanCW[position];
                WingedEdge beforeLasteEdge = FanCW[position - 1];
                if(lastEdge.leftFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.rightFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.endCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCCW == beforeLasteEdge) {
                    FanCW.Add(lastEdge.endCW);
                }
                if(lastEdge.endCCW == beforeLasteEdge) {
                    FanCW.Add(lastEdge.startCW);
                }
            }
        }
        else if (positionVertex == startVertex)
        {
            FanCW.Add(this.startCW);
            while (!isDone)
            {
                int position = FanCW.Count -1 ;
                WingedEdge lastEdge = FanCW[position];
                WingedEdge beforeLasteEdge = FanCW[position - 1];
                if(lastEdge.leftFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.rightFace == null) {
                    isDone = true;
                    break;
                }
                if(lastEdge.endCCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCCW == this) {
                    isDone = true;
                    break;
                }
                if(lastEdge.startCW == beforeLasteEdge) {
                    FanCW.Add(lastEdge.endCCW);
                }
                if(lastEdge.endCW == beforeLasteEdge) {
                    FanCW.Add(lastEdge.startCCW);
                }
            }
        }
        return FanCW;
    }
}

public class Vertex
{
    private int index;
    private Vector3 pos;

    public Vertex(int i, Vector3 pos)
    {
        index = i;
        this.pos = pos;
    }

    public Vector3 GetPos()
    {
        return pos;
    }

    public int getIndex()
    {
        return index;
    }
}

public class Face
{
    private int i;
    public WingedEdge edge;

    public Face(int i)
    {
        this.i = i;
    }
}
