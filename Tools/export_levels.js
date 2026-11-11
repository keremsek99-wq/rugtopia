// Regenerates Assets/Data/Levels/*.json, Assets/Resources/levels.json and CoreTests/Fixtures/parity.json
// from the HTML5 prototype (single source of truth for level data while the Unity port is young).
// Usage: node Tools/export_levels.js [path/to/game.js]
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const GAME = process.argv[2] || path.join(ROOT, 'prototype/game.js');
const R = require(path.join(__dirname, 'stub.js'))(GAME);
const src = fs.readFileSync(GAME, 'utf8');
const ver = (src.match(/version:'(v[\d.]+)/) || [])[1] || 'v3.1';

function snapshot(m) {
  return { hp: m.hp, active: m.active, belt: m.belt.map(s => s.c + s.n + (s.k ? 'k' + s.k : '')).join(','),
    lanes: m.lanes.map(l => l.length).join(','), dead: R.mDead(m), won: m.active < 0 };
}
const all = [], parity = { source: path.basename(GAME), rng: [], levels: [] };
// raw RNG parity samples
for (const seed of [0, 1, 1000, 1000 + 7919 * 3 + 104729 * 2, 77]) { const r = R.rng(seed); const a = []; for (let i = 0; i < 6; i++) a.push(r()); parity.rng.push({ seed, values: a }); }

R.LEVELS.forEach((d, i) => {
  const L = R.buildLevel(i);
  const sol = R.solveLevel(i, null, 400000);
  const lvl = {
    index: i, id: i + 1, nameEn: d.name.en, nameTr: d.name.tr, tag: d.tag,
    pattern: { t: d.spec.t, w: d.spec.w, h: d.spec.h, c: d.spec.c, k: d.spec.k || 0, b: d.spec.b || '' },
    rows: d.p.slice(), belt: d.belt, lanes: d.lanes, cap: d.cap, shuffle: d.shuffle, seed: d.seed,
    hidden: d.hidden, covered: d.covered || '', knots: d.knots,
    generated: { lanes: L.lanes.map(l => ({ spools: l.map(s => ({ c: s.color, n: s.left, hidden: !!s.hidden, knot: s.knot || 0, seq: s.seq })) })) },
    solution: sol.path || []
  };
  all.push(lvl);
  fs.writeFileSync(path.join(ROOT, 'Assets/Data/Levels', 'level_' + String(i + 1).padStart(2, '0') + '.json'), JSON.stringify(lvl, null, 1));
  // parity: replay the JS solver path and a few seeded random sequences through the JS instant model
  const seqs = [{ name: 'solution', moves: sol.path || [] }];
  const rr = R.rng(5150 + i);
  for (let k = 0; k < 2; k++) {
    const m = R.mClone(R.mFromLevel(L)); R.mSettle(m); const mv = [];
    while (!R.mDead(m) && m.active >= 0) { const ne = []; m.lanes.forEach((l, j) => { if (l.length) ne.push(j); }); const j = ne[Math.floor(rr() * ne.length)]; mv.push(j); R.mTake(m, j); }
    seqs.push({ name: 'random' + k, moves: mv });
  }
  const recs = seqs.map(sq => {
    const m = R.mClone(R.mFromLevel(L)); R.mSettle(m); const steps = [snapshot(m)];
    for (const j of sq.moves) { R.mTake(m, j); steps.push(snapshot(m)); }
    return { name: sq.name, moves: sq.moves, steps };
  });
  const bot = R.botSkilledStats(i, null, 200);
  parity.levels.push({ index: i, solverNodes: sol.nodes, botRuns: 200, botFail: bot.fail, botTaps: bot.taps, sequences: recs });
});
fs.writeFileSync(path.join(ROOT, 'Assets/Resources/levels.json'), JSON.stringify({ version: ver, levels: all }));
fs.writeFileSync(path.join(ROOT, 'CoreTests/Fixtures/parity.json'), JSON.stringify(parity));
console.log('exported', all.length, 'levels; parity sequences', parity.levels.reduce((a, l) => a + l.sequences.length, 0));

// ---- UI strings (en/tr) -> CSV for Unity Localization (String Table "UI"; import via the table's CSV extension) ----
{
  const start = src.indexOf('var STR = {');
  let depth = 0, end = start + 'var STR = '.length;
  for (; end < src.length; end++) { const ch = src[end]; if (ch === '{') depth++; else if (ch === '}') { depth--; if (depth === 0) { end++; break; } } }
  const STR = (new Function('return ' + src.slice(start + 'var STR = '.length, end)))();
  const keys = Object.keys(STR.en);
  const q = v => '"' + String(v == null ? '' : v).replace(/"/g, '""').replace(/\n/g, '\\n') + '"';
  const lines = ['Key,English(en),Turkish(tr)'];
  keys.forEach(k => { if (typeof STR.en[k] === 'string') lines.push([k, STR.en[k], STR.tr && STR.tr[k]].map(q).join(',')); });
  R.LEVELS.forEach((d, i) => lines.push([q('level.' + (i + 1) + '.name'), q(d.name.en), q(d.name.tr)].join(',')));
  fs.writeFileSync(path.join(ROOT, 'Assets/Localization/UI_strings.csv'), lines.join('\n') + '\n');
  console.log('strings', lines.length - 1);
}
