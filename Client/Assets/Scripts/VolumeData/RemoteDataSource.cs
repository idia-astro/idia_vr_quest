using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private float[] _floatArray;
        private bool _requiresUpload;

        public RemoteDataSource(string folder, string name)
        {
            IsValid = false;
            Progress = 0;
            DataState = DataState.Empty;
            _fileId = -1;

            OpenFile(folder, name);
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

                var fileId = await backendService.OpenFile(folder, name);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var dataResponse = await backendService.GetData(fileId);
                _floatArray = new float[dataResponse.RawData.Length / 4];
                // TODO: this could probably be optimised, because ToByteArray creates another copy
                var byteArray = dataResponse.RawData.ToByteArray();
                var dataSize = byteArray.Length;
                Buffer.BlockCopy(byteArray, 0, _floatArray, 0, dataSize);
                sw.Stop();
                var timeMs = sw.ElapsedMilliseconds;
                var rate = (dataSize * 1e-3) / timeMs;
                Debug.Log($"Received {(dataSize / 1.0e6):F1} MB of data for fileId={fileId} in {timeMs:F1} ms ({rate:F1} MB/s)");
                await backendService.CloseFile(fileId);

                float minVal = Single.MaxValue;
                float maxVal = -Single.MaxValue;

                for (var i = 0; i < dataSize / 4; i++)
                {
                    var val = _floatArray[i];
                    if (!Single.IsNaN(val))
                    {
                        minVal = Mathf.Min(minVal, val);
                        maxVal = Mathf.Max(maxVal, val);
                    }
                }

                Debug.Log(minVal + " " + maxVal);
                FloatDataBounds = new Vector2(minVal, maxVal);
                
                FloatDataTexture = new Texture3D(DataSourceDims.x, DataSourceDims.y, DataSourceDims.z, TextureFormat.RFloat, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Point
                };
                FloatDataTexture.SetPixelData(_floatArray, 0);
                FloatDataTexture.Apply();
            }
            catch (RpcException ex)
            {
                Debug.LogError(ex.StatusCode + ex.Message);
                return false;
            }

            return true;
        }

        public void Update()
        {
            
        }

        public async Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax)
        {
            await Task.Delay(10);
            return false;
        }
    }
}