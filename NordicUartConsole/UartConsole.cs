using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace NordicUartConsole
{
	internal class UartConsole(BluetoothLEDevice device, GattDeviceService service) : IDisposable
	{
		static readonly Guid UartServiceUuid = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
		static readonly Guid RxCharacteristic = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
		static readonly Guid TxCharacteristic = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");

		public BluetoothLEDevice Device { get; private set; } = device;
		public GattDeviceService Service { get; private set; } = service;

		static public async Task<IEnumerable<UartConsole>> FindConsolesAsync()
		{
			var selector = BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected);
			var connected = await DeviceInformation.FindAllAsync(selector);

			var consoles = new List<UartConsole>();

			foreach (var device in connected)
			{
				var bleDevice = await device.GetBluetoothLEDeviceAsync();
				if (bleDevice == null)
				{
					continue;
				}

				var result = await bleDevice.GetGattServicesForUuidAsync(UartServiceUuid);
				if (result.Services.Count > 0)
				{
					consoles.Add(new UartConsole(bleDevice, result.Services[0]));
				}
				else
				{
					bleDevice.Dispose();
				}
			}

			return consoles;
		}

		private static string GetLine(IBuffer buffer)
		{
			var str = new UTF8Encoding().GetString(buffer.ToArray());
			var endIndex = str.IndexOf("\r\n");
			if (endIndex >= 0)
			{
				str = str[..endIndex];
			}

			return str;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			var result = await Service.GetCharacteristicsForUuidAsync(TxCharacteristic);
			if (result.Status != GattCommunicationStatus.Success)
			{
				throw new Exception($"Failed to get TX characteristic. Status = ${result.Status}");
			}

			var tx = result.Characteristics[0];
			tx.ValueChanged += (sender, e) =>
			{
				var line = GetLine(e.CharacteristicValue);
				Console.WriteLine(line);
			};

			await tx.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

			await cancellationToken;
		}

		public void Dispose()
		{
			Device.Dispose();
			Service.Dispose();
		}
	}
}
