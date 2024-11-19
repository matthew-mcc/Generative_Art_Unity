using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

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
    

    // 0 for RD, 1 for L_G
    int currentModel;

    float dt;

    private void Start() {
        currentModel = 0;
        // rD.enabled = true;
        rD.SetActive(true);
        fast_Laplacian.SetActive(false);
        laplacianControls.SetActive(false);

        dt = 1 / dtSlider.value;
        Time.fixedDeltaTime = dt;
    }

    private void Update() {


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
        }
        

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

    // ============================== Laplacian Controls ==============================

    




    

}

