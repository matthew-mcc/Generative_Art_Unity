using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



public class DifferentialGrowth : MonoBehaviour
{
    [SerializeField] SimSettings settings;


    public List<Particle> clump;
    // 99% sure these aren't actually doing anything...
    private const int screenHeight = 1;
    private const int screenWidth = 1;
    private const float pixelsPerUnit = 1f;
    
    
    int currPIdx = 0;
    LineRenderer lineRenderer;

    void InitializeSimulation(){

       
        InitializeLineRenderer();

        clump = new List<Particle>();
        int numParticles = settings.initialParticleCount;

        float radius;
        float angle = 0;
        for (int i = 0; i < numParticles; i++){
            if(settings.includeRandomness){
            radius = settings.initialRadius + UnityEngine.Random.Range(-1.0f, 1.0f);
            }
            
            else{
                radius = settings.initialRadius;
            }

            angle = Mathf.Lerp(0, Mathf.PI * 2, (float) i / numParticles); // map each particle to a particular angle in a circle
            Vector2 initialPosition = new Vector2(screenWidth/2f + radius * Mathf.Cos(angle), screenHeight/2f + radius * Mathf.Sin(angle));
            
            Particle particle = new Particle(initialPosition.x, initialPosition.y, settings);
            clump.Add(particle);

        }
    }

    public void ResetSimulation()
    {

        clump.Clear();
        lineRenderer.positionCount = 0;
        InitializeSimulation();
    }

    // Start is called before the first frame update
    void Start()
    {

        InitializeSimulation();
    
        
    }

    void InitializeLineRenderer(){

        GameObject lineGO = new GameObject("ClumpLineRenderer");
        lineRenderer = lineGO.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 0;
        lineRenderer.loop = true; // Enable loop for circular rendering

        // Assign the color
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
    }

    
    void FixedUpdate(){
        
        if (settings.isSimRunning){
            Time.fixedDeltaTime = 1 / 30f;

            if (clump.Count <= 2500){
                // Debug.Log("Maxed out!");
                DrawAllParticles();
            }
            
        }
        
        
    }

    void Update(){

        if (settings.isSimRunning){
            if (clump.Count <= 2500){
            UpdateAllParticles();
            }
          
        }
        // Debug.Log(clump.Count);
    }


void UpdateAllParticles(){
    // Update particles
    for (int i = 0; i < clump.Count; i++)
    {
        clump[i].update(clump, i);
    }

    foreach (Particle p in clump)
    {
        p.updatePosition();
    }

    // Insert new particles
    insert(clump);
}


void DrawAllParticles()
{
        // Update LineRenderer positions
        lineRenderer.positionCount = clump.Count;
        for (int i = 0; i < clump.Count; i++)
        {
            Vector3 pos = new Vector3(clump[i].position.x, clump[i].position.y, 0);
            lineRenderer.SetPosition(i, pos);

            
        }

        
}
    
void insert(List<Particle> c)
    {
    
        for (int i = 0; i < c.Count; i++)
        {
            Particle p = c[i];
            Particle p1 = c[(i + 1) % c.Count]; // Circular connection

            // Calculate difference vector and distance
            Vector2 diff = p1.position - p.position;

            if (diff.magnitude > settings.insertDistance)
            {
                // Calculate midpoint
                Vector2 midPoint = p.position + (diff * 0.5f);

                Particle newParticle = new Particle(midPoint.x, midPoint.y, settings);
                c.Insert((i + 1) % c.Count, newParticle);

                
            }
        }
    }

    
}
