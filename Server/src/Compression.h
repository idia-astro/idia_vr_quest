#ifndef IDIAVRSERVER_COMPRESSION_H
#define IDIAVRSERVER_COMPRESSION_H

#include <vector>

#include <zfp.h>

#define DEFAULT_COMPRESSION_PRECISION 12

int CompressFloat3D(const float* srcArray, std::vector<char>& compression_buffer, size_t& compressed_size, uint32_t width, uint32_t height, uint32_t depth,
    uint32_t precision = DEFAULT_COMPRESSION_PRECISION);

#endif // IDIAVRSERVER_COMPRESSION_H
