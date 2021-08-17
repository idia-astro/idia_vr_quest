#include "Image.h"

const std::vector<int>& Image::Dimensions() {
    return _dimensions;
}
const std::vector<HeaderEntry>& Image::Header() {
    return _header;
}
bool Image::FillImageInfo(DataApi::ImageInfo* imageInfo) {
    if (!imageInfo || !IsValid()) {
        return false;
    }

    *imageInfo->mutable_dimensions() = {_dimensions.begin(), _dimensions.end()};

    for (const auto& entry : _header) {
        auto* out_header = imageInfo->mutable_header()->Add();
        out_header->set_key(entry.key);
        out_header->set_value(entry.value);
        out_header->set_comment(entry.comment);
    }

    return true;
}
const std::string& Image::ErrorMessage() {
    return _error_message;
}
void Image::SetFileId(int id) {
    _file_id = id;
}
