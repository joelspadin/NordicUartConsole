using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using ConsoleTools;
using NordicUartConsole;

var rootCommand = new RootCommand("Nordic UART Service (NUS) console");

rootCommand.SetHandler(async (context) =>
{
	var token = context.GetCancellationToken();

	try {
		var services = await UartConsole.FindConsolesAsync();
		using var console = await SelectConsoleAsync(services.ToList(), token);

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

async static Task<UartConsole?> SelectConsoleAsync(List<UartConsole> consoles, CancellationToken cancellationToken)
{
	if (consoles.Count > 1)
	{
		return await ShowConsoleMenuAsync(consoles, cancellationToken);
	}

	return consoles.FirstOrDefault();
}

async static Task<UartConsole?> ShowConsoleMenuAsync(List<UartConsole> consoles, CancellationToken cancellationToken)
{
	UartConsole? result = null;

	// TODO: write a custom menu which hides the cursor and overwrites each line
	// without first clearing the console.
	var menu = new ConsoleMenu()
		.Configure(config =>
		{
			config.WriteHeaderAction = () => Console.WriteLine("Select a device:");
			config.WriteItemAction = item => Console.Write("{0}", item.Name);
			config.Selector = "> ";
			config.SelectedItemBackgroundColor = Console.BackgroundColor;
			config.SelectedItemForegroundColor = Console.ForegroundColor;
		});

	foreach (var console in consoles)
	{
		menu.Add(console.Device.Name, () =>
		{
			result = console;
			menu.CloseMenu();
		});
	}

	await menu.ShowAsync(cancellationToken);

	return result;
}