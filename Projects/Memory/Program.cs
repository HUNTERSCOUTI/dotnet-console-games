﻿using System;
using System.Globalization;
using static Towel.Statics;

Tile[,] board;
(int Row, int Column)? firstSelection = null;
(int Row, int Column)? secondSelection = null;
(int Row, int Column) selection = (0, 0);
bool closeRequested = false;
bool pendingConfirmation = false;

try
{
	Console.BackgroundColor = ConsoleColor.Black;
	Console.ForegroundColor = ConsoleColor.White;
	Console.CursorVisible = false;
	Console.Clear();
	while (!closeRequested)
	{
		selection = (0, 0);
		Console.Clear();
		RandomizeBoard();
		EnsureConsoleSize();
		while ((!closeRequested && !AllTilesVisible()) || pendingConfirmation)
		{
			EnsureConsoleSize();
			if (!closeRequested)
			{
				Render();
				GetInput();
			}
		}
		if (!closeRequested)
		{
			selection = (-1, -1);
			Render();
			Console.WriteLine();
			Console.WriteLine(" You Win!");
			Console.WriteLine(" Play again [enter] or exit [escape]?");
			bool enter = false;
			while (!closeRequested && !enter)
			{
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.Enter:
						enter = true;
						break;
					case ConsoleKey.Escape:
						closeRequested = true;
						break;
				}
			}
		}
	}
}
finally
{
	Console.ResetColor();
	Console.CursorVisible = true;
	Console.Clear();
}

void RandomizeBoard()
{
	board = new Tile[5, 10];
	for (int i = 0, k = 2; i < board.GetLength(0); i++)
	{
		for (int j = 0; j < board.GetLength(1); j++, k++)
		{
			board[i, j] = new Tile { Value = k / 2 };
		}
	}
	Shuffle<Tile>(
		start: 0,
		end: board.GetLength(0) * board.GetLength(1) - 1,
		get: i =>
		{
			int row = i / board.GetLength(1);
			int column = i % board.GetLength(1);
			return board[row, column];
		},
		set: (i, v) => board[i / board.GetLength(1), i % board.GetLength(1)] = v);
}

void EnsureConsoleSize()
{
	int width = Console.WindowWidth;
	int height = Console.WindowHeight;
	int minWidth = board.GetLength(1) * 3 + 4;
	int minHeight = board.GetLength(0) * 2 + 13;
	while (!closeRequested && (width < minWidth || height < minHeight))
	{
		Console.Clear();
		Console.WriteLine("Increase console size and press [enter]...");
		bool enter = false;
		while (!closeRequested && !enter)
		{
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.Enter:
					enter = true;
					break;
				case ConsoleKey.Escape:
					closeRequested = true;
					break;
			}
		}
		width = Console.WindowWidth;
		height = Console.WindowHeight;
		Console.Clear();
	}
}

void Render()
{
	Console.CursorVisible = false;
	Console.BackgroundColor = ConsoleColor.Black;
	Console.ForegroundColor = ConsoleColor.White;
	Console.SetCursorPosition(0, 0);
	Console.WriteLine();
	Console.WriteLine(" Memory");
	Console.WriteLine();
	Console.WriteLine();
	for (int i = 0; i < board.GetLength(0); i++)
	{
		Console.Write("  ");
		for (int j = 0; j < board.GetLength(1); j++)
		{
			Console.Write(' ');
			if (firstSelection is not null && secondSelection is not null &&
				(firstSelection == (i, j) || secondSelection == (i, j)))
			{
				var (a, b) = (firstSelection.Value, secondSelection.Value);
				if (board[a.Row, a.Column].Value == board[b.Row, b.Column].Value)
				{
					Console.BackgroundColor = ConsoleColor.DarkGreen;
				}
				else
				{
					Console.BackgroundColor = ConsoleColor.DarkRed;
				}
			}
			else if (firstSelection == (i, j) || secondSelection == (i, j))
			{
				Console.BackgroundColor = ConsoleColor.DarkYellow;
			}
			else if (selection == (i, j))
			{
				Console.BackgroundColor = ConsoleColor.DarkCyan;
			}
			else if (board[i, j].Visible)
			{
				Console.BackgroundColor= ConsoleColor.DarkGray;
			}
			else
			{
				Console.BackgroundColor = ConsoleColor.White;
			}
			if (board[i, j].Visible)
			{
				if (board[i, j].Value < 10)
				{
					Console.Write('0');
				}
				Console.Write(board[i, j].Value.ToString(CultureInfo.InvariantCulture));
				Console.BackgroundColor = ConsoleColor.Black;
			}
			else
			{
				Console.Write("  ");
			}
			Console.BackgroundColor = ConsoleColor.Black;
		}
		Console.WriteLine();
		Console.WriteLine();
	}
	Console.WriteLine();
	Console.WriteLine(" Controls...");
	Console.WriteLine(" - arrow keys: change selction");
	Console.WriteLine(" - enter: confirm & acknowledge");
	Console.WriteLine(" - escape: exit");
}

void GetInput()
{
	if (pendingConfirmation)
	{
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.Enter:
				if (pendingConfirmation)
				{
					pendingConfirmation = false;
					Tile a = board[firstSelection!.Value.Row, firstSelection.Value.Column];
					Tile b = board[secondSelection!.Value.Row, secondSelection.Value.Column];
					if (a.Value != b.Value)
					{
						a.Visible = false;
						b.Visible = false;
					}
					firstSelection = null;
					secondSelection = null;
					return;
				}
				break;
			case ConsoleKey.Escape:
				closeRequested = true;
				break;
		}
	}
	else
	{
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.RightArrow:
				selection = (selection.Row, selection.Column == board.GetLength(1) - 1 ? 0 : selection.Column + 1);
				break;
			case ConsoleKey.LeftArrow:
				selection = (selection.Row, selection.Column == 0 ? board.GetLength(1) - 1 : selection.Column - 1);
				break;
			case ConsoleKey.UpArrow:
				selection = (selection.Row == 0 ? board.GetLength(0) - 1 : selection.Row - 1, selection.Column);
				break;
			case ConsoleKey.DownArrow:
				selection = (selection.Row == board.GetLength(0) - 1 ? 0 : selection.Row + 1, selection.Column);
				break;
			case ConsoleKey.Enter:
				if (!board[selection.Row, selection.Column].Visible)
				{
					board[selection.Row, selection.Column].Visible = true;
					if (firstSelection is null)
					{
						firstSelection = selection;
					}
					else if (firstSelection != selection)
					{
						secondSelection = selection;
						pendingConfirmation = true;
					}
				}
				break;
			case ConsoleKey.Escape:
				closeRequested = true;
				break;
		}
	}
}

bool AllTilesVisible()
{
	for (int i = 0, k = 2; i < board.GetLength(0); i++)
	{
		for (int j = 0; j < board.GetLength(1); j++, k++)
		{
			if (!board[i, j].Visible)
			{
				return false;
			}
		}
	}
	return true;
}

class Tile
{
	public int Value { get; set; }
	public bool Visible { get; set; }
}
