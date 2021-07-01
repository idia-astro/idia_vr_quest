using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VolumeData
{
    public class RemoteDataSource : IVolumeDataSource
    {
        public bool IsValid { get; private set; }
        public Texture3D ScaledDataTexture { get; }
        public Vector3Int ScaledDataDownsamplingFactors { get; }
        public int ScaledDataLimit { get; set; }
        public Texture3D FloatDataTexture { get; }
        
        public Vector3Int FloatDataDownsamplingFactors { get; }
        public int FloatDataLimit { get; set; }
        public Vector2 FloatDataBounds { get; }
        public DataState DataState { get; private set; }
        public float Progress { get; private set; }
        public Vector3Int DataSourceDims { get; }
        public Vector3Int CropMin { get; }
        public Vector3Int CropMax { get; }
        
        public Vector3Int ScaledDataDims => new Vector3Int(ScaledDataTexture?.width ?? 0, ScaledDataTexture?.height ?? 0, ScaledDataTexture?.depth ?? 0);
        public Vector3Int FloatDataDims => new Vector3Int(FloatDataTexture?.width ?? 0, FloatDataTexture?.height ?? 0, FloatDataTexture?.depth ?? 0);
        
        public RemoteDataSource(string path)
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