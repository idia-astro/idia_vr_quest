#ifndef IDIAVRSERVER_IMAGE_H
#define IDIAVRSERVER_IMAGE_H

#include <string>
#include <vector>

struct HeaderEntry {
    std::string key;
    std::string value;
    std::string comment;
};

struct WcsEntry {
    int axis;
    double ref_pix;
    double ref_val;
    double delta;
};

class Image {
protected:
    std::string _error_message;
    std::string _hdu_name;
    int _hdu_num;
    std::vector<int> _dimensions;
    std::string _unit;
    std::vector<HeaderEntry> _header;
    std::vector<WcsEntry> _wcs;

public:
    const std::vector<int>& Dimensions();
    const std::vector<HeaderEntry>& Header();
    virtual bool IsValid() = 0;
};

#endif // IDIAVRSERVER_IMAGE_H
