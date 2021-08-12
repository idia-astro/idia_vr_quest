using System.Threading.Tasks;
using DataApi;
using Grpc.Core;
using JetBrains.Annotations;

namespace Services
{
   public class BackendService
    {
        private static readonly string Address = "localhost";
        private static readonly int Port = 50051;

        private readonly Channel _channel;
        private readonly FileBrowser.FileBrowserClient _fileBrowserClient;
        private static BackendService _instance;

        private BackendService()
        {
            _channel = new Channel(Address, Port, ChannelCredentials.Insecure);
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

        public async Task<ImageInfo> GetImageInfo(string directory, string filename, string hdu = "")
        {
            return await _fileBrowserClient.GetImageInfoAsync(new ImageInfoRequest { DirectoryName = directory, FileName = filename, Hdu = hdu });
        }
    }
}