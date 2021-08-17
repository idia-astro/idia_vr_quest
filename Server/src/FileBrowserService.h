#ifndef SERVER_FILEBROWSERIMPL_H
#define SERVER_FILEBROWSERIMPL_H

#include <filesystem>
#include <string>
#include <vector>

#include "DataApi.grpc.pb.h"

namespace fs = std::filesystem;

class FileBrowserService final : public DataApi::FileBrowser::Service {
private:
    fs::path _base_path;

public:
    FileBrowserService(fs::path path);
    grpc::Status GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) override;
    grpc::Status GetImageInfo(grpc::ServerContext* context, const DataApi::FileRequest* req, DataApi::ImageInfo* res) override;
};

#endif // SERVER_FILEBROWSERIMPL_H
