#ifndef SERVER_FILEBROWSERIMPL_H
#define SERVER_FILEBROWSERIMPL_H

#include <filesystem>
#include <string>
#include <vector>

#include "../DataApi/DataApi.grpc.pb.h."

namespace fs = std::filesystem;

class FileBrowserImpl final : public DataApi::FileBrowser::Service {
private:
    fs::path _base_path;

public:
    FileBrowserImpl(fs::path path);
    grpc::Status GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) override;
    grpc::Status GetImageInfo(grpc::ServerContext* context, const DataApi::ImageInfoRequest* req, DataApi::ImageInfo* res) override;

private:
    static std::vector<long> GetImageDimensions(fs::path path);
};

#endif // SERVER_FILEBROWSERIMPL_H
