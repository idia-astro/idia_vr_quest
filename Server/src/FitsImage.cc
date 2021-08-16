#include "FitsImage.h"

#include <regex>

#include <fmt/format.h>

FitsImage::FitsImage(const fs::path& path, const std::string& hdu_name, int hdu_num) {
    _hdu_name = hdu_name;
    int fits_status = 0;
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
        auto l = hdu_name.copy(ext_name, FLEN_VALUE - 1);
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

    if (_hdu_name.empty()) {
        _hdu_name = "PRIMARY";
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

    _wcs.resize(_dimensions.size());

    int error_status = 0;
    char key_tmp[FLEN_KEYWORD];
    char val_tmp[FLEN_VALUE];
    char comment_tmp[FLEN_COMMENT];
    int index = 1;
    while (!error_status) {
        fits_read_keyn(_fptr, index, key_tmp, val_tmp, comment_tmp, &error_status);
        if (!error_status) {
            auto header = HeaderEntry{key_tmp, val_tmp, comment_tmp};

            std::regex wcs_regex("^(CRPIX|CRVAL|CDELT)(\\d+)$");
            std::smatch sm;

            if (header.key == "EXTNAME") {
                _hdu_name = TrimFitsValue(header.value);
            } else if (header.key == "BUNIT") {
                _unit = TrimFitsValue(header.value);
            } else if (std::regex_match(header.key, sm, wcs_regex)) {
                try {
                    int axis = std::stoi(sm[2]);
                    if (axis < 1 || axis > _wcs.size()) {
                        _error_message = fmt::format("Invalid WCS entry {}", header.key);
                        return false;
                    }

                    double val = std::stod(header.value); // FITS axes start from 1
                    auto& wcs = _wcs[axis - 1];
                    wcs.axis = axis;

                    if (sm[1] == "CRPIX") {
                        wcs.ref_pix = val;
                    } else if (sm[1] == "CRVAL") {
                        wcs.ref_val = val;
                    } else {
                        wcs.delta = val;
                    }
                } catch (std::invalid_argument) {
                    _error_message = fmt::format("Invalid WCS axis {}", sm[2].str());
                }
            }
            _header.push_back(header);
        }
        index++;
    }

    return error_status == KEY_OUT_BOUNDS;
}

bool FitsImage::CloseFile() {
    if (!_fptr) {
        return false;
    }

    int status;
    fits_close_file(_fptr, &status);
    _fptr = nullptr;
    return status == 0;
}

std::string FitsImage::TrimFitsValue(const std::string& val_string) {
    auto length = val_string.length();
    if (length < 2) {
        return "";
    }
    if (val_string.at(0) == '\'' && val_string.at(length - 1) == '\'') {
        return val_string.substr(1, length - 2);
    }
    return std::string();
}
