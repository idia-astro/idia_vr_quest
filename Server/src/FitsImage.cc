#include "FitsImage.h"

FitsImage::FitsImage(const fs::path& path, const std::string& hdu_name, int hdu_num) {
    _hdu_name = hdu_name;
    int fits_status = 0;
    int threadsafe = fits_is_reentrant();
    fits_open_file(&_fptr, path.string().c_str(), READONLY, &fits_status);
    if (!_fptr || fits_status) {
        _error_message = "Cannot open file " + path.string();
        return;
    }

    int hdu_type;
    _hdu_name = "";

    // Switch to correct HDU
    if (!hdu_name.empty()) {
        char ext_name[FLEN_VALUE];
        auto l = hdu_name.copy(ext_name, FLEN_VALUE -1);
        ext_name[l] = '\0';
        fits_movnam_hdu(_fptr, IMAGE_HDU, ext_name, 0, &fits_status);
        if (fits_status == BAD_HDU_NUM) {
            _error_message = "Cannot find hdu " + hdu_name;
            CloseFile();
            return;
        }
    } else if (hdu_num > 0) {
        fits_movabs_hdu(_fptr, hdu_num, &hdu_type, &fits_status);
    } else {
        fits_get_hdu_type(_fptr, &hdu_type, &fits_status);
    }

    if (fits_status || hdu_type != IMAGE_HDU) {

        CloseFile();
        return;
    }

    // Fill dimensions
    int num_dim;
    fits_get_img_dim(_fptr, &num_dim, &fits_status);
    if (num_dim <= 0 || fits_status) {
        _error_message = "Cannot determine file dimensions";
        CloseFile();
        return;
    }

    std::vector<long> dims_long;
    dims_long.resize(num_dim);
    fits_get_img_size(_fptr, num_dim, dims_long.data(), &fits_status);
    if (fits_status) {
        _error_message = "Cannot get file dimensions";
        CloseFile();
        return;
    }
    std::copy(dims_long.begin(), dims_long.end(), std::back_inserter(_dimensions));


    if (!FillHeaders()) {
        _error_message = "Cannot read file headers";
        CloseFile();
        return;
    }
}

FitsImage::~FitsImage() {
    CloseFile();
}

bool FitsImage::IsValid() {
    return _fptr != nullptr && _error_message.empty();
}

bool FitsImage::FillHeaders() {
    // TODO: Assign HDU name from file after reading headers
    if (!IsValid()) {
        return false;
    }

    return true;
}

bool FitsImage::CloseFile() {
    if (!_fptr) {
        return false;
    }

    int status;
    fits_close_file (_fptr, &status);
    _fptr = nullptr;
    return status == 0;
}

const std::vector<int>& FitsImage::Dimensions() {
    return _dimensions;
}

