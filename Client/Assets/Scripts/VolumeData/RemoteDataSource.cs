using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Services;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Util;
using Debug = UnityEngine.Debug;

namespace VolumeData
{
    public class RemoteDataSource : VolumeDataSource
    {
        private int _fileId;
        private bool _requiresUpload;
        private int _channel;

        private Texture2D _floatSliceTexture;
        private Texture2D _scaledSliceTexture;

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
                if (imageInfo?.Dimensions == null)
                {
                    Debug.LogError($"Error loading file information for {name}");
                    return false;
                }

                if (imageInfo.Dimensions?.Count < 3)
                {
                    Debug.LogError($"Image file {name} has fewer than 3 dimensions");
                    return false;
                }

                _fileId = await backendService.OpenFile(folder, name);

                var imageDims = new Vector3Int(imageInfo.Dimensions[0], imageInfo.Dimensions[1], imageInfo.Dimensions[2]);
                FindDownsamplingFactors(Config.Instance.maxCubeSizeMb, imageDims.x, imageDims.y, imageDims.z, out int xyFactor, out int zFactor);
                var downsampledDims = Vector3Int.CeilToInt(new Vector3((float)imageDims.x / xyFactor, (float)imageDims.y / xyFactor, (float)imageDims.z / zFactor));
                Debug.Log(
                    $"Downsampling cube from {imageDims.x}x{imageDims.y}x{imageDims.z} to {downsampledDims.x}x{downsampledDims.y}x{downsampledDims.z} (xy: {xyFactor}, z: {zFactor})");
                var imageSizeMb = 1.0e-6 * imageDims.x * imageDims.y * imageDims.z * sizeof(float);
                var downsampledSizeMb = 1.0e-6 * downsampledDims.x * downsampledDims.y * downsampledDims.z * sizeof(float);
                Debug.Log($"Cube size decreased from {imageSizeMb:F1} MB to {downsampledSizeMb:F1} MB (limit={Config.Instance.maxCubeSizeMb} MB)");
                InitTextures(new Vector3Int(imageInfo.Dimensions[0], imageInfo.Dimensions[1], imageInfo.Dimensions[2]));
                await StreamData(_fileId);
            }
            catch (RpcException ex)
            {
                Debug.LogError(ex.StatusCode + ex.Message);
                return false;
            }

            return true;
        }


        private async Task StreamData(int fileId)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var config = Config.Instance;
                int totalSize = 0;
                int slice = 0;
                int pixelsPerSlice = DataSourceDims.x * DataSourceDims.y;

                var scaledPixels = new byte[pixelsPerSlice];
                var backendService = BackendService.Instance;

                int numPoints = pixelsPerSlice * config.slicesPerMessage;

                using var destArray = new NativeArray<float>(numPoints, Allocator.Persistent);
                using var call = backendService.GetData(fileId, config.compressionPrecision, config.slicesPerMessage);

                while (await call.ResponseStream.MoveNext())
                {
                    var dataResponse = call.ResponseStream.Current;
                    FloatDataBounds = new Vector2(dataResponse.MinValue, dataResponse.MaxValue);

                    var dataSize = dataResponse.RawData.Length;
                    totalSize += dataSize;
                    var decompressedData = DecompressData(dataResponse.RawData.ToByteArray(), destArray, DataSourceDims.x, DataSourceDims.y, dataResponse.NumChannels,
                        config.compressionPrecision);
                    var numProcessedSlices = UpdateTextures(decompressedData, dataResponse.NumChannels, scaledPixels, slice);
                    slice += numProcessedSlices;
                }


                sw.Stop();

                var timeMs = sw.ElapsedMilliseconds;
                var rate = (totalSize * 1e-3) / timeMs;
                Debug.Log($"Received {(totalSize / 1.0e6):F1} MB of data for fileId={_fileId} in {timeMs:F1} ms ({rate:F1} MB/s)");
            }
            catch (RpcException ex)
            {
                Debug.LogError(ex.StatusCode + ex.Message);
            }
        }

        private static unsafe NativeArray<float> DecompressData(byte[] srcArray, NativeArray<float> destArray, int width, int height, int depth, int precision)
        {
            float* destPtr = (float*)destArray.GetUnsafePtr();
            fixed (byte* srcPtr = srcArray)
            {
                int compressedSize = srcArray.Length;

                int errorCode = NativeFunctions.DecompressFloat3D(srcPtr, compressedSize, destPtr, width, height, depth, precision);
                if (errorCode != 0)
                {
                    Debug.LogError("Error decompressing ZFP stream");
                }
            }

            return destArray;
        }

        private unsafe int UpdateTextures(NativeArray<float> sourcePixels, int numSlices, byte[] scaledPixels, int startingSlice)
        {
            int pixelsPerSlice = DataSourceDims.x * DataSourceDims.y;
            float* arrayPtr = (float*)sourcePixels.GetUnsafePtr();

            fixed (byte* destPtr = scaledPixels)
            {
                for (int i = 0; i < numSlices; i++)
                {
                    var minValue = FloatDataBounds.x;
                    var maxValue = FloatDataBounds.y;

                    float* srcPtr = arrayPtr + pixelsPerSlice * i;
                    NativeFunctions.ScaleArray(srcPtr, destPtr, pixelsPerSlice, minValue, maxValue, 0);
                    _scaledSliceTexture.SetPixelData(scaledPixels, 0);
                    _floatSliceTexture.SetPixelData(sourcePixels, 0, i * pixelsPerSlice);

                    _scaledSliceTexture.Apply();
                    _floatSliceTexture.Apply();
                    Graphics.CopyTexture(_floatSliceTexture, 0, 0, 0, 0, DataSourceDims.x, DataSourceDims.y, FloatDataTexture, startingSlice + i, 0, 0, 0);
                    Graphics.CopyTexture(_scaledSliceTexture, 0, 0, 0, 0, DataSourceDims.x, DataSourceDims.y, ScaledDataTexture, startingSlice + i, 0, 0, 0);
                }
            }

            return numSlices;
        }

        private void InitTextures(Vector3Int newDims)
        {
            DataSourceDims = newDims;
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

            _floatSliceTexture = new Texture2D(DataSourceDims.x, DataSourceDims.y, TextureFormat.RFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _scaledSliceTexture = new Texture2D(DataSourceDims.x, DataSourceDims.y, TextureFormat.R8, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
        }

        public override void Update()
        {
        }

        public override async Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax)
        {
            await Task.Delay(10);
            return false;
        }

        public override void Dispose()
        {
            if (_fileId >= 0)
            {
                Debug.Log($"Closing image with fileId={_fileId}");
                _ = BackendService.Instance.CloseFile(_fileId);
            }
        }
    }
}