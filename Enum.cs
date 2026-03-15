using System;

public class ConsoleScreen
{
    private readonly Vec2d _offset;
    private readonly Vec2d _size;

    // Le constructeur rend la classe "immuable" dans son fonctionnement
    public ConsoleScreen(Vec2d offset, Vec2d size)
    {
        _offset = offset;
        _size = size;
    }

    public void Setup()
    {
        Console.Clear();
        Console.CursorVisible = false;
    }

    public void DrawText(int x, int y, string text, ConsoleColor? color = null)
    {
        Console.SetCursorPosition(x, y);
        if (color.HasValue) Console.ForegroundColor = color.Value;
        Console.Write(text);
        Console.ResetColor();
    }

    public void DrawTextColor(int x, int y, (string text, ConsoleColor color) info) 
        => DrawText(x, y, info.text, info.color);

    public void DrawCell(int cx, int cy, string glyph, ConsoleColor color)
    {
        // Utilise l'offset interne pour positionner automatiquement
        DrawText(_offset.X + cx, _offset.Y + cy, glyph, color);
    }
}

public static class GameConstants
{
    public enum State
    {
        Playing,
        Won,
        Canceled
    }

    public enum CellType
    {
        Corridor = 0,
        Wall = 1,
        Player = 2,
        Exit = 3
    }
}

public static class KeyboardController
{
    // On définit les actions possibles pour le jeu
    public enum Action { None, Up, Down, Left, Right, Quit }

    /// <summary>
    /// Lit la prochaine touche et retourne l'action correspondante.
    /// </summary>
    public static Action GetAction()
    {
        if (!Console.KeyAvailable) return Action.None;

        var key = Console.ReadKey(true).Key;
        return key switch
        {
            ConsoleKey.Z or ConsoleKey.UpArrow    => Action.Up,
            ConsoleKey.S or ConsoleKey.DownArrow  => Action.Down,
            ConsoleKey.Q or ConsoleKey.LeftArrow  => Action.Left,
            ConsoleKey.D or ConsoleKey.RightArrow => Action.Right,
            ConsoleKey.Escape                     => Action.Quit,
            _                                     => Action.None
        };
    }

    /// <summary>
    /// Attend une pression de touche pour bloquer l'exécution (fin de partie).
    /// </summary>
    public static void WaitForKey() => Console.ReadKey(true);
}