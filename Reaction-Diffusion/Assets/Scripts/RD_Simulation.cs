using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RD_Simulation : MonoBehaviour
{

    public int width = 256;
    public int height = 256;
    public ComputeShader computeShader;

    public float deltaTime = 0.1f;
    public float feedRate = 0.055f;
    public float killRate = 0.062f;
    public float diffusionRateA = 1.0f;
    public float diffusionRateB = 0.5f;

    private RenderTexture currentGrid;
    private RenderTexture nextGrid;
    private RenderTexture displayGrid;

    bool updateDisplay = true;

    // Start is called before the first frame update
    void Start()
    {

        Time.fixedDeltaTime = 1 / 60.0f;

        
        Initialize();
        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayGrid;

    }

    void Initialize(){

        // renderTexture = display
        displayGrid = new RenderTexture(width, height, 24);
        displayGrid.enableRandomWrite = true;
        displayGrid.Create();   

        currentGrid = new RenderTexture(width, height, 24);
        currentGrid.enableRandomWrite = true;
        currentGrid.Create();

        nextGrid = new RenderTexture(width, height, 24);
        nextGrid.enableRandomWrite = true;
        nextGrid.Create();

        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);

        computeShader.SetTexture(0, "displayGrid", displayGrid);  
        computeShader.SetTexture(0, "currentGrid", currentGrid);
        computeShader.SetTexture(0, "nextGrid", nextGrid);
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        UpdateShaderParameters();
    }

 

    void UpdateShaderParameters(){
        computeShader.SetFloat("deltaTime", deltaTime);
        computeShader.SetFloat("feedRate", feedRate);
        computeShader.SetFloat("killRate", killRate);
        computeShader.SetFloat("diffusionRateA", diffusionRateA);
        computeShader.SetFloat("diffusionRateB", diffusionRateB);
    }
    
    void Simulate(){
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetTexture(1, "currentGrid", currentGrid);
        computeShader.SetTexture(1, "nextGrid", nextGrid);
        computeShader.SetTexture(1, "displayGrid", displayGrid);

        UpdateShaderParameters();

        computeShader.Dispatch(1, width / 8, height / 8, 1);

        Graphics.Blit(nextGrid, currentGrid);
    }

    void Display(){
        computeShader.SetTexture(1, "currentGrid", currentGrid);
        computeShader.SetTexture(1, "displayGrid", displayGrid);
        computeShader.Dispatch(1, width/8, height/8, 1);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < 4; i++){
            Simulate();
        }
        
        updateDisplay = true;

    }

    private void Update() {
        if (updateDisplay){
            Display();
            updateDisplay = false;
        }
    }
    
}
