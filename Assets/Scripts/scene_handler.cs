using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scene_handler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Quaternion init_rot = Quaternion.Euler(90, 0, 0);
        Renderer init_rend = GetComponent<MeshRenderer>();

        init_rend.material = Resources.Load<Material>("SimpleTownLite/_Materials/SimpleTownLite_Dumpster");
        transform.localScale = new Vector3(3f, 3f, 6f);
        transform.rotation = init_rot;
        transform.position = new Vector3(1f, 5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = pos;
    }
}
