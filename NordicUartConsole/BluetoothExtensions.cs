using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace NordicUartConsole
{
	internal static class BluetoothExtensions
	{
		static public async Task<BluetoothLEDevice?> GetBluetoothLEDeviceAsync(this DeviceInformation device)
		{
			try
			{
				return await BluetoothLEDevice.FromIdAsync(device.Id);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
	}
}
