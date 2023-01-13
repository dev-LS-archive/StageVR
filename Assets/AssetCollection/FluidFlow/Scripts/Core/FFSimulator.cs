using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    /// <summary>
    /// Adds fluid simulation to a specified TextureChannel of a FluidCanvas.
    /// </summary>
    public class FFSimulator : MonoBehaviour
    {
        #region Public Properties

        // General

        [Tooltip("Target Fluid Canvas.")]
        public FFCanvas Canvas;

        [Tooltip("Name of the TextureChannel being simualted.")]
        public string TargetTextureChannel;

        [Tooltip("Keep updating fluid simulation, when no RenderTarget is visible?")]
        public bool UpdateInvisible = false;

        [Tooltip("Halt simulation after inactivity?")]
        public bool UseTimeout = true;

        [Min(0)]
        [Tooltip("Inactivity time (seconds) after which the simulation is halted.")]
        public float Timeout = 5;

        public InitializationState State { get; private set; }

        // Gravity

        [Tooltip("Update the gravity as soon as the simulator is initialized completely.")]
        public bool UpdateOnInitialized = true;

        [Tooltip("Set when or how often the gravity map is updated.")]
        public Updater GravityUpdater = new Updater(Updater.Mode.CUSTOM, .1f);

        [Tooltip("Is the gravity map influenced by the RenderTargets' normal map?")]
        public bool UseNormalMaps = true;

        [Tooltip("Name of the normal map texture property of the RenderTargets' materials.")]
        public string NormalPropertyName = "_NormalTex";

        [Tooltip("Fallback normal texture, when no material property with the given name is found.")]
        public Texture2D NormalTextureFallback;

        [Tooltip("Influence of the normal map on the generation of the gravity map.")]
        public float NormalInfluence = 1;

        // Fluid

        [Tooltip("Set when or how often the fluid simulation is updated.")]
        public Updater FluidUpdater = new Updater(Updater.Mode.FIXED, .025f);

        [Min(0)]
        [Tooltip("Amount of fluid retained in each pixel of the fluid texture depending on the angle of the surface.")]
        public float FluidRetainedFlat = 1.1f;

        [Min(0)]
        [Tooltip("Amount of fluid retained in each pixel of the fluid texture depending on the angle of the surface.")]
        public float FluidRetainedSteep = 0.9f;

        // Evaportation

        [Tooltip("Enable evaporation of fluid over time?")]
        public bool UseEvaporation = true;

        [Tooltip("Set when or how often the amount of fluid is reduced.")]
        public Updater EvaporationUpdater = new Updater(Updater.Mode.FIXED, .05f);

        [Min(float.Epsilon)]
        [Tooltip("Amount of fluid evaporating in each evaporation update.")]
        public float EvaporationAmount = .01f;

        [Min(0)]
        [Tooltip("Additional timeout after the simulator became inactive, until the evaportation updates stop.")]
        public float EvaporationTimeout = 10;

        #endregion Public Properties

        #region Private Variables

        private Vector3 worldGravity;
        private int fluidTextureId;
        private RenderTexture flowTexture;
        private float remainingSimulationTime = 0;
        private float remainingEvaporationTime = 0;

        #endregion Private Variables

        #region Public Methods

        /// <summary>
        /// Create internal flow rendertexture.
        /// </summary>
        public void Initialize()
        {
            if (State == InitializationState.UNINITIALIZED) {
                State = InitializationState.INITIALIZING;
                Canvas.Initialize();
                worldGravity = Physics.gravity.normalized;
                fluidTextureId = Shader.PropertyToID(TargetTextureChannel);
                flowTexture = InternalTextures.CreateRenderTexture(InternalTextures.FlowFormat, Canvas.Resolution);
                flowTexture.filterMode = FilterMode.Point;

                IEnumerator initializeDelayed()
                {
                    var stitchMapDrawer = new StitchMapDrawer(flowTexture, Canvas.InitializeAsync);
                    foreach (var rt in Canvas.RenderTargets)
                        stitchMapDrawer.AddStitches(rt);
                    yield return stitchMapDrawer.Draw();

                    yield return new WaitUntil(() => Canvas.State == InitializationState.INITIALIZED);

                    if (UpdateOnInitialized)
                        UpdateGravity();
                    State = InitializationState.INITIALIZED;
                }
                StartCoroutine(initializeDelayed());
            }
        }

        /// <summary>
        /// Release internal flow rendertexture.
        /// </summary>
        public void Uninitialize()
        {
            if (State == InitializationState.INITIALIZED) {
                flowTexture.Release();
                State = InitializationState.UNINITIALIZED;
            }
        }

        /// <summary>
        /// Manually reset the evaporation and simulation timeout timers.
        /// </summary>
        public void ResetTimeout()
        {
            remainingSimulationTime = Timeout;
            remainingEvaporationTime = EvaporationTimeout;
        }

        /// <summary>
        /// Set a new world space gravity direction, and update the internal flow map.
        /// </summary>
        public void UpdateGravity(Vector3 gravity)
        {
            worldGravity = gravity.normalized;
            UpdateGravity();
        }

        /// <summary>
        /// Update the internal flow map.
        /// </summary>
        public void UpdateGravity()
        {
            if (UseNormalMaps)
                Gravity.GenerateGravityMap(Canvas.RenderTargets, flowTexture, worldGravity, Shader.PropertyToID(NormalPropertyName), NormalTextureFallback, NormalInfluence);
            else
                Gravity.GenerateGravityMap(Canvas.RenderTargets, flowTexture, worldGravity);
        }

        /// <summary>
        /// Manually simulate the target fluid texture one step.
        /// </summary>
        public void UpdateFluid()
        {
            using (var paintScope = Canvas.BeginPaintScope(fluidTextureId, false)) {
                if (paintScope.IsValid) {
                    Fluid.Simulate(paintScope.Target, flowTexture, new Vector2(FluidRetainedSteep, FluidRetainedFlat));
                }
            }
        }

        /// <summary>
        /// Manually evaporate the set amount of fluid from the target fluid texture.
        /// </summary>
        public void UpdateEvaporation()
        {
            using (var paintScope = Canvas.BeginPaintScope(fluidTextureId, false)) {
                if (paintScope.IsValid) {
                    Fluid.Fade(paintScope.Target, EvaporationAmount);
                }
            }
        }

        #endregion Public Methods

        #region Private

        private void Awake()
        {
            GravityUpdater.AddListener(UpdateGravity);
            FluidUpdater.AddListener(UpdateFluid);
            EvaporationUpdater.AddListener(UpdateEvaporation);
            Canvas.OnTextureChannelUpdated.AddListener((TextureChannelIdentifier textureId) => {
                if (textureId == fluidTextureId)
                    ResetTimeout();
            });
            if (Canvas.InitializeOnAwake)
                Initialize();
        }

        private void OnDestroy()
        {
            Uninitialize();
        }

        private void LateUpdate()
        {
            if (State == InitializationState.INITIALIZED && (UpdateInvisible || Canvas.IsVisible())) {
                if (!UseTimeout || remainingSimulationTime > 0) {
                    GravityUpdater.Update();
                    FluidUpdater.Update();
                    remainingSimulationTime -= Time.deltaTime;
                }
                if (UseEvaporation) {
                    if (!UseTimeout || remainingEvaporationTime > 0) {
                        EvaporationUpdater.Update();
                        if (remainingSimulationTime <= 0)
                            remainingEvaporationTime -= Time.deltaTime;
                    }
                }
            }
        }

        #endregion Private
    }
}