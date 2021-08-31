#ifndef NATIVEFUNCTIONS__NATIVEFUNCTIONS_H_
#define NATIVEFUNCTIONS__NATIVEFUNCTIONS_H_

#ifdef WIN32
#define DllExport __declspec (dllexport)
#else
#define DllExport
#endif

extern "C" {
DllExport int DecompressFloat3D(unsigned char* src_buffer, int compressed_size, float* dest_array, int width, int height, int depth, int precision);
DllExport void ScaleArray(const float* src_array, unsigned char* dest_array, int length, float min_value, float max_value, unsigned char nan_value);
}

#endif //NATIVEFUNCTIONS__NATIVEFUNCTIONS_H_
