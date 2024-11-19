using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    private RenderTexture displayGrid;

    public Texture2D initMap; // todo

    bool updateDisplay = true;

    public int colorMode = 0;


    // Simulation Control things

    public Simulation_Handler sim;

    public void ResetSimulation(){
        // Clear the current grid and next grid by reinitializing them
        if (currentGrid != null) currentGrid.Release();
        if (nextGrid != null) nextGrid.Release();
        if (displayGrid != null) displayGrid.Release();

        currentGrid = new RenderTexture(width, height, 24);
        currentGrid.enableRandomWrite = true;
        currentGrid.Create();

        nextGrid = new RenderTexture(width, height, 24);
        nextGrid.enableRandomWrite = true;
        nextGrid.Create();

        displayGrid = new RenderTexture(width, height, 24);
        displayGrid.enableRandomWrite = true;
        displayGrid.Create();   

        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayGrid;

        // Reset compute shader textures to the newly created grids
        computeShader.SetTexture(0, "currentGrid", currentGrid);
        computeShader.SetTexture(0, "nextGrid", nextGrid);
        computeShader.SetTexture(0, "displayGrid", displayGrid);
        computeShader.SetTexture(0, "initMap", initMap);

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

        // Color mode
        computeShader.SetInt("colorMode", colorMode);

        computeShader.SetTexture(0, "displayGrid", displayGrid);  
        computeShader.SetTexture(0, "currentGrid", currentGrid);
        computeShader.SetTexture(0, "nextGrid", nextGrid);
        computeShader.SetTexture(0, "initMap", initMap);
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        

        UpdateShaderParameters();
    }

 

    public void UpdateShaderParameters(){
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

        // Close but no cigar
        if(Input.GetKeyDown(KeyCode.Space)){
            // // Set the RenderTexture we want to read from
            // RenderTexture renderTexture = displayGrid;
            // RenderTexture.active = renderTexture;

            // // Ensure the GPU has finished all commands
            

            // // Create a new Texture2D with the same dimensions and format as the RenderTexture
            // Texture2D tex2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

            // // Read the pixels from the currently active RenderTexture
            // tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            // tex2D.Apply();

            // // Encode to PNG (or JPG)
            // byte[] bytes = tex2D.EncodeToPNG(); // Use EncodeToJPG for JPG output
            // string filePath = Path.Combine(Application.dataPath, "Output_Images/Test.png");

            // // Ensure the directory exists
            // Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // // Write the file
            // File.WriteAllBytes(filePath, bytes);

            // Debug.Log($"Image saved to {filePath}");

            // // Clean up
            // RenderTexture.active = null;
            // Destroy(tex2D);
        }
    }
    
}
