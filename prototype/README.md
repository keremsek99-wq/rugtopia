# Rugtopia: Weave the World — HTML5 prototype v3.1 (Weft Path core rule)

Live: https://ilmek-prototype.ilmek-oyun.workers.dev/ (or open `index.html` offline). Portrait, touch or mouse. EN by default; EN/TR toggle top-right on the menu.

## What's new in v2.1 (UI/UX pass)
- Colour-blind-aware palette (madder, saffron, indigo, cream, walnut, sky + apricot, teal) and a symbol per colour (diamond, star, circle, cross, square, moon, triangle, hexagon) on every spool; the active row always shows symbols, and the menu toggle "Rug symbols" adds them to every rug cell. Lighter carpet backing.
- Belt tension: counter turns amber with a "Last slot!" label and a pulsing ring on the last free slot (plus a warning sound + vibration once), red when full.
- Bigger touch targets: front lane spools up to 28 px radius, deeper spools larger, 46x46 HUD buttons.
- Simplified HUD: "Level 21 · HARD · Row 3/8"; the level name fades out after ~2.5 s; coins/progress bar removed from the play HUD.
- Near-miss fail screen: "So close! / Belt jammed" + "N stitches left", mini rug preview with the missing stitches outlined, one green primary Continue (price on the button), neutral Retry, X to give up, "Win streak N → 0", "Next continue: X".
- Win sequence: gold frame + sparkles around the finished rug, stars pop in one by one with rising tones, coin count-up with coins flying to a counter pill, tap to skip.
- Juice: thread glow, row-complete light sweep, pitch ladder per row.

## What's new in v3.1
- Debug → "Reset economy": coins, booster inventory and lives back to the defaults, and the payer, No Ads, Rug Pass and offer flags cleared; level progress, stars and analytics are kept. Use it before a playtest, so that fake shop purchases (for example the Starter pack + coinXXL) don't skew booster use.
- Level 5 no longer has a 2-slot belt (belt 3). Only the bonus level 20 still uses 2 slots.
- Normal levels 11+ have a bit more bite: retuned to 19–24% skilled-bot fail (was 11–21%), still inside the 10–25% band. Hard 15/19/25, very hard 28 and the boss are unchanged until they are playtested.
- Time model recalibrated from Kerem's v3 win times (L1–12: 5–23 s). Weaving overlaps with tapping, so time is now per tap: about 0.6 s/tap at Kerem's pace and 1.0 s/tap for an average new player (estimate), plus 2 s.
- The save key is unchanged (`rugtopia_save_v3`), so v3 progress carries over.

## Core rules (v3: Weft Path / Atkı Yolu)
- Spools wait in lanes. Tap a lane to send its front spool onto the looping belt (limited slots).
- The rug is woven bottom-up by a shuttle, one stitch at a time, boustrophedon: the bottom row left→right, the next row right→left, and so on (gold arrow = direction).
- Only the glowing stitch at the shuttle tip can be woven. A belt spool of that colour fires; every other spool waits on the belt and keeps its slot. Empty spools leave.
- The NEXT strip under the rug shows the next colour runs (colour, symbol, ×count, ↩ = direction flip); it never reveals a still-covered row.
- Win: whole rug woven. Fail: belt full and no spool matches the shuttle tip.
- `?variant=weft` (the old 5-level test) now simply opens the main game. Save key is new (`rugtopia_save_v3`), so progress starts fresh.

## What's new in v3
- All 30 levels redesigned for colour ORDER: bigger rugs (up to 10x11, boss 12x14), new order-heavy patterns (vertical stripes, diagonal bands, steps), lower spool capacities, tighter belts (2–6 slots), more shuffle. Tags unchanged (Hard 15/19/25, Very hard 28, Bonus 20, Boss 30).
- New "skilled player" bot for calibration: plans 2 taps ahead, reads the NEXT strip (12 stitches, not into covered rows), 15% random slips. Cross-check: on Kerem's 5 old variant levels (all won first try) it fails 0–2.8%, matching his playtest. Expert (0% slips) and sloppy (30%) rates are in the report too.
- FTUE for the new rule: L1 hand pointer + "tap the spool that matches the glowing stitch" → "the shuttle weaves one stitch at a time ▶" → "at the row end it flips ◀"; L2 "only the shuttle tip can be woven / other colours wait and use a slot"; L3 "read the NEXT strip".
- Boosters re-checked: +1 Slot (more parking room — more valuable now), Comb (pick any of the top 4 spools in a lane — fixes order problems), Shuttle now weaves the next 5 stitches along the shuttle path regardless of colour (takes the matching threads from the spools furthest back, so the thread budget stays exact).
- Knots: a knotted spool still unties after one more row is finished.
- Difficulty report has time-to-win estimates (see v3.1 for the current time model).

## What's in v2
- FTUE: levels 1–3 are guided (hand + forced taps on L1–2, hints on L3), cost no lives, boosters hidden.
- 30 levels (Anatolia region), sawtooth tags: Hard 15/19/25/28, Bonus 20, Boss rug 30 (12x14). Tag pills on the map + HUD, banner on entry.
- Obstacles: hidden spool "?" (L6+), covered rows (L12+, revealed when the row becomes active), knotted spool (L21+; fires only after one more row is woven).
- Boosters: +1 Slot (L10, 600 coins, 3 free), Comb — take any of the top 4 spools in a lane (L18, 700, 2 free), Shuttle — weaves 5 stitches of the active row (L25, 900, 2 free).
- Economy: win 40/60/80/150/100 coins (normal/hard/very hard/boss/bonus), continue 900/1500/2100 (+1 slot +1 comb), refill lives 900.
- 5 lives, 30-min refill; win streak (3+ wins in a row = free extra belt slot, from L25).
- Fake shop with all 12 IAP SKUs (Starter, Rescue S/L, No Ads, No Ads Bundle, Rug Pass, 6 coin packs) — "Simulate purchase", no real payment.
- Fake ads: rewarded x2 coins on win (L8+, 5/day), rewarded +1 life (3/day), interstitial before result from L20 (>=90 s and 2 levels apart, never first level of session, never for payers / No Ads). Offers: Starter after L12, No Ads after first interstitial, No Ads Bundle after L27.
- Local analytics (localStorage): per-level starts / wins / fails / near-miss / quits / continues / booster use / avg win time + event log. Menu -> Debug: table vs target band, Export JSON, cheats (unlock all, coins, lives, time skip, reset).

## Difficulty tooling (Node, `tools/`)
- `node tools/verify.js` — DFS proves every level solvable without boosters; skilled / expert / sloppy / casual bot fail rates vs tag band + time-to-win estimates. Report: `tools/difficulty-report.json`.
- `node tools/tune.js [levels]` — re-tunes belt/lanes/cap/shuffle/seed against the skilled bot and writes the TUNED block in game.js.
- `tools/test2.js` — headless Chrome play-through (needs puppeteer-core).
