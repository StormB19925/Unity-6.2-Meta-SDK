using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Threading.Tasks;

public class LocalRoomLoader : MonoBehaviour
{
    [Header("Settings")]
    public bool LoadOnStart = true;

    private void Start()
    {
        if (LoadOnStart)
        {
            _ = LoadLocalRoomAsync();
        }
    }

    public async Task LoadLocalRoomAsync()
    {
        if (MRUK.Instance == null)
        {
            Debug.LogError("LocalRoomLoader: MRUK Instance not found.");
            return;
        }

        Debug.Log("LocalRoomLoader: initializing room...");

        // FIX: Replaces the complex deprecated logic.
        // This single line does ALL the work:
        // 1. Checks if a room exists on disk.
        // 2. If YES: Loads it.
        // 3. If NO: Automatically launches the Room Setup (Space Capture).
        // 4. Returns Success/Failure.
        var result = await MRUK.Instance.LoadSceneFromDevice(requestSceneCaptureIfNoDataFound: true);

        if (result == MRUK.LoadDeviceResult.Success)
        {
            Debug.Log("LocalRoomLoader: Room loaded successfully.");
        }
        else
        {
            Debug.LogWarning($"LocalRoomLoader: Room load failed or cancelled. Result: {result}");
        }
    }
}