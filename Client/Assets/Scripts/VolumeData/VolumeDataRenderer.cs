using System;
using System.Diagnostics;
using DataApi;
using Grpc.Core;
using Services;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

            // try
            // {
            //     var list = await backendService.GetFileList("fits");
            //     Debug.Log(list);
            //     ImageInfo cubeInfo = null;
            //     foreach (var file in list.Files)
            //     {
            //         var imageInfo = await backendService.GetImageInfo(list.DirectoryName, file.Name);
            //         var unitEntry = imageInfo.Header.SingleOrDefault(e => e.Key == "BUNIT");
            //
            //         if (unitEntry != null)
            //         {
            //             Debug.Log($"Image units: {unitEntry.Value}");
            //         }
            //
            //         if (imageInfo.Dimensions.Count >= 3 && imageInfo.Dimensions[2] > 1)
            //         {
            //             Debug.Log($"{file.Name} is 3D ({imageInfo.Dimensions[2]} channels)");
            //             cubeInfo ??= imageInfo;
            //         }
            //     }
            // }
            // catch (RpcException ex)
            // {
            //     Debug.LogError(ex.StatusCode + ex.Message);
            // }

            try {
            var fileId = await backendService.OpenFile("fits/vr", "m81.fits");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var res = await backendService.GetData(fileId);
            var rawByteSize = res.RawData.Length;
            var floatByteSize = res.Data.Count * 4;
            var maxSize = Math.Max(rawByteSize, floatByteSize);
            sw.Stop();
            var timeMs = sw.ElapsedMilliseconds;
            var rate = (maxSize * 1e-3) / timeMs;
            Debug.Log($"Received {maxSize} bytes of data for fileId={fileId} in {timeMs:F1} ms ({rate:F1} MB/s)");
            await backendService.CloseFile(fileId);
            }
            catch (RpcException ex)
            {
                Debug.LogError(ex.StatusCode + ex.Message);
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