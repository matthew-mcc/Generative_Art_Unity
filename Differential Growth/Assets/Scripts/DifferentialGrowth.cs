using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



class Particle{
    public Vector2 position;
    public Vector2 velocity;
    SpriteRenderer spriteRenderer;
    float repulusionDistance = 5f;
    float k = 0.0001f;

    // public Particle(float x, float y, SpriteRenderer sr){
    public Particle(float x, float y){
        position = new Vector2(x, y);
        velocity = new Vector2(0, 0);
        // circle = circ;
        // spriteRenderer = sr;
        

        
        

    }
    public void update(List<Particle> c, int n){
        Vector2 diff = new Vector2(0, 0);
        Vector2 forces = new Vector2(0, 0);
        float distance;

        foreach(Particle p in c){
            if(p != this){
                diff = p.position - position;
                distance = diff.magnitude;

                if (distance < repulusionDistance){
                    diff.Normalize();
                    diff = diff * -1 / (distance * distance);
                    // forces.
                    forces += diff;
                }
            }
        }

        // For mass 1, this is unecessary
        // Vector2 acceleration = new Vector2(0, 0);
        // acceleration = forces;
        // acceleration = acceleration / 1;
        // velocity += acceleration;
        int neighbor = (n + 1) % c.Count;
        Particle temp = c[neighbor];
        diff = temp.position - position;
        distance = diff.magnitude;
        diff.Normalize();
        diff = diff * -1 / (distance*distance);
        if (distance < 0.5f){ //0.5f == insertDistance / 2 (NEED TO CNNECT THEM ACROSS CLASSES)
            diff *= -1;
        }
        forces += diff;

        neighbor = ((n - 1) + c.Count) % c.Count;
        temp = c[neighbor];
        diff = temp.position - position;
        distance = diff.magnitude;
        diff.Normalize();
        diff = diff * -1 / (distance*distance);
        if (distance < 0.5f){ //0.5f == insertDistance / 2 (NEED TO CNNECT THEM ACROSS CLASSES)
            diff *= -1;
        }
        forces += diff;
        

        forces = forces * k; // similar to spring constant
        velocity += forces;
    }

    public void updatePosition(){
        position += velocity;
        // Debug.Log(velocity);
        velocity = velocity * 0.7f;
    }

    public void display(Vector2 cameraCenter){
        // TODO: Display COde
        // spriteRenderer.transform.position = new Vector3(position.x, position.y, 0);
        spriteRenderer.transform.position = new Vector3(position.x, position.y, 0);
    }
}

public class DifferentialGrowth : MonoBehaviour
{
    [SerializeField] Sprite circle_sprite;


    List<Particle> clump;
    // int width = 10;
    // int height = 10;

    private const int screenHeight = 1;
    private const int screenWidth = 1;
    private const float pixelsPerUnit = 1f;
    
    float insertDistance = 1f;
    
    int currPIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Adjust the camera settings
        // SetupCamera();

        clump = new List<Particle>();
        int numParticles = 4;
        float radius = 1.0f;
        float angle = 0;
        for (int i = 0; i < numParticles; i++){
            angle = Mathf.Lerp(0, Mathf.PI * 2, (float) i / numParticles); // map each particle to a particular angle in a circle
            Vector2 initialPosition = new Vector2(screenWidth/2f + radius * Mathf.Cos(angle), screenHeight/2f + radius * Mathf.Sin(angle));
            
            Particle particle = new Particle(initialPosition.x, initialPosition.y);
            clump.Add(particle);

        }

        
    }

    void SetupCamera() {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            Debug.LogError("Main Camera is not set in the scene!");
            return;
        }

        // Set orthographic camera size and pixels per unit mapping
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = screenHeight / 2f / pixelsPerUnit;

        // Center the camera in the pixel space
        mainCamera.transform.position = new Vector3(screenWidth / 2f, screenHeight / 2f, -10f);
    }

    // Update is called once per frame
    void Update(){
        
        Vector2 cameraCenter = Camera.main.transform.position;

        DrawAllParticles(cameraCenter);
    }

    GameObject DrawLine(Vector2 start, Vector2 end)
    {
        // Create a new Line Renderer GameObject
        GameObject lineGO = new GameObject("Line");
        LineRenderer lineRenderer = lineGO.AddComponent<LineRenderer>();

        // Configure the Line Renderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.025f;
        lineRenderer.endWidth = 0.025f;

        // Set positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, new Vector3(start.x, start.y, 0));
        lineRenderer.SetPosition(1, new Vector3(end.x, end.y, 0));

        return lineGO; // Return the created line for tracking
    }

    List<GameObject> lines = new List<GameObject>();

    void DrawAllParticles(Vector2 cameraCenter)
    {
        // Clear previous lines
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        // Draw new lines
        for (int i = 0; i < clump.Count; i++)
        {
            Particle p = clump[i];
            Particle p1 = clump[(i + 1) % clump.Count];
            GameObject lineGO = DrawLine(p.position, p1.position);
            lines.Add(lineGO);
        }

        // Update and move particles
        for (int i = 0; i < clump.Count; i++)
        {
            Particle p = clump[i];
            p.update(clump, i);
        }

        foreach (Particle p in clump)
        {
            p.updatePosition();
        }

        insert(clump);
    }
    
void insert(List<Particle> c)
    {
        int initialCount = c.Count; // Prevent infinite growth

        for (int i = 0; i < initialCount; i++)
        {
            Particle p = c[i];
            Particle p1 = c[(i + 1) % c.Count]; // Circular connection

            // Calculate difference vector and distance
            Vector2 diff = p1.position - p.position;

            if (diff.magnitude > insertDistance)
            {
                // Calculate midpoint
                Vector2 midPoint = p.position + (diff * 0.5f);

                Particle newParticle = new Particle(midPoint.x, midPoint.y);
                c.Insert((i + 1) % c.Count, newParticle);

                // Break to prevent multiple inserts in a single iteration
                return;
            }
        }
    }
}
