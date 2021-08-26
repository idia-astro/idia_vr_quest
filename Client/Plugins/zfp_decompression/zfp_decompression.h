#ifndef ZFP_DECOMPRESSION_ZFP_DECOMPRESSION_H
#define ZFP_DECOMPRESSION_ZFP_DECOMPRESSION_H

#define DllExport __declspec (dllexport)

extern "C"
{
  DllExport int DecompressFloat2D(unsigned char* srcBuffer, int compressedSize, float* destArray, int width, int height, int precision);
};

#endif //ZFP_DECOMPRESSION_ZFP_DECOMPRESSION_H