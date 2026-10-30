namespace SampleGame
{
    // Ordinary business code: no kingsSafe attributes, types or API calls.
    public sealed class GameState
    {
        public int Ammo;
        public GameState(int ammo) { Ammo = ammo; }
        public int Fire(int shots) { Ammo -= shots; return Score(Ammo); }
        private int Score(int remaining) { return remaining * 3 + 7; }
    }
}
