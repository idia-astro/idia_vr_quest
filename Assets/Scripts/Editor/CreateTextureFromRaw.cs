using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Adapted from https://docs.unity3d.com/Manual/class-Texture3D.html

namespace VolumeData
{
    public class CreateTextureFromRaw : MonoBehaviour
    {
        [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As FP32 Texture")]
        static void FloatImport() => CreateAssetFromFile(TextureFormat.RFloat);

        [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As FP16 Texture")]
        static void HalfImport() => CreateAssetFromFile(TextureFormat.RHalf);

        [MenuItem("AssetGen/Volumes/Import from FP32 Raw Array/As U8 Texture")]
        static void CharImport() => CreateAssetFromFile(TextureFormat.R8);

        private static string _lastDirectory = "";

        static void CreateAssetFromFile(TextureFormat format)
        {
            string path = EditorUtility.OpenFilePanel("Select input file", _lastDirectory, "arr,mhd");
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            _lastDirectory = Path.GetDirectoryName(path);

            if (path.Length != 0)
            {
                Texture3D texture = null;
                if (extension.ToLower() == ".mhd")
                {
                    // Read each line of the MHD file, check for DimSize and ElementDataFile
                    var stream = File.OpenText(path);
                    string dataFilePath = null;

                    Vector3Int dims = Vector3Int.zero;
                    Regex dataFileRegex = new Regex(@"^\s*[Ee]lement[Dd]ata[Ff]ile\s*=\s*(.*?)\s*$");
                    Regex dimsRegex = new Regex(@"^\s*[Dd]im[Ss]ize\s*=\s*(\d+)\s+(\d+)\s+(\d+)\s*$");

                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        var dataFileMatch = dataFileRegex.Match(line);
                        if (dataFileMatch?.Groups?.Count == 2)
                        {
                            dataFilePath = Path.Combine(_lastDirectory, dataFileMatch.Groups[1].Value);
                            continue;
                        }

                        var dimsMatch = dimsRegex.Match(line);

                        if (dimsMatch?.Groups?.Count == 4 &&
                            int.TryParse(dimsMatch.Groups[1].Value, out var w) &&
                            int.TryParse(dimsMatch.Groups[2].Value, out var h) &&
                            int.TryParse(dimsMatch.Groups[3].Value, out var d))
                        {
                            dims = new Vector3Int(w, h, d);
                        }
                    }

                    if (dataFilePath != null && dims.x > 0 && dims.y > 0 && dims.z > 0)
                    {
                        texture = CreateTextureFromFile(format, dataFilePath, dims.x, dims.y, dims.z);
                    }
                }
                else
                {
                    texture = CreateTextureFromFile(format, path);
                }

                if (texture)
                {
                    AssetDatabase.CreateAsset(texture, $"Assets/Textures/{filename}_{format}_{texture.width}x{texture.height}x{texture.depth}.asset");
                }
            }
        }

        public static Texture3D CreateTextureFromFile(TextureFormat format, string path, int width = -1, int height = -1, int depth = -1)
        {
            var fileContent = File.ReadAllBytes(path);
            int numPixels = fileContent.Length / 4;
            float[] dataValues = new float[numPixels];
            float minValue = Single.MaxValue;
            float maxValue = Single.MinValue;
            for (int i = 0; i < numPixels; i++)
            {
                var val = BitConverter.ToSingle(fileContent, i * 4);
                dataValues[i] = val;
                if (!float.IsNaN(val))
                {
                    maxValue = Math.Max(maxValue, val);
                    minValue = Math.Min(minValue, val);
                }
            }

            // Assume image is a cube if width, height or depth are -1
            if (width < 0 || height < 0 || depth < 0)
            {
                int size = Mathf.RoundToInt((float) Math.Pow(numPixels, 1.0f / 3.0f));
                width = height = depth = size;
            }

            Texture3D texture = new Texture3D(width, height, depth, format, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            if (format == TextureFormat.RFloat)
            {
                texture.SetPixelData(dataValues, 0);
            }
            else if (format == TextureFormat.R8)
            {
                float range = maxValue - minValue;
                Debug.Log($"Scaling floating point array from [{minValue}, {maxValue}] to [0, 1]");
                byte[] byteValues = new byte[numPixels];
                for (int i = 0; i < numPixels; i++)
                {
                    var val = dataValues[i];
                    // handle numerical errors for min and max values
                    if (val == minValue)
                    {
                        val = 0;
                    }
                    else if (val == maxValue)
                    {
                        val = 255;
                    }
                    else
                    {
                        val = 255 * (val - minValue) / range;
                    }

                    byteValues[i] = (byte) Mathf.RoundToInt(val);
                }

                texture.SetPixelData(byteValues, 0);
            }
            else
            {
                Color[] colorValues = new Color[numPixels];
                for (int i = 0; i < numPixels; i++)
                {
                    var val = dataValues[i];
                    colorValues[i] = new Color(val, val, val);
                }

                texture.SetPixels(colorValues);
            }

            texture.Apply();
            return texture;
        }
    }
}