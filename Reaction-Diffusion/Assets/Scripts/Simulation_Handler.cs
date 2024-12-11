using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
// using System.Numerics;
using Unity.VisualScripting;

public class Simulation_Handler : MonoBehaviour
{

    public bool isSimulationRunning = false;

    
    // General Controls
    public Button playPause;
    public TMP_Dropdown modelDropdown;
    public Slider dtSlider;
    public TMP_Text dtSliderText;
    public GameObject rdControls;
    public GameObject laplacianControls;

    // "Panels"
    public GameObject fast_Laplacian;
    public GameObject rD;

    // RD Specific Controls
    public Slider feedRateSlider;
    public TMP_Text feedRateSlider_Text;
    public Slider killRateSlider;
    public TMP_Text killRateSlider_Text;

    public Slider diffuseRateASlider;
    public TMP_Text diffuseRateASlider_Text;
    public Slider diffuseRateBSlider;
    public TMP_Text diffuseRateBSlider_Text;

    // Directional Bias
    public GameObject rd_directionalBiasControls;
    public TMP_Dropdown rd_directionalBias_dropdown;
    public Slider directionalSegmentsSlider;
    public TMP_Text directionalSegmentsSlider_text;
    public Slider directionalBiasSlider;
    public TMP_Text directionalBiasSlider_text;

    // Laplacian Specific Controls
    public Slider etaSlider;
    public TMP_Text etaSlider_Text;
    public Slider R1Slider;
    public TMP_Text R1Slider_Text;

    // Init Maps
    public TMP_Dropdown laplacian_inits_dropdown;
    public TMP_Dropdown rd_inits_dropdown;

    // Color Modes
    public TMP_Dropdown rd_colors_dropdown;
    public TMP_Dropdown laplacian_colors_dropdown;
    // 0 for RD, 1 for L_G
    int currentModel;

    float dt;

    private void Start() {
        currentModel = 0;
        // rD.enabled = true;
        rD.SetActive(true);
        fast_Laplacian.SetActive(false);
        laplacianControls.SetActive(false);

        rd_directionalBiasControls.SetActive(false);

        dt = 1 / dtSlider.value;
        Time.fixedDeltaTime = dt;
    }

    private void FixedUpdate() {


        if (isSimulationRunning){
            playPause.GetComponentInChildren<TMP_Text>().text = "Pause";
        }
        else{
            playPause.GetComponentInChildren<TMP_Text>().text = "Play";
        }


        // Reaction Diffusion
        if(currentModel == 0){
            // rD.enabled = true;
            // fast_Laplacian.enabled = false;
            rD.SetActive(true);
            fast_Laplacian.SetActive(false);
        }

        else if (currentModel == 1){
            // rD.enabled = false;
            rD.SetActive(false);
            // fast_Laplacian.enabled = true;
            fast_Laplacian.SetActive(true);
        }

        // Keep included.
        if (Input.GetKeyDown(KeyCode.Space)){
            PlayPause();
        }

        // Mouse Input for Laplacian Growth
        // Vector3 mousePos = Input.mousePosition;
        // Debug.Log(mousePos);
        
        // This gets me a position from -5 to 5 in both axes.
        Camera mainCam = Camera.main;
        Vector2 mousePos = Input.mousePosition;

        Vector3 testPoint = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));
        // Debug.Log(testPoint);

        Vector2Int test = GetGridIndex(testPoint);
        // Debug.Log(test);
        fast_Laplacian.GetComponent<Fast_Laplacian>().targetPoint = test;
    }

    private static float MapRange(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
    }

    Vector2Int GetGridIndex(Vector3 worldPos){
        Vector2Int gridPoint = new Vector2Int();

        if (worldPos.x > 5 || worldPos.x < -5 || worldPos.y > 5 || worldPos.y < -5){
            // Debug.Log("Out of bounds!");
            gridPoint.x = 0;
            gridPoint.y = 0;
            
        }
        else{
            // Debug.Log("In bounds!");
            int height = fast_Laplacian.GetComponent<Fast_Laplacian>().height;
            int width = fast_Laplacian.GetComponent<Fast_Laplacian>().width;

            float x = MapRange(worldPos.x, -5, 5, 0, width);
            float y = MapRange(worldPos.y, -5, 5, 0, height);
            
            gridPoint.x = (int)x;
            gridPoint.y = (int)y;


        }



        return gridPoint;
    }


    // ============================== General Controls ==============================

    public void PlayPause(){
        if (isSimulationRunning){
            isSimulationRunning = false;
            // playPause.GetComponentInChildren<TMP_Text>().text = "Play";
        }
        else{
            isSimulationRunning = true;
            // playPause.GetComponentInChildren<TMP_Text>().text = "Pause";
        }

        
    }

    public void ChangeDeltaTime(){
        float newDt = dtSlider.value;
        dtSliderText.text = string.Format("FPS: {0}", (int)newDt);
        Time.fixedDeltaTime = 1 / newDt;
    }

    public void Regrow(){
    
        
        
        Debug.Log("Regrow");
        if (currentModel == 0){
            rD.GetComponent<RD_Simulation>().ResetSimulation(); // This does not reset simulation!
        }
        if (currentModel == 1){
            fast_Laplacian.GetComponent<Fast_Laplacian>().ResetSimulation();
        }
        
        isSimulationRunning = true;

    
    }

   
    public void ChangeModel(){
        // 0 == ReactionDiffusion
        // 1 == LaplacianGrowth
        // Debug.Log(modelDropdown.value);

        currentModel = modelDropdown.value;

        if (currentModel == 0){
            // rD.GetComponent<RD_Simulation>().ResetSimulation(); // This does not reset simulation!
            isSimulationRunning = false;
            rdControls.SetActive(true);
            laplacianControls.SetActive(false);

        }
        else if (currentModel == 1){
            // fast_Laplacian.GetComponent<Fast_Laplacian>().ResetSimulation();
            isSimulationRunning = false;
            rdControls.SetActive(false);
            laplacianControls.SetActive(true);
            dtSlider.value = 30f;
            dtSliderText.text = string.Format("FPS: {0}", (int)30f);
        }
        

    }

    public void SaveImage(){

        // Pause simulation, so they know what image they are getting!
        if (isSimulationRunning){
            isSimulationRunning = false;
            
        }

        DateTime dt = DateTime.Now;
        string outputPath = "Output_Images/" + dt.ToString("yyyy-MM-dd_HH-mm-ss");
        

        RenderTexture rt = rD.GetComponent<RD_Simulation>().displayGrid; // Default to RD so unity doesn't complain

        if (currentModel == 0){
            outputPath += "_Reaction_Diffusion" + ".png";
            rt = rD.GetComponent<RD_Simulation>().displayGrid;
        }

        else if (currentModel == 1){
            outputPath += "_Laplacian_Growth" + ".png";
            rt = fast_Laplacian.GetComponent<Fast_Laplacian>().renderTexture;
        }

        // Everything else is not specific to model.
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(outputPath, bytes);
     

        Debug.Log($"Image Saved to: {outputPath}");
    }


    
    // ============================== RD Controls ==============================

    public void ChangeFeedRate(){
        float newFeedRate = feedRateSlider.value;
        feedRateSlider_Text.text = string.Format("Feed Rate: {0:F4}", (float)newFeedRate);

        rD.GetComponent<RD_Simulation>().feedRate = newFeedRate;
        
        // Regrow();
    }

    public void ChangeKillRate(){
        float newKillRate = killRateSlider.value;
        killRateSlider_Text.text = string.Format("Kill Rate: {0:F4}", (float)newKillRate);

        rD.GetComponent<RD_Simulation>().killRate = newKillRate;
      
        // Regrow();
    }

    public void ChangeDiffusionRateA(){
        float newDiffusionRate = diffuseRateASlider.value;
        diffuseRateASlider_Text.text = string.Format("Diff-A: {0:F3}", (float)newDiffusionRate);

        rD.GetComponent<RD_Simulation>().diffusionRateA = newDiffusionRate;
    }

    public void ChangeDiffusionRateB(){
        float newDiffusionRate = diffuseRateBSlider.value;
        diffuseRateBSlider_Text.text = string.Format("Diff-B: {0:F3}", (float)newDiffusionRate);

        rD.GetComponent<RD_Simulation>().diffusionRateB = newDiffusionRate;
    }

    public void ResetParams(){
        // Reset value, slider and texts
        float defaultFeedRate = 0.0325f;
        float defaultKillRate = 0.062f;
        float defaultDiffusionRateA = 1.0f;
        float defaultDiffusionRateB = 0.4f;
        // Feed Rate
        rD.GetComponent<RD_Simulation>().feedRate = defaultFeedRate;
        feedRateSlider.value = defaultFeedRate;
        feedRateSlider_Text.text = string.Format("Feed Rate: {0:F4}", (float)defaultFeedRate);

        // Kill Rate
        rD.GetComponent<RD_Simulation>().killRate = defaultKillRate;
        killRateSlider.value = defaultKillRate;
        killRateSlider_Text.text = string.Format("Kill Rate: {0:F4}", (float)defaultKillRate);

        // Diffusion rate A
        rD.GetComponent<RD_Simulation>().diffusionRateA = defaultDiffusionRateA;
        diffuseRateASlider.value = defaultDiffusionRateA;
        diffuseRateASlider_Text.text = string.Format("Diff-A: {0:F3}", (float)defaultDiffusionRateA);

        // Diffusion rate B
        rD.GetComponent<RD_Simulation>().diffusionRateB = defaultDiffusionRateB;
        diffuseRateBSlider.value = defaultDiffusionRateB;
        diffuseRateBSlider_Text.text = string.Format("Diff-B: {0:F3}", (float)defaultDiffusionRateB);
        
        Regrow();
      
    }

    public void RandomizeParams(){
        float randomFeedRate = UnityEngine.Random.Range(0f, 0.1f);
        float randomKillRate = UnityEngine.Random.Range(0.045f, 0.07f);
        float randomDiffusionRateA = UnityEngine.Random.Range(0f, 1f);
        float randomDiffusionRateB = UnityEngine.Random.Range(0f, 1f);

        // Feed Rate
        rD.GetComponent<RD_Simulation>().feedRate = randomFeedRate;
        feedRateSlider.value = randomFeedRate;
        feedRateSlider_Text.text = string.Format("Feed Rate: {0:F4}", (float)randomFeedRate);

        // Kill Rate
        rD.GetComponent<RD_Simulation>().killRate = randomKillRate;
        killRateSlider.value = randomKillRate;
        killRateSlider_Text.text = string.Format("Kill Rate: {0:F4}", (float)randomKillRate);

        // Diffusion rate A
        rD.GetComponent<RD_Simulation>().diffusionRateA = randomDiffusionRateA;
        diffuseRateASlider.value = randomDiffusionRateA;
        diffuseRateASlider_Text.text = string.Format("Diff-A: {0:F3}", (float)randomDiffusionRateA);

        // Diffusion rate B
        rD.GetComponent<RD_Simulation>().diffusionRateB = randomDiffusionRateB;
        diffuseRateBSlider.value = randomDiffusionRateB;
        diffuseRateBSlider_Text.text = string.Format("Diff-B: {0:F3}", (float)randomDiffusionRateB);
        
        Regrow();

    }


    // DIRECTIONAL BIAS
    public void ChangeDirectionalBiasMode(){
        int directionalModel = rd_directionalBias_dropdown.value;
        rd_directionalBiasControls.SetActive(true);

        // None
        if (rd_directionalBias_dropdown.value == 0){
            rD.GetComponent<RD_Simulation>().directionalMode = directionalModel;

            rd_directionalBiasControls.SetActive(false);

        }
        else{
            rD.GetComponent<RD_Simulation>().directionalMode = directionalModel;
        }

        // Regrow(); 
    }
    
    public void ChangeDirectionalBias(){
        float newDirectionalBias = directionalBiasSlider.value;
        directionalBiasSlider_text.text = string.Format("Bias: {0:F3}", newDirectionalBias);
        rD.GetComponent<RD_Simulation>().directionalBias = newDirectionalBias;
    }

    public void ChangeDirectionalSegments(){
        int newDirectionalSegments = (int) directionalSegmentsSlider.value;
        directionalSegmentsSlider_text.text = $"Segments: {newDirectionalSegments}";
        rD.GetComponent<RD_Simulation>().numDirectionalSegments = newDirectionalSegments;
    }

    public void ChangeInitialMap_RD(){
        int newInitMapMode = rd_inits_dropdown.value;
        rD.GetComponent<RD_Simulation>().initialConcentrationMap = newInitMapMode;
        Regrow();
    }

    public void ChaneColorMode_RD(){
        int newColorMode = rd_colors_dropdown.value;
        rD.GetComponent<RD_Simulation>().colorMode = newColorMode;
        // Regrow();
    }

    // ============================== Laplacian Controls ==============================

    public void ChangeEta(){
        float newEta = etaSlider.value;
        etaSlider_Text.text = string.Format("eta: {0:F3}", (float)newEta);

        fast_Laplacian.GetComponent<Fast_Laplacian>().eta = newEta;
        
        Regrow();
    }

    public void ChangeR1(){
        float newR1 = R1Slider.value;
        R1Slider_Text.text = string.Format("R1: {0:F3}", (float)newR1);

        fast_Laplacian.GetComponent<Fast_Laplacian>().R1 = newR1;

        Regrow();
    }

    public void ChangeInitialMap_Laplacian(){
        int newInitMapMode = laplacian_inits_dropdown.value;
        fast_Laplacian.GetComponent<Fast_Laplacian>().initialMapMode = newInitMapMode;
        Regrow();
    }

    public void ResetParams_Laplacian(){
        // Reset value, slider and texts
        float defaultEta = 5.0f;
        float defaultR1 = 0.5f;

        // eta
        fast_Laplacian.GetComponent<Fast_Laplacian>().eta = defaultEta;
        etaSlider.value = defaultEta;
        etaSlider_Text.text = string.Format("eta: {0:F3}", defaultEta);

        // R1
        fast_Laplacian.GetComponent<Fast_Laplacian>().R1 = defaultR1;
        R1Slider.value = defaultR1;
        R1Slider_Text.text = string.Format("R1: {0:F3}", defaultR1);
        
        Regrow();
    }

    public void RandomizeParams_Laplacian(){
        float randomEta = UnityEngine.Random.Range(etaSlider.minValue, etaSlider.maxValue);
        float randomR1 = UnityEngine.Random.Range(R1Slider.minValue, R1Slider.maxValue);

        fast_Laplacian.GetComponent<Fast_Laplacian>().eta = randomEta;
        etaSlider.value = randomEta;
        etaSlider_Text.text = string.Format("eta: {0:F3}", randomEta);

        fast_Laplacian.GetComponent<Fast_Laplacian>().R1 = randomR1;
        R1Slider.value = randomR1;
        R1Slider_Text.text = string.Format("R1: {0:F3}", randomR1);

        Regrow();
    }
    
    public void ChangeColorMode_laplacian(){
        int newColorMode = laplacian_colors_dropdown.value;
        fast_Laplacian.GetComponent<Fast_Laplacian>().colorMode = newColorMode;
        Regrow();
    }

    
    




    

}

