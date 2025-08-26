using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;

namespace AntiCheat.Class;

public static class ServerIP
{
    private delegate nint CNetworkSystem_UpdatePublicIp(nint a1);
    private static CNetworkSystem_UpdatePublicIp? _networkSystemUpdatePublicIp;

    public static string Get()
    {
        nint _networkSystem = NativeAPI.GetValveInterface(0, "NetworkSystemVersion001");

        unsafe
        {
            if (_networkSystemUpdatePublicIp == null)
            {
                nint funcPtr = *(nint*)(*(nint*)_networkSystem + 256);
                _networkSystemUpdatePublicIp = Marshal.GetDelegateForFunctionPointer<CNetworkSystem_UpdatePublicIp>(funcPtr);
            }

            byte* ipBytes = (byte*)(_networkSystemUpdatePublicIp(_networkSystem) + 4);
            return $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}.{ipBytes[3]}";
        }
    }
}