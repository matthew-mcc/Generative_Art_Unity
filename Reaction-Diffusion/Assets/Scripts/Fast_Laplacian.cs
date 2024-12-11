using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
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

    int seedX = 200;
    int seedY = 200;
    bool needsDisplayUpdate = true;
    // Simulation Controls
    public Simulation_Handler sim;
    public Vector2Int targetPoint;

    public Texture2D initMap; 
    public Laplacian_InitMap_Helper initHelper;

    public int colorMode = 0;

    public bool followingMouse = false;

    // 0 = center, 1 = random seed, 2 = 20 random points, 3 = square, 4 = triangle, 5 = circle, 6 = cross
    public int initialMapMode = 0; // 0 = Square, 1 = Triangle, 2 = Circle

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

        targetPoint = new Vector2Int();
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

        computeShader.SetInt("seedX", seedX);
        computeShader.SetInt("seedY", seedY);
        // computeShader.SetVector("colorA", new Vector4(1f, 0f, 0f, 1f)); // Example red
        // computeShader.SetVector("colorB", new Vector4(0f, 0f, 1f, 1f)); // Example blue
        // computeShader.SetVector("colorA", new Vector4(0.5f, 0f, 0.5f, 1f)); // Purple
        // computeShader.SetVector("colorB", new Vector4(0f, 1f, 0f, 1f));     // Green
        computeShader.SetVector("colorB", new Vector4(1f, 0f, 0f, 1f)); // Red
        computeShader.SetVector("colorA", new Vector4(1f, 1f, 0f, 1f)); // Yellow

        computeShader.SetInt("colorMode", colorMode);
        computeShader.SetFloat("colorW", 0.5f);
        computeShader.SetFloat("colorZ", 0.2f);
        
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        aggregateMap1D = new int[width * height];
        
    }

    private void InitializeAlgorithm(){
        aggregateMap = new int[width, height];
        candidateSites = new Dictionary<Vector2Int, float>();
        pointCharges = new List<Vector2Int>();

        

        switch (initialMapMode)
        {
            case 0:
                aggregateMap[200, 200] = 1;
                break;
            case 1:
                int randX = (int) UnityEngine.Random.Range(0, width);
                int randY = (int) UnityEngine.Random.Range(0, height);
                aggregateMap[randX, randY] = 1;
                break;

            case 2: 
                // i random points
                for (int i = 0; i < 20; i++){
                    int x = (int) UnityEngine.Random.Range(0, width);
                    int y = (int) UnityEngine.Random.Range(0, height);
                    aggregateMap[x, y] = 1;
                    
                }
                break;
            case 3:
                initHelper.DrawSquareOutline(aggregateMap, 150, 150, 100); // Square with top-left corner at (150,150) and side length 100
                break;

            case 4:
                initHelper.DrawTriangleOutline(aggregateMap, new Vector2Int(100, 100), new Vector2Int(200, 300), new Vector2Int(300, 100)); // Triangle with vertices
                break;

            case 5:
                initHelper.DrawCircleOutline(aggregateMap, 200, 200, 50); // Circle with center at (200,200) and radius 50
                break;
            
            case 6:
                for(int i = 0; i < width; i++){
                    aggregateMap[i, 200] = 1;
                }
                for (int i = 0; i < height; i++){
                    aggregateMap[200, i] = 1;
                }
                break;

        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (aggregateMap[x, y] == 1)
                {
                    Dictionary<Vector2Int, float> newCandidateSites = AddCandidateSites(new Vector2Int(x, y));
                    pointCharges.Add(new Vector2Int(x, y));

                    // Calculate Potentials
                    CalculatePotentials(new List<Vector2Int>(newCandidateSites.Keys));
                }
            }
        }
    }


//     private void InitializeAlgorithm()
// {
//     aggregateMap = new int[width, height];
//     candidateSites = new Dictionary<Vector2Int, float>();
//     pointCharges = new List<Vector2Int>();


//     // Parameters for the square
//     int startX = 150; // Top-left corner X
//     int startY = 150; // Top-left corner Y
//     int sideLength = 100; // Length of the sides

//     // Draw the square outline
//     DrawSquareOutline(aggregateMap, startX, startY, sideLength);

//     // Initialize candidate sites and potentials
//     for (int x = 0; x < width; x++)
//     {
//         for (int y = 0; y < height; y++)
//         {
//             if (aggregateMap[x, y] == 1)
//             {
//                 Dictionary<Vector2Int, float> newCandidateSites = AddCandidateSites(new Vector2Int(x, y));
//                 pointCharges.Add(new Vector2Int(x, y));

//                 // Calculate Potentials
//                 CalculatePotentials(new List<Vector2Int>(newCandidateSites.Keys));
//             }
//         }
//     }
// }



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

        foreach (var charge in pointCharges)
        {
            float r = Vector2Int.Distance(site, charge);
            if (r > 0)
            {
                phi += (1 - R1 / r);
            }
        }

        // This works really well, let's see if we can connect it to a particular point instead of just linear / horizontal
        // Add a vertical directional bias


        // RIGHT BIAS
        // float rightBias = Mathf.Lerp(0.5f, 15f, (float)site.x / width); // Right bias
        // phi *= rightBias; // Amplify potential based on bias


        if (followingMouse){
            // Must have a way to make this more pronounced...
            // candidateSites[site] = phi;
            // Vector2Int targetPoint = new Vector2Int(0, 0); // 0, 0 is bottom left of the grid
            // Add directional bias toward the target point
            float distanceToTarget = Vector2Int.Distance(site, targetPoint);
            // Debug.Log(distanceToTarget); 
            float maxDistance = Mathf.Sqrt(width * width + height * height);
            
            float directionalBias = distanceToTarget / maxDistance;

            // Debug.Log($"Distance to Target: {distanceToTarget}");
            // Calculate bias using an inverse-distance weighting (closer to target = higher bias)
            // float directionalBias = Mathf.Lerp(50f, 10900f, distanceToTarget / maxDistance); // Adjustable weights
            if (distanceToTarget > 0){
                // phi /= directionalBias; // Amplify potential based on distance to target point
                phi += (1 / directionalBias) * 5;
                
            }
        }
        
        // Debug.Log(phi);
        candidateSites[site] = phi; // Store the calculated potential
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
                for (int i = 0; i < 5; i++){
                    IterateAlgorithm();
                    iterations++;
                }
                // UpdateTexture();
                needsDisplayUpdate = true;
            }
            
        }
        
    }

    void Update(){
        
        if (needsDisplayUpdate){
            UpdateTexture();
            needsDisplayUpdate = false;
        }

        if (Input.GetMouseButton(0)){
            // Debug.Log("down");
            followingMouse = true;
        }
        else{
            // Debug.Log("up");
            followingMouse = false;
        }

        
    }


    private void UpdateTexture(){

        // This is likely the issue...
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
