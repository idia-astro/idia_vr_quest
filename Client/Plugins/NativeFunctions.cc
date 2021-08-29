#include <zfp.h>

#ifdef WIN32
#define DllExport __declspec (dllexport)
#else
#define DllExport
#endif

extern "C"{
    DllExport int Test(int val) {
        return val * 10;
    }

    DllExport int DecompressFloat2D(unsigned char* srcBuffer, int compressedSize, float* destArray, int width, int height, int precision) {
        int status = 0;    /* return value: 0 = success */
        zfp_type type;     /* array scalar type */
        zfp_field* field;  /* array meta data */
        zfp_stream* zfp;   /* compressed stream */
        bitstream* stream; /* bit stream to write to or read from */
        type = zfp_type_float;
        field = zfp_field_2d(destArray, type, width, height);
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