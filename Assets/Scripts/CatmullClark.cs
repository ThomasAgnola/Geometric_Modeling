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
    private Dictionary<Vertex, Vertex> verticesdictionnary = new Dictionary<Vertex, Vertex>();

    public CatmullClark(HalfEdgeMesh half_mesh)
    {
        int index = 0;
        /*for (int i = 0; i < half_mesh.faces.Count*4; i++)
        {
            newfaces.Add(new Face(i));
        }

        for (int i = 0; i < half_mesh.edges.Count; i++)
        {
            HalfEdge current_edge = half_mesh.edges[i];
            Vertex new_Pos = midPoint(current_edge.startVertex, current_edge.endVertex);
            newedges.Add(new HalfEdge(i, current_edge.startVertex, new_Pos, newfaces[i]));
            newedges.Add(new HalfEdge((i+1), new_Pos, current_edge.endVertex, newfaces[i]));
        }*/

        /*for (int i = 0; i < half_mesh.vertices.Count-1; i++)
        {
            newvertices.Add(half_mesh.vertices[i]);
            Vertex mid = midPoint(half_mesh.vertices[i], half_mesh.vertices[i+1]);
            newvertices.Add(mid);
        }*/

        int faceEdge_parcour = 0;
        for (int i = 0; i < half_mesh.faces.Count; i++)
        {
            int face_index = newfaces.Count;
            newfaces.Add(new Face(face_index));
            newfaces.Add(new Face(face_index+1));
            newfaces.Add(new Face(face_index+2));
            newfaces.Add(new Face(face_index+3));

            // récupération des edges de la face
            HalfEdge current_edge_left = half_mesh.faceEdges[faceEdge_parcour++];
            HalfEdge current_edge_up = half_mesh.faceEdges[faceEdge_parcour++];
            HalfEdge current_edge_right = half_mesh.faceEdges[faceEdge_parcour++];
            HalfEdge current_edge_down = half_mesh.faceEdges[faceEdge_parcour++];

            //Calcul des points milieu
            Vertex left_midPos = midPoint(current_edge_left.startVertex, current_edge_left.endVertex);
            Vertex right_midPos = midPoint(current_edge_right.startVertex, current_edge_right.endVertex);
            Vertex up_midPos = midPoint(current_edge_up.startVertex, current_edge_up.endVertex);
            Vertex down_midPos = midPoint(current_edge_down.startVertex, current_edge_down.endVertex);

            //Calcul de l'isobarycentre
            Vertex face_center = CenterMidPoint(current_edge_left.startVertex, current_edge_right.startVertex, current_edge_up.startVertex, current_edge_down.startVertex);

            // Subdivision of a Face ( 1 -> 4 )
            //1st Face
            int edge_index = newedges.Count;
            newedges.Add(new HalfEdge(edge_index, current_edge_left.startVertex, left_midPos, newfaces[index]));
            newedges.Add(new HalfEdge(edge_index+1, left_midPos, face_center, newfaces[index]));
            newedges.Add(new HalfEdge(edge_index+2, face_center, down_midPos, newfaces[index]));
            newedges.Add(new HalfEdge(edge_index+3, down_midPos, current_edge_left.startVertex, newfaces[index]));
            
            edge_index = newedges.Count;
            //2nd Face
            newedges.Add(new HalfEdge(edge_index, left_midPos, current_edge_left.endVertex, newfaces[index+1]));
            newedges.Add(new HalfEdge(edge_index+1, current_edge_left.endVertex, up_midPos, newfaces[index+1]));
            newedges.Add(new HalfEdge(edge_index+2, up_midPos, face_center, newfaces[index+1]));
            newedges.Add(new HalfEdge(edge_index+3, face_center, left_midPos, newfaces[index+1]));

            edge_index = newedges.Count;
            //3rd Face
            newedges.Add(new HalfEdge(edge_index, down_midPos, face_center, newfaces[index+2]));
            newedges.Add(new HalfEdge(edge_index+1, face_center, right_midPos, newfaces[index+2]));
            newedges.Add(new HalfEdge(edge_index+2, right_midPos, current_edge_right.endVertex, newfaces[index+2]));
            newedges.Add(new HalfEdge(edge_index+3, current_edge_right.endVertex, down_midPos, newfaces[index+2]));
            
            edge_index = newedges.Count;
            //4th Face
            newedges.Add(new HalfEdge(edge_index, face_center, up_midPos, newfaces[index+3]));
            newedges.Add(new HalfEdge(edge_index+1, up_midPos, current_edge_up.endVertex, newfaces[index+3]));
            newedges.Add(new HalfEdge(edge_index+2, current_edge_up.endVertex, right_midPos, newfaces[index+3]));
            newedges.Add(new HalfEdge(edge_index+3, right_midPos, face_center, newfaces[index+3]));

            // add Vertices and checking every duplicate possible
            Vertex key = current_edge_left.startVertex;
            Vertex out_edge;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(current_edge_left.startVertex);
                verticesdictionnary.Add(key, current_edge_left.startVertex);
            }
            key = current_edge_up.startVertex;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(current_edge_up.startVertex);
                verticesdictionnary.Add(key, current_edge_up.startVertex);
            }
            key = current_edge_right.startVertex;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(current_edge_right.startVertex);
                verticesdictionnary.Add(key, current_edge_right.startVertex);
            }
            key = current_edge_down.startVertex;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(current_edge_down.startVertex);
                verticesdictionnary.Add(key, current_edge_down.startVertex);
            }
            key = left_midPos;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(left_midPos);
                verticesdictionnary.Add(key, left_midPos);
            }
            key = up_midPos;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(up_midPos);
                verticesdictionnary.Add(key, up_midPos);
            }
            key = right_midPos;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(right_midPos);
                verticesdictionnary.Add(key, right_midPos);
            }
            key = down_midPos;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(down_midPos);
                verticesdictionnary.Add(key, down_midPos);
            }
            key = face_center;
            if( !verticesdictionnary.TryGetValue(key, out out_edge) )
            {
                newvertices.Add(face_center);
                verticesdictionnary.Add(key, face_center);
            }
        }

    }

    Vertex midPoint(Vertex start, Vertex end)
    {
        // Calcul du point milieu d'une edge
        Vertex mid_Point = new Vertex();
        mid_Point.pos.x = (end.pos.x + start.pos.x) / 2;
        mid_Point.pos.y = (end.pos.y + start.pos.y) / 2;
        mid_Point.pos.z = (end.pos.z + start.pos.z) / 2;
        mid_Point.index = start.index + 1;
        return mid_Point;
    }

    Vertex CenterMidPoint(Vertex left, Vertex right, Vertex up, Vertex down)
    {
        // Calcul de l'isobarycentre
        Vertex centerMid = new Vertex();
        centerMid.pos.x = midPoint(left, right).pos.x;
        centerMid.pos.y = midPoint(down, up).pos.y;
        centerMid.pos.z = (left.pos.z + right.pos.z + up.pos.z + down.pos.z) / 4;
        return centerMid;
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
