#ifndef IDIAVRSERVER_FITSIMAGE_H
#define IDIAVRSERVER_FITSIMAGE_H

#include <filesystem>

// Has to be included first on Windows platforms due to TBYTE #define redefinition
#include <spdlog/spdlog.h>

#include <fitsio.h>

#include "Image.h"

namespace fs = std::filesystem;

class FitsImage : public Image {
private:
    fitsfile* _fptr;
    std::string _file_name;

public:
    FitsImage(const fs::path& path, const std::string& hdu_name, int hdu_num);
    ~FitsImage() override;
    bool IsValid() override;

private:
    bool FillHeaders();
    bool CloseFile();
    static std::string TrimFitsValue(const std::string& val_string);
};

#endif // IDIAVRSERVER_FITSIMAGE_H
