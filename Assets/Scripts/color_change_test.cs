using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class color_change_test : MonoBehaviour
{
    // Start is called before the first frame update

    public float R, G, B, A;
    private Renderer rend;
    private Color newColor;

    void Start()
    {
        rend = gameObject.GetComponent("Renderer") as Renderer;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Material m = new Material(Shader.Find("Standard"));
        m.SetFloat("_Mode", 2);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        */
        rend.material.SetFloat("_Mode", 2);

        newColor = new Color(R, G, B, A);
        rend.material.color = newColor;
    }
}
