using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNormals : MonoBehaviour
{
    void OnDrawGizmos()
    {
        /*Gizmos.color = Color.yellow;

        for(int i = 0; i < end.z; i++)
        {
            Vector3 curPos = new Vector3(0, 0, i);
            Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
            float terrainX = curPos.x / terrainSize.x;
            float terrainZ = curPos.z / terrainSize.z;

            Vector3 normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(terrainX, terrainZ);

            curPos.y = Terrain.activeTerrain.SampleHeight(curPos);
            Gizmos.DrawRay(curPos, normal);
        }*/

        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
 
        /*for (int i = 0; i < 50; i += 2) // use steps of more than 1 for smoother interaction in editor
        {
            for (int k = 0; k < 50; k += 2)
            {
                // not sure why the gizmo y-heights are scaled out from 0,0 (about 5X) when doing it this way.
                // Uncomment this and comment the second one to see:
                //Vector3 pivot = new Vector3(i, Terrain.activeTerrain.terrainData.GetHeight(i, k), k);
                Vector3 pivot = new Vector3(i, 0, k);
 
                // this way fixes it (pretty much)--WHY??
                //Vector3 pivot = new Vector3(i, Terrain.activeTerrain.terrainData.GetHeight(i * 5, k * 5), k);
 
                float x = pivot.x / terrainSize.x;
                float z = pivot.z / terrainSize.z;
                Vector3 interpolatedNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(x, z);
    
                pivot.y = Terrain.activeTerrain.SampleHeight(pivot);

                RaycastHit hit;
                if(Physics.Raycast(new Vector3(i, 100, k), Vector3.down, out hit, Mathf.Infinity))
                {
                    //Debug.Log("Ray hit below");
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(hit.point, 0.2f);
                    Gizmos.DrawRay(hit.point, hit.normal);
                }

                //GUI.color = Color.blue;
                
                //Gizmos.Drawray()

                // JUST USE RAYCAST!!!
            }
        }*/

        /*for (double i = 0; i < 50; i += 0.25) // use steps of more than 1 for smoother interaction in editor
        {
            RaycastHit hit;
            if(Physics.Raycast(new Vector3((float)i, 100, 0), Vector3.down, out hit, Mathf.Infinity))
            {
                //Debug.Log("Ray hit below");
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(hit.point, 0.2f);
                Gizmos.DrawRay(hit.point, hit.normal);
            }

            
        }*/

        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else Debug.Log("raycast miss");
    }
}
