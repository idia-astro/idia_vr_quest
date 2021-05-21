using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VolumeData
{
    public class MhdDataSource : IVolumeDataSource
    {
        public bool IsValid { get; private set; }
        public ComputeBuffer ScaledDataBuffer { get; }
        public Vector3Int ScaledDataDims { get; }
        public Vector3Int ScaledDataDownsamplingFactors { get; }
        public int ScaledDataLimit { get; set; }
        public ComputeBuffer FloatDataBuffer { get; }
        public Vector3Int FloatDataDims { get; }
        public Vector3Int FloatDataDownsamplingFactors { get; }
        public int FloatDataLimit { get; set; }
        public Vector2 FloatDataBounds { get; }
        public DataState DataState { get; private set; }
        public float Progress { get; private set; }
        public Vector3Int DataSourceDims { get; }
        public Vector3Int CropMin { get; }
        public Vector3Int CropMax { get; }

        public MhdDataSource(string path)
        {
            IsValid = false;
            Progress = 0;
            DataState = DataState.Empty;
        }

        public async Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax)
        {
            await Task.Delay(10);
            return false;
        }
    }
}