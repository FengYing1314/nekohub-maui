using Microsoft.Maui.Devices;

namespace nekohub_maui.Services;

public static class AppConfig
{
    // Resolve the backend base URI depending on platform/emulator
    public static Uri GetApiBaseUri()
    {
        // Default dev backend port (adjust in README if different)
        const string defaultPort = "5249";
        string host = "localhost";
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            host = "10.0.2.2"; // Android emulator loopback to host
        }
        return new Uri($"http://{host}:{defaultPort}/");
    }
}
