using System;
using System.Runtime.InteropServices;

public static class NativeFunctions
{
    [DllImport("nativeFunctions")]
    public static extern unsafe int DecompressFloat3D(byte* srcBuffer, int compressedSize, float* destArray, int width, int height, int depth, int precision);

    [DllImport("nativeFunctions")]
    public static extern unsafe void ScaleArray(float* srcPtr, byte* destPtr, int length, float minValue, float maxValue, byte nanValue);
}