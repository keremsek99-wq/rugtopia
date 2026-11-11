using System;
using System.Collections.Generic;

namespace Rugtopia.Core
{
    // Plain serializable DTOs. Public fields so both UnityEngine.JsonUtility and System.Text.Json (IncludeFields) can read them.
    [Serializable] public class PatternSpec { public string t; public int w; public int h; public string c; public int k; public string b; }
    [Serializable] public class SpoolData { public string c; public int n; public bool hidden; public int knot; public int seq; }
    [Serializable] public class LaneData { public List<SpoolData> spools = new List<SpoolData>(); }
    [Serializable] public class GeneratedData { public List<LaneData> lanes = new List<LaneData>(); }

    [Serializable]
    public class LevelData
    {
        public int index;
        public int id;
        public string nameEn;
        public string nameTr;
        public string tag;           // ftue | normal | hard | veryhard | boss | bonus
        public PatternSpec pattern;
        public string[] rows;        // top row first, one char per stitch colour
        public int belt;             // belt slots
        public int lanes;
        public int cap;              // max threads per spool
        public double shuffle;
        public int seed;
        public double hidden;        // share of hidden '?' spools
        public string covered;       // "", "top" or "alt"
        public int knots;
        public GeneratedData generated; // lanes as dealt by the JS prototype (parity reference)
        public int[] solution;          // one solving lane sequence found by the JS solver
    }

    [Serializable]
    public class LevelPack { public string version; public List<LevelData> levels = new List<LevelData>(); }
}
