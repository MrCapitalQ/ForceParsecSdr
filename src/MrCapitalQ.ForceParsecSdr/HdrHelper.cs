using System.Linq;
using System.Runtime.InteropServices;
using WindowsDisplayAPI;
using DisplayConfigDeviceInfoType = Windows.Win32.Devices.Display.DISPLAYCONFIG_DEVICE_INFO_TYPE;
using DisplayConfigGetAdvancedColorInfo = Windows.Win32.Devices.Display.DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO;
using DisplayConfigSetAdvancedColorState = Windows.Win32.Devices.Display.DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE;
using PInvoke = Windows.Win32.PInvoke;

namespace MrCapitalQ.ForceParsecSdr;

internal static class HdrHelper
{
    /// <summary>
    /// Gets whether the primary monitor supports HDR.
    /// </summary>
    public static bool IsHdrAvailable() => GetHdrInfo().IsHdrAvailable;

    /// <summary>
    /// Gets whether the primary monitor currently has HDR enabled.
    /// </summary>
    public static bool IsHdrEnabled() => GetHdrInfo().IsHdrEnabled;

    /// <summary>
    /// Enables or disables the primary monitor's HDR display mode.
    /// </summary>
    /// <param name="isEnabled">Whether HDR should be enabled.</param>
    /// <returns>Whether the action was successful.</returns>
    public static bool SetHdrState(bool isEnabled)
    {
        var display = Display.GetDisplays().FirstOrDefault(x => x.IsGDIPrimary);
        if (display is null)
            return false;

        var adapter = display.Adapter.ToPathDisplayAdapter();
        var setAdvancedColorState = new DisplayConfigSetAdvancedColorState()
        {
            header = new()
            {
                type = DisplayConfigDeviceInfoType.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,
                size = (uint)Marshal.SizeOf<DisplayConfigSetAdvancedColorState>(),
                adapterId = new()
                {
                    HighPart = adapter.AdapterId.HighPart,
                    LowPart = adapter.AdapterId.LowPart
                },
                id = display.ToPathDisplayTarget().TargetId
            },
            Anonymous = new()
            {
                value = isEnabled ? 1u : 0
            }
        };

        return PInvoke.DisplayConfigSetDeviceInfo(setAdvancedColorState.header) == 0;
    }

    private static (bool IsHdrAvailable, bool IsHdrEnabled) GetHdrInfo()
    {
        var display = Display.GetDisplays().FirstOrDefault(x => x.IsGDIPrimary);
        if (display is null)
            return (false, false);

        var adapter = display.Adapter.ToPathDisplayAdapter();
        var getAdvancedColorInfo = new DisplayConfigGetAdvancedColorInfo()
        {
            header = new()
            {
                type = DisplayConfigDeviceInfoType.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                size = (uint)Marshal.SizeOf<DisplayConfigGetAdvancedColorInfo>(),
                adapterId = new()
                {
                    HighPart = adapter.AdapterId.HighPart,
                    LowPart = adapter.AdapterId.LowPart
                },
                id = display.ToPathDisplayTarget().TargetId
            }
        };

        if (PInvoke.DisplayConfigGetDeviceInfo(ref getAdvancedColorInfo.header) != 0)
            return (false, false);

        var isHdrAvailable = getAdvancedColorInfo.Anonymous.value.GetBit(0);
        var isHdrEnabled = getAdvancedColorInfo.Anonymous.value.GetBit(1);
        return (isHdrAvailable, isHdrEnabled);
    }

    private static bool GetBit(this uint b, int bitNumber) => (b & (1 << bitNumber)) != 0;
}
