#include "Compression.h"

int CompressFloat3D(const float* srcArray, std::vector<char>& compression_buffer, size_t& compressed_size, int width, int height, int depth, int precision) {
    int status = 0;
    zfp_type type = zfp_type_float;
    zfp_field* field;
    zfp_stream* zfp;
    size_t buffer_size;
    bitstream* stream;

    if (depth > 1) {
        field = zfp_field_3d((void*)srcArray, type, width, height, depth);
    } else {
        field = zfp_field_2d((void*)srcArray, type, width, height);
    }

    zfp = zfp_stream_open(nullptr);
    zfp_stream_set_precision(zfp, precision);

    // Resize buffer if necessary
    buffer_size = zfp_stream_maximum_size(zfp, field);
    if (compression_buffer.size() < buffer_size) {
        compression_buffer.resize(buffer_size);
    }
    stream = stream_open(compression_buffer.data(), buffer_size);
    zfp_stream_set_bit_stream(zfp, stream);
    zfp_stream_rewind(zfp);

    compressed_size = zfp_compress(zfp, field);
    if (!compressed_size) {
        status = 1;
    }

    zfp_field_free(field);
    zfp_stream_close(zfp);
    stream_close(stream);

    return status;
}