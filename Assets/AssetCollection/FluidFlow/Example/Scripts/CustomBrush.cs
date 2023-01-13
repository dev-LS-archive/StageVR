using System;
using UnityEngine;
using FluidFlow;

public class CustomBrush : MonoBehaviour
{
    public FFCanvas Canvas;

    public string TargetChannel;
    public FFBrushSO Brush;
    public float Radius = .2f;

    public void Drawer()
    {
        Canvas.DrawSphere(TargetChannel, Brush, transform.position, Radius);
    }
}
