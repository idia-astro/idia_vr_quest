using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Services;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VolumeData
{
    public class RemoteDataSource : IVolumeDataSource
    {
        public bool IsValid { get; private set; }
        public Texture3D ScaledDataTexture { get; private set; }
        public Vector3Int ScaledDataDownsamplingFactors { get; }
        public int ScaledDataLimit { get; set; }
        public Texture3D FloatDataTexture { get; private set; }

        public Vector3Int FloatDataDownsamplingFactors { get; }
        public int FloatDataLimit { get; set; }
        public Vector2 FloatDataBounds { get; private set; }
        public DataState DataState { get; private set; }
        public float Progress { get; private set; }
        public Vector3Int DataSourceDims { get; private set; }
        public Vector3Int CropMin { get; }
        public Vector3Int CropMax { get; }

        public Vector3Int ScaledDataDims => new Vector3Int(ScaledDataTexture?.width ?? 0, ScaledDataTexture?.height ?? 0, ScaledDataTexture?.depth ?? 0);
        public Vector3Int FloatDataDims => new Vector3Int(FloatDataTexture?.width ?? 0, FloatDataTexture?.height ?? 0, FloatDataTexture?.depth ?? 0);

        private int _fileId;
        private bool _requiresUpload;
        private int _channel;

        public RemoteDataSource(string folder, string name)
        {
            IsValid = false;
            Progress = 0;
            DataState = DataState.Empty;
            _fileId = -1;
            _requiresUpload = false;
            _channel = 0;

            _ = OpenFile(folder, name);
        }

        private async Task<bool> OpenFile(string folder, string name)
        {
            try
            {
                var backendService = BackendService.Instance;
                var imageInfo = await backendService.GetImageInfo(folder, name);
                if (imageInfo == null || imageInfo.Dimensions == null)
                {
                    Debug.LogError($"Error loading file information for {name}");
                    return false;
                }

                if (imageInfo.Dimensions?.Count < 3)
                {
                    Debug.LogError($"Image file {name} has fewer than 3 dimensions");
                    return false;
                }

                DataSourceDims = new Vector3Int(imageInfo.Dimensions[0], imageInfo.Dimensions[1], imageInfo.Dimensions[2]);
                _fileId = await backendService.OpenFile(folder, name);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var dataResponse = await backendService.GetData(_fileId);
                var dataSize = dataResponse.RawData.Length;
                sw.Stop();
                
                var timeMs = sw.ElapsedMilliseconds;
                var rate = (dataSize * 1e-3) / timeMs;
                Debug.Log($"Received {(dataSize / 1.0e6):F1} MB of data for fileId={_fileId} in {timeMs:F1} ms ({rate:F1} MB/s)");
                FloatDataBounds = new Vector2(dataResponse.MinValue, dataResponse.MaxValue);
                
                sw.Reset();
                sw.Start();
                UpdateTextures(dataResponse.RawData);
                sw.Stop();
                
                timeMs = sw.ElapsedMilliseconds;
                Debug.Log($"Applied texture in {timeMs} ms");

            }
            catch (RpcException ex)
            {
                Debug.LogError(ex.StatusCode + ex.Message);
                return false;
            }

            return true;
        }

        private void UpdateTextures(ByteString rawData)
        {
            FloatDataTexture = new Texture3D(DataSourceDims.x, DataSourceDims.y, DataSourceDims.z, TextureFormat.RFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            
            ScaledDataTexture = new Texture3D(DataSourceDims.x, DataSourceDims.y, DataSourceDims.z, TextureFormat.R8, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };  
            
            var numPixels = rawData.Length / 4;
            var minValue = FloatDataBounds.x;
            var maxValue = FloatDataBounds.y;
            var range = maxValue - minValue;

            var floatArray = FloatDataTexture.GetPixelData<float>(0);
            var scaledArray = ScaledDataTexture.GetPixelData<byte>(0);

            // Update all FP32 data in one go
            FloatDataTexture.SetPixelData(rawData.ToByteArray(), 0);
            
            // Convert scaled array one-by-one
            for (int i = 0; i < numPixels; i++)
            {
                var val = floatArray[i];
                // handle numerical errors for min and max values
                if (val == minValue)
                {
                    val = 0;
                }
                else if (val == maxValue)
                {
                    val = 255;
                }
                else
                {
                    val = 255 * (val - minValue) / range;
                }

                scaledArray[i] = (byte) Mathf.RoundToInt(val);
            }

            ScaledDataTexture.Apply();
            FloatDataTexture.Apply();
        }

        public void Update()
        {
            
        }

        public async Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax)
        {
            await Task.Delay(10);
            return false;
        }

        public void Dispose()
        {
            if (_fileId >= 0)
            {
                Debug.Log($"Closing image with fileId={_fileId}");
                _ = BackendService.Instance.CloseFile(_fileId);
            }
        }
    }
}