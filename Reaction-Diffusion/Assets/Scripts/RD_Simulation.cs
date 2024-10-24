using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RD_Simulation : MonoBehaviour
{

    public int width = 256;
    public int height = 256;
    public ComputeShader computeShader;

    public RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();      

        computeShader.SetTexture(0, "Result", renderTexture);  
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = renderTexture;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
