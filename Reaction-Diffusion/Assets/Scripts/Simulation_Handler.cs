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

    public Fast_Laplacian fast_Laplacian;
    public RD_Simulation rD;

    // 0 for RD, 1 for L_G
    int currentModel;

    private void Start() {
        currentModel = 0;
        rD.enabled = true;
    }

    public void PlayPause(){
        if (isSimulationRunning){
            isSimulationRunning = false;
            playPause.GetComponentInChildren<TMP_Text>().text = "Play";
        }
        else{
            isSimulationRunning = true;
            playPause.GetComponentInChildren<TMP_Text>().text = "Pause";
        }

        
    }

    public void Regrow(){
        Debug.Log("Regrow");
        isSimulationRunning = true;
    }

   
    public void ChangeModel(){
        // 0 == ReactionDiffusion
        // 1 == LaplacianGrowth
        // Debug.Log(modelDropdown.value);

        currentModel = modelDropdown.value;
        

    }

    private void Update() {
        
        // Reaction Diffusion
        if(currentModel == 0){
            rD.enabled = true;
            fast_Laplacian.enabled = false;
        }

        else if (currentModel == 1){
            rD.enabled = false;
            fast_Laplacian.enabled = true;
        }


    }




    

}

