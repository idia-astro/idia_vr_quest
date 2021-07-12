#ifndef SERVER_FILEBROWSERIMPL_H
#define SERVER_FILEBROWSERIMPL_H

#include "../DataApi/DataApi.grpc.pb.h."

class FileBrowserImpl final : public DataApi::FileBrowser::Service {
    grpc::Status GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) override;
};

#endif //SERVER_FILEBROWSERIMPL_H
