using System;

// ============================================================
//  LABYRINTHE ASCII - C# Console
//  Tableau : int[50, 20]  (largeur=50, hauteur=20)
//  0 = couloir   1 = mur   2 = joueur   3 = sortie
//  Déplacement : Z/Q/S/D ou flèches
//  ✅ Optimisé : seules les cellules modifiées sont redessinées
//               via Console.SetCursorPosition()
// ============================================================

int[,] grille = new int[50, 20];

int largeur = 50;
int hauteur = 20;

// Décalage vertical dans la console (nombre de lignes du titre)
int offsetY = 3;
int offsetX = 0;

// ── Génération du labyrinthe par « recursive backtracker » ──
int cellW = largeur / 2;   // 25
int cellH = hauteur / 2;   // 10

for (int y = 0; y < hauteur; y++)
    for (int x = 0; x < largeur; x++)
        grille[x, y] = 1;

int[] stackX = new int[cellW * cellH];
int[] stackY = new int[cellW * cellH];
int stackTop = 0;

bool[,] visited = new bool[cellW, cellH];

int[] dx = { 0, 1, 0, -1 };
int[] dy = { -1, 0, 1, 0 };

Random rng = new Random();

int startCX = 0, startCY = 0;
visited[startCX, startCY] = true;
grille[startCX * 2, startCY * 2] = 0;

stackX[stackTop] = startCX;
stackY[stackTop] = startCY;
stackTop++;

while (stackTop > 0)
{
    int cx = stackX[stackTop - 1];
    int cy = stackY[stackTop - 1];

    int[] ordre = { 0, 1, 2, 3 };
    for (int i = 3; i > 0; i--)
    {
        int j = rng.Next(i + 1);
        int tmp = ordre[i]; ordre[i] = ordre[j]; ordre[j] = tmp;
    }

    bool found = false;
    for (int d = 0; d < 4; d++)
    {
        int nx = cx + dx[ordre[d]];
        int ny = cy + dy[ordre[d]];
        if (nx >= 0 && nx < cellW && ny >= 0 && ny < cellH && !visited[nx, ny])
        {
            grille[cx * 2 + dx[ordre[d]], cy * 2 + dy[ordre[d]]] = 0;
            grille[nx * 2, ny * 2] = 0;
            visited[nx, ny] = true;
            stackX[stackTop] = nx;
            stackY[stackTop] = ny;
            stackTop++;
            found = true;
            break;
        }
    }
    if (!found) stackTop--;
}

// ── Position joueur et sortie ──
int joueurX = 0, joueurY = 0;
int sortieX = (cellW - 1) * 2;
int sortieY = (cellH - 1) * 2;

grille[joueurX, joueurY] = 2;
grille[sortieX, sortieY] = 3;

// ── Dessin initial complet (une seule fois) ──
Console.Clear();
Console.CursorVisible = false;

Console.SetCursorPosition(0, 0);
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║          🏃 LABYRINTHE ASCII  C#  🏃             ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.ResetColor();

for (int y = 0; y < hauteur; y++)
{
    for (int x = 0; x < largeur; x++)
    {
        Console.SetCursorPosition(offsetX + x, offsetY + y);
        int cell = grille[x, y];
        if (cell == 1)      { Console.ForegroundColor = ConsoleColor.DarkGray;  Console.Write("█"); }
        else if (cell == 2) { Console.ForegroundColor = ConsoleColor.Yellow;    Console.Write("@"); }
        else if (cell == 3) { Console.ForegroundColor = ConsoleColor.Green;     Console.Write("★"); }
        else                { Console.ForegroundColor = ConsoleColor.DarkBlue;  Console.Write("·"); }
    }
}

Console.SetCursorPosition(0, offsetY + hauteur + 1);
Console.ForegroundColor = ConsoleColor.DarkCyan;
Console.Write("  [Z/↑] Haut   [S/↓] Bas   [Q/←] Gauche   [D/→] Droite   [Échap] Quitter");
Console.ResetColor();

// ── Action locale : redessiner UNE seule cellule via SetCursorPosition ──
void DessinerCellule(int cx, int cy)
{
    Console.SetCursorPosition(offsetX + cx, offsetY + cy);
    int cell = grille[cx, cy];
    if (cell == 1)      { Console.ForegroundColor = ConsoleColor.DarkGray;  Console.Write("█"); }
    else if (cell == 2) { Console.ForegroundColor = ConsoleColor.Yellow;    Console.Write("@"); }
    else if (cell == 3) { Console.ForegroundColor = ConsoleColor.Green;     Console.Write("★"); }
    else                { Console.ForegroundColor = ConsoleColor.DarkBlue;  Console.Write("·"); }
    Console.ResetColor();
}

// ── Boucle de jeu ──
bool gagné = false;

while (!gagné)
{
    ConsoleKey touche = Console.ReadKey(true).Key;

    int nx2 = joueurX;
    int ny2 = joueurY;

    if      (touche == ConsoleKey.Z || touche == ConsoleKey.UpArrow)    ny2--;
    else if (touche == ConsoleKey.S || touche == ConsoleKey.DownArrow)  ny2++;
    else if (touche == ConsoleKey.Q || touche == ConsoleKey.LeftArrow)  nx2--;
    else if (touche == ConsoleKey.D || touche == ConsoleKey.RightArrow) nx2++;
    else if (touche == ConsoleKey.Escape) break;

    if (nx2 >= 0 && nx2 < largeur && ny2 >= 0 && ny2 < hauteur && grille[nx2, ny2] != 1)
    {
        if (grille[nx2, ny2] == 3) gagné = true;

        // ✅ Efface l'ancienne position (couloir) → 1 seule case redessinée
        grille[joueurX, joueurY] = 0;
        DessinerCellule(joueurX, joueurY);

        // ✅ Dessine la nouvelle position → 1 seule case redessinée
        joueurX = nx2;
        joueurY = ny2;
        grille[joueurX, joueurY] = 2;
        DessinerCellule(joueurX, joueurY);
    }
}

// ── Écran de victoire ──
Console.SetCursorPosition(0, offsetY + hauteur + 3);
if (gagné)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  ╔════════════════════════════════╗");
    Console.WriteLine("  ║   🎉  FÉLICITATIONS !  🎉      ║");
    Console.WriteLine("  ║   Vous avez trouvé la sortie ! ║");
    Console.WriteLine("  ╚════════════════════════════════╝");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n  Partie abandonnée. À bientôt !");
    Console.ResetColor();
}

Console.SetCursorPosition(0, offsetY + hauteur + 8);
Console.WriteLine("  Appuyez sur une touche pour quitter...");
Console.CursorVisible = true;
Console.ReadKey(true);
