#region Constants
var size = new Vec2d(50, 20);

var offset = new Vec2d(0,3);

var message = new Vec2d(3,5);

const string sHeader = """
    ╔══════════════════════════════════════════════════╗
    ║          🏃 LABYRINTHE ASCII  C#  🏃             ║
    ╚══════════════════════════════════════════════════╝
    """;
const string sInstructions = "  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter";
const string sWin = """
    ╔════════════════════════════════╗
    ║   🎉  FÉLICITATIONS !  🎉      ║
    ║   Vous avez trouvé la sortie ! ║
    ╚════════════════════════════════╝
""";
const string sCanceled = "\n  Partie abandonnée. À bientôt !";
const string sPressKey = "  Appuyez sur une key pour quitter...";

const ConsoleColor SuccessColor     = ConsoleColor.Green;
const ConsoleColor DangerColor      = ConsoleColor.Red;
const ConsoleColor InfoColor        = ConsoleColor.Cyan;
const ConsoleColor InstructionColor = ConsoleColor.DarkCyan;
const ConsoleColor WallColor        = ConsoleColor.DarkGray;
const ConsoleColor CorridorColor    = ConsoleColor.DarkBlue;
const ConsoleColor PlayerColor      = ConsoleColor.Yellow;
const ConsoleColor ExitColor        = ConsoleColor.Green;
#endregion 

var grid = new GameConstants.CellType[size.X,size.Y];

var playerX = 0;
var playerY = 0;
var mode = GameConstants.State.Playing;

GenerateMaze(grid, playerX, playerY);
DrawScreen();

// ... (Initialisation, génération, etc.)

while (mode == GameConstants.State.Playing)
{
    // On récupère l'intention du joueur via le contrôleur
    var action = KeyboardController.GetAction();

    int nx2 = playerX;
    int ny2 = playerY;

    // On traite l'action de manière logique
    switch (action)
    {
        case KeyboardController.Action.Up:    ny2--; break;
        case KeyboardController.Action.Down:  ny2++; break;
        case KeyboardController.Action.Left:  nx2--; break;
        case KeyboardController.Action.Right: nx2++; break;
        case KeyboardController.Action.Quit:  mode = GameConstants.State.Canceled; break;
        case KeyboardController.Action.None:  continue; // On ne fait rien si aucune touche utile
    }

    // Logique de collision et déplacement (inchangée mais plus claire)
    if (InBound(nx2, size.X) && InBound(ny2, size.Y) && grid[nx2, ny2] != GameConstants.CellType.Wall)
    {
        if (grid[nx2, ny2] == GameConstants.CellType.Exit) mode = GameConstants.State.Won;

        UpdateCell(playerX, playerY, GameConstants.CellType.Corridor);
        playerX = nx2; 
        playerY = ny2;
        UpdateCell(playerX, playerY, GameConstants.CellType.Player);
    }
}

// ... (Affichage de fin)

// Utilisation de la méthode utilitaire pour quitter
KeyboardController.WaitForKey();

DrawTextColorXY(0, offset.Y +size.Y + message.X,
    mode == GameConstants.State.Won 
    ? (sWin, SuccessColor) 
    : (sCanceled, DangerColor)
);
DrawTextXY(0, offset.Y +size.Y + message.X + message.Y, sPressKey);
Console.CursorVisible = true;
Console.ReadKey(true);

#region Functions

void DrawTextXY(int x, int y, string text, ConsoleColor? color = null)
{
    Console.SetCursorPosition(x, y);
    if(color.HasValue)
    {
        Console.ForegroundColor = color.Value;
    }
    Console.Write(text);
    Console.ResetColor();
}

void DrawTextColorXY(int x, int y, (string text, ConsoleColor color) info) =>
    DrawTextXY(x, y, info.text, info.color);

void DrawCell(int cx, int cy) => DrawTextColorXY(
    offset.X + cx, 
    offset.Y + cy,
    grid[cx, cy] switch
    {
        GameConstants.CellType.Wall   => ("█", WallColor),
        GameConstants.CellType.Player => ("@", PlayerColor),
        GameConstants.CellType.Exit   => ("★", ExitColor),
        _               => ("·", CorridorColor)
    });

void UpdateCell(int cx, int cy, GameConstants.CellType type)
{
    grid[cx, cy] = type;
    DrawCell(cx, cy);
}

void DrawScreen()
{
    Console.Clear();
    Console.CursorVisible = false;

    DrawTextXY(0, 0, sHeader, InfoColor);
    for (var y = 0; y <size.Y; y++)
    {
        for (var x = 0; x < size.X; x++)
        {
            DrawCell(x, y);
        }
    }
    DrawTextXY(0, offset.Y +size.Y, sInstructions, InstructionColor);
}

bool InBound(int val, int max) => val >= 0 && val < max;

void GenerateMaze(GameConstants.CellType[,] grid, int playerStartX, int playerStartY)
{
    for (var y = 0; y <size.Y; y++)
        for (var x = 0; x < size.X; x++)
            grid[x, y] = GameConstants.CellType.Wall;

    int[] dx = [ 0, 1, 0, -1 ];
    int[] dy = [ -1, 0, 1, 0 ];
    int[][] orders = [
        [ 0, 1, 2, 3 ], [ 0, 1, 3, 2 ], [ 0, 2, 1, 3 ], [ 0, 2, 3, 1 ], [ 0, 3, 1, 2 ], [ 0, 3, 2, 1 ],
        [ 1, 0, 2, 3 ], [ 1, 0, 3, 2 ], [ 1, 2, 0, 3 ], [ 1, 2, 3, 0 ], [ 1, 3, 0, 2 ], [ 1, 3, 2, 0 ],
        [ 2, 0, 1, 3 ], [ 2, 0, 3, 1 ], [ 2, 1, 0, 3 ], [ 2, 1, 3, 0 ], [ 2, 3, 0, 1 ], [ 2, 3, 1, 0 ],
        [ 3, 0, 1, 2 ], [ 3, 0, 2, 1 ], [ 3, 1, 0, 2 ], [ 3, 1, 2, 0 ], [ 3, 2, 0, 1 ], [ 3, 2, 1, 0 ]
    ];
    var rng = new Random();

    GenerateMazeRec(playerStartX, playerStartY);

    var outX = (size.X  - 1) & ~1;
    var outY = (size.Y - 1) & ~1;

    grid[playerStartX, playerStartY] = GameConstants.CellType.Player;
    grid[outX, outY] = GameConstants.CellType.Exit;
    
    void GenerateMazeRec(int x, int y)
    {
        grid[x, y] = GameConstants.CellType.Corridor;
        foreach (var dir in orders[rng.Next(orders.Length)])
        {
            if( InMaze(x, dx[dir], size.X , out var nx) && 
                InMaze(y, dy[dir],size.Y, out var ny) && 
                grid[nx, ny] == GameConstants.CellType.Wall)
            {
                grid[(x + nx) / 2, (y + ny) / 2] = GameConstants.CellType.Corridor;
                GenerateMazeRec(nx, ny);
            }
        }
        bool InMaze(int val, int delta, int max, out int next) => 
            InBound(next = val + delta * 2, max);
    }
}
#endregion
public record Vec2d(int X, int Y);