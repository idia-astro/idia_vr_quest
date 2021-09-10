using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VolumeData
{
    public enum DataState
    {
        Empty,
        Partial,
        Complete
    }

    public abstract class VolumeDataSource : IDisposable
    {
        /// <summary>
        /// Flag indicating whether the data source is valid and ready for requests.
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        ///  Texture3D holding the current region data in 8-bit unsigned byte format (R8),
        /// after it has been scaled from the original floating-point range to [0, 255].
        /// The maximum data size is determined by <c>ScaledDataLimit</c>.
        /// </summary>
        public Texture3D ScaledDataTexture { get; protected set; }

        /// <summary>
        /// Current dimensions of <c>ScaledDataTexture</c>.
        /// </summary>
        public Vector3Int ScaledDataDims => new Vector3Int(ScaledDataTexture?.width ?? 0, ScaledDataTexture?.height ?? 0, ScaledDataTexture?.depth ?? 0);

        /// <summary>
        /// Downsampling factors used to fill <c>ScaledDataBuffer</c>
        /// </summary>
        public Vector3Int ScaledDataDownsamplingFactors { get; protected set; }

        /// <summary>
        /// Maximum size (in voxels) of <c>ScaledDataBuffer</c>. Changes to this value may only take effect
        /// after the next data crop is made.
        /// </summary>
        public int ScaledDataLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Texture3D FloatDataTexture { get; protected set; }

        /// <summary>
        /// Compute buffer holding the current region data in 32-bit floating point format (FP32).
        /// after it has been scaled from the original floating-point range to [0, 255]
        /// The maximum data size is determined by <c>FloatDataLimit</c>.
        /// </summary>
        public Vector3Int FloatDataDims => new Vector3Int(FloatDataTexture?.width ?? 0, FloatDataTexture?.height ?? 0, FloatDataTexture?.depth ?? 0);

        /// <summary>
        /// Downsampling factors used to fill <c>FloatDataTexture</c>. This may differ from
        /// <c>ScaledDataDownsamplingFactors</c> due to memory bandwidth limitations.
        /// </summary>
        public Vector3Int FloatDataDownsamplingFactors { get; protected set; }

        /// <summary>
        /// Maximum size (in voxels) of <c>FloatDataTexture</c>. This may differ from
        /// <c>ScaledDataLimit</c> due to memory bandwidth limitations. Changes to this value may only
        /// take effect after the next data crop is made.
        /// </summary>
        public int FloatDataLimit { get; protected set; }

        /// <summary>
        /// Minimum and maximum bounds of the data contained in <c>FloatDataTexture</c>.
        /// These bounds are used to scale the floating-point data when filling <c>ScaledDataTexture</c>.
        /// </summary>
        public Vector2 FloatDataBounds { get; protected set; }

        /// <summary>
        /// Enum indicating whether data is empty, partially filled or complete
        /// </summary>
        public DataState DataState { get; protected set; }

        /// <summary>
        /// Progress of the current data request, in the range [0, 1]
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// Dimensions of the full data source.
        /// </summary>
        public Vector3Int DataSourceDims { get; protected set; }

        /// <summary>
        ///  Left-bottom-front corner of the current crop region.
        /// </summary>
        public Vector3Int CropMin { get; protected set; }

        /// <summary>
        ///  Right-top-back corner of the current crop region.
        /// </summary>
        public Vector3Int CropMax { get; protected set; }

        /// <summary>
        /// Requests a cropped selection of the data asynchronously. <c>ScaledDataTexture</c> and
        /// <c>FloatDataTexture</c> will be updated when data is available.
        /// </summary>
        /// <param name="cropMin">Left-bottom-front corner of the requested crop region.</param>
        /// <param name="cropMax">Right-top-back corner of the requested crop region.</param>
        /// <returns>Task with value indicating whether the request was successful or not.</returns>
        public abstract Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax);

        public abstract void Update();

        public abstract void Dispose();

        public static void FindDownsamplingFactors(long maxCubeSizeInMb, long regionXDim, long regionYDim, long regionZDim, out int xyFactor, out int zFactor)
        {
            var maxRegionSize = 2048;
            xyFactor = 1;
            zFactor = 1;

            // TODO: this doesn't need to be a loop!
            while (regionXDim / xyFactor > maxRegionSize || regionYDim / xyFactor > maxRegionSize)
            {
                xyFactor++;
            }

            while (regionZDim / zFactor > maxRegionSize)
            {
                zFactor++;
            }

            long maximumElements = maxCubeSizeInMb * 1000000 / 4;
            while (regionXDim * regionYDim * regionZDim / (xyFactor * xyFactor * zFactor) > maximumElements)
            {
                var scaledSizeX = regionXDim / xyFactor;
                var scaledSizeY = regionYDim / xyFactor;
                var scaledSizeZ = regionZDim / zFactor;
                // Attempt to even out dimensions
                if (scaledSizeZ > scaledSizeX || scaledSizeZ > scaledSizeY)
                {
                    zFactor++;
                }
                else
                {
                    xyFactor++;
                }
            }
        }
    }
}