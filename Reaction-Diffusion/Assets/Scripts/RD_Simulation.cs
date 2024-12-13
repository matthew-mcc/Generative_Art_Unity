using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering;
using ComputeShaderUtility;


public class RD_Simulation : MonoBehaviour
{

    public int width = 256;
    public int height = 256;
    public ComputeShader computeShader;

    public float deltaTime = 0.1f;
    public float feedRate = 0.0325f;
    public float killRate = 0.062f;
    public float diffusionRateA = 1.0f;
    public float diffusionRateB = 0.4f;

    private RenderTexture currentGrid;
    private RenderTexture nextGrid;
    public RenderTexture displayGrid;

    public Texture2D initMap; // todo

    bool updateDisplay = true;

    public int colorMode = 0;
    public int orientationDirection = 0; // 0, 1, 2, 3

    // Directional Bias Modes
    public int directionalMode = 0;
    public int numDirectionalSegments = 20;
    public float directionalBias = 0.005f;

    // 0 = square, 1 = circle, 2 = 3 squares, 3 = line, 4 = triangle
    public int initialConcentrationMap = 0;
    // Simulation Control things

    public Simulation_Handler sim;
    // public ComputeHelper 
    public FilterMode filterMode = FilterMode.Point;
	public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

    public void ResetSimulation(){

        ComputeHelper.CreateRenderTexture(ref currentGrid, width, height, filterMode, format, "currentGrid");
		ComputeHelper.CreateRenderTexture(ref nextGrid, width, height, filterMode, format, "nextGrid");
		ComputeHelper.CreateRenderTexture(ref displayGrid, width, height, filterMode, format, "displayGrid");

        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayGrid;

        // Reset compute shader textures to the newly created grids
        computeShader.SetTexture(0, "currentGrid", currentGrid);
        // computeShader.SetTexture(0, "nextGrid", nextGrid);
        computeShader.SetTexture(0, "displayGrid", displayGrid);
        computeShader.SetTexture(0, "initMap", initMap);

        computeShader.SetInt("initialMapMode", initialConcentrationMap);

        // Reapply initial shader parameters and dispatch the compute shader to reset
        UpdateShaderParameters();
        computeShader.Dispatch(0, width / 8, height / 8, 1);
        updateDisplay = true;
    }

    // Start is called before the first frame update
    void Start()
    {

        // Time.fixedDeltaTime = 1 / 60.0f; <-- Now handled in Simulation_Handler

        
        Initialize();
        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayGrid;
        
    }

    void Initialize(){

        ComputeHelper.CreateRenderTexture(ref currentGrid, width, height, filterMode, format, "currentGrid");
		ComputeHelper.CreateRenderTexture(ref nextGrid, width, height, filterMode, format, "nextGrid");
		ComputeHelper.CreateRenderTexture(ref displayGrid, width, height, filterMode, format, "displayGrid");


        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);

        // Color mode
        computeShader.SetInt("colorMode", colorMode);

        // computeShader.SetTexture(0, "displayGrid", displayGrid);  
        computeShader.SetTexture(0, "currentGrid", currentGrid);
        // computeShader.SetTexture(0, "nextGrid", nextGrid);
        // computeShader.SetTexture(0, "initMap", initMap);
        computeShader.SetInt("orientationDirection", orientationDirection);

        computeShader.SetInt("initialMapMode", initialConcentrationMap);


        // Directional Bias
        computeShader.SetInt("directionalMode", directionalMode);
        computeShader.SetInt("numberDirectionalSegments", numDirectionalSegments);
        computeShader.SetFloat("directionalBiasModifier", directionalBias);
        computeShader.Dispatch(0, width / 8, height / 8, 1);
        // ComputeHelper.Dispatch(computeShader, width, height, 1, 0);

        

        // UpdateShaderParameters();
    }

 

    public void UpdateShaderParameters(){
        // computeShader.SetFloat("deltaTime", deltaTime);
        computeShader.SetFloat("feedRate", feedRate);
        computeShader.SetFloat("killRate", killRate);
        computeShader.SetFloat("diffusionRateA", diffusionRateA);
        computeShader.SetFloat("diffusionRateB", diffusionRateB);
        computeShader.SetInt("orientationDirection", orientationDirection);

        // Directional Bias
        computeShader.SetInt("directionalMode", directionalMode);
        computeShader.SetInt("numberDirectionalSegments", numDirectionalSegments);
        computeShader.SetFloat("directionalBiasModifier", directionalBias);
        computeShader.SetInt("colorMode", colorMode);
        computeShader.SetInt("initialMapMode", initialConcentrationMap);
    }
    
    void Simulate(){
        // computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetTexture(1, "currentGrid", currentGrid);
        computeShader.SetTexture(1, "nextGrid", nextGrid);
        // computeShader.SetTexture(1, "displayGrid", currentGrid);

        UpdateShaderParameters();

        computeShader.Dispatch(1, width / 8, height / 8, 1);
        // ComputeHelper.Dispatch(computeShader, width, height, 1, 1);

        // Graphics.Blit(nextGrid, currentGrid);
        ComputeHelper.CopyRenderTexture(nextGrid, currentGrid);
    }

    void Display(){
        computeShader.SetTexture(2, "currentGrid", currentGrid);
        computeShader.SetTexture(2, "displayGrid", displayGrid);
        computeShader.SetTexture(2, "nextGrid", nextGrid);
        computeShader.Dispatch(2, width, height, 1);
        // ComputeHelper.Dispatch(computeShader, width, height, 1, 2);
        
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {

        // Only run if simulation controls tell us we can
        if (sim.isSimulationRunning){
            for (int i = 0; i < 4; i++){
                Simulate();
            }
            
            updateDisplay = true;
        }


    }

    private void Update() {
        if (updateDisplay){
            Display();
            updateDisplay = false;
        }
        

        if(Input.GetKeyDown(KeyCode.P)){
            Debug.Log($"Feed Rate: {feedRate}");
            Debug.Log($"Kill Rate: {killRate}");
            Debug.Log($"Diffusion Rate A: {diffusionRateA}");
            Debug.Log($"Diffusion Rate B: {diffusionRateB}");
        }

    }
    
}
