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

    
    public Button playPause;
    public TMP_Dropdown modelDropdown;
    public Slider dtSlider;
    public TMP_Text dtSliderText;

    public GameObject fast_Laplacian;
    public GameObject rD;

    // 0 for RD, 1 for L_G
    int currentModel;

    float dt;

    private void Start() {
        currentModel = 0;
        // rD.enabled = true;
        rD.SetActive(true);
        fast_Laplacian.SetActive(false);

        dt = 1 / dtSlider.value;
        Time.fixedDeltaTime = dt;
    }

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

        }
        else if (currentModel == 1){
            // fast_Laplacian.GetComponent<Fast_Laplacian>().ResetSimulation();
            isSimulationRunning = false;
        }
        

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




    

}

