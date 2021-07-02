namespace Models
{
    public enum FileType : uint
    {
        Unknown = 0,
        Fits = 1,
        Hdf5 = 2,
        Mhd = 3
    }

    public struct ExtendedFileInfoRequest
    {
        public string directoryName;
        public string fileName;
    }
    public struct ExtendedFileInfoResponse
    {
        public string fileName;
        public int[] dimensions;
        public FileType fileType;
    }
}