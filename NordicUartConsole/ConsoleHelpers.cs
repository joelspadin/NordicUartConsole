namespace NordicUartConsole;

using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.System.Console;

internal static class ConsoleHelpers
{
	static public IDisposable Colorize(ConsoleColor color)
	{
		return new ColorizeContext(color);
	}

	private class ColorizeContext : IDisposable
	{
		readonly ConsoleColor OldColor = Console.ForegroundColor;

        public ColorizeContext(ConsoleColor color)
        {
			Console.ForegroundColor = color;
        }

		public void Dispose()
		{
			Console.ForegroundColor = OldColor;
		}
    }

	static public IDisposable EnableVirtualTerminal()
	{
		return new VirtualTerminalContext();
	}

	private class VirtualTerminalContext : IDisposable
	{
		static readonly CONSOLE_MODE VirtualTerminalFlags =
			CONSOLE_MODE.ENABLE_PROCESSED_OUTPUT |
			CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT |
			CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

		readonly SafeFileHandle OutputHandle = PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
		readonly CONSOLE_MODE OldMode = 0;

		public VirtualTerminalContext()
		{
			PInvoke.GetConsoleMode(OutputHandle, out OldMode);
			PInvoke.SetConsoleMode(OutputHandle, OldMode | VirtualTerminalFlags);
		}

		public void Dispose()
		{
			PInvoke.SetConsoleMode(OutputHandle, OldMode);
		}
	}

	static public IDisposable HideCursor()
	{
		return new HideCursorContext();
	}

	private class HideCursorContext : IDisposable
	{
		readonly bool OldVisible = Console.CursorVisible;

		public HideCursorContext()
		{
			Console.CursorVisible = false;
		}

		public void Dispose()
		{
			Console.CursorVisible = OldVisible;
		}
	}
}
