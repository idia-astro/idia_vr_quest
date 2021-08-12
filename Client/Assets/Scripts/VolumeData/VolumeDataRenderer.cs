using System;
using DataApi;
using Grpc.Core;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace VolumeData
{
    [RequireComponent(typeof(Renderer))]
    public class VolumeDataRenderer : MonoBehaviour
    {
        public Texture3D dataTexture;
        public Material rayMarchingMaterial;

        [Header("Rendering settings")] [Range(16, 512)]
        public int defaultStepCount = 128;

        [Range(0, 1)] public float jitter = 1.0f;
        public ColorMapEnum colormap = ColorMapEnum.Turbo;

        [Header("Data settings")] public float dataMin = 0;
        public float dataMax = 1;
        public float threshold = 0;

        [Header("Adaptive performance settings")]
        public bool useAdaptiveStepCount = true;

        [Range(16, 512)] public int minimumStepCount = 32;
        [Range(16, 512)] public int maximumStepCount = 512;
        public float rateOfChange = 0.05f;
        [Range(0, 1)] public float movingAverageAlpha = 0.1f;

        #region Material property IDs

        private struct MaterialID
        {
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int MaxSteps = Shader.PropertyToID("_MaxSteps");
            public static readonly int DataMin = Shader.PropertyToID("_DataMin");
            public static readonly int DataMax = Shader.PropertyToID("_DataMax");
            public static readonly int Threshold = Shader.PropertyToID("_Threshold");
            public static readonly int Jitter = Shader.PropertyToID("_Jitter");
            public static readonly int NumColorMaps = Shader.PropertyToID("_NumColorMaps");
            public static readonly int ColorMapIndex = Shader.PropertyToID("_ColorMapIndex");
        }

        #endregion

        private Material _materialInstance;
        private Renderer _renderer;
        private float _targetGpuUsage;
        private float _currentStepCount;
        private TMP_Text _debugTextOverlay;

        void Awake()
        {
            _materialInstance = Instantiate(rayMarchingMaterial);
            _materialInstance.SetTexture(MaterialID.MainTex, dataTexture);
            _materialInstance.SetInt(MaterialID.NumColorMaps, ColorMapUtils.NumColorMaps);

            // Limit step count based on data size
            var w = dataTexture.width;
            var h = dataTexture.height;
            var d = dataTexture.depth;
            float diag = Mathf.Sqrt(w * w + h * h + d * d);
            maximumStepCount = Math.Min(maximumStepCount, Mathf.RoundToInt(diag));
            _renderer = GetComponent<Renderer>();
        }

        async void Start()
        {
            var obj = GameObject.Find("DebugTextOverlay");
            _debugTextOverlay = obj?.GetComponent<TMP_Text>();

            _materialInstance.SetTexture(MaterialID.MainTex, dataTexture);
            _renderer.material = _materialInstance;

            _targetGpuUsage = 0.9f;
            Unity.XR.Oculus.Stats.PerfMetrics.EnablePerfMetrics(true);
            Camera.main.depthTextureMode = DepthTextureMode.Depth;

            var backendService = BackendService.Instance;
            var list = await backendService.GetFileList("fits");
            Debug.Log(list);
            foreach (var file in list.Files)
            {
                var imageInfo = await backendService.GetImageInfo(list.DirectoryName, file.Name);
                Debug.Log(imageInfo);
            }
        }

        void Update()
        {
            if (useAdaptiveStepCount)
            {
                UpdateStepSize();
            }

            UpdateMaterialParameters();
        }

        private void UpdateStepSize()
        {
            float gpuUsage = Unity.XR.Oculus.Stats.PerfMetrics.GPUUtilization;
            if (gpuUsage > 0.0f)
            {
                float prevStepCount = _currentStepCount;
                float deltaStepCount = rateOfChange * _currentStepCount * (_targetGpuUsage - gpuUsage) / _targetGpuUsage;
                // More aggressively decrease step count than increase
                if (deltaStepCount < 0)
                {
                    deltaStepCount *= 2.0f;
                }

                _currentStepCount += deltaStepCount;
                _currentStepCount = Mathf.Clamp(_currentStepCount, minimumStepCount, maximumStepCount);

                if (_debugTextOverlay)
                {
                    _debugTextOverlay.text = $"GPU={(gpuUsage * 100):0.0}%; stepCount: {Mathf.RoundToInt(prevStepCount)} -> {Mathf.RoundToInt(_currentStepCount)}";
                }
            }
            else
            {
                _currentStepCount = defaultStepCount;
            }
        }

        private void UpdateMaterialParameters()
        {
            _materialInstance.SetInt(MaterialID.MaxSteps, Mathf.RoundToInt(_currentStepCount));
            _materialInstance.SetFloat(MaterialID.DataMin, dataMin);
            _materialInstance.SetFloat(MaterialID.DataMax, dataMax);
            _materialInstance.SetFloat(MaterialID.Threshold, threshold);
            _materialInstance.SetFloat(MaterialID.Jitter, jitter);
            _materialInstance.SetInt(MaterialID.ColorMapIndex, colormap.GetHashCode());
        }
    }
}