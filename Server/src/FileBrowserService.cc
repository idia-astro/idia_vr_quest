#include "FileBrowserService.h"

#include <chrono>
#include <filesystem>

#include "FitsImage.h"
#include "Util.h"

#define FITS_MAGIC_NUM 0x2020454c504d4953
// TODO: check why this isn't working!
#define HDF5_MAGIC_NUM 0x0a1a0a0d46444889

namespace fs = std::filesystem;

FileBrowserService::FileBrowserService(fs::path path) : _base_path(path) {
    _file_id_counter = 0;
}

grpc::Status FileBrowserService::GetFileList(grpc::ServerContext* context, const DataApi::FileListRequest* req, DataApi::FileList* res) {
    fs::path directory_path = _base_path / req->directory_name();

    if (!fs::exists(directory_path)) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "Directory does not exist");
    }

    res->set_directory_name(req->directory_name());
    for (const auto& entry : fs::directory_iterator(directory_path)) {
        const auto& path = entry.path();
        if (entry.is_directory()) {
            auto* dir = res->add_sub_directories();
            auto modified = std::filesystem::last_write_time(path);
            dir->set_date(std::chrono::duration_cast<std::chrono::milliseconds>(modified.time_since_epoch()).count());
            dir->set_name(path.filename().string());
            dir->set_num_items(GetFolderItemCount(path));
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
    fs::path path = _base_path / req->directory_name() / req->file_name();
    if (!fs::exists(path)) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "File does not exist");
    }

    res->set_file_name(req->file_name());
    res->add_dimensions(0);
    auto magic_number = GetMagicNumber(path);

    if (magic_number != FITS_MAGIC_NUM) {
        return grpc::Status(grpc::StatusCode::INTERNAL, "File type not supported");
    }

    res->set_file_type(DataApi::FileType::Fits);
    std::unique_ptr<Image> image = std::make_unique<FitsImage>(path, req->hdu_name(), req->hdu_num());

    if (!image || !image->IsValid()) {
        return grpc::Status(grpc::StatusCode::INTERNAL, image->ErrorMessage());
    }

    if (!image->FillImageInfo(res)) {
        return grpc::Status(grpc::StatusCode::INTERNAL, "File has invalid headers");
    }

    return grpc::Status::OK;
}
grpc::Status FileBrowserService::OpenImage(grpc::ServerContext* context, const DataApi::FileRequest* req, DataApi::OpenImageResponse* res) {
    fs::path path = _base_path / req->directory_name() / req->file_name();
    if (!fs::exists(path)) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "File does not exist");
    }

    auto magic_number = GetMagicNumber(path);

    if (magic_number != FITS_MAGIC_NUM) {
        return grpc::Status(grpc::StatusCode::INTERNAL, "File type not supported");
    }

    std::unique_ptr<Image> image = std::make_unique<FitsImage>(path, req->hdu_name(), req->hdu_num());

    if (!image || !image->IsValid()) {
        return grpc::Status(grpc::StatusCode::INTERNAL, image->ErrorMessage());
    }

    image->SetFileId(_file_id_counter);
    image->LoadData();
    res->set_file_id(_file_id_counter);

    std::unique_lock image_lock(_image_map_mutex);
    _image_map[_file_id_counter] = std::move(image);
    _file_id_counter++;
    return grpc::Status::OK;
}
grpc::Status FileBrowserService::CloseImage(grpc::ServerContext* context, const DataApi::CloseFileRequest* req, google::protobuf::Empty* res) {
    std::unique_lock image_lock(_image_map_mutex);

    if (!_image_map.count(req->file_id())) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "File ID does not exist");
    }

    _image_map.at(req->file_id()).reset();
    return grpc::Status::OK;
}

grpc::Status FileBrowserService::GetData(grpc::ServerContext* context, const DataApi::GetDataRequest* req, grpc::ServerWriter<DataApi::DataResponse>* writer) {
    std::shared_lock image_lock(_image_map_mutex);
    if (!_image_map.count(req->file_id())) {
        return grpc::Status(grpc::StatusCode::INVALID_ARGUMENT, "File ID does not exist");
    }

    const auto& image = _image_map.at(req->file_id());
    if (!image->DataLoaded()) {
        return grpc::Status(grpc::StatusCode::INTERNAL, "Image data not available");
    }

    DataApi::DataResponse res;

    int channel = 0;
    int channels_per_message = std::max(req->channels_per_message(), 1);
    int num_channels = image->Dimensions()[2];
    float min_value;
    float max_value;

    image->GetMinMax(min_value, max_value);
    res.set_min_value(min_value);
    res.set_max_value(max_value);

    while (channel < num_channels) {
        if (!image->FillImageData(res, channel, channels_per_message, req->precision())) {
            return grpc::Status(grpc::StatusCode::INTERNAL, "Problem accessing image data");
        }
        writer->Write(res);
        channel += channels_per_message;
    }

    return grpc::Status::OK;
}