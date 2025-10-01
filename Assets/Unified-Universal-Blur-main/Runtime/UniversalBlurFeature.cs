using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unified.UniversalBlur.Runtime
{
    public class UniversalBlurFeature : ScriptableRendererFeature
    {
        [Header("Blur Settings")]
        [Range(1, 12)][SerializeField] private int iterations = 4;
        [Range(1f, 10f)][SerializeField] private float downsample = 2.0f;

        [Tooltip("Enable mipmaps for more efficient blur")]
        [SerializeField] private bool enableMipMaps = true;
        [SerializeField] private float scale = 1f;
        [SerializeField] private float offset = 1f;

        [Space]

        [Header("Advanced Settings")]
        [SerializeField] private ScaleBlurWith scaleBlurWith = ScaleBlurWith.ScreenHeight;
        [SerializeField] private float scaleReferenceSize = 1080f;

        [Space]

        [SerializeField] private BlurType blurType;

        [Tooltip("For Overlay Canvas: AfterRenderingPostProcessing" +
                 "\n\nOther: BeforeRenderingTransparents (will hide transparents)")]
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

        [Header("Blur Control")]
        [SerializeField] private bool enableBlur = true;

        private float _intensity = 1.0f;

        [SerializeField]
        [HideInInspector]
        [Reload("Shaders/Blur.shader")]
        private Shader shader;

        private Material _material;
        private UniversalBlurPass _blurPass;
        private float _renderScale;

        // ”прощенна€ защита - только один флаг
        private bool _isRendering = false;

        public float Intensity
        {
            get => _intensity;
            set => _intensity = Mathf.Clamp(value, 0f, 1f);
        }

        public bool EnableBlur
        {
            get => enableBlur;
            set => enableBlur = value;
        }

        public override void Create()
        {
            _blurPass = new UniversalBlurPass();
            _blurPass.renderPassEvent = injectionPoint;
        }

        public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            base.OnCameraPreCull(renderer, in cameraData);
            _renderScale = cameraData.renderScale;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // ”прощенна€ защита - только от рекурсии в одном вызове
            if (_isRendering)
                return;

            _isRendering = true;

            try
            {
                // Early exits to prevent unnecessary processing
                if (!enableBlur || _intensity <= 0.001f)
                {
                    return;
                }

                if (!TrySetShadersAndMaterials())
                {
                    Debug.LogErrorFormat("{0}.AddRenderPasses(): Missing material. {1} render pass will not be added.", GetType().Name, name);
                    return;
                }

                // Skip for specific camera types
                if (renderingData.cameraData.isPreviewCamera ||
                    renderingData.cameraData.isSceneViewCamera ||
                    ShouldSkipForCamera(renderingData.cameraData))
                {
                    return;
                }

                var passData = GetBlurConfig(renderingData);
                _blurPass.Setup(passData);

                renderer.EnqueuePass(_blurPass);
            }
            finally
            {
                _isRendering = false;
            }
        }

        private bool ShouldSkipForCamera(in CameraData cameraData)
        {
            return (cameraData.camera.cameraType == CameraType.Reflection) ||
                   (cameraData.camera.cameraType == CameraType.Preview);
        }

        protected override void Dispose(bool disposing)
        {
            _blurPass?.Dispose();
            CoreUtils.Destroy(_material);
        }

        private bool TrySetShadersAndMaterials()
        {
            if (shader == null)
                shader = Shader.Find("Unify/Internal/Blur");

            if (_material == null && shader != null)
                _material = CoreUtils.CreateEngineMaterial(shader);

            return _material != null;
        }

        private BlurConfig GetBlurConfig(in RenderingData renderingData)
        {
            var (width, height) = GetTargetResolution(renderingData);

            return new BlurConfig
            {
                Scale = CalculateScale(),

                Width = width,
                Height = height,

                Material = _material,
                Intensity = _intensity,
                Downsample = downsample,
                Offset = offset,
                BlurType = blurType,
                Iterations = iterations,

                EnableMipMaps = enableMipMaps
            };
        }

        private (int width, int height) GetTargetResolution(in RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            var width = Mathf.RoundToInt(descriptor.width / downsample);
            var height = Mathf.RoundToInt(descriptor.height / downsample);

            return (width, height);
        }

        private float CalculateScale() => scaleBlurWith switch
        {
            ScaleBlurWith.ScreenHeight => scale * (Screen.height / scaleReferenceSize) * _renderScale,
            ScaleBlurWith.ScreenWidth => scale * (Screen.width / scaleReferenceSize) * _renderScale,
            _ => scale
        };

        // Public methods to control blur
        public void EnableBlurEffect() => enableBlur = true;
        public void DisableBlurEffect() => enableBlur = false;
        public void SetBlurIntensity(float intensity) => Intensity = intensity;
    }
}