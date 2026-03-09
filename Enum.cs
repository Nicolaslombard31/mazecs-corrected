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