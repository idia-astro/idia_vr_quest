#ifndef IDIAVRSERVER_COMPRESSION_H
#define IDIAVRSERVER_COMPRESSION_H

#include <vector>

#include <zfp.h>

#define DEFAULT_COMPRESSION_PRECISION 12
#define MIN_COMPRESSION_PRECISION 4
#define MAX_COMPRESSION_PRECISION 31

int CompressFloat3D(const float* srcArray, std::vector<char>& compression_buffer, size_t& compressed_size, int width, int height, int depth,
    int precision = DEFAULT_COMPRESSION_PRECISION);

#endif // IDIAVRSERVER_COMPRESSION_H
