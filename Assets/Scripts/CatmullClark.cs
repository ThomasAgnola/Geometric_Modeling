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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
