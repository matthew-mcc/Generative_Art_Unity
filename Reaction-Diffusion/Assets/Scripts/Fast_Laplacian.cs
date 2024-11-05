using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Fast_Laplacian : MonoBehaviour
{

    private Texture2D displayTexture; 
    public int width = 100;      // Width of the grid
    public int height = 100;     // Height of the grid
    public int seedCount = 1;    // Initial seed points
    private int[,] aggregateMap;

    private Dictionary<Vector2Int, float> candidateSites; // Candidate sites and their potentials

    private float R1 = 1.0f;     // Constant for potential calculations
    private float eta = 2.0f;    // Exponent parameter in Equation 5

    // Start is called before the first frame update
    void Start()
    {
        InitializeAlgorithm();
    }

    private void InitializeAlgorithm()
    {
        // INITIALIZE DISPLAY TEXTURE
        displayTexture = new Texture2D(width, height);
        displayTexture.filterMode = FilterMode.Point; // Keep pixels crisp
        displayTexture.wrapMode = TextureWrapMode.Clamp; // No repeating

        // Set the texture to the material
        var material = GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayTexture;

        // INITIALIZE ALGORITHM
        aggregateMap = new int[width, height];
        aggregateMap[width/2, height/2] = 1; // Initial Seed

        candidateSites = new Dictionary<Vector2Int, float>();

        // Add candidates for the initial seed 
        AddCandidateSites(new Vector2Int(width / 2, height / 2));

        // Calculate Potentials
        CalculatePotentials();
        
    }

    void IterateAlgorithm(){

        

        // 1. Select a growth site based on EQN 5.
        Vector2Int growthSite = SelectGrowthSite();

        // 2. Add the new charge at growth site
        aggregateMap[growthSite.x, growthSite.y] = 1;

        // 3. Update the potential at all candidate sites according to EQN 4.
        UpdatePotentials(growthSite);
        // CalculatePotentials();

        // 4. Add the new candidate sites surroudning the growth site
        AddCandidateSites(growthSite);

        // 5. Calculate the potential at new candidates using EQN 3.
        CalculatePotentials();
        
    
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
        // Sum of (phi^eta) for all candidate sites
        float total = 0;
        foreach (float potential in candidateSites.Values)
        {
            total += Mathf.Pow(potential, eta);
        }

        // Generate a random value for weighted selection
        float randomValue = UnityEngine.Random.value * total;
        float cumulative = 0;

        // Iterate over candidates to find the selected growth site
        foreach (var site in candidateSites)
        {
            cumulative += Mathf.Pow(site.Value, eta);
            if (cumulative >= randomValue)
            {
                return site.Key;
            }
        }

        // If for some reason no site was selected, return a random candidate
        return candidateSites.Keys.First();
    }

    private void CalculatePotentials()
{
    // Create a separate list of keys to avoid modifying the dictionary while iterating
    var keys = new List<Vector2Int>(candidateSites.Keys);
    
    foreach (var site in keys)
    {
        float phi = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (aggregateMap[x, y] == 1) // Active site
                {
                    float r = Vector2Int.Distance(site, new Vector2Int(x, y));

                    if (r > 0)
                    {
                        phi += (1 - R1 / r);
                    }
                }
            }
        }

        // Update the potential in the dictionary after calculating it
        candidateSites[site] = phi;
    }
}

    // Update is called once per frame
    void FixedUpdate()
    {
        IterateAlgorithm();
        UpdateTexture();   
    }

    // Updates the display texture based on the map array
    private void UpdateTexture()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Set color based on whether the point is occupied
                Color color = aggregateMap[x, y] > 0 ? Color.white : Color.black;
                displayTexture.SetPixel(x, y, color);
            }
        }

        // Apply changes to update the texture
        displayTexture.Apply();
    }

    private void AddCandidateSites(Vector2Int growthSite)
    {
        // Add neighboring sites to candidateSites if they are within bounds and unoccupied
        var neighbors = GetNeighbors(growthSite);

        foreach (var neighbor in neighbors)
        {
            if (aggregateMap[neighbor.x, neighbor.y] == 0 && !candidateSites.ContainsKey(neighbor))
            {
                candidateSites[neighbor] = 0; // Initial potential will be calculated in CalculatePotentials
            }
        }
    }


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
