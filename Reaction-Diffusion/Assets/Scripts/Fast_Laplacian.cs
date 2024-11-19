using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Fast_Laplacian : MonoBehaviour
{

    // private Texture2D displayTexture;

    public RenderTexture renderTexture; 
    public ComputeShader computeShader;
    private int[] aggregateMap1D; 
    private ComputeBuffer aggregateBuffer;

    public int width = 100;      // Width of the grid
    public int height = 100;     // Height of the grid
    public int seedCount = 1;    // Initial seed points
    private int[,] aggregateMap;

    private Dictionary<Vector2Int, float> candidateSites; // Candidate sites and their potentials
    private List<Vector2Int> pointCharges;

    public float R1 = 0.50f;     // Constant for potential calculations
    public float eta = 6.0f;    // Exponent parameter in Equation 5
    public int maxIterations = 1000;
    int iterations = 0;


    // Simulation Controls
    public Simulation_Handler sim;

    public void ResetSimulation(){
        if (renderTexture != null) renderTexture.Release();
        InitializeComputeShader();

        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = renderTexture;

        InitializeAlgorithm();

        iterations = 0;

        if (aggregateBuffer != null) aggregateBuffer.Release();

        UpdateTexture();
        

    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeAlgorithm();
        InitializeComputeShader();

        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = renderTexture;

        // Time.fixedDeltaTime = 1 / 240f; <-- Now handled in Simulation_Handler
        
    }

    private void InitializeComputeShader()
    {
        
        
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        aggregateMap1D = new int[width * height];
        
    }

    private void InitializeAlgorithm()
    {

        // INITIALIZE ALGORITHM
        aggregateMap = new int[width, height];
        int initialSeedX = UnityEngine.Random.Range(0, width);
        int initialSeedY = UnityEngine.Random.Range(0, height);
        
        // Alternatively, guarantee one seed in the center
        // int initialSeedX = width / 2;
        // int initialSeedY = height / 2;

        aggregateMap[initialSeedX, initialSeedY] = 1; // Initial seed

        candidateSites = new Dictionary<Vector2Int, float>();
        pointCharges = new List<Vector2Int>();

        // Add candidates for the initial seed 
        Dictionary<Vector2Int, float> newCandidateSites = AddCandidateSites(new Vector2Int(initialSeedX, initialSeedY));
        pointCharges.Add(new Vector2Int(initialSeedX, initialSeedY));

        // Calculate Potentials
        CalculatePotentials(new List<Vector2Int>(newCandidateSites.Keys));
        
    }

    void IterateAlgorithm(){

        

        // 1. Select a growth site based on EQN 5.
        Vector2Int growthSite = SelectGrowthSite();

        // 2. Add the new charge at growth site
        aggregateMap[growthSite.x, growthSite.y] = 1;
        pointCharges.Add(new Vector2Int(growthSite.x, growthSite.y));
        
        // 3. Update the potential at all candidate sites according to EQN 4.
        UpdatePotentials(growthSite);
        // CalculatePotentials();

        // 4. Add the new candidate sites surroudning the growth site
        Dictionary<Vector2Int, float> newCandidateSites = AddCandidateSites(growthSite);

        // 5. Calculate the potential at new candidates using EQN 3.
        CalculatePotentials(new List<Vector2Int>(newCandidateSites.Keys));
        
    
    }

    private void UpdatePotentials(Vector2Int newGrowthSite)
    {
        foreach (var site in candidateSites.Keys.ToList()) // Create a list of keys to avoid modifying during iteration
        {
            // Calculate the distance from the new growth site to the candidate site
            float distance = Vector2Int.Distance(newGrowthSite, site);

            if (distance > 0)
            {
                // Calculate the additional potential contribution using (1 - R1 / r)
                float potentialContribution = 1 - (R1 / distance);

                // Update the potential at the candidate site using φ_i^{t+1} = φ_i^t + (1 - R1 / r)
                candidateSites[site] += potentialContribution;
            }
        }
    }

    private Vector2Int SelectGrowthSite()
    {
        // Step 1: Find the minimum and maximum potentials for normalization
        float minPhi = candidateSites.Values.Min();
        float maxPhi = candidateSites.Values.Max();

        var candidateSite = candidateSites.Keys.First();


        // Step 2: Calculate normalized potentials Φ_i and their powers (Φ_i^eta), and sum them up
        Dictionary<Vector2Int, float> weightedPotentials = new Dictionary<Vector2Int, float>();
        float totalWeightedPotential = 0;
        
        foreach (var site in candidateSites)
        {
            // Normalize Φ_i = (φ_i - φ_min) / (φ_max - φ_min)
            float normalizedPhi = (site.Value - minPhi) / (maxPhi - minPhi);
            float weightedPotential = Mathf.Pow(normalizedPhi, eta);
            
            weightedPotentials[site.Key] = weightedPotential;
            totalWeightedPotential += weightedPotential;
        }

        // Step 3: Generate a random value for weighted selection, scaled by the total weighted potential
        float randomValue = UnityEngine.Random.value * totalWeightedPotential;
        float cumulative = 0;

        // Step 4: Iterate over weighted potentials to find the selected growth site based on probability
        foreach (var site in weightedPotentials)
        {
            cumulative += site.Value;
            if (cumulative >= randomValue)
            {
                candidateSite = site.Key;
                candidateSites.Remove(site.Key);
                // return site.Key;
                return candidateSite;
            }
        }

        // If no site was selected due to rounding errors, return a random candidate
        
        candidateSites.Remove(candidateSites.Keys.First());
        // return candidateSites.Keys.First();
        return candidateSite;
    }


    // Eqn 3 or 10, based on paper version.
    // φ_i = sum_{n, j=0}{1 - (R_1 / r_i,j)}
    private void CalculatePotentials(List<Vector2Int> keys)
    {
        
        foreach (var site in keys) // this is i
        {
            float phi = 0;

            foreach (var charge in pointCharges){
                float r = Vector2Int.Distance(site, charge);
                if (r > 0){
                    phi+= (1 - R1 / r);
                }
            }


            candidateSites[site] = phi;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (sim.isSimulationRunning){
            if (iterations >= maxIterations){
            Debug.Log("Maxed out!");
            }
            else{
                IterateAlgorithm();
                iterations++;
                UpdateTexture();   
            }
        }
        
        
    }


    private void UpdateTexture(){
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                aggregateMap1D[y * width + x] = aggregateMap[x, y];
            }
        }

        // Set up the compute buffer and copy data to it
        if (aggregateBuffer != null) aggregateBuffer.Release(); // Release previous buffer if exists
        aggregateBuffer = new ComputeBuffer(aggregateMap1D.Length, sizeof(int));
        aggregateBuffer.SetData(aggregateMap1D);
        computeShader.SetBuffer(0, "aggregateMap", aggregateBuffer);

        // Dispatch the compute shader
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        // Release the buffer to avoid memory leak
        aggregateBuffer.Release();

        
    }

    
    private Dictionary<Vector2Int, float> AddCandidateSites(Vector2Int growthSite)
    {
        // Add neighboring sites to candidateSites if they are within bounds and unoccupied
        var neighbors = GetNeighbors(growthSite);
        Dictionary<Vector2Int, float> newCandidateSites = new Dictionary<Vector2Int, float>();
        foreach (var neighbor in neighbors)
        {
            if (aggregateMap[neighbor.x, neighbor.y] == 0 && !candidateSites.ContainsKey(neighbor) && !pointCharges.Contains(neighbor))
            {
                
                newCandidateSites[neighbor] = 0;
            }
        }
        return newCandidateSites;
    }


    // Should give us 8 neighbors for a 2D grid (up, down, left, right, 4 diagonals)
    private List<Vector2Int> GetNeighbors(Vector2Int site)
    {
        var neighbors = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = site.x + dx;
                int ny = site.y + dy;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    neighbors.Add(new Vector2Int(nx, ny));
                }
            }
        }
        return neighbors;
    }
}
