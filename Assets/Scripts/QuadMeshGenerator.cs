using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuadMeshGenerator : MonoBehaviour
{
    [SerializeField] MeshFilter m_mf;
    // Start is called before the first frame update

    Mesh m_QuadMesh;
    Mesh m_WingedMesh;
    WingedEdgeMesh test;

    private void Awake()
    {
        if (!m_mf) m_mf = GetComponent<MeshFilter>();
        /*
        m_QuadMesh = CreateQuad(new Vector3(4, 0, 2));
        
        Debug.Log(ExportMeshToCSV(m_QuadMesh));
        */
        m_QuadMesh = CreatePlane(new Vector3(2, 0, 2), 2, 1);
        //m_QuadMesh = CreateCube(new Vector3(2, 2, 2));
        //m_QuadMesh = CreateChip(new Vector3(2, 2, 2));
        test = new WingedEdgeMesh(m_QuadMesh);
        //m_WingedMesh = test.getMesh();
        
        //m_mf.mesh = m_WingedMesh;
        m_mf.mesh = m_QuadMesh;
        Debug.Log(ExportMeshToCSV(m_QuadMesh));
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    Mesh CreateQuad(Vector3 size)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        int[] quads = new int[4];

        Vector3 halfsize = size * .5f;

        vertices[0] = new Vector3(halfsize.x, 0, halfsize.z);
        vertices[1] = new Vector3(-halfsize.x, 0, halfsize.z);
        vertices[2] = new Vector3(-halfsize.x, 0, -halfsize.z);
        vertices[3] = new Vector3(halfsize.x, 0, -halfsize.z);

        quads[0] = 0;
        quads[1] = 3;
        quads[2] = 2;
        quads[3] = 1;

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreatePlane(Vector3 size, int nSegmentX, int nSegmentZ)
    {
        Mesh mesh = new Mesh();
        mesh.name = "plane";

        Vector3 halfsize = size * .5f;
        Vector3[] vertices = new Vector3[((nSegmentX + 1) * (nSegmentZ + 1))];
        int[] quads = new int[4 * nSegmentX * nSegmentZ];

        int index = 0;
        for (int i = 0; i < nSegmentX + 1; i++)
        {
            float kx = (float)i / nSegmentX;
            for (int j = 0; j < nSegmentZ + 1; j ++)
            {
                float kz = (float)j / nSegmentZ;
                float x = Mathf.Lerp(-halfsize.x, halfsize.x, kx);
                float z = Mathf.Lerp(-halfsize.z, halfsize.z, kz);
                //float y = .125f * (Mathf.Sin(kz * kx * Mathf.PI * 2 * 2));
                float y = size.y;
                vertices[index++] = new Vector3(x, y, z);
            }
        }

        index = 0;
        for (int i = 0; i < nSegmentX; i++)
        {
            int indexOffset = i * (nSegmentZ + 1);
            for (int j = 0; j < nSegmentZ; j ++)
            {
                quads[index++] = indexOffset + j;
                quads[index++] = indexOffset + j + 1;
                quads[index++] = indexOffset + j + 1 + (nSegmentZ +1);
                quads[index++] = indexOffset + j + (nSegmentZ + 1);
            }
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;

    }

    Mesh CreateCube(Vector3 size)
    {
        Mesh mesh = new Mesh();

        mesh.name = "cube";

        Vector3 halfsize = size * .5f;
        //Vector3[] vertices = new Vector3[6 * 4];
        Vector3[] vertices =
        {
            // point bas
            new Vector3(halfsize.x, 0, halfsize.z),
            new Vector3(halfsize.x, 0, -halfsize.z),
            new Vector3(-halfsize.x, 0, -halfsize.z),
            new Vector3(-halfsize.x, 0, halfsize.z),

            // point haut
            new Vector3(halfsize.x, size.y, halfsize.z),
            new Vector3(halfsize.x, size.y, -halfsize.z),
            new Vector3(-halfsize.x, size.y, -halfsize.z),
            new Vector3(-halfsize.x, size.y, halfsize.z),
        };
        
        int[] quads =
        {
            // face bas
            3, 2, 1, 0,

            // face arriere
            0, 4, 7, 3,

            // face avant
            2, 6, 5, 1,

            // face gauche
            3, 7, 6, 2,

            // face droite
            1, 5, 4, 0,

            // face haut
            4, 5, 6, 7,
        };
        
        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    
    Mesh CreateChip(Vector3 size)
    {
        Mesh mesh = new Mesh();

        mesh.name = "chip";

        Vector3 halfsize = size * .5f;
        Vector3[] vertices =
        {
            // point bas
            new Vector3(halfsize.x, 0, halfsize.z),
            new Vector3(halfsize.x, 0, -halfsize.z),
            new Vector3(-halfsize.x, 0, -halfsize.z),
            new Vector3(-halfsize.x, 0, halfsize.z),

            // point haut
            new Vector3(halfsize.x, size.y, halfsize.z),
            new Vector3(halfsize.x, size.y, -halfsize.z),
            new Vector3(-halfsize.x, size.y, -halfsize.z),
            new Vector3(-halfsize.x, size.y, halfsize.z),
        };

        int[] quads =
        {
            // Face bas
            0, 1, 2, 3,

            // Face bas -> haut
            3, 2, 1, 0,

            // Face arriere
            0, 4, 7, 3,

            // Face arriere -> avant
            3, 7, 4, 0,

            // Face haut
            4, 5, 6, 7,

            // Face haut -> bas
            7, 6, 5, 4,
        };

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }


    private void OnDrawGizmos()
    {
        if (!m_QuadMesh) return;

        DrawQuads();

        DrawEdges();
        
    }

    void DrawQuads(){

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 24;
        guiStyle.normal.textColor = Color.red;

        Vector3[] vertices = m_QuadMesh.vertices;
        int[] quads = m_QuadMesh.GetIndices(0);

        for(int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(vertices[i]);
            Handles.Label(pos, new GUIContent(i.ToString()), guiStyle);
        }

        Gizmos.color = Color.green;
        guiStyle.normal.textColor = Color.blue;
        /*for (int i = 0; i < quads.Length; i+=4)
        {
            string str = (i / 4).ToString() + "("; 
            Vector3 centroidPos = Vector3.zero;
            for (int j = 0; j < 4; j++)
            {
                str += quads[i + j].ToString() +( (j < 3) ? ",":"" );
                Vector3 pos = transform.TransformPoint(vertices[quads[i + j]]);
                Vector3 nextPos = transform.TransformPoint(vertices[quads[i + (j + 1) % 4]]);
                centroidPos += pos;
                Gizmos.DrawLine(pos, nextPos);
            }
            str += ")";
            centroidPos *= .25f;
     
            Handles.Label(centroidPos, new GUIContent(str), guiStyle);
        }*/
    }

    void DrawEdges(){
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 24;
        guiStyle.normal.textColor = Color.blue;

        for(int i = 0; i < test.edges.Count; i++)
        {
            WingedEdge currentEdge = test.edges[i];
            string indexStartCW = null;
            string indexStartCCW = null;
            string indexEndCW = null;
            string indexEndCCW = null;
            Vector3 startpos = transform.TransformPoint(currentEdge.startVertex.GetPos());
            Vector3 endpos = transform.TransformPoint(currentEdge.endVertex.GetPos());
            if ( currentEdge.startCW != null ) indexStartCW = currentEdge.startCW.index.ToString();
            if ( currentEdge.startCCW != null ) indexStartCCW = currentEdge.startCCW.index.ToString();
            if ( currentEdge.endCW != null ) indexEndCW = currentEdge.endCW.index.ToString();
            if ( currentEdge.endCCW != null ) indexEndCCW = currentEdge.endCCW.index.ToString();
            Handles.Label((startpos+endpos)/2, new GUIContent(i.ToString() + 
            "(" + indexStartCW + ")" +
            "(" + indexStartCCW + ")" + 
            "(" + indexEndCW +")" + 
            "(" + indexEndCCW +")"), guiStyle);
            Gizmos.DrawLine(startpos, endpos);
        }
    }
    string ExportMeshToCSV(Mesh mesh)
    {
        List<string> lines = new List<string>();
        lines.Add("Vertex Table\t\t\t\tQuads Table");

        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);

        int nLines = Mathf.Max(vertices.Length, quads.Length / 4);
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            lines.Add($"{i}\t{vertex.x:N2}\t{vertex.y:N2}\t{vertex.z:N2}");
        }
        for (int i = 0; i < nLines; i++)
        {
            lines.Add("\t\t\t\t");
        }
        for (int i = 0; i < (quads.Length / 4); i++)
        {
            lines[i + 1] += $"{i}\t{quads[4 * i]}\t{quads[4 * i + 1]}\t{quads[4 * i + 2]}\t{quads[4 * i + 3]}";
        }
        return string.Join("\n", lines);
    }
}
