using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FluidFlow;

public class Demo : MonoBehaviour
{
    public FFCanvas Canvas;
    public FFSimulator Simulator;

    public Animator[] Animators;
    public Transform[] Models;

    public SphereBrush Sphere;
    public CapsuleBrush Capsule;
    public DiscBrush Disc;
    public DecalProjector Projector;

    public Dropdown DrawSelect;
    public Toggle ContinuousToggle;
    public Dropdown ModeSelect;
    public Dropdown ColorSelect;
    public Dropdown TextureSelect;
    public Slider FluidAmount;
    public Slider FadeAmount;
    public Slider Size;

    public Texture NormalTex;
    public Texture BloodTex;
    public Texture Blood1Tex;
    public Texture LogoTex;
    public Texture CheckerTex;

    private enum DrawShape
    {
        Mouse,
        Sphere,
        Capsule,
        Disc,
        Projector
    }

    private DrawShape drawShape;

    private enum DrawMode
    {
        Fluid,
        Color,
        Normal
    }

    private DrawMode drawMode;

    private enum ColorMode
    {
        Checker,
        Red,
        Green,
        Blue,
        Pink,
        White,
        Gray,
    }

    private ColorMode colorMode;

    private enum TextureMode
    {
        None,
        Blood,
        Blood2,
        Logo
    }

    private TextureMode textureMode;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Updater continuousDrawUpdater = new Updater(Updater.Mode.FIXED, .02f);

    private void Start()
    {
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
        SelectModel(0);
        SetAnimation(true);
        SetEvaportation(true);

        {
            var options = new List<string>();
            options.AddRange(System.Enum.GetNames(typeof(DrawShape)));
            DrawSelect.SetOptions(options);
        }

        {
            var textureOpts = new List<string>();
            textureOpts.AddRange(System.Enum.GetNames(typeof(TextureMode)));
            TextureSelect.SetOptions(textureOpts);
            TextureSelect.value = 1;
        }

        {
            var colorOpts = new List<string>();
            colorOpts.AddRange(System.Enum.GetNames(typeof(ColorMode)));
            ColorSelect.SetOptions(colorOpts);
        }

        SetSize(Size.value);

        DrawSelect.onValueChanged.AddListener((int i) => UpdateDraw());
        ModeSelect.onValueChanged.AddListener((int i) => UpdateDraw());
        ColorSelect.onValueChanged.AddListener((int i) => UpdateDraw());
        TextureSelect.onValueChanged.AddListener((int i) => UpdateDraw());
        Size.onValueChanged.AddListener(SetSize);
        UpdateDraw();

        continuousDrawUpdater.AddListener(DoDraw);
    }

    private void Update()
    {
        Utility.DebugFrustum(getProjection());
        if (!OnUI()) {
            if (ContinuousToggle.isOn && Input.GetMouseButton(0))
                continuousDrawUpdater.Update();
            else if (!ContinuousToggle.isOn && Input.GetMouseButtonDown(0))
                continuousDrawUpdater.Invoke();
        }
    }

    public void DoDraw()
    {
        var useProjection = drawShape == DrawShape.Mouse || drawShape == DrawShape.Projector;
        FFDecal.ColorSource colorSource = Color.white; // color source can be implicitly created form color or texture
        switch (colorMode) {
            case ColorMode.Red:
                colorSource = Color.red;
                break;

            case ColorMode.Green:
                colorSource = Color.green;
                break;

            case ColorMode.Blue:
                colorSource = Color.blue;
                break;

            case ColorMode.Pink:
                colorSource = Color.magenta;
                break;

            case ColorMode.White:
                colorSource = Color.white;
                break;

            case ColorMode.Gray:
                colorSource = Color.gray;
                break;

            case ColorMode.Checker:
                if (useProjection) // texture can only be used as color source, when using decal projection mode
                    colorSource = CheckerTex;
                break;
        }

        if (useProjection) {
            FFDecal decal;
            FFDecal.Mask mask = new FFDecal.Mask();

            switch (textureMode) {
                case TextureMode.Blood:
                    mask = FFDecal.Mask.AlphaMask(BloodTex);
                    break;

                case TextureMode.Blood2:
                    mask = FFDecal.Mask.AlphaMask(Blood1Tex);
                    break;

                case TextureMode.Logo:
                    mask = FFDecal.Mask.AlphaMask(LogoTex);
                    break;
            }

            switch (drawMode) {
                case DrawMode.Fluid:
                    decal = new FFDecal(mask, FFDecal.Channel.Fluid("_FluidTex", colorSource, FluidAmount.value));
                    break;

                case DrawMode.Normal:
                    decal = new FFDecal(
                        FFDecal.Mask.AlphaMask(Blood1Tex),
                        FFDecal.Channel.Normal("_NormalTex", NormalTex, 2f));
                    break;

                case DrawMode.Color:
                default:
                    decal = new FFDecal(mask, FFDecal.Channel.Color("_MainTex", colorSource));
                    break;
            }
            if (drawShape == DrawShape.Mouse) {
                Canvas.ProjectDecal(decal, getProjection());
            } else {
                Projector.Draw(Canvas, decal);
            }
        } else {
            string targetChannel = drawMode == DrawMode.Fluid ? "_FluidTex" : "_MainTex";
            FFBrush brush;
            if (drawMode == DrawMode.Fluid) {
                brush = FFBrush.Fluid(colorSource.Color, FluidAmount.value, FadeAmount.value);
            } else {
                brush = FFBrush.SolidColor(colorSource.Color, FadeAmount.value);
            }

            switch (drawShape) {
                case DrawShape.Sphere:
                    Sphere.Draw(Canvas, targetChannel, brush);
                    break;

                case DrawShape.Capsule:
                    Capsule.Draw(Canvas, targetChannel, brush);
                    break;

                case DrawShape.Disc:
                    Disc.Draw(Canvas, targetChannel, brush);
                    break;
            }
        }
    }

