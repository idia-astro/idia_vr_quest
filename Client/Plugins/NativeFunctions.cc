#include <zfp.h>

#ifdef WIN32
#define DllExport __declspec (dllexport)
#else
#define DllExport
#endif

extern "C" {
DllExport int Test(int val) {
    return val * 10;
}

DllExport int DecompressFloat3D(unsigned char* srcBuffer, int compressedSize, float* destArray, int width, int height, int depth, int precision) {
    int status = 0;
    zfp_type type = zfp_type_float;
    zfp_field* field;
    zfp_stream* zfp;
    bitstream* stream;
    type = zfp_type_float;
    field = zfp_field_3d(destArray, type, width, height, depth);
    zfp = zfp_stream_open(nullptr);

    zfp_stream_set_precision(zfp, precision);
    stream = stream_open(srcBuffer, compressedSize);
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
}