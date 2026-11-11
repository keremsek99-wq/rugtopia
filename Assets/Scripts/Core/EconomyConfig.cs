using System;

namespace Rugtopia.Core
{
    /// <summary>Economy constants copied from prototype v3.1 (game.js REWARD/PRICE/CONT/UNLOCK/FREE). Level indices are 0-based.</summary>
    public static class EconomyConfig
    {
        public const int StartCoins = 300;
        public const int PriceSlot = 600, PriceComb = 700, PriceShuttle = 900, PriceRefill = 900;
        public static readonly int[] ContinuePrices = { 900, 1500, 2100 };
        public const int UnlockSlot = 7, UnlockComb = 13, UnlockShuttle = 17, UnlockStreak = 24, UnlockRv2x = 7, UnlockInterstitial = 29;
        public const int FreeSlot = 3, FreeComb = 2, FreeShuttle = 2; // granted with the booster intro
        public const int MaxLives = 5, PassLives = 8;
        public static readonly TimeSpan LifeRefill = TimeSpan.FromMinutes(30);
        public const int StreakForBonusSlot = 3;

        public static int Reward(string tag)
        {
            switch (tag)
            {
                case "hard": return 60;
                case "veryhard": return 80;
                case "boss": return 150;
                case "bonus": return 200;
                default: return 40; // ftue, normal
            }
        }
        public static int ContinuePrice(int continuesUsed) { return ContinuePrices[Math.Min(continuesUsed, ContinuePrices.Length - 1)]; }
        public static bool LosesLifeOnFail(int levelIndex) { return levelIndex >= 15; }
    }
}
