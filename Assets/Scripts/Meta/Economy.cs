using System;
using System.Collections.Generic;
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
            Analytics.Track("streak_changed", ("to", S.streak.ToString()), ("reason", "win"));
            while (S.bestSeconds.Count <= levelIndex) S.bestSeconds.Add(0);
            if (S.bestSeconds[levelIndex] <= 0 || seconds < S.bestSeconds[levelIndex]) S.bestSeconds[levelIndex] = seconds;
            SaveService.Save();
            return reward;
        }

        public static void OnFailExit(int levelIndex)
        {
            BreakStreak("exit");
            if (EconomyConfig.LosesLifeOnFail(levelIndex)) LoseLife();
            SaveService.Save();
        }

        public static void BreakStreak(string reason)
        {
            int was = S.streak;
            if (was <= 0) return;
            S.streak = 0;
            SaveService.Save();
            Analytics.Track("streak_changed", ("from", was.ToString()), ("to", "0"), ("reason", reason));
        }

        public static bool DailyUnlocked { get { return S.maxLevel >= EconomyConfig.UnlockDaily; } }
        public static bool GalleryUnlocked { get { return S.maxLevel >= EconomyConfig.UnlockGallery; } }
        public static bool StreakUnlocked { get { return S.maxLevel >= EconomyConfig.UnlockStreak; } }

        public static int DailyCoin(int day)
        {
            int[] table = { 40, 70, 100, 140, 200, 280, 100 };
            if (day < 0) day = 0;
            if (day > 6) day = 6;
            return table[day];
        }

        public static bool DailyClaimedToday()
        {
            return S.dailyLast == Day(0);
        }

        /// <summary>The day index that can be claimed now, or -1 when today is already claimed. A missed day returns 0.</summary>
        public static int DailyOffer()
        {
            if (S.dailyLast == Day(0)) return -1;
            if (S.dailyLast != Day(-1)) S.dailyDay = 0;
            if (S.dailyDay < 0 || S.dailyDay > 6) S.dailyDay = 0;
            return S.dailyDay;
        }

        public static void ClaimDaily()
        {
            int day = DailyOffer();
            if (day < 0) return;
            S.coins += DailyCoin(day);
            if (day == 6) { S.invSlot += 1; S.invComb += 1; S.invShuttle += 1; }
            S.dailyLast = Day(0);
            S.dailyDay = (day + 1) % 7;
            SaveService.Save();
            Analytics.Track("daily_claim", ("day", (day + 1).ToString()));
        }

        public static bool IsHung(int levelIndex)
        {
            return S.hung != null && S.hung.Contains(levelIndex);
        }

        public static void Hang(int levelIndex)
        {
            if (S.hung == null) S.hung = new List<int>();
            if (!S.hung.Contains(levelIndex)) S.hung.Add(levelIndex);
            SaveService.Save();
            Analytics.Track("gallery_hang", ("level", (levelIndex + 1).ToString()));
        }

        static string Day(int offset) { return DateTime.UtcNow.AddDays(offset).ToString("yyyyMMdd"); }

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
