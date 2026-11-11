using System;
using Rugtopia.Core;

namespace Rugtopia.Meta
{
    public enum Booster { Slot, Comb, Shuttle }

    /// <summary>Coins, boosters and lives on top of SaveData, using the v3.1 constants in Core/EconomyConfig.
    /// No IAP/ads yet (StoreKit / ad mediation are later milestones).</summary>
    public static class Economy
    {
        static SaveData S { get { return SaveService.Data; } }

        public static int UnlockLevel(Booster b)
        {
            switch (b) { case Booster.Slot: return EconomyConfig.UnlockSlot; case Booster.Comb: return EconomyConfig.UnlockComb; default: return EconomyConfig.UnlockShuttle; }
        }
        public static int Price(Booster b)
        {
            switch (b) { case Booster.Slot: return EconomyConfig.PriceSlot; case Booster.Comb: return EconomyConfig.PriceComb; default: return EconomyConfig.PriceShuttle; }
        }
        public static int Inventory(Booster b)
        {
            switch (b) { case Booster.Slot: return S.invSlot; case Booster.Comb: return S.invComb; default: return S.invShuttle; }
        }
        static void AddInventory(Booster b, int n)
        {
            switch (b) { case Booster.Slot: S.invSlot += n; break; case Booster.Comb: S.invComb += n; break; default: S.invShuttle += n; break; }
        }

        public static bool IsUnlocked(Booster b, int levelIndex) { return levelIndex >= UnlockLevel(b); }
        public static bool CanAfford(Booster b) { return Inventory(b) > 0 || S.coins >= Price(b); }

        /// <summary>Spends one booster: inventory first, else coins. Returns false if neither.</summary>
        public static bool Consume(Booster b)
        {
            if (Inventory(b) > 0) AddInventory(b, -1);
            else if (S.coins >= Price(b)) S.coins -= Price(b);
            else return false;
            SaveService.Save();
            return true;
        }

        /// <summary>First time a booster unlocks, the player gets free ones (prototype FREE table).</summary>
        public static void GrantIntroBooster(Booster b)
        {
            string key = b.ToString();
            if (S.seenIntros.Contains(key)) return;
            S.seenIntros.Add(key);
            AddInventory(b, b == Booster.Slot ? EconomyConfig.FreeSlot : b == Booster.Comb ? EconomyConfig.FreeComb : EconomyConfig.FreeShuttle);
            SaveService.Save();
        }

        public static bool FreeContinueToday()
        {
            return S.freeContinueDay != DateTime.UtcNow.ToString("yyyyMMdd");
        }

        public static int ContinueCost()
        {
            return FreeContinueToday() ? 0 : EconomyConfig.ContinuePrice(0);
        }

        /// <summary>Interstitial stub: level 30 or later, and never on the first calendar day.</summary>
        public static bool AllowInterstitial(int levelIndex)
        {
            if (levelIndex < EconomyConfig.UnlockInterstitial) return false;
            if (S.firstPlayUtc == 0) return false;
            var first = new DateTime(S.firstPlayUtc, DateTimeKind.Utc).Date;
            return first != DateTime.UtcNow.Date;
        }

        public static bool TryContinue(GameState g)
        {
            bool free = FreeContinueToday();
            int price = free ? 0 : EconomyConfig.ContinuePrice(g.Continues);
            if (!free && S.coins < price) return false;
            if (!g.Continue()) return false;
            if (free) S.freeContinueDay = DateTime.UtcNow.ToString("yyyyMMdd");
            else { S.coins -= price; S.invComb += 1; }
            SaveService.Save();
            return true;
        }

        public static int OnWin(int levelIndex, string tag, float seconds)
        {
            int reward = EconomyConfig.Reward(tag);
            S.coins += reward;
            S.maxLevel = Math.Max(S.maxLevel, levelIndex + 1);
            S.streak++;
            while (S.bestSeconds.Count <= levelIndex) S.bestSeconds.Add(0);
            if (S.bestSeconds[levelIndex] <= 0 || seconds < S.bestSeconds[levelIndex]) S.bestSeconds[levelIndex] = seconds;
            SaveService.Save();
            return reward;
        }

        public static void OnFailExit(int levelIndex)
        {
            S.streak = 0;
            if (EconomyConfig.LosesLifeOnFail(levelIndex)) LoseLife();
            SaveService.Save();
        }

        // ---- lives (5, one back every 30 min) ----
        public static int Lives { get { Tick(); return S.lives; } }
        public static bool HasLife { get { return Lives > 0; } }
        public static TimeSpan NextLifeIn
        {
            get
            {
                Tick();
                if (S.lives >= EconomyConfig.MaxLives) return TimeSpan.Zero;
                var next = new DateTime(S.livesTicksUtc, DateTimeKind.Utc) + EconomyConfig.LifeRefill;
                var d = next - DateTime.UtcNow; return d < TimeSpan.Zero ? TimeSpan.Zero : d;
            }
        }
        static void LoseLife()
        {
            Tick();
            if (S.lives >= EconomyConfig.MaxLives) S.livesTicksUtc = DateTime.UtcNow.Ticks;
            S.lives = Math.Max(0, S.lives - 1);
        }
        static void Tick()
        {
            if (S.lives >= EconomyConfig.MaxLives) return;
            var start = new DateTime(S.livesTicksUtc, DateTimeKind.Utc);
            long gained = (DateTime.UtcNow - start).Ticks / EconomyConfig.LifeRefill.Ticks;
            if (gained <= 0) return;
            S.lives = (int)Math.Min(EconomyConfig.MaxLives, S.lives + gained);
            S.livesTicksUtc = start.AddTicks(gained * EconomyConfig.LifeRefill.Ticks).Ticks;
        }
        public static bool RefillWithCoins()
        {
            if (S.coins < EconomyConfig.PriceRefill) return false;
            S.coins -= EconomyConfig.PriceRefill; S.lives = EconomyConfig.MaxLives; SaveService.Save(); return true;
        }
    }
}
