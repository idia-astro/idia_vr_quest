#ifndef IDIAVRSERVER_FITSIMAGE_H
#define IDIAVRSERVER_FITSIMAGE_H

#include <cfitsio/fitsio.h>
#include <filesystem>
#include <string>
#include <vector>

namespace fs = std::filesystem;

class FitsImage {
private:
    fitsfile* _fptr;
    std::string _error_message;
    std::string _hdu_name;
    int _hdu_num;
    std::vector<int> _dimensions;
    std::vector<double> _ref_pix;
    std::vector<double> _ref_val;
    std::vector<double> _ref_delta;
    std::string _unit;
public:
    FitsImage(const fs::path& path, const std::string& hdu_name, int hdu_num);
    ~FitsImage();
    const std::vector<int>& Dimensions();
    bool IsValid();

private:
    bool FillHeaders();
    bool CloseFile();
};

#endif // IDIAVRSERVER_FITSIMAGE_H
