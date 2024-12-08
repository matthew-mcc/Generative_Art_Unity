using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimSettings : MonoBehaviour
{
    public Sprite circleSprite;
    public float insertDistance = 0.5f; // Repulsion dist = insert * 5
    // public float repulsionDistance = 2.5f; 
    public float k = 0.0001f;
    public float velocityDamping = 0.7f;
    public int initialParticleCount = 4;
    public float initialRadius = 1.0f;
    public bool includeRandomness = false;

    public bool isSimRunning = false;

    [SerializeField] DifferentialGrowth differentialGrowth;

    // ============= UI REFERENCES ===============

    [SerializeField] TMP_Text fpsText;
    [SerializeField] TMP_Text runningText; 

    [SerializeField] Slider insertSlider;
    [SerializeField] TMP_Text insertSlider_text;

    [SerializeField] Slider velocitySlider;
    [SerializeField] TMP_Text velocitySlider_text;

    [SerializeField] Slider radiusSlider;
    [SerializeField] TMP_Text radiusSlider_text;

    [SerializeField] Slider particlesSlider;
    [SerializeField] TMP_Text particlesSlider_text;

    [SerializeField] Slider kFactorSlider;
    [SerializeField] TMP_Text kFactorSlider_text;

    [SerializeField] Slider kValueSlider;
    [SerializeField] TMP_Text kValueSlider_text;

    [SerializeField] Toggle randomToggle;


    // NEED TO CHANGE THESE DEFAULT VALUES.
    private int kVal = 5;
    private int kFac = 5;




    private void Update() {
        if (isSimRunning){
            runningText.text = "Running!";
        }
        else if (!isSimRunning){
            runningText.text = "Paused!";
        }

        if(differentialGrowth.clump.Count >=2500){
            runningText.text = "Maxed!";
        }

        fpsText.text = $"FPS: {1/Time.deltaTime:F0}";


        if (Input.GetKeyDown(KeyCode.Space)){
            if (isSimRunning){
                isSimRunning = false;
            }
        else if (!isSimRunning){
            isSimRunning = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.R)){
            differentialGrowth.ResetSimulation();
        }
    }


    // ====== UI FUNCS FOR SLIDERS ETC... ======

    public void ChangeInsertDist(){
        float newInsertDist = insertSlider.value;
        insertSlider_text.text = $"Insert: {newInsertDist:F2}";
        // differentialGrowth.
        insertDistance = newInsertDist;
        differentialGrowth.ResetSimulation();
    }

    public void ChangeVelocityDamp(){
        float newVelDamp = velocitySlider.value;
        velocitySlider_text.text = $"vel-damp: {newVelDamp:F2}";
        velocityDamping = newVelDamp;
        differentialGrowth.ResetSimulation();
    }

    public void ChangeInitialRadius(){
        float newRadius = radiusSlider.value;
        radiusSlider_text.text = $"init r: {newRadius:F2}";
        initialRadius = newRadius;
        differentialGrowth.ResetSimulation();
    }

    public void ChangeInitialParticles(){
        int newParticles = (int) particlesSlider.value;
        particlesSlider_text.text = $"init particles: {newParticles}";
        initialParticleCount = newParticles;
        differentialGrowth.ResetSimulation();
    }

    public void ChangeKValue(){
        int newKVal = (int) kValueSlider.value;
        kValueSlider_text.text = $"K-val: {newKVal}";
        kVal = newKVal;

        k = kVal * Mathf.Pow(10, -kFac);

        Debug.Log($"New k: {k}");
        // Debug.Log()
        differentialGrowth.ResetSimulation();
        // Debug.Log()
    }

    public void ChangeKFactor(){
        int newKFac = (int) kFactorSlider.value;
        kFactorSlider_text.text = $"K-fac: {newKFac}";
        kFac = newKFac;

        k = kVal * Mathf.Pow(10, -kFac);


        Debug.Log($"New k: {k}");
        differentialGrowth.ResetSimulation();
    }

    public void ChangeRandomNoise(){
        
        // Debug.Log(randomToggle.isOn);

        includeRandomness = randomToggle.isOn;
        differentialGrowth.ResetSimulation();

    }



}