#include "Image.h"

const std::vector<int>& Image::Dimensions() {
    return _dimensions;
}
const std::vector<HeaderEntry>& Image::Header() {
    return _header;
}
