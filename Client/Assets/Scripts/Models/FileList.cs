namespace Models
{
    public struct FileInfo
    {
        public string name;
        public long size;
        public long date;
    }

    public struct DirectoryInfo
    {
        public string name;
        public long numItems;
        public long date;
    }

    public struct FileListRequest
    {
        public string directoryName;
    }
    
    public struct FileListResponse
    {
        public string directoryName;
        public DirectoryInfo[] subDirectories;
        public FileInfo[] files;
    }
}