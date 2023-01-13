using System.Collections;
using UnityEngine;
using FluidFlow;

public static class FFSimulatorExtensions
{
    public static void FadeOut(this FFSimulator simulator, float amountPerSecond = 5f, float duration = 2f)
    {
        IEnumerator fade()
        {
            float time = duration;
            while (time >= 0) {
                using (var paintScope = simulator.Canvas.BeginPaintScope(simulator.TargetTextureChannel, false)) {
                    if (paintScope.IsValid)
                        Fluid.Fade(paintScope.Target, amountPerSecond * Time.deltaTime);
                }
                yield return null;
                time -= Time.deltaTime;
            }
        };
        simulator.StartCoroutine(fade());
    }
}