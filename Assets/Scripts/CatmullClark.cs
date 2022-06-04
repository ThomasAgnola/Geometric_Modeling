using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullClark : MonoBehaviour
{
    /*
    1
    Creation de nouveau points
    - Face Points
    - Edge Points
        isobarycentre des vertices extrémités de l'edge et des faces points des faces adjacentes
    
    2 
    Calcul des nouvelles positions des vertices d'extremités avant l'étape 1
    - newPos = (1/m)*Q + (2/m)*R + ((m-3)/m)*oldPos
        m = valeur de la vertice, i.e nombre d'arêtes incidents
        Q = moyenne des Face Points des faces adjacentes à la vertices
        R = moyenne des positions des points milieux ( mid Points ) des arrêtes incidents
       
    
    */
    
    List<Face> newfaces = new List<Face>();
    List<HalfEdge> newedges = new List<HalfEdge>();
    List<Vertex> newvertices = new List<Vertex>();

    public CatmullClark(HalfEdgeMesh half_mesh)
    {
        int index = 0;
        for (int i = 0; i < half_mesh.faces.Count*4; i++)
        {
            newfaces.Add(new Face(i));
        }

        for (int i = 0; i < half_mesh.edges.Count; i++)
        {
            HalfEdge current_edge = half_mesh.edges[i];
            Vertex new_Pos = midPoint(current_edge.startVertex, current_edge.endVertex);
            newedges.Add(new HalfEdge(i, current_edge.startVertex, new_Pos, newfaces[i]));
            newedges.Add(new HalfEdge((i+1), new_Pos, current_edge.endVertex, newfaces[i]));
        }

        for (int i = 0; i < half_mesh.vertices.Count-1; i++)
        {
            newvertices.Add(half_mesh.vertices[i]);
            Vertex mid = midPoint(half_mesh.vertices[i], half_mesh.vertices[i+1]);
            newvertices.Add(mid);
        }

    }

    Vertex midPoint(Vertex start, Vertex end)
    {
        Vertex mid_Point = new Vertex();
        mid_Point.pos.x = (end.pos.x + start.pos.x) / 2;
        mid_Point.pos.y = (end.pos.y + start.pos.y) / 2;
        mid_Point.pos.z = (end.pos.z + start.pos.z) / 2;
        mid_Point.index = start.index + 1;
        return mid_Point;
    }

    public List<Face> GetFaces()
    {
        return newfaces;
    }

    public List<HalfEdge> GetHalfEdges()
    {
        return newedges;
    }

    public List<Vertex> GetVertices()
    {
        return newvertices;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
