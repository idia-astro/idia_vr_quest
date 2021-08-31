#include "Image.h"
#include <spdlog/spdlog.h>

#include "Compression.h"

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

bool Image::FillImageData(DataApi::DataResponse& res, int channelOffset, int num_channels, int precision) {
    if (!DataLoaded()) {
        return false;
    }

    // crop to the remaining channels
    num_channels = std::min(num_channels, _dimensions[2] - channelOffset);
    auto pixels_per_channel = (_dimensions[0] * _dimensions[1]);
    auto num_pixels = num_channels * pixels_per_channel;
    precision = std::max(MIN_COMPRESSION_PRECISION, std::min(MAX_COMPRESSION_PRECISION, precision));

    float* src_array = _data_cube.data() + channelOffset * pixels_per_channel;
    std::vector<char> compressed_data;
    size_t compressed_size = 0;

    auto compression_error_status = CompressFloat3D(src_array, compressed_data, compressed_size, _dimensions[0], _dimensions[1], num_channels, precision);
    compressed_data.resize(compressed_size);

    if (compression_error_status) {
        spdlog::error("Error compressing data");
        return false;
    }

    res.set_num_channels(num_channels);
    res.set_precision(precision);
    *res.mutable_raw_data() = {compressed_data.begin(), compressed_data.end()};
    return true;
}

void Image::GetMinMax(float& min_val, float& max_val) {
    min_val = std::numeric_limits<float>::max();
    max_val = -std::numeric_limits<float>::max();
    auto num_pixels = (_dimensions[0] * _dimensions[1] * _dimensions[2]);
    for (auto i = 0; i < num_pixels; i++) {
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
}
