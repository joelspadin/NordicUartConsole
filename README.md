# Nordic UART Service (NUS) Console

This is a Windows console app which displays data sent from a Bluetooth LE device using the [Nordic UART Service](https://developer.nordicsemi.com/nRF_Connect_SDK/doc/latest/nrf/libraries/bluetooth_services/services/nus.html).

It does not currently support sending data back to the BLE device.

## Usage

1. Install the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. Pair a BLE device supporting the UART service (Settings > Bluetooth & devices > Add device).
3. Open a terminal to the NordicUartConsole folder and run:

    ```sh
    dotnet run
    ```

If there are multiple devices with the UART service connected, it will prompt you to select a device.

### Zephyr Logging

To display logs from [Zephyr RTOS](https://www.zephyrproject.org/), see the suggested configuration in the [BLE logging sample](https://github.com/zephyrproject-rtos/zephyr/tree/main/samples/subsys/logging/ble_backend). Typically, you will need to set the following in your `.conf` file:

```ini
CONFIG_LOG=y
CONFIG_LOG_BACKEND_BLE=y
CONFIG_LOG_PROCESS_THREAD_STACK_SIZE=2048

# Set these to a value big enough to hold a full log message.
CONFIG_BT_L2CAP_TX_MTU=600
CONFIG_BT_BUF_ACL_RX_SIZE=600
```