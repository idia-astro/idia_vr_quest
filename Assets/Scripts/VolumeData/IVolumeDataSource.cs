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

    public interface IVolumeDataSource
    {
        /// <summary>
        /// Flag indicating whether the data source is valid and ready for requests.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///  Compute buffer holding the current region data in 8-bit unsigned byte format (R8),
        /// after it has been scaled from the original floating-point range to [0, 255].
        /// The maximum data size is determined by <c>ScaledDataLimit</c>.
        /// </summary>
        public ComputeBuffer ScaledDataBuffer { get; }

        /// <summary>
        /// Current dimensions of <c>ScaledDataBuffer</c>.
        /// </summary>
        public Vector3Int ScaledDataDims { get; }

        /// <summary>
        /// Downsampling factors used to fill <c>ScaledDataBuffer</c>
        /// </summary>
        public Vector3Int ScaledDataDownsamplingFactors { get; }

        /// <summary>
        /// Maximum size (in voxels) of <c>ScaledDataBuffer</c>. Changes to this value may only take effect
        /// after the next data crop is made.
        /// </summary>
        public int ScaledDataLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ComputeBuffer FloatDataBuffer { get; }

        /// <summary>
        /// Compute buffer holding the current region data in 32-bit floating point format (FP32).
        /// after it has been scaled from the original floating-point range to [0, 255]
        /// The maximum data size is determined by <c>FloatDataLimit</c>.
        /// </summary>
        public Vector3Int FloatDataDims { get; }

        /// <summary>
        /// Downsampling factors used to fill <c>FloatDataBuffer</c>. This may differ from
        /// <c>ScaledDataDownsamplingFactors</c> due to memory bandwidth limitations.
        /// </summary>
        public Vector3Int FloatDataDownsamplingFactors { get; }

        /// <summary>
        /// Maximum size (in voxels) of <c>FloatDataBuffer</c>. This may differ from
        /// <c>ScaledDataLimit</c> due to memory bandwidth limitations. Changes to this value may only
        /// take effect after the next data crop is made.
        /// </summary>
        public int FloatDataLimit { get; set; }

        /// <summary>
        /// Minimum and maximum bounds of the data contained in <c>FloatDataBuffer</c>.
        /// These bounds are used to scale the floating-point data when filling <c>ScaledDataBuffer</c>.
        /// </summary>
        public Vector2 FloatDataBounds { get; }

        /// <summary>
        /// Enum indicating whether data is empty, partially filled or complete
        /// </summary>
        public DataState DataState { get; }

        /// <summary>
        /// Progress of the current data request, in the range [0, 1]
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Dimensions of the full data source.
        /// </summary>
        public Vector3Int DataSourceDims { get; }

        /// <summary>
        ///  Left-bottom-front corner of the current crop region.
        /// </summary>
        public Vector3Int CropMin { get; }

        /// <summary>
        ///  Right-top-back corner of the current crop region.
        /// </summary>
        public Vector3Int CropMax { get; }

        /// <summary>
        /// Requests a cropped selection of the data asynchronously. <c>ScaledDataBuffer</c> and
        /// <c>FloatDataBuffer</c> will be updated when data is available.
        /// </summary>
        /// <param name="cropMin">Left-bottom-front corner of the requested crop region.</param>
        /// <param name="cropMax">Right-top-back corner of the requested crop region.</param>
        /// <returns>Task with value indicating whether the request was successful or not.</returns>
        public Task<bool> RequestCrop(Vector3Int cropMin, Vector3Int cropMax);
    }
}