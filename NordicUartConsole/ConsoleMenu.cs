namespace NordicUartConsole;

internal class ConsoleMenu<T>(string title, IEnumerable<T> items, Func<T, string>? formatter)
{
	public string Title { get; } = title;
	public List<T> Items { get; private set; } = items.ToList();
	public Func<T, string> Formatter { get; } = formatter ?? ((item) => item?.ToString() ?? "");

	public ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;
	public ConsoleColor FocusColor { get; set; } = ConsoleColor.Green;

	private int _focusIndex = 0;
	private int _scrollIndex = 0;

	public T Show(CancellationToken cancellationToken)
	{
		using (ConsoleHelpers.EnableVirtualTerminal())
		using (ConsoleHelpers.HideCursor())
		{
			try
			{
				while (true)
				{
					var width = Console.WindowWidth;
					var height = Console.WindowHeight - 2;

					_scrollIndex = GetScrollIndex(height);

					DrawMenu(width, height);

					if (HandleInput(height, cancellationToken))
					{
						return Items[_focusIndex];
					}

					ResetCursorToTop(height);
				}
			}
			finally
			{
				// Add one blank line at the end to separate further output from the menu.
				Console.WriteLine();
			}
		}
	}

	private bool HandleInput(int height, CancellationToken cancellationToken)
	{
		var task = Task.Run(() => Console.ReadKey(false));
		
		// For some reason, using the cancellation token on Task.Run() and awaiting it will
		// still wait for a key to be pressed before throwing the exception when canceled.
		Task.WaitAny([task], cancellationToken);

		switch (task.Result.Key)
		{
			case ConsoleKey.Enter:
				return true;

			case ConsoleKey.Escape:
				throw new OperationCanceledException();

			case ConsoleKey.UpArrow:
				_focusIndex -= 1;
				break;

			case ConsoleKey.DownArrow:
				_focusIndex += 1;
				break;

			case ConsoleKey.PageUp:
				_focusIndex -= height;
				break;

			case ConsoleKey.PageDown:
				_focusIndex += height;
				break;

			case ConsoleKey.Home:
				_focusIndex = 0;
				break;

			case ConsoleKey.End:
				_focusIndex = Items.Count - 1;
				break;
		}

		_focusIndex = Math.Clamp(_focusIndex, 0, Items.Count - 1);
		return false;
	}

	private void DrawMenu(int width, int height)
	{
		Console.WriteLine(Title);

		var displayCount = GetDisplayCount(height);

		for (int i = 0; i < displayCount; i++)
		{
			var index = _scrollIndex + i;
			var focused = index == _focusIndex;

			var atStart = _scrollIndex == 0;
			var atEnd = _scrollIndex + displayCount >= Items.Count;
			var showMore = (!atStart && i == 0) || (!atEnd && i == displayCount - 1);

			DrawItem(item: Items[index], focused: focused, width: width, showMore: showMore);
		}
	}

	private void DrawItem(T item, bool focused, int width, bool showMore)
	{
		var color = focused ? FocusColor : DefaultColor;
		var prefix = focused ? "> " : "  ";

		var text = prefix + (showMore ? "..." : Formatter(item));
		text = PadItem(text, width);

		using (ConsoleHelpers.Colorize(color))
		{
			Console.WriteLine(text);
		}
	}

	private static string PadItem(string text, int width)
	{
		// Clear the rest of the line to hide leftover text from other menu items when scrolling.
		// Truncate if needed, since each item must not wrap to the next line.
		return text.PadRight(width)[..width];
	}

	private int GetScrollIndex(int height)
	{
		var displayCount = GetDisplayCount(height);

		if (Items.Count < displayCount)
		{
			return 0;
		}

		var firstDisplayed = _scrollIndex;
		var lastDisplayed = _scrollIndex + displayCount - 1;

		if (_focusIndex <= firstDisplayed)
		{
			return Math.Max(0, _focusIndex - 1);
		}

		if (_focusIndex >= lastDisplayed)
		{
			return Math.Min(Items.Count - 1, _focusIndex + 1) - (displayCount - 1);
		}

		return _scrollIndex;
	}

	private int GetDisplayCount(int height)
	{
		return Math.Min(Items.Count, height);
	}

	private void ResetCursorToTop(int height)
	{
		var displayCount = GetDisplayCount(height);

		Console.CursorTop = Math.Max(0, Console.CursorTop - displayCount - 1);
	}
}


