using UnityEditor;
using UnityEngine;

public class CreateTextureSphere : MonoBehaviour
{
    [MenuItem("AssetGen/Volumes/Sphere/SphereFloat256")]
    static void SphereFloat256() => CreateSphere3D(256, TextureFormat.RFloat);
    [MenuItem("AssetGen/Volumes/Sphere/SphereFloat128")]
    static void SphereFloat128() => CreateSphere3D(128, TextureFormat.RFloat);
    [MenuItem("AssetGen/Volumes/Sphere/SphereFloat64")]
    static void SphereFloat64() => CreateSphere3D(64, TextureFormat.RFloat);
    
    [MenuItem("AssetGen/Volumes/Sphere/SphereHalf128")]
    static void SphereHalf128() => CreateSphere3D(128, TextureFormat.RHalf);
    [MenuItem("AssetGen/Volumes/Sphere/SphereHalf192")]
    static void SphereHalf192() => CreateSphere3D(192, TextureFormat.RHalf);
    [MenuItem("AssetGen/Volumes/Sphere/SphereHalf256")]
    static void SphereHalf256() => CreateSphere3D(256, TextureFormat.RHalf);
    
    [MenuItem("AssetGen/Volumes/Sphere/SphereChar128")]
    static void SphereChar128() => CreateSphere3D(128, TextureFormat.R8);

    static void CreateSphere3D(int size, TextureFormat format)
    {
        // Configure the texture
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;
        texture.filterMode = FilterMode.Point;

        var N = size * size * size;
        Color[] generatedData = new Color[N];

        var diagonal = new Vector3Int(size / 2, size / 2, size / 2);
        for (int k = 0; k < size; k++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    Vector3Int r = new Vector3Int(i, j, k);
                    r -= diagonal;
                    int index = i + j * size + k * (size * size);
                    if (r.magnitude < diagonal.magnitude / 2)
                    {
                        generatedData[index] = Color.white;
                    }
                    else
                    {
                        generatedData[index] = Color.black;
                    }
                }
            }                
        }

        texture.SetPixels(generatedData);
        
        //texture.SetPixelData(generatedData, 0, 0);
        //
        // // Copy the color values to the texture
        // texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();        

        // Save the texture to your Unity Project
        AssetDatabase.CreateAsset(texture, $"Assets/Textures/TextureSphere_{format}_{size}x{size}x{size}.asset");
    }
}