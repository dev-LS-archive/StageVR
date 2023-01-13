using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluidFlow;

/// <summary>
/// Example snippets for drawing to a FFCanvas
/// </summary>
public class DrawExamples : MonoBehaviour
{
    public FFCanvas Canvas;
    public string ColorProperty = "_MainTex";
    public string FluidProperty = "_FluidTex";
    public string NormalProperty = "_NormalTex";

    public KeyCode ActivationKey = KeyCode.Return;
    public KeyCode LeftKey = KeyCode.LeftArrow;
    public KeyCode RightKey = KeyCode.RightArrow;
    public KeyCode ClearKey = KeyCode.Space;

    [Header("Decal")]
    public Texture DecalColorTexture;

    public Texture DecalNormalTexture;

    public Color DecalColor = Color.red;

    public Texture MaskTexture;

    public FFDecalSO DecalScriptableObject;

    [Header("Decal Projection")]
    public Transform Projector;

    public bool Perspective = true;

    [Header("Sphere Brush")]
    public Transform Brush;

    public float BrushSize = .1f;

    public Color BrushColor = Color.red;

    private int testCase = 0;
    private const int testCaseCount = 11;

    [Range(0f, 1f)]
    public float BrushFade = .5f;

    private void Update()
    {
        // change test case
        var change = (Input.GetKeyDown(LeftKey) ? -1 : 0) + (Input.GetKeyDown(RightKey) ? 1 : 0);
        if (change != 0) {
            testCase += change;
            if (testCase >= testCaseCount)
                testCase = 0;
            if (testCase < 0)
                testCase = testCaseCount - 1;
            Debug.Log("Current test case: " + testCase);
        }

        if (Input.GetKeyDown(ClearKey))
            Canvas.InitializeTextureChannels();

        var projector = getProjector();
        Utility.DebugFrustum(projector);

        if (!Input.GetKeyDown(ActivationKey))
            return;

        // use texture as color source for decal channel
        var textureChannel = FFDecal.Channel.Color(ColorProperty, DecalColorTexture);
        // use solid color as color source for decal channel
        var colorChannel = FFDecal.Channel.Color(ColorProperty, DecalColor);
        // decal channel for drawing fluid (allows to specify fluid amount)
        var fluidChannel = FFDecal.Channel.Fluid(FluidProperty, DecalColorTexture, 2f);
        // decal channel for drawing normal maps (allows to specify normal scale)
        var normalChannel = FFDecal.Channel.Normal(NormalProperty, DecalNormalTexture);

        switch (testCase) {
            // DECAL PROJECTION
            case 0:
                // implicitly convert decal channel to a single channel decal
                Canvas.ProjectDecal(textureChannel, projector);
                break;

            case 1:
                // single channel decal using the alpha channel of a texture as mask
                Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.AlphaMask(MaskTexture), colorChannel), projector);
                break;

            case 2:
                // single channel decal using the red channel of a texture as mask
                Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.TextureMask(MaskTexture, FFDecal.Mask.Component.R), colorChannel), projector);
                break;

            case 3:
                // single channel decal using the red and alpha channel of a texture as mask
                Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.TextureMask(MaskTexture, FFDecal.Mask.Component.R | FFDecal.Mask.Component.A), colorChannel), projector);
                break;

            case 4:
                // two channel decal using the alpha channel of a texture as mask
                Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.AlphaMask(MaskTexture), normalChannel, fluidChannel), projector);
                break;

            case 5:
                // project single channel fluid decal
                Canvas.ProjectDecal(fluidChannel, projector);
                break;

            case 6:
                // project single channel normal decal
                Canvas.ProjectDecal(normalChannel, projector);
                break;

            case 7:
                // implicitly convert decal scriptable object to decal
                Canvas.ProjectDecal(DecalScriptableObject, projector);
                break;

            // BRUSHES (sphere used as example, others work similarly)
            case 8:
                // implicitly convert color to solid color brush (with fade = 0)
                Canvas.DrawSphere(ColorProperty, BrushColor, Brush.position, BrushSize);
                break;

            case 9:
                // solid color brush with fade
                Canvas.DrawSphere(ColorProperty, FFBrush.SolidColor(BrushColor, BrushFade), Brush.position, BrushSize);
                break;

            case 10:
                // fluid brush with fade
                Canvas.DrawSphere(FluidProperty, FFBrush.Fluid(BrushColor, 2f, BrushFade), Brush.position, BrushSize);
                break;
        }
    }

    private FFProjector getProjector()
    {
        if (Perspective) {
            return FFProjector.Perspective(Projector, 45, 1, .1f, 1f);
        } else {
            return FFProjector.Orthogonal(Projector, .1f, .1f, .1f, 1f);
        }
    }
}