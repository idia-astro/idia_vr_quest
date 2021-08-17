#include "FileBrowserService.h"

#include <chrono>
#include <filesystem>

#include "FitsImage.h"
#include "Util.h"

#define FITS_MAGIC_NUM 0x2020454c504d4953
// TODO: check why this isn't working!
#define HDF5_MAGIC_NUM 0x0a1a0a0d46444889

namespace fs = std::filesystem;

FileBrowserService::FileBrowserService(fs::path path) : _base_path(path) {}

grpc::Status FileBrowserService::GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) {
    fs::path directory_path = _base_path / req->directoryname();

    if (!fs::exists(directory_path)) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "Directory does not exist");
    }

    res->set_directoryname(req->directoryname());
    for (const auto& entry : fs::directory_iterator(directory_path)) {
        const auto& path = entry.path();
        if (entry.is_directory()) {
            auto* dir = res->add_subdirectories();
            auto modified = std::filesystem::last_write_time(path);
            dir->set_date(std::chrono::duration_cast<std::chrono::milliseconds>(modified.time_since_epoch()).count());
            dir->set_name(path.filename().string());
            dir->set_numitems(GetFolderItemCount(path));
        } else if (entry.is_regular_file()) {
            auto* file = res->add_files();
            auto modified = std::filesystem::last_write_time(path);
            file->set_date(std::chrono::duration_cast<std::chrono::milliseconds>(modified.time_since_epoch()).count());
            file->set_name(path.filename().string());
            file->set_size(fs::file_size(path));
        }
    }
    return grpc::Status::OK;
}

grpc::Status FileBrowserService::GetImageInfo(grpc::ServerContext* context, const DataApi::FileRequest* req, DataApi::ImageInfo* res) {
    fs::path path = _base_path / req->directoryname() / req->filename();
    if (!fs::exists(path)) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "File does not exist");
    }

    res->set_filename(req->filename());
    res->add_dimensions(0);
    auto magic_number = GetMagicNumber(path);

    std::unique_ptr<Image> image;
    if (magic_number == FITS_MAGIC_NUM) {
        res->set_filetype(DataApi::FileType::Fits);
        image.reset(new FitsImage(path, req->hduname(), req->hdunum()));
    } else if (magic_number == HDF5_MAGIC_NUM) {
        res->set_filetype(DataApi::FileType::Hdf5);
        // TODO: add HDF5 support
        image.reset();
    } else {
        res->set_filetype(DataApi::FileType::Unknown);
        image.reset();
    }

    if (image && image->IsValid()) {
        std::vector<int> dims = image->Dimensions();
        *res->mutable_dimensions() = {dims.begin(), dims.end()};

        const auto& header = image->Header();
        for (const auto& entry : header) {
            auto* out_header = res->mutable_header()->Add();
            out_header->set_key(entry.key);
            out_header->set_value(entry.value);
            out_header->set_comment(entry.comment);
        }
    }

    return grpc::Status::OK;
}
