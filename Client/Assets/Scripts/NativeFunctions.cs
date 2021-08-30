using System;
using System.Runtime.InteropServices;

public static class NativeFunctions
{
    [DllImport("nativeFunctions")]
    public static extern int DecompressFloat3D(IntPtr srcBuffer, int compressedSize, IntPtr destArray, int width, int height, int depth, int precision);

    [DllImport("nativeFunctions")]
    public static extern int Test(int val);
}