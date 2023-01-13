using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluidFlow;

public class SimpleDrawSphere : MonoBehaviour
{
    public FFCanvas Canvas;

    public string TargetChannel;
    public FFBrushSO Brush;
    public float Radius = .2f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Canvas.DrawSphere(TargetChannel, Brush, transform.position, Radius);
        }
    }
}