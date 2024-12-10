using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laplacian_InitMap_Helper : MonoBehaviour
{
    
// Helper Method: Draw Square Outline
public void DrawSquareOutline(int[,] grid, int startX, int startY, int sideLength)
{
    int endX = startX + sideLength - 1;
    int endY = startY + sideLength - 1;

    // Draw top side
    for (int x = startX; x <= endX; x++)
        grid[x, startY] = 1;

    // Draw bottom side
    for (int x = startX; x <= endX; x++)
        grid[x, endY] = 1;

    // Draw left side
    for (int y = startY; y <= endY; y++)
        grid[startX, y] = 1;

    // Draw right side
    for (int y = startY; y <= endY; y++)
        grid[endX, y] = 1;
}
    // Drawing Triangle Initial Maps

    // https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
public void DrawTriangleOutline(int[,] grid, Vector2Int p1, Vector2Int p2, Vector2Int p3)
{
    DrawLine(grid, p1.x, p1.y, p2.x, p2.y); // Side 1
    DrawLine(grid, p2.x, p2.y, p3.x, p3.y); // Side 2
    DrawLine(grid, p3.x, p3.y, p1.x, p1.y); // Side 3
}

// Helper Method: Draw Circle Outline
// https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
public void DrawCircleOutline(int[,] grid, int centerX, int centerY, int radius)
{
    int x = 0;
    int y = radius;
    int d = 1 - radius;

    while (x <= y)
    {
        PlotCirclePoints(grid, centerX, centerY, x, y);
        PlotCirclePoints(grid, centerX, centerY, y, x);

        x++;
        if (d < 0)
        {
            d += 2 * x + 1;
        }
        else
        {
            y--;
            d += 2 * (x - y) + 1;
        }
    }
}

private void PlotCirclePoints(int[,] grid, int cx, int cy, int x, int y)
{
    if (IsWithinBounds(grid, cx + x, cy + y)) grid[cx + x, cy + y] = 1;
    if (IsWithinBounds(grid, cx - x, cy + y)) grid[cx - x, cy + y] = 1;
    if (IsWithinBounds(grid, cx + x, cy - y)) grid[cx + x, cy - y] = 1;
    if (IsWithinBounds(grid, cx - x, cy - y)) grid[cx - x, cy - y] = 1;

    if (IsWithinBounds(grid, cx + y, cy + x)) grid[cx + y, cy + x] = 1;
    if (IsWithinBounds(grid, cx - y, cy + x)) grid[cx - y, cy + x] = 1;
    if (IsWithinBounds(grid, cx + y, cy - x)) grid[cx + y, cy - x] = 1;
    if (IsWithinBounds(grid, cx - y, cy - x)) grid[cx - y, cy - x] = 1;
}

// Helper Method: Draw Line (for Triangle)
private void DrawLine(int[,] grid, int x0, int y0, int x1, int y1)
{
    int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
    int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
    int err = dx + dy, e2;

    while (true)
    {
        if (IsWithinBounds(grid, x0, y0)) grid[x0, y0] = 1;

        if (x0 == x1 && y0 == y1) break;
        e2 = 2 * err;
        if (e2 >= dy) { err += dy; x0 += sx; }
        if (e2 <= dx) { err += dx; y0 += sy; }
    }
}

// Helper Method: Check Bounds
private bool IsWithinBounds(int[,] grid, int x, int y)
{
    return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
}
}
