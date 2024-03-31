namespace NordicUartConsole;

internal static class StringExtensions
{
	public static string RemovePrefix(this string str, string prefix)
	{
		return str.StartsWith(prefix) ? str[prefix.Length..] : str;
	}

	public static string RemoveSuffix(this string str, string suffix)
	{
		return str.EndsWith(suffix) ? str[..^suffix.Length] : str;
	}
}
