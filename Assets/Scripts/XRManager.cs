using System;
using UnityEngine;
using UnityEngine.XR;

public class XRManager : MonoBehaviour
{
    void Start()
    {
        OVRManager.gpuLevel = 5;
        // Set 80 Hz refresh if it is supported (e.g. Quest2)
        TrySetFrequency(80.0f);

        // Most aggressive foveated rendering approach
        SetFoveatedRenderingLevel(OVRManager.FixedFoveatedRenderingLevel.HighTop);
    }

    private static bool TrySetFrequency(float desiredFrequency)
    {
        if (OVRManager.display == null)
        {
            Debug.LogWarning($"No headset detected!");
            return false;
        }
        
        foreach (var f in OVRManager.display.displayFrequenciesAvailable)
        {
            if (Mathf.Approximately(f, desiredFrequency))
            {
                OVRPlugin.systemDisplayFrequency = f;
                Debug.Log($"Setting headset frequency to {f} Hz");
                return true;
            }
        }
        Debug.LogWarning($"Unable to set headset frequency to {desiredFrequency} Hz");
        return false;
    }

    private static void SetFoveatedRenderingLevel(OVRManager.FixedFoveatedRenderingLevel level)
    {
        OVRManager.useDynamicFixedFoveatedRendering = false;
        OVRManager.fixedFoveatedRenderingLevel = level;
    }
}