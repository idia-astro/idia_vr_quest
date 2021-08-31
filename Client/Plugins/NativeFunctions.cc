#include "NativeFunctions.h"

#include <cmath>

#include <zfp.h>


int DecompressFloat3D(unsigned char* src_buffer, int compressed_size, float* dest_array, int width, int height, int depth, int precision) {
    int status = 0;
    zfp_type type = zfp_type_float;
    zfp_field* field;
    zfp_stream* zfp;
    bitstream* stream;
    field = zfp_field_3d(dest_array, type, width, height, depth);
    zfp = zfp_stream_open(nullptr);

    zfp_stream_set_precision(zfp, precision);
    stream = stream_open(src_buffer, compressed_size);
    zfp_stream_set_bit_stream(zfp, stream);
    zfp_stream_rewind(zfp);

    if (!zfp_decompress(zfp, field)) {
        status = 1;
    }

    zfp_field_free(field);
    zfp_stream_close(zfp);
    stream_close(stream);

    return status;
}

void ScaleArray(const float* src_array, unsigned char* dest_array, int length, float min_value, float max_value, unsigned char nan_value) {
    float range = max_value - min_value;

#pragma omp parallel for
    for (int i = 0; i < length; i++) {
        float val = src_array[i];
        if (std::isfinite(val)) {
            // handle numerical errors for min and max values
            if (val == min_value)
            {
                dest_array[i] = 0;
            }
            else if (val == max_value)
            {
                dest_array[i] = 255;
            }
            else
            {
                val = 255 * (val - min_value) / range;
                dest_array[i] = lround(val);
            }
        } else {
            dest_array[i] = nan_value;
        }
    }
}
