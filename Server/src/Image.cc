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

bool Image::DataLoaded() {
    if (_dimensions.size() < 3) {
        return false;
    }

    auto num_voxels = _data_cube.num_elements();
    return (num_voxels == _dimensions[0] * _dimensions[1] * _dimensions[2]);
}

bool Image::FillImageData(DataApi::DataResponse* res) {
    if (!DataLoaded()) {
        return false;
    }

    auto num_voxels = _data_cube.num_elements();

    float min_val = std::numeric_limits<float>::max();
    float max_val = -std::numeric_limits<float>::max();

    for (unsigned int i = 0; i < _data_cube.num_elements(); i++ )
    {
        auto val = _data_cube.data()[i];
        if (std::isfinite(val)) {
            if (val < min_val) {
                min_val = val;
            }
            if (val > max_val) {
                max_val = val;
            }
        }
    }

    res->set_rawdata(_data_cube.data(), num_voxels * sizeof(float));
    res->set_minvalue(min_val);
    res->set_maxvalue(max_val);
    return true;
}