    public void UpdateDraw()
    {
        System.Enum.TryParse(DrawSelect.options[DrawSelect.value].text, out drawShape);
        System.Enum.TryParse(ModeSelect.options[ModeSelect.value].text, out drawMode);
        System.Enum.TryParse(ColorSelect.options[ColorSelect.value].text, out colorMode);
        System.Enum.TryParse(TextureSelect.options[TextureSelect.value].text, out textureMode);

        Projector.enabled = drawShape == DrawShape.Projector;
        Sphere.enabled = drawShape == DrawShape.Sphere;
        Capsule.enabled = drawShape == DrawShape.Capsule;
        Disc.enabled = drawShape == DrawShape.Disc;

        var useDecal = drawShape == DrawShape.Mouse || drawShape == DrawShape.Projector;
        FluidAmount.transform.parent.gameObject.SetActive(ModeSelect.value == 0);
        FadeAmount.transform.parent.gameObject.SetActive(!useDecal);
        ColorSelect.transform.parent.gameObject.SetActive(!(useDecal && ModeSelect.value == 2));
        TextureSelect.transform.parent.gameObject.SetActive(useDecal && ModeSelect.value != 2);

        {
            var modeOpts = new List<string>();
            modeOpts.Add(DrawMode.Fluid.ToString());
            modeOpts.Add(DrawMode.Color.ToString());
            if (useDecal) {
                modeOpts.Add(DrawMode.Normal.ToString());
            }
            ModeSelect.SetOptions(modeOpts);
            var index = modeOpts.IndexOf(drawMode.ToString());
            drawMode = index >= 0 ? (DrawMode)index : 0;
            ModeSelect.SetValueWithoutNotify(index >= 0 ? index : 0);
            System.Enum.TryParse(ModeSelect.options[ModeSelect.value].text, out drawMode);
        }
    }

    public void SetSize(float size)
    {
        Projector.ProjectionSize = Size.value;

        Sphere.Radius = Size.value;

        Capsule.Radius = Size.value * .4f;
        Capsule.Height = Size.value * 3;

        Disc.Thickness = Size.value * .04f;
        Disc.Radius = Size.value * 2;
    }

    public void SelectModel(int index)
    {
        for (int i = 0; i < Models.Length; i++)
            Models[i].gameObject.SetActive(i == index);
    }

    public void SetAnimation(bool enabled)
    {
        for (int i = 0; i < Animators.Length; i++)
            Animators[i].enabled = enabled;
    }

    public void ResetTextures()
    {
        Canvas.InitializeTextureChannels();
    }

    public void SetEvaportation(bool enabled)
    {
        Simulator.UseEvaporation = enabled;
    }

    public void FadeOutFluid()
    {
        Simulator.FadeOut(15, 1);
    }

    public void SaveFluidTex()
    {
        Canvas.SaveTextureChannel("_FluidTex", Application.dataPath + "/export.png");
    }

    private Matrix4x4 getProjection()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var size = Size.value;
        return Matrix4x4.Ortho(-size * .5f, size * .5f, -size * .5f, size * .5f, .01f, 10) *
            Matrix4x4.TRS(ray.origin, Quaternion.LookRotation(-ray.direction), Vector3.one).inverse;
    }

    private bool OnUI()
    {
        var pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);
        return results.Count > 0;
    }
}

public static class DemoUtil
{
    public static void SetOptions(this Dropdown dropdown, List<string> newOptions)
    {
        dropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();
        foreach (var opt in newOptions)
            options.Add(new Dropdown.OptionData(opt.ToString()));
        dropdown.AddOptions(options);
    }
}