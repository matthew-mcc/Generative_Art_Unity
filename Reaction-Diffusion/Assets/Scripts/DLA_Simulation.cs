using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLA_Simulation : MonoBehaviour
{
public int width = 64;
    public int height = 64;
    public ComputeShader computeShader;

    public int numCharges = 1;
    public int numCandidates = 4;
    public float R1 = 1;
    public float eta = 0.1f;

    private RenderTexture displayGrid;
    private ComputeBuffer chargePositionsBuffer;
    private ComputeBuffer candidatePositionsBuffer;
    private ComputeBuffer potentialBuffer;
    private ComputeBuffer growthSiteBuffer;

    private List<Vector3> chargePositions = new List<Vector3>();
    private List<Vector3> candidatePositions = new List<Vector3>();

    void Start()
    {
        Time.fixedDeltaTime = 1 / 60.0f;

        Initialize();
        var material = transform.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = displayGrid;
    }

    void Initialize()
    {
        displayGrid = new RenderTexture(width, height, 24);
        displayGrid.enableRandomWrite = true;
        displayGrid.Create();

        chargePositionsBuffer = new ComputeBuffer(numCharges, sizeof(float) * 3);
        candidatePositionsBuffer = new ComputeBuffer(numCandidates, sizeof(float) * 3);
        potentialBuffer = new ComputeBuffer(numCandidates, sizeof(float));
        growthSiteBuffer = new ComputeBuffer(1, sizeof(float));

        for (int i = 0; i < numCharges; i++)
            chargePositions.Add(new Vector3(Random.Range(0, width), Random.Range(0, height), 0));
        for (int i = 0; i < numCandidates; i++)
            candidatePositions.Add(new Vector3(Random.Range(0, width), Random.Range(0, height), 0));

        chargePositionsBuffer.SetData(chargePositions.ToArray());
        candidatePositionsBuffer.SetData(candidatePositions.ToArray());

        computeShader.SetInt("numCharges", numCharges);
        computeShader.SetInt("numCandidates", numCandidates);
        computeShader.SetFloat("R1", R1);
        computeShader.SetFloat("eta", eta);
        computeShader.SetTexture(0, "Result", displayGrid);
        computeShader.SetBuffer(0, "chargePositions", chargePositionsBuffer);
        computeShader.SetBuffer(0, "candidatePositions", candidatePositionsBuffer);
        computeShader.SetBuffer(0, "potentialBuffer", potentialBuffer);
        computeShader.SetBuffer(0, "growthSiteBuffer", growthSiteBuffer);

        computeShader.Dispatch(0, width / 8, height / 8, 1);
    }

    void Simulate()
    {
        computeShader.SetBuffer(1, "chargePositions", chargePositionsBuffer);
        computeShader.SetBuffer(1, "candidatePositions", candidatePositionsBuffer);
        computeShader.SetBuffer(1, "potentialBuffer", potentialBuffer);
        computeShader.Dispatch(1, numCandidates, 1, 1);

        computeShader.SetBuffer(2, "potentialBuffer", potentialBuffer);
        computeShader.SetBuffer(2, "growthSiteBuffer", growthSiteBuffer);
        computeShader.Dispatch(2, 1, 1, 1);

        computeShader.SetBuffer(3, "candidatePositions", candidatePositionsBuffer);
        computeShader.SetBuffer(3, "potentialBuffer", potentialBuffer);
        computeShader.Dispatch(3, numCandidates, 1, 1);

        float[] growthSiteIndex = new float[1];
        growthSiteBuffer.GetData(growthSiteIndex);
        Debug.Log("Selected growth site index: " + growthSiteIndex[0]);
    }

    void FixedUpdate()
    {
        Simulate();
    }

    private void OnDestroy()
    {
        chargePositionsBuffer.Release();
        candidatePositionsBuffer.Release();
        potentialBuffer.Release();
        growthSiteBuffer.Release();
        displayGrid.Release();
    }
}
