namespace CarteTraveller.Services
{
    public static class Dice
    {
        // Lance un nombre défini de dés à 6 faces
        public static int Roll(int diceCount = 2)
        {
            int total = 0;
            for (int i = 0; i < diceCount; i++)
            {
                // Random.Shared.Next(min, max) : max est exclusif, donc 1 à 7 donne 1 à 6
                total += System.Random.Shared.Next(1, 7);
            }
            return total;
        }
    }
}
