#include "FileBrowserImpl.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using DataApi::FileBrowser;
using DataApi::FileListRequest;
using DataApi::FileList;
using DataApi::ImageInfoRequest;
using DataApi::ImageInfo;

grpc::Status FileBrowserImpl::GetFileList(ServerContext *context, const FileListRequest *req, FileList *res) {
    res->set_directoryname(req->directoryname());
    return Status::OK;
}
