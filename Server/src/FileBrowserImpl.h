#ifndef SERVER_FILEBROWSERIMPL_H
#define SERVER_FILEBROWSERIMPL_H

#include <filesystem>
#include <string>

#include "../DataApi/DataApi.grpc.pb.h."

namespace fs = std::filesystem;

class FileBrowserImpl final : public DataApi::FileBrowser::Service {
private:
    fs::path base_path;

public:
    FileBrowserImpl(fs::path path);
    grpc::Status GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) override;
};

#endif //SERVER_FILEBROWSERIMPL_H
