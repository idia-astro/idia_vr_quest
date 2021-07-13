#include "FileBrowserImpl.h"

#include <filesystem>
#include <exception>
#include <string>

namespace fs = std::filesystem;

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using grpc::StatusCode;
using DataApi::FileBrowser;
using DataApi::FileListRequest;
using DataApi::FileList;
using DataApi::ImageInfoRequest;
using DataApi::ImageInfo;


FileBrowserImpl::FileBrowserImpl(fs::path path) : base_path(path){

}

grpc::Status FileBrowserImpl::GetFileList(ServerContext *context, const FileListRequest *req, FileList *res) {
    fs::path directory_path = base_path / req->directoryname();

    if (fs::exists(directory_path)){
        res->set_directoryname(req->directoryname());
        for (auto& entry: fs::directory_iterator(directory_path)) {
            const auto& path = entry.path();
            if (entry.is_directory()) {
                auto dir = res->add_subdirectories();
                dir->set_name(path.filename().string());
            } else if (entry.is_regular_file()) {
                auto file = res->add_files();
                file->set_name(path.filename().string());
            }
        }

        return Status::OK;
    } else {
        return Status(StatusCode::INVALID_ARGUMENT, "Directory does not exist");
    }


}
