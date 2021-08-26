using System;
using System.Runtime.InteropServices;

public static class NativeFunctions
{
    [DllImport("zfp_decompression")]
    public static extern int DecompressFloat2D(IntPtr srcBuffer, int compressedSize, IntPtr destArray, int width, int height, int precision);
}