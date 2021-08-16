#ifndef IDIAVRSERVER_FITSIMAGE_H
#define IDIAVRSERVER_FITSIMAGE_H

#include <filesystem>

#include <fitsio.h>

#include "Image.h"

namespace fs = std::filesystem;

class FitsImage : public Image {
private:
    fitsfile* _fptr;

public:
    FitsImage(const fs::path& path, const std::string& hdu_name, int hdu_num);
    ~FitsImage();
    bool IsValid() override;

private:
    bool FillHeaders();
    bool CloseFile();
    static std::string TrimFitsValue(const std::string& val_string);
};

#endif // IDIAVRSERVER_FITSIMAGE_H
