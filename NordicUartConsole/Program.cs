using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using NordicUartConsole;

var deviceArg = new Argument<string>(
	name: "device",
	description: "Name of the BLE device to which to connect")
{
	Arity = ArgumentArity.ZeroOrOne,
};

var rootCommand = new RootCommand("Nordic UART Service (NUS) console")
{
	deviceArg,
};

rootCommand.SetHandler(async (context) =>
{
	var deviceName = context.ParseResult.GetValueForArgument(deviceArg);
	var token = context.GetCancellationToken();

	try {
		var services = await UartConsole.FindConsolesAsync();
		using var console = SelectConsole(services.ToList(), deviceName, token);

		if (console == null)
		{
			Console.WriteLine("No UART services found");
			return;
		}

		Console.WriteLine($"Connected to {console.Device.Name}");
		await console.RunAsync(token);
	}
	catch (OperationCanceledException)
	{
		// End program.
	}
});

var parser = new CommandLineBuilder(rootCommand)
	.UseVersionOption()
	.UseHelp()
	.UseEnvironmentVariableDirective()
	.UseParseDirective()
	.UseSuggestDirective()
	.RegisterWithDotnetSuggest()
	.UseTypoCorrections()
	.UseParseErrorReporting()
	.CancelOnProcessTermination()
#if !DEBUG
	.UseExceptionHandler()
#endif
	.Build();

return await parser.InvokeAsync(args);

static UartConsole? SelectConsole(List<UartConsole> consoles, string deviceName, CancellationToken cancellationToken)
{
	if (!string.IsNullOrEmpty(deviceName))
	{
		return consoles.Find((c) => c.Device.Name.Equals(deviceName, StringComparison.CurrentCultureIgnoreCase));
	}

	if (consoles.Count > 1)
	{
		var menu = new ConsoleMenu<UartConsole>("Select a device:", consoles, (c) => c.Device.Name);

		return menu.Show(cancellationToken);
	}

	return consoles.FirstOrDefault();
}
