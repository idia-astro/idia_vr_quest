#ifndef SERVER_FILEBROWSERIMPL_H
#define SERVER_FILEBROWSERIMPL_H

#include <filesystem>
#include <shared_mutex>
#include <string>
#include <vector>

#include <DataApi.grpc.pb.h>
#include "Image.h"

namespace fs = std::filesystem;

class FileBrowserService final : public DataApi::FileBrowser::Service {
private:
    fs::path _base_path;
    std::unordered_map<int, std::unique_ptr<Image>> _image_map;
    mutable std::shared_mutex _image_map_mutex;
    int _file_id_counter;

public:
    FileBrowserService(fs::path path);
    grpc::Status GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) override;
    grpc::Status GetImageInfo(grpc::ServerContext* context, const DataApi::FileRequest* req, DataApi::ImageInfo* res) override;
    grpc::Status OpenImage(grpc::ServerContext* context, const DataApi::FileRequest* req, DataApi::OpenImageResponse* res) override;
    grpc::Status CloseImage(grpc::ServerContext* context, const DataApi::CloseFileRequest* req, ::google::protobuf::Empty* res) override;
};

#endif // SERVER_FILEBROWSERIMPL_H
