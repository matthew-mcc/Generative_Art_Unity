using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLA_Simulation : MonoBehaviour
{

    // Node class represents each point in the cluster
    private class Node
    {
        public Node parent;
        public int x, y;

        public Node(Node parent, int x, int y)
        {
            this.parent = parent;
            this.x = x;
            this.y = y;
        }

        // Recursive visit function to update visitation map
        public void Visit(int[,] map)
        {
            map[x, y]++;
            parent?.Visit(map);
        }
    }


    public int width = 100;      // Width of the grid
    public int height = 100;     // Height of the grid
    public int seedCount = 1;    // Initial seed points
    public bool cyclic = true;   // Enable or disable cyclic boundaries
    private Node[,] nodes;       // Grid of nodes
    private int[,] map;          // Grid of visitation counts
    private int totalNodes;      // Total nodes in the cluster
    private Texture2D displayTexture; 


    private void Start()
    {
        Debug.Log("Starting!");
        InitializeGrid();
        PlaceInitialSeeds();
        CreateDisplayTexture();
        StartCoroutine(RunSimulation());
        
    }
    // Creates and sets up the display texture
    private void CreateDisplayTexture()
    {
        displayTexture = new Texture2D(width, height);
        displayTexture.filterMode = FilterMode.Point; // Keep pixels crisp
        displayTexture.wrapMode = TextureWrapMode.Clamp; // No repeating

        // Set the texture to the material
        var material = GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayTexture;
    }

    // Initializes the grid with empty nodes and visitation map
    private void InitializeGrid()
    {
        nodes = new Node[width, height];
        map = new int[width, height];
        totalNodes = seedCount;
    }

    // Places initial seed nodes randomly on the grid
    private void PlaceInitialSeeds()
    {
        for (int i = 0; i < seedCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            nodes[x, y] = new Node(null, x, y);
            map[x, y] = 1;
        }
    }

    // Runs the DLA simulation
    private IEnumerator RunSimulation()
    {
        while (totalNodes < width * height)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            // If the starting location is already occupied, continue
            if (nodes[x, y] != null) continue;

            // Perform random walk until a neighboring node is found
            bool hit = false;
            while (!hit)
            {
                int lastX = x;
                int lastY = y;

                // Move randomly in a compass direction (N, S, E, W)
                int direction = Random.Range(0, 4);
                switch (direction)
                {
                    case 0: x += 1; break; // Right
                    case 1: x -= 1; break; // Left
                    case 2: y += 1; break; // Up
                    case 3: y -= 1; break; // Down
                }

                // Handle grid boundaries
                if (cyclic)
                {
                    x = (x + width) % width;
                    y = (y + height) % height;
                }
                else if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    break; // Stop the walk if out of bounds in non-cyclic mode
                }

                // Check if the location is occupied by a node
                if (nodes[x, y] != null)
                {
                    hit = true;
                    Node newNode = new Node(nodes[x, y], lastX, lastY);
                    nodes[lastX, lastY] = newNode;
                    newNode.Visit(map);
                    totalNodes++;
                }

                yield return null; // Wait for the next frame to continue
            }
            UpdateTexture();
        }

        Debug.Log("Simulation complete!");

    }

    // Updates the display texture based on the map array
    private void UpdateTexture()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Set color based on whether the point is occupied
                Color color = map[x, y] > 0 ? Color.white : Color.black;
                displayTexture.SetPixel(x, y, color);
            }
        }

        // Apply changes to update the texture
        displayTexture.Apply();
    }


}
