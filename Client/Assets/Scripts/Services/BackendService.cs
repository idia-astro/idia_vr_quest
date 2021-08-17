using System.Threading.Tasks;
using DataApi;
using Grpc.Core;

namespace Services
{
   public class BackendService
    {
        private static readonly string Target = "a100gpu.idia.ac.za";

        private readonly Channel _channel;
        private readonly FileBrowser.FileBrowserClient _fileBrowserClient;
        private static BackendService _instance;

        private BackendService()
        {
            _channel = new Channel(Target, ChannelCredentials.Insecure);
            _fileBrowserClient = new FileBrowser.FileBrowserClient(_channel);
        }

        public static BackendService Instance
        {
            get
            {
                _instance ??= new BackendService();
                return _instance;
            }
        }
        
        public async void Close()
        {
            await _channel.ShutdownAsync();
        }        
        
        // Utility wrappers around the grpc calls
        public async Task<FileList> GetFileList(string directory)
        {
            return await _fileBrowserClient.GetFileListAsync(new FileListRequest{DirectoryName = directory});
        }

        public async Task<ImageInfo> GetImageInfo(string directory, string filename, string hduName = "")
        {
            return await _fileBrowserClient.GetImageInfoAsync(new FileRequest { DirectoryName = directory, FileName = filename, HduName = hduName });
        }
        
        public async Task<ImageInfo> GetImageInfo(string directory, string filename, int hduNum)
        {
            return await _fileBrowserClient.GetImageInfoAsync(new FileRequest { DirectoryName = directory, FileName = filename, HduNum = hduNum });
        }

        public async Task<int> OpenFile(string directory, string filename, string hduName = "")
        {
            var res = await _fileBrowserClient.OpenImageAsync(new FileRequest { DirectoryName = directory, FileName = filename, HduName = hduName });
            return res.FileId;
        }
        
        public async Task<int> OpenFile(string directory, string filename, int hduNum)
        {
            var res = await _fileBrowserClient.OpenImageAsync(new FileRequest { DirectoryName = directory, FileName = filename, HduNum = hduNum });
            return res.FileId;
        }

        public async Task CloseFile(int fileId)
        {
            await _fileBrowserClient.CloseImageAsync(new CloseFileRequest{FileId = fileId});
        }
    }
}