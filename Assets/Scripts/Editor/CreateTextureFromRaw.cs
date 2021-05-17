using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Adapted from https://docs.unity3d.com/Manual/class-Texture3D.html

public class CreateTextureFromRaw : MonoBehaviour
{
    [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As FP32 Texture")]
    static void FloatImport() => CreateTextureFromFile(TextureFormat.RFloat);

    [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As FP16 Texture")]
    static void HalfImport() => CreateTextureFromFile(TextureFormat.RHalf);

    [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As U8 Texture")]
    static void CharImport() => CreateTextureFromFile(TextureFormat.R8);

    private static string _lastDirectory = "";
    static void CreateTextureFromFile(TextureFormat format)
    {
        string path = EditorUtility.OpenFilePanel("Select input file", _lastDirectory, "arr");
        string filename = Path.GetFileNameWithoutExtension(path);
        _lastDirectory = Path.GetDirectoryName(path);
        
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            int N = fileContent.Length / 4;
            Color[] inputColors = new Color[N];
            float minValue = Single.MaxValue;
            float maxValue = Single.MinValue;
            for (int i = 0; i < N; i++)
            {
                var val = BitConverter.ToSingle(fileContent, i * 4);
                inputColors[i] = new Color(val, val, val, val);
                if (!Single.IsNaN(val))
                {
                    maxValue = Math.Max(maxValue, val);
                    minValue = Math.Min(minValue, val);
                }
            }

            int size = Mathf.RoundToInt((float) Math.Pow(N, 1.0f / 3.0f));
            Texture3D texture = new Texture3D(size, size, size, format, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            // Scale to range [0.0f, 1.0f] for 8-bit textures
            if (format == TextureFormat.R8)
            {
                float range = maxValue - minValue;
                for (int i = 0; i < N; i++)
                {
                    var val = inputColors[i].r;
                    // handle numerical errors for min and max values
                    if (val == minValue)
                    {
                        val = 0.0f;
                    }
                    else if (val == maxValue)
                    {
                        val = 1.0f;
                    }
                    else
                    {
                        val = (val - minValue) / range;
                    }

                    inputColors[i] = new Color(val, val, val, val);
                }
            }

            texture.SetPixels(inputColors);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, $"Assets/Textures/{filename}_{format}_{size}x{size}x{size}.asset");
        }
    }
}