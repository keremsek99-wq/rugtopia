/* Rugtopia (working title; formerly İlmek) — HTML5 prototype v2 (vanilla JS + canvas)
   v2: FTUE (1–3), 30 levels with sawtooth difficulty tags, hidden spools / veiled rows / knots,
   boosters (+1 Slot, Comb, Shuttle), coins, lives, win streak, fake shop + fake ads, local analytics. */
(function () {
  'use strict';

  // ---------- Colors ----------
  // v2.1 colorblind-aware palette (core 6: R Y N C K T; extended: O G; symbol-only: M L ...)
  var PAL = {
    R: '#C9343F', Y: '#F5B700', N: '#2E4A9E', C: '#F6ECD9', K: '#4B2C20', T: '#62C4F0',
    O: '#F08A3C', G: '#12A08F', M: '#F07C9A', L: '#7BB661', W: '#FFFFFF', B: '#22222A', I: '#1E2F66', U: '#9FD8F5', S: '#F7E15A'
  };
  var SYM = { R: 'diamond', Y: 'star', N: 'circle', C: 'cross', K: 'square', T: 'moon', O: 'triangle', G: 'hexagon', M: 'heart', L: 'drop', W: 'ring', B: 'bar', I: 'chevron', U: 'wave', S: 'dot' };
  function inkFor(col) { var n = parseInt(PAL[col].slice(1), 16), r = n >> 16, g = n >> 8 & 255, b = n & 255; return (0.299 * r + 0.587 * g + 0.114 * b) > 150 ? '#2B1A14' : '#FFFFFF'; }

  // ---------- i18n ----------
  var STR = {
    en: {
      title: 'Rugtopia', sub: 'WEAVE THE WORLD', note: 'Prototype v2 · original patterns inspired by weaving traditions worldwide.',
      level: 'Level', belt: 'BELT', empty: 'empty', beltFull: 'Belt is full!', done: 'Rug complete!',
      winTitle: 'Rug Woven!', next: 'Continue', levels: 'Map', play: 'Play', region: 'Anatolia · Levels 1–30',
      nextRegion: 'Next region: Andes (31+) · coming soon',
      failTitle: 'Belt Jammed', failSub: 'No spool fits the glowing row.', woven: 'woven', soClose: 'So close!', left1: 'stitch left', leftN: 'stitches left', row: 'Row', streakLost: 'Win streak', nextCont: 'Next continue', cbOn: 'Rug symbols: ON', cbOff: 'Rug symbols: OFF', lastSlot: 'Last slot!', tapSkip: 'Tap to skip',
      cont: 'Continue: +1 Slot +1 Comb', retry: 'Retry (−1 life)', retryFree: 'Retry', giveUp: 'Give up (−1 life)', giveUpFree: 'Back to map',
      notEnough: 'Not enough coins', slotAdded: '+1 slot added', coins: 'coins',
      vRegion: 'Weft Path test · 5 levels', weftRule: 'Weft Path: the shuttle weaves one stitch at a time ▶', weftRule2: 'Only the glowing stitch can be woven · rows alternate ▶ ◀', tagHard: 'HARD', tagVeryhard: 'SUPER HARD', tagBoss: 'BOSS RUG', tagBonus: 'BONUS',
      bannerHard: 'Hard level!', bannerVeryhard: 'Super hard level!', bannerBoss: 'Boss rug!', bannerBonus: 'Bonus level!',
      bSlot: '+1 Slot', bComb: 'Comb', bShuttle: 'Shuttle', locked: 'Lv', combMode: 'Comb: tap any spool in a lane',
      shuttleDone: 'Shuttle wove 5 stitches', noCells: 'Nothing to weave',
      x2: 'x2 coins (watch ad)', x2Done: 'Coins doubled!', streak: 'Win streak', streakBonus: 'Streak bonus: +1 slot!',
      lives: 'Lives', full: 'FULL', outLives: 'Out of lives', outLivesSub: 'Next life in', refill: 'Refill 5 lives', adLife: '+1 life (watch ad)',
      shop: 'Shop', close: 'Close', debug: 'Debug', protoBuy: 'Prototype: no real payment.', simBuy: 'Simulate purchase (test)', cancel: 'Cancel',
      bought: 'Simulated purchase', adTitle: 'Ad (simulated)', adSub: 'Placement', adClose: 'Close ad', adWait: 'Ad ends in',
      noAdsTitle: 'Tired of ads?', noAdsSub: 'Remove interstitial ads forever. Rewarded ads stay optional.',
      starterTitle: 'Starter Pack', starterSub: '2,000 coins + 1 of each booster + 30 min unlimited lives',
      quitTitle: 'Leave level?', quitSub: 'You will lose a life.', quit: 'Leave', stay: 'Keep weaving',
      ok: 'Got it!', newMech: 'New!', newBooster: 'New booster!', free: 'free',
      tut1: 'Tap the spool that matches the glowing stitch!', tut2: 'The shuttle weaves one stitch at a time ▶', tut3: 'At the row end the shuttle flips direction ◀', nextStrip: 'NEXT',
      tut4: 'Only the glowing stitch at the shuttle tip can be woven.', tut5: 'Other colours wait on the belt and use a slot!', tut6: 'Read the NEXT strip: queue the colours in order.',
      tutDone: 'You are a weaver now!', infLives: 'Unlimited', passOn: 'Rug Pass active: max 8 lives', lockedLevel: 'Locked',
      day: 'today', perDay: '/day', limitReached: 'Daily ad limit reached'
    },
    tr: {
      title: 'Rugtopia', sub: 'DÜNYAYI DOKU', note: 'Prototip v2 · dünya dokuma geleneklerinden esinlenen özgün desenler.',
      level: 'Bölüm', belt: 'KONVEYÖR', empty: 'boş', beltFull: 'Konveyör dolu!', done: 'Halı tamam!',
      winTitle: 'Halı Dokundu!', next: 'Devam', levels: 'Harita', play: 'Oyna', region: 'Anadolu · Bölüm 1–30',
      nextRegion: 'Sonraki bölge: And Dağları (31+) · yakında',
      failTitle: 'Konveyör Tıkandı', failSub: 'Parlayan satıra uyan makara yok.', woven: 'dokundu', soClose: 'Az kaldı!', left1: 'ilmek kaldı', leftN: 'ilmek kaldı', row: 'Sıra', streakLost: 'Galibiyet serisi', nextCont: 'Sonraki devam', cbOn: 'Halı sembolleri: AÇIK', cbOff: 'Halı sembolleri: KAPALI', lastSlot: 'Son yuva!', tapSkip: 'Geçmek için dokun',
      cont: 'Devam: +1 Yuva +1 Tarak', retry: 'Tekrar (−1 can)', retryFree: 'Tekrar', giveUp: 'Vazgeç (−1 can)', giveUpFree: 'Haritaya dön',
      notEnough: 'Coin yetmiyor', slotAdded: '+1 yuva eklendi', coins: 'coin',
      weftRule: 'Atkı Yolu: mekik ilmekleri sırayla tek tek dokur ▶', weftRule2: 'Sadece parlayan ilmek dokunur · sıralar yön değiştirir ▶ ◀', vRegion: 'Atkı Yolu testi · 5 bölüm', tagHard: 'ZOR', tagVeryhard: 'ÇOK ZOR', tagBoss: 'BOSS HALI', tagBonus: 'BONUS',
      bannerHard: 'Zor bölüm!', bannerVeryhard: 'Çok zor bölüm!', bannerBoss: 'Boss halı!', bannerBonus: 'Bonus bölüm!',
      bSlot: '+1 Yuva', bComb: 'Tarak', bShuttle: 'Mekik', locked: 'Bl', combMode: 'Tarak: şeritten bir makaraya dokun',
      shuttleDone: 'Mekik 5 ilmek dokudu', noCells: 'Dokunacak ilmek yok',
      x2: 'x2 coin (reklam izle)', x2Done: 'Coinler ikiye katlandı!', streak: 'Kazanma serisi', streakBonus: 'Seri bonusu: +1 yuva!',
      lives: 'Can', full: 'DOLU', outLives: 'Canın bitti', outLivesSub: 'Sonraki can', refill: '5 canı doldur', adLife: '+1 can (reklam izle)',
      shop: 'Mağaza', close: 'Kapat', debug: 'Debug', protoBuy: 'Prototip: gerçek ödeme yok.', simBuy: 'Satın almayı simüle et (test)', cancel: 'Vazgeç',
      bought: 'Simüle satın alma', adTitle: 'Reklam (simülasyon)', adSub: 'Yerleşim', adClose: 'Reklamı kapat', adWait: 'Reklam bitiyor',
      noAdsTitle: 'Reklamlardan sıkıldın mı?', noAdsSub: 'Geçiş reklamlarını kalıcı kaldır. Ödüllü reklamlar isteğe bağlı kalır.',
      starterTitle: 'Başlangıç Paketi', starterSub: '2.000 coin + her booster\'dan 1 + 30 dk sınırsız can',
      quitTitle: 'Bölümden çık?', quitSub: 'Bir can kaybedeceksin.', quit: 'Çık', stay: 'Dokumaya devam',
      ok: 'Anladım!', newMech: 'Yeni!', newBooster: 'Yeni booster!', free: 'bedava',
      tut1: 'Parlayan ilmeğin rengindeki makaraya dokun!', tut2: 'Mekik ilmekleri tek tek dokur ▶', tut3: 'Sıra bitince mekik yön değiştirir ◀', nextStrip: 'SIRADA',
      tut4: 'Sadece mekik ucundaki parlayan ilmek dokunur.', tut5: 'Diğer renkler konveyörde bekler ve yuva kaplar!', tut6: 'SIRADA şeridini oku: renkleri doğru sırayla diz.',
      tutDone: 'Artık bir dokumacısın!', infLives: 'Sınırsız', passOn: 'Halı Pass aktif: en fazla 8 can', lockedLevel: 'Kilitli',
      day: 'bugün', perDay: '/gün', limitReached: 'Günlük reklam sınırı doldu'
    }
  };
  var MECH = {
    hidden: { en: ['Hidden spool', 'Its color shows only when it reaches the front of its lane.'], tr: ['Gizli makara', 'Rengi ancak şeridin önüne gelince görünür.'] },
    covered: { en: ['Veiled row', 'You cannot see a veiled row\'s colors until it starts glowing.'], tr: ['Örtülü satır', 'Örtülü satırın renkleri, satır parlamaya başlayana kadar görünmez.'] },
    knot: { en: ['Knotted spool', 'A knotted spool on the belt fires only after one more row is woven.'], tr: ['Düğümlü makara', 'Konveyördeki düğümlü makara, bir satır daha dokununca ip atar.'] },
    slot: { en: ['+1 Slot', 'Adds one belt slot for this level. 3 free to start!'], tr: ['+1 Yuva', 'Bu bölüm için konveyöre bir yuva ekler. 3 tane bedava!'] },
    comb: { en: ['Comb', 'Pull any spool from any lane, even from the back. 2 free!'], tr: ['Tarak', 'Herhangi bir şeritten, arkadaki bir makarayı bile çek. 2 tane bedava!'] },
    shuttle: { en: ['Shuttle', 'Instantly weaves 5 stitches of the glowing row. 2 free!'], tr: ['Mekik', 'Parlayan satırın 5 ilmeğini anında dokur. 2 tane bedava!'] },
    streak: { en: ['Win streak', 'Win 3 levels in a row without losing a life: your next level starts with +1 slot.'], tr: ['Kazanma serisi', 'Can kaybetmeden arka arkaya 3 bölüm kazan: sonraki bölüm +1 yuvayla başlar.'] }
  };
  var lang = 'en';
  function T(k) { return (STR[lang] && STR[lang][k]) || STR.en[k] || k; }

  // ---------- Economy (per plan Page; all values are estimates to tune) ----------
  var REWARD = { ftue: 40, normal: 40, hard: 60, veryhard: 80, boss: 150, bonus: 200 };
  var PRICE = { slot: 600, comb: 700, shuttle: 900, refill: 900 };
  var CONT = [900, 1500, 2100];
  var UNLOCK = { slot: 9, comb: 17, shuttle: 24, streak: 24, rv2x: 7, inter: 19 }; // 0-based level index
  var FREE = { slot: 3, comb: 2, shuttle: 2 };
  var REFILL_MS = 30 * 60 * 1000, MAX_LIVES = 5, PASS_LIVES = 8;
  var AD = { interMinGapMs: 90 * 1000, interMinLevels: 2, rv2xPerDay: 5, rvLifePerDay: 3 };
  var SKUS = [
    { id: 'starter', en: 'Starter Pack', tr: 'Başlangıç Paketi', price: 1.99, coins: 2000, inv: { slot: 1, comb: 1, shuttle: 1 }, infMin: 30, oneTime: true },
    { id: 'rescueS', en: 'Rescue Pack S', tr: 'Kurtarma Paketi S', price: 2.99, coins: 900, inv: { comb: 1 } },
    { id: 'rescueL', en: 'Rescue Pack L', tr: 'Kurtarma Paketi L', price: 6.99, coins: 3900, inv: { slot: 1, shuttle: 1 } },
    { id: 'noads', en: 'No Ads', tr: 'Reklamsız', price: 7.99, noAds: true },
    { id: 'noadsBundle', en: 'No Ads Bundle', tr: 'Reklamsız Paket', price: 12.99, noAds: true, coins: 3000, inv: { slot: 1, comb: 1, shuttle: 1 } },
    { id: 'pass', en: 'Rug Pass (4 weeks)', tr: 'Halı Pass (4 hafta)', price: 9.99, pass: true },
    { id: 'coinS', en: '1,000 coins', tr: '1.000 coin', price: 1.99, coins: 1000 },
    { id: 'coinM', en: '2,800 coins', tr: '2.800 coin', price: 4.99, coins: 2800 },
    { id: 'coinL', en: '6,000 coins', tr: '6.000 coin', price: 9.99, coins: 6000 },
    { id: 'coinXL', en: '13,000 coins', tr: '13.000 coin', price: 19.99, coins: 13000 },
    { id: 'coinXXL', en: '35,000 coins', tr: '35.000 coin', price: 49.99, coins: 35000 },
    { id: 'coinMega', en: '75,000 coins', tr: '75.000 coin', price: 99.99, coins: 75000 }
  ];

  // ---------- Patterns (original generic geometry) ----------
  function grid(w, h, f) { var out = [], x, y, s; for (y = 0; y < h; y++) { s = ''; for (x = 0; x < w; x++) s += f(x, y); out.push(s); } return out; }
  function pattern(sp) {
    var w = sp.w, h = sp.h, c = sp.c.split(''), cx = (w - 1) / 2, cy = (h - 1) / 2, R = Math.min(cx, cy) + 0.5, f;
    function at(i) { return c[Math.min(i, c.length - 1)]; }
    switch (sp.t) {
      case 'stripes': f = function (x, y) { return c[y % c.length]; }; break;
      case 'vstripes': f = function (x, y) { return c[Math.floor(x / (sp.k || 1)) % c.length]; }; break;
      case 'diag': f = function (x, y) { return c[Math.floor((x + y) / (sp.k || 1)) % c.length]; }; break;
      case 'steps': f = function (x, y) { return c[(Math.floor(x / 2) + y) % c.length]; }; break;
      case 'diamond': f = function (x, y) { var d = Math.abs(x - cx) + Math.abs(y - cy); return at(d <= R * 0.45 ? 2 : d <= R ? 1 : 0); }; break;
      case 'rings': f = function (x, y) { return c[Math.floor(Math.abs(x - cx) + Math.abs(y - cy)) % c.length]; }; break;
      case 'zigzag': f = function (x, y) { return c[Math.floor(y + Math.abs((x % 4) - 1.5)) % c.length]; }; break;
      case 'chevron': f = function (x, y) { return c[Math.floor(y + Math.abs(x - cx)) % c.length]; }; break;
      case 'checker': f = function (x, y) { return c[(Math.floor(x / 2) + Math.floor(y / 2)) % c.length]; }; break;
      case 'lattice': f = function (x, y) { return ((x + y) % 3 === 0 || (x - y + 99) % 3 === 0) ? c[1] : ((x + y) % 6 === 3 ? at(2) : c[0]); }; break;
      case 'hourglass': f = function (x, y) { var dx = Math.abs(x - cx), dy = Math.abs(y - cy); return dx <= dy * 0.9 ? (dx < 0.6 ? at(2) : c[1]) : c[0]; }; break;
      case 'star': f = function (x, y) {
        var dx = Math.abs(x - cx), dy = Math.abs(y - cy);
        if (dx + dy <= R * 0.5) return at(2);
        if ((dx < 0.6 || dy < 0.6 || Math.abs(dx - dy) < 0.6) && dx + dy <= R * 1.4) return c[1];
        return (c.length > 3 && (dx + dy) > R * 1.5) ? c[3] : c[0];
      }; break;
      case 'cross': f = function (x, y) {
        var dx = Math.abs(x - cx), dy = Math.abs(y - cy);
        if (dx < 1 && dy < 1) return at(2); if (dx < 1 || dy < 1) return c[1];
        return (Math.floor(dx + dy) % 3 === 0) ? at(3) : c[0];
      }; break;
      case 'baklava': f = function (x, y) { var tx = (x % 4) - 1.5, ty = (y % 4) - 1.5; return Math.abs(tx) + Math.abs(ty) <= 1.5 ? c[1] : c[0]; }; break;
      case 'medallion': f = function (x, y) {
        var dx = Math.abs(x - cx), dy = Math.abs(y - cy), d = dx / (cx + 0.5) * R + dy / (cy + 0.5) * R;
        if (d <= R * 0.35) return at(3);
        if (d <= R * 0.7) return Math.floor(d) % 2 ? c[1] : at(2);
        if (d <= R * 0.78) return at(4);
        return ((x + y) % 4 === 0 || (x - y + 400) % 4 === 0) ? c[1] : c[0];
      }; break;
      default: throw new Error('pattern ' + sp.t);
    }
    var rows = grid(w, h, f);
    if (sp.b) rows = rows.map(function (row, y) { return row.split('').map(function (ch, x) { return (x === 0 || y === 0 || x === w - 1 || y === h - 1) ? sp.b : ch; }).join(''); });
    return rows;
  }

  // ---------- Levels ----------
  // tag: ftue | normal | hard | veryhard | boss | bonus. Tuned params (belt, lanes, cap, shuffle, seed) come from the offline tuner.
  var SRC = [
    ['First Threads', 'İlk İplikler', { t: 'vstripes', w: 4, h: 3, c: 'RC', k: 2 }, 'ftue', {}],
    ['Little Diamond', 'Küçük Elmas', { t: 'diamond', w: 5, h: 5, c: 'CR' }, 'ftue', {}],
    ['Three Bands', 'Üç Şerit', { t: 'diag', w: 6, h: 5, c: 'RCN', k: 2 }, 'ftue', {}],
    ['Baklava', 'Baklava', { t: 'diamond', w: 6, h: 6, c: 'CNR' }, 'ftue', {}],
    ['Water Path', 'Su Yolu', { t: 'zigzag', w: 7, h: 6, c: 'CT' }, 'ftue', {}],
    ['Hidden Gold', 'Saklı Altın', { t: 'diamond', w: 7, h: 7, c: 'RYC' }, 'ftue', { hidden: 0.3 }],
    ['Chevron Steps', 'Zikzak Basamak', { t: 'chevron', w: 7, h: 7, c: 'CRK' }, 'ftue', { hidden: 0.2 }],
    ['Little Star', 'Küçük Yıldız', { t: 'star', w: 8, h: 8, c: 'NYC' }, 'ftue', { hidden: 0.2 }],
    ['Arrowheads', 'Ok Uçları', { t: 'chevron', w: 8, h: 8, c: 'RCN' }, 'ftue', { hidden: 0.25 }],
    ['Framed Diamond', 'Çerçeveli Elmas', { t: 'diamond', w: 8, h: 8, c: 'CRN', b: 'K' }, 'ftue', { hidden: 0.2 }],
    ['Garden Grid', 'Bahçe Izgarası', { t: 'checker', w: 9, h: 8, c: 'GCY' }, 'normal', { hidden: 0.25 }],
    ['Veiled Diamond', 'Örtülü Elmas', { t: 'rings', w: 9, h: 9, c: 'RCN' }, 'normal', { covered: 'top' }],
    ['Comb Stripes', 'Tarak Şeritleri', { t: 'vstripes', w: 9, h: 8, c: 'RYNC', k: 2 }, 'normal', { hidden: 0.25 }],
    ['Hourglass', 'Kum Saati', { t: 'hourglass', w: 9, h: 9, c: 'CRN' }, 'normal', { hidden: 0.2, covered: 'top' }],
    ['Twin Stars', 'İkiz Yıldız', { t: 'star', w: 9, h: 9, c: 'NRYC' }, 'hard', { hidden: 0.3 }],
    ['Lattice', 'Kafes', { t: 'lattice', w: 9, h: 9, c: 'CTR' }, 'normal', { covered: 'alt' }],
    ['Stepped Rings', 'Basamaklı Halkalar', { t: 'steps', w: 9, h: 10, c: 'YRNC' }, 'normal', { hidden: 0.25 }],
    ['Weft Waves', 'Atkı Dalgaları', { t: 'zigzag', w: 10, h: 9, c: 'RCNY' }, 'normal', { hidden: 0.2, covered: 'top' }],
    ['Medallion', 'Madalyon', { t: 'diamond', w: 9, h: 10, c: 'CRY', b: 'N' }, 'hard', { hidden: 0.3, covered: 'alt' }],
    ['Sunny Stripes', 'Güneşli Şeritler', { t: 'stripes', w: 8, h: 6, c: 'YO' }, 'bonus', {}],
    ['Knot Lozenge', 'Düğümlü Baklava', { t: 'baklava', w: 10, h: 10, c: 'CR', b: 'N' }, 'normal', { knots: 1 }],
    ['Crossroads', 'Kavşak', { t: 'cross', w: 10, h: 10, c: 'CRNY' }, 'normal', { knots: 1, hidden: 0.2 }],
    ['Trellis', 'Çardak', { t: 'lattice', w: 10, h: 10, c: 'GCK' }, 'normal', { knots: 1, covered: 'top' }],
    ['Bullseye', 'Hedef', { t: 'rings', w: 10, h: 10, c: 'RCYN' }, 'normal', { knots: 2, hidden: 0.2 }],
    ['Shuttle Run', 'Mekik Yolu', { t: 'diag', w: 10, h: 10, c: 'NRYC', k: 2 }, 'hard', { knots: 2, hidden: 0.3 }],
    ['Mosaic', 'Mozaik', { t: 'checker', w: 10, h: 10, c: 'TCRY' }, 'normal', { knots: 1, covered: 'alt' }],
    ['Zigzag Border', 'Zikzak Kenar', { t: 'zigzag', w: 10, h: 10, c: 'CRN', b: 'K' }, 'normal', { knots: 2, hidden: 0.2 }],
    ['Grand Lattice', 'Büyük Kafes', { t: 'lattice', w: 10, h: 11, c: 'RNY' }, 'normal', { knots: 2, hidden: 0.3, covered: 'alt' }],
    ['Twilight Bands', 'Alacakaranlık', { t: 'diag', w: 10, h: 10, c: 'NTYRC', k: 3 }, 'normal', { knots: 1, hidden: 0.2 }],
    ['Anatolian Grand Rug', 'Büyük Anadolu Halısı', { t: 'medallion', w: 12, h: 14, c: 'CRNYT', b: 'K' }, 'boss', { knots: 3, hidden: 0.3, covered: 'top' }]
  ];
  /*TUNED*/var TUNED = [{"belt":3,"lanes":2,"cap":4,"shuffle":0,"seed":0},{"belt":3,"lanes":2,"cap":4,"shuffle":0,"seed":0},{"belt":3,"lanes":2,"cap":4,"shuffle":0,"seed":0},{"belt":4,"lanes":4,"cap":5,"shuffle":1,"seed":2},{"belt":3,"lanes":2,"cap":5,"shuffle":0.6,"seed":0},{"belt":3,"lanes":3,"cap":3,"shuffle":0.6,"seed":7},{"belt":4,"lanes":3,"cap":3,"shuffle":0.3,"seed":2},{"belt":4,"lanes":2,"cap":4,"shuffle":1,"seed":2},{"belt":3,"lanes":5,"cap":3,"shuffle":0.3,"seed":4},{"belt":4,"lanes":3,"cap":4,"shuffle":0.3,"seed":1},{"belt":4,"lanes":3,"cap":5,"shuffle":0.6,"seed":5},{"belt":4,"lanes":3,"cap":4,"shuffle":0.6,"seed":1},{"belt":5,"lanes":2,"cap":5,"shuffle":1,"seed":1},{"belt":4,"lanes":3,"cap":3,"shuffle":0.3,"seed":5},{"belt":4,"lanes":5,"cap":4,"shuffle":0.3,"seed":4},{"belt":3,"lanes":3,"cap":3,"shuffle":1,"seed":3},{"belt":4,"lanes":3,"cap":5,"shuffle":0.6,"seed":5},{"belt":3,"lanes":5,"cap":5,"shuffle":0.6,"seed":1},{"belt":4,"lanes":3,"cap":4,"shuffle":1,"seed":4},{"belt":2,"lanes":5,"cap":5,"shuffle":0.6,"seed":2},{"belt":4,"lanes":5,"cap":5,"shuffle":0.3,"seed":1},{"belt":5,"lanes":4,"cap":4,"shuffle":0.3,"seed":6},{"belt":3,"lanes":2,"cap":5,"shuffle":0.3,"seed":7},{"belt":6,"lanes":4,"cap":3,"shuffle":0,"seed":2},{"belt":6,"lanes":5,"cap":4,"shuffle":0,"seed":4},{"belt":5,"lanes":4,"cap":4,"shuffle":1,"seed":1},{"belt":6,"lanes":5,"cap":4,"shuffle":0,"seed":4},{"belt":4,"lanes":4,"cap":3,"shuffle":0,"seed":6},{"belt":6,"lanes":5,"cap":3,"shuffle":0,"seed":2},{"belt":6,"lanes":4,"cap":4,"shuffle":1,"seed":2}];/*END*/
  var DEFAULT_TUNE = function (i) { return { belt: i < 3 ? 3 : i < 20 ? 4 : 5, lanes: i < 3 ? 2 : i < 10 ? 3 : 4, cap: i < 10 ? 5 : 6, shuffle: 0.3, seed: 0 }; };
  var LEVELS = SRC.map(function (s, i) {
    var tu = (TUNED && TUNED[i]) || DEFAULT_TUNE(i);
    return { name: { en: s[0], tr: s[1] }, region: { en: 'Anatolia', tr: 'Anadolu' }, spec: s[2], p: pattern(s[2]), tag: s[3],
      hidden: s[4].hidden || 0, covered: s[4].covered || 0, knots: s[4].knots || 0, weft: true,
      belt: tu.belt, lanes: tu.lanes, cap: tu.cap, shuffle: tu.shuffle, seed: tu.seed };
  });
  // ---------- Signature-rule variant: ?variant=weft ("Weft Path" / "Atkı Yolu") ----------
  // Each row is woven in ONE shuttle pass: bottom row left→right, next row right→left, ...
  // Only the stitch under the shuttle head can be woven, so colour ORDER matters, not just colour counts.
  var VARIANT = '';
  try { VARIANT = window.__VARIANT || ((location.search.match(/[?&]variant=(\w+)/) || [])[1] || ''); } catch (e) {}
  VARIANT = ''; // v3: Weft Path is the core rule; ?variant=weft simply loads the main game
  var VSRC = [
    ['Weft Path I', 'Atkı Yolu I', { t: 'stripes', w: 6, h: 5, c: 'RC' }, 'ftue'],
    ['Weft Path II', 'Atkı Yolu II', { t: 'diamond', w: 6, h: 6, c: 'CNR' }, 'normal'],
    ['Weft Path III', 'Atkı Yolu III', { t: 'chevron', w: 7, h: 6, c: 'CRK' }, 'normal'],
    ['Weft Path IV', 'Atkı Yolu IV', { t: 'checker', w: 7, h: 7, c: 'GCY' }, 'hard'],
    ['Weft Path V', 'Atkı Yolu V', { t: 'star', w: 7, h: 7, c: 'NYC' }, 'normal']
  ];
  /*VTUNED*/var VTUNED = [{"belt":3,"lanes":4,"cap":4,"shuffle":0.2,"seed":1},{"belt":3,"lanes":2,"cap":3,"shuffle":0.2,"seed":2},{"belt":4,"lanes":3,"cap":5,"shuffle":0.2,"seed":1},{"belt":4,"lanes":4,"cap":3,"shuffle":0.4,"seed":0},{"belt":4,"lanes":3,"cap":4,"shuffle":0.2,"seed":1}];/*VEND*/
  if (VARIANT) {
    LEVELS = VSRC.map(function (s, i) {
      var tu = VTUNED[i] || { belt: 4, lanes: 3, cap: 4, shuffle: 0.2, seed: 0 };
      return { name: { en: s[0], tr: s[1] }, region: { en: 'Weft Path test', tr: 'Atkı Yolu testi' }, spec: s[2], p: pattern(s[2]), tag: s[3], weft: true,
        hidden: 0, covered: 0, knots: 0, belt: tu.belt, lanes: tu.lanes, cap: tu.cap, shuffle: tu.shuffle, seed: tu.seed };
    });
  }
  function weftPath(rows, cols) { // [r,c] in shuttle order
    var out = [];
    for (var k = 0; k < rows; k++) { var r = rows - 1 - k; for (var j = 0; j < cols; j++) out.push([r, k % 2 === 0 ? j : cols - 1 - j]); }
    return out;
  }
  var TARGET = { ftue: [0, 10], normal: [10, 25], hard: [40, 55], veryhard: [55, 70], boss: [60, 75], bonus: [0, 5] };

  // ---------- Helpers ----------
  function rng(seed) { // mulberry32
    return function () {
      seed |= 0; seed = seed + 0x6D2B79F5 | 0;
      var t = Math.imul(seed ^ seed >>> 15, 1 | seed);
      t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
      return ((t ^ t >>> 14) >>> 0) / 4294967296;
    };
  }
  function lerp(a, b, t) { return a + (b - a) * t; }
  function clamp(v, a, b) { return v < a ? a : v > b ? b : v; }
  function easeOutBack(t) { var c1 = 1.70158, c3 = c1 + 1; return 1 + c3 * Math.pow(t - 1, 3) + c1 * Math.pow(t - 1, 2); }
  function shade(hex, amt) {
    var n = parseInt(hex.slice(1), 16), r = n >> 16, g = n >> 8 & 255, b = n & 255;
    r = clamp(Math.round(r + amt), 0, 255); g = clamp(Math.round(g + amt), 0, 255); b = clamp(Math.round(b + amt), 0, 255);
    return 'rgb(' + r + ',' + g + ',' + b + ')';
  }
  function rgba(hex, a) { var n = parseInt(hex.slice(1), 16); return 'rgba(' + (n >> 16) + ',' + (n >> 8 & 255) + ',' + (n & 255) + ',' + a + ')'; }
  function fmtCoins(n) { return String(n).replace(/\B(?=(\d{3})+(?!\d))/g, lang === 'tr' ? '.' : ','); }
  function fmtPrice(p) { return lang === 'tr' ? p.toFixed(2).replace('.', ',') + ' $' : '$' + p.toFixed(2); }
  function mmss(ms) { var s = Math.max(0, Math.ceil(ms / 1000)), m = Math.floor(s / 60); s %= 60; return m + ':' + (s < 10 ? '0' : '') + s; }

  function isCovered(def, rows, r) {
    if (!def.covered) return false;
    if (def.covered === 'top') return r < Math.floor(rows / 2);
    if (def.covered === 'alt') return (rows - 1 - r) % 2 === 1;
    return false;
  }

  // Level build: always produces a spool sequence that empties row by row, then deals it into lanes
  function buildLevel(idx, over) {
    var def = over || LEVELS[idx];
    var rows = def.p.length, cols = def.p[0].length, r, c, i;
    var cells = [];
    for (r = 0; r < rows; r++) {
      cells.push([]);
      for (c = 0; c < cols; c++) cells[r].push({ color: def.p[r][c], state: 0, pop: 0, veil: isCovered(def, rows, r) });
    }
    var seq = [], open = {}, order = [];
    if (def.weft) order = weftPath(rows, cols); else for (r = rows - 1; r >= 0; r--) for (c = 0; c < cols; c++) order.push([r, c]);
    for (var oi = 0; oi < order.length; oi++) {
      r = order[oi][0]; c = order[oi][1];
      var col = def.p[r][c];
      if (!open[col]) { open[col] = { color: col, left: 0 }; seq.push(open[col]); }
      open[col].left++;
      if (open[col].left >= def.cap) delete open[col];
    }
    var rand = rng(1000 + idx * 7919 + (def.seed || 0) * 104729);
    var lanes = [];
    for (i = 0; i < def.lanes; i++) lanes.push([]);
    for (i = 0; i < seq.length; i++) {
      var lane = rand() < def.shuffle ? Math.floor(rand() * def.lanes) : i % def.lanes;
      var s = seq[i];
      s.seq = i; s.total = s.left; s.hidden = rand() < def.hidden; s.knot = 0; s.x = 0; s.y = 0; s.scale = 1;
      s.cool = 0; s.leaving = 0; s.pending = 0; s.kick = 0;
      lanes[lane].push(s);
    }
    var kr = rng(77 + idx * 31 + (def.seed || 0)), placed = 0, guard = 0;
    while (placed < (def.knots || 0) && guard++ < 500 && seq.length > 3) {
      var k = 2 + Math.floor(kr() * (seq.length - 2));
      if (!seq[k].knot) { seq[k].knot = 1; placed++; }
    }
    lanes.forEach(function (l) { if (l[0]) l[0].hidden = false; });
    var belt = [];
    for (i = 0; i < def.belt; i++) belt.push(null);
    return { idx: idx, def: def, rows: rows, cols: cols, cells: cells, lanes: lanes, belt: belt, active: rows - 1,
      weft: !!def.weft, wpath: def.weft ? order : null, hp: 0,
      threads: [], failT: 0, winT: 0, rowFlash: 0, flashRow: -1, shake: 0, beltPhase: 0, done: false,
      continues: 0, boostersUsed: 0, t0: 0, playT: 0, taps: 0, combMode: false, banner: 0, x2: false, ended: false };
  }

  // ---------- Instant model (shared by solver, bot simulation, tutorial hints) ----------
  function mFromLevel(L) {
    var m = { B: L.belt.length, lanes: L.lanes.map(function (l) { return l.map(function (s) { return { c: s.color, n: s.left, k: s.knot || 0 }; }); }),
      belt: [], rows: L.cells.map(function (r) { return r.map(function (c) { return c.color; }); }), active: L.rows - 1, need: null };
    if (L.weft) { m.weft = true; m.cols = L.cols; m.veil = L.cells.map(function (r) { return !!r[0].veil; }); m.seqc = L.wpath.map(function (p) { return L.cells[p[0]][p[1]].color; }); m.hp = L.hp || 0; m.active = L.rows - 1 - Math.floor(m.hp / L.cols); }
    mNeed(m); return m;
  }
  function mNeed(m) { m.need = {}; if (m.active < 0) return; if (m.weft) { if (m.hp < m.seqc.length) m.need[m.seqc[m.hp]] = 1; return; } var row = m.rows[m.active]; for (var i = 0; i < row.length; i++) m.need[row[i]] = (m.need[row[i]] || 0) + 1; }
  function mSettle(m) {
    if (m.weft) return mSettleWeft(m);
    var prog = true;
    while (prog && m.active >= 0) {
      prog = false;
      for (var i = 0; i < m.belt.length; i++) {
        var s = m.belt[i];
        if (s.k === 0 && s.n > 0 && m.need[s.c] > 0) { var q = Math.min(s.n, m.need[s.c]); s.n -= q; m.need[s.c] -= q; prog = true; }
      }
      m.belt = m.belt.filter(function (s) { return s.n > 0; });
      var done = true; for (var key in m.need) if (m.need[key] > 0) { done = false; break; }
      if (done) { m.active--; m.belt.forEach(function (s) { if (s.k > 0) s.k--; }); mNeed(m); prog = true; }
    }
  }
  function mSettleWeft(m) {
    var L2 = m.seqc.length;
    while (m.hp < L2) {
      var h = m.seqc[m.hp], s = null;
      for (var i = 0; i < m.belt.length; i++) { var b = m.belt[i]; if (b.k === 0 && b.n > 0 && b.c === h) { s = b; break; } }
      if (!s) break;
      while (s.n > 0 && m.hp < L2 && m.seqc[m.hp] === h) {
        s.n--; m.hp++;
        if (m.hp % m.cols === 0) m.belt.forEach(function (o) { if (o.k > 0) o.k--; });
      }
      m.belt = m.belt.filter(function (o) { return o.n > 0; });
    }
    m.active = m.hp >= L2 ? -1 : (m.rows.length - 1 - Math.floor(m.hp / m.cols));
    mNeed(m);
  }
  function mTake(m, i) { var s = m.lanes[i].shift(); m.belt.push({ c: s.c, n: s.n, k: s.k }); mSettle(m); }
  function mClone(m) {
    return { B: m.B, lanes: m.lanes.map(function (l) { return l.slice(); }), belt: m.belt.map(function (s) { return { c: s.c, n: s.n, k: s.k }; }),
      rows: m.rows, active: m.active, need: Object.assign({}, m.need), weft: m.weft, seqc: m.seqc, hp: m.hp, cols: m.cols, veil: m.veil };
  }
  function mKey(m) {
    var nk = Object.keys(m.need).sort().map(function (k) { return k + m.need[k]; }).join('');
    return m.lanes.map(function (l) { return l.length; }).join(',') + '|' + m.belt.map(function (s) { return s.c + s.n + (s.k ? 'k' + s.k : ''); }).join(',') + '|' + m.active + '|' + nk + (m.weft ? '|' + m.hp : '');
  }
  // what a player can see ahead: the preview strip (next PREVIEW stitches, never into a still-covered row)
  var PREVIEW = 12;
  function mVisible(m) {
    var out = [];
    if (!m.weft) return out;
    var act = m.active;
    for (var k = m.hp; k < m.seqc.length && out.length < PREVIEW; k++) {
      var r = m.rows.length - 1 - Math.floor(k / m.cols);
      if (r !== act && m.veil && m.veil[r]) break;
      out.push(m.seqc[k]);
    }
    return out;
  }
  function mDist(m, c) { if (!m.weft) return m.need[c] > 0 ? 0 : 99; for (var k = m.hp; k < m.seqc.length; k++) if (m.seqc[k] === c) return k - m.hp; return 1e6; }
  // Skilled-player bot: reads the preview strip, weighs every lane by 1-step simulation (+ a safety look 2 steps ahead),
  // and makes a random "human slip" with probability eps.
  function mDead(m) { if (m.active < 0) return false; if (m.belt.length >= m.B) return true; for (var i = 0; i < m.lanes.length; i++) if (m.lanes[i].length) return false; return true; }
  function mEval(m) {
    if (m.active < 0) return 1e5;
    if (mDead(m)) return -1e6;
    var vis = mVisible(m), e = m.hp * 3 + (m.B - m.belt.length) * 2;
    for (var i = 0; i < m.belt.length; i++) { var s = m.belt[i], ix = vis.indexOf(s.c); e += ix >= 0 ? (PREVIEW - ix) / 4 : -4; if (s.k) e -= 1.5; }
    return e;
  }
  function mLook(m, depth) { // best evaluation reachable in `depth` more taps (a player planning a few taps ahead)
    var e = mEval(m);
    if (depth <= 0 || m.active < 0 || e <= -1e5) return e;
    var best = -1e7;
    for (var i = 0; i < m.lanes.length; i++) { if (!m.lanes[i].length) continue; var c = mClone(m); mTake(c, i); var v = mLook(c, depth - 1); if (v > best) best = v; }
    return best === -1e7 ? e : Math.max(best, e > -1e5 && m.belt.length < m.B ? -1e5 : best);
  }
  var BOT_DEPTH = 2;
  function botSkilledRun(m0, rand, eps) {
    var m = mClone(m0), taps = 0; mSettle(m);
    while (m.active >= 0) {
      if (mDead(m)) return { win: false, taps: taps, hp: m.hp };
      var cands = [];
      for (var i = 0; i < m.lanes.length; i++) {
        if (!m.lanes[i].length) continue;
        var c = mClone(m); mTake(c, i);
        cands.push({ i: i, sc: mLook(c, BOT_DEPTH - 1) });
      }
      var alive = cands.filter(function (x) { return x.sc > -1e5; }), pick;
      if (alive.length && rand() < eps) pick = alive[Math.floor(rand() * alive.length)];
      else { var best = Math.max.apply(null, cands.map(function (x) { return x.sc; })), top = cands.filter(function (x) { return x.sc >= best - 0.01; }); pick = top[Math.floor(rand() * top.length)]; }
      mTake(m, pick.i); taps++;
    }
    return { win: true, taps: taps, hp: m.hp };
  }
  var BOT_EPS = 0.15;
  function botSkilledStats(idx, over, runs, eps) {
    var m0 = mFromLevel(buildLevel(idx, over)), f = 0, taps = 0, w = 0, rand = rng(9191 + idx);
    for (var k = 0; k < runs; k++) { var r = botSkilledRun(m0, rand, eps == null ? BOT_EPS : eps); if (r.win) { w++; taps += r.taps; } else f++; }
    return { fail: f / runs, taps: w ? taps / w : 0 };
  }
  function useful(m, i) { var f = m.lanes[i][0]; return !!f && f.k === 0 && m.need[f.c] > 0; }
  // Depth-first search over take orders; returns { ok, path, nodes, exhausted }
  function solveModel(m0, limit) {
    var seen = new Set(), nodes = 0, cut = false;
    limit = limit || 400000;
    function rec(m, path) {
      if (m.active < 0) return path;
      if (++nodes > limit) { cut = true; return null; }
      if (m.belt.length >= m.B) return null;
      var key = mKey(m); if (seen.has(key)) return null; seen.add(key);
      var idx = [];
      for (var i = 0; i < m.lanes.length; i++) if (m.lanes[i].length) idx.push(i);
      idx.sort(function (a, b) { return mDist(m, m.lanes[a][0].c) - mDist(m, m.lanes[b][0].c); });
      for (var j = 0; j < idx.length; j++) {
        var c = mClone(m); mTake(c, idx[j]);
        var r = rec(c, path.concat(idx[j])); if (r) return r;
        if (cut) return null;
      }
      return null;
    }
    var m = mClone(m0); mSettle(m);
    var p = rec(m, []);
    return { ok: !!p, path: p, nodes: nodes, exhausted: cut };
  }
  function solveLevel(idx, over, limit) { return solveModel(mFromLevel(buildLevel(idx, over)), limit); }
  // Semi-random "casual player" bot: with prob pSmart picks a lane whose front can fire now, else any lane
  function botRun(m0, rand, pSmart) {
    var m = mClone(m0); mSettle(m);
    while (m.active >= 0) {
      if (m.belt.length >= m.B) return false;
      var ne = [], us = [];
      for (var i = 0; i < m.lanes.length; i++) if (m.lanes[i].length) { ne.push(i); if (useful(m, i)) us.push(i); }
      if (!ne.length) return false;
      var pool = (us.length && rand() < pSmart) ? us : ne;
      mTake(m, pool[Math.floor(rand() * pool.length)]);
    }
    return true;
  }
  // New-player bot (reference for ftue/normal): no lookahead and no jam check. If a lane front matches the shuttle tip it takes it
  // with prob pHit; otherwise it takes a front whose colour is in the next `look` stitches with prob pLook; else a random lane.
  var NEW_BOT = { pHit: 0.9, look: 4, pLook: 0.5 };
  function botNewRun(m0, rand, cfg) {
    cfg = cfg || NEW_BOT;
    var m = mClone(m0), taps = 0; mSettle(m);
    while (m.active >= 0) {
      if (m.belt.length >= m.B) return { win: false, taps: taps, hp: m.hp };
      var ne = [], hit = [], soon = [], vis = mVisible(m).slice(0, cfg.look);
      for (var i = 0; i < m.lanes.length; i++) if (m.lanes[i].length) { ne.push(i); var f = m.lanes[i][0]; if (useful(m, i)) hit.push(i); else if (!f.k && vis.indexOf(f.c) >= 0) soon.push(i); }
      if (!ne.length) return { win: false, taps: taps, hp: m.hp };
      var pool = hit.length && rand() < cfg.pHit ? hit : soon.length && rand() < cfg.pLook ? soon : ne;
      mTake(m, pool[Math.floor(rand() * pool.length)]); taps++;
    }
    return { win: true, taps: taps, hp: m.hp };
  }
  function botNewStats(idx, over, runs, cfg) {
    var m0 = mFromLevel(buildLevel(idx, over)), f = 0, taps = 0, w = 0, rand = rng(7373 + idx);
    for (var k = 0; k < runs; k++) { var r = botNewRun(m0, rand, cfg); if (r.win) { w++; taps += r.taps; } else f++; }
    return { fail: f / runs, taps: w ? taps / w : 0 };
  }
  function botFailRate(idx, over, runs, pSmart) {
    var m0 = mFromLevel(buildLevel(idx, over)), f = 0, rand = rng(4242 + idx);
    for (var k = 0; k < runs; k++) if (!botRun(m0, rand, pSmart == null ? 0.6 : pSmart)) f++;
    return f / runs;
  }

  // ---------- Canvas ----------
  var cv = document.getElementById('c');
  var ctx = cv.getContext('2d');
  var LW = 400, LH = 780;
  var scale = 1, offX = 0, offY = 0, dpr = 1;
  function resize() {
    dpr = Math.min(window.devicePixelRatio || 1, 3);
    var w = window.innerWidth, h = window.innerHeight;
    cv.width = Math.round(w * dpr); cv.height = Math.round(h * dpr);
    scale = Math.min(w / LW, h / LH);
    offX = (w - LW * scale) / 2; offY = (h - LH * scale) / 2;
  }
  window.addEventListener('resize', resize);
  resize();

  // ---------- Audio ----------
  var ac = null;
  function audio() {
    if (!ac) { try { ac = new (window.AudioContext || window.webkitAudioContext)(); } catch (e) { ac = null; } }
    if (ac && ac.state === 'suspended') ac.resume();
  }
  var SCALE = [261.6, 293.7, 329.6, 392.0, 440.0, 523.3, 587.3, 659.3, 784.0, 880.0];
  function tone(freq, dur, type, vol, delay) {
    if (!ac) return;
    var t0 = ac.currentTime + (delay || 0);
    var o = ac.createOscillator(), g = ac.createGain();
    o.type = type || 'triangle'; o.frequency.value = freq;
    g.gain.setValueAtTime(0.0001, t0);
    g.gain.exponentialRampToValueAtTime(vol || 0.15, t0 + 0.01);
    g.gain.exponentialRampToValueAtTime(0.0001, t0 + dur);
    o.connect(g); g.connect(ac.destination); o.start(t0); o.stop(t0 + dur + 0.02);
  }
  function sPluck(i) { tone(SCALE[i % SCALE.length], 0.18, 'triangle', 0.08); }
  function sTap() { tone(520, 0.08, 'sine', 0.1); }
  function sRow() { tone(523.3, 0.25, 'triangle', 0.1); tone(659.3, 0.25, 'triangle', 0.1, 0.07); tone(784, 0.35, 'triangle', 0.1, 0.14); }
  function sBad() { tone(160, 0.2, 'square', 0.05); }
  function sWin() { [523.3, 659.3, 784, 1046.5].forEach(function (f, i) { tone(f, 0.4, 'triangle', 0.12, i * 0.1); }); }
  function sFail() { tone(330, 0.3, 'sawtooth', 0.05); tone(220, 0.45, 'sawtooth', 0.05, 0.2); }
  function sCoin() { tone(988, 0.08, 'square', 0.04); tone(1319, 0.12, 'square', 0.04, 0.07); }
  function sMagic() { [660, 880, 1100, 1320].forEach(function (f, i) { tone(f, 0.15, 'sine', 0.07, i * 0.05); }); }
  function buzz(ms) { if (navigator.vibrate) try { navigator.vibrate(ms); } catch (e) {} }

  // ---------- Save ----------
  var SAVE_KEY = 'rugtopia_save_v3';
  function freshSave() {
    return { v: 2, lang: 'en', best: {}, max: 0, coins: 300, inv: { slot: 0, comb: 0, shuttle: 0 }, lives: MAX_LIVES, livesTs: Date.now(),
      infUntil: 0, passUntil: 0, noAds: false, payer: false, streak: 0, seen: {}, daily: { date: '', rv2x: 0, rvLife: 0 },
      offers: {}, cb: false, lastInterTs: 0, levelsSinceInter: 99, interCount: 0, timeOffset: 0, an: { lv: {}, ev: [] } };
  }
  var save = freshSave();
  try { var loaded = JSON.parse(localStorage.getItem(SAVE_KEY)); if (loaded && loaded.v === 2) save = Object.assign(freshSave(), loaded); } catch (e) {}
  if (save.lang === 'tr' || save.lang === 'en') lang = save.lang;
  function persist() { try { localStorage.setItem(SAVE_KEY, JSON.stringify(save)); } catch (e) {} }
  function now() { return Date.now() + (save.timeOffset || 0); }
  function today() { var d = new Date(now()); return d.getFullYear() + '-' + (d.getMonth() + 1) + '-' + d.getDate(); }
  function daily() { if (save.daily.date !== today()) save.daily = { date: today(), rv2x: 0, rvLife: 0 }; return save.daily; }

  // ---------- Lives ----------
  function passActive() { return save.passUntil > now(); }
  function infActive() { return save.infUntil > now(); }
  function maxLives() { return passActive() ? PASS_LIVES : MAX_LIVES; }
  function livesTick() {
    var mx = maxLives();
    if (save.lives >= mx) { save.livesTs = now(); return; }
    var n = Math.floor((now() - save.livesTs) / REFILL_MS);
    if (n > 0) { save.lives = Math.min(mx, save.lives + n); save.livesTs += n * REFILL_MS; if (save.lives >= mx) save.livesTs = now(); persist(); }
  }
  function nextLifeMs() { return save.lives >= maxLives() ? 0 : REFILL_MS - (now() - save.livesTs); }
  function hasLife() { livesTick(); return infActive() || save.lives > 0; }
  function loseLife(reason) {
    save.streak = 0;
    if (infActive()) { an('life_lost', { reason: reason, infinite: true }); persist(); return; }
    livesTick();
    if (save.lives >= maxLives()) save.livesTs = now();
    save.lives = Math.max(0, save.lives - 1);
    an('life_lost', { reason: reason, livesLeft: save.lives });
    persist();
  }

  // ---------- Analytics (local only) ----------
  function lvRec(n) {
    var r = save.an.lv[n];
    if (!r) r = save.an.lv[n] = { starts: 0, wins: 0, fails: 0, nearMiss: 0, quits: 0, continues: 0, b: { slot: 0, comb: 0, shuttle: 0 }, winTime: 0, firstTryWins: 0, curAttempt: 0, attemptsToWin: [] };
    return r;
  }
  function an(type, data) {
    var e = { t: new Date(now()).toISOString(), type: type };
    if (L && type !== 'purchase_sim' && type !== 'ad_rv_life') e.level = L.idx + 1;
    if (data) for (var k in data) e[k] = data[k];
    save.an.ev.push(e);
    if (save.an.ev.length > 4000) save.an.ev.splice(0, save.an.ev.length - 4000);
    if (e.level) {
      var r = lvRec(e.level);
      if (type === 'level_start') { r.starts++; r.curAttempt++; }
      else if (type === 'level_win') { r.wins++; r.winTime += e.time || 0; if (r.curAttempt === 1 && !e.continues) r.firstTryWins++; r.attemptsToWin.push(r.curAttempt); r.curAttempt = 0; }
      else if (type === 'level_fail') { r.fails++; if (e.progress >= 0.85) r.nearMiss++; }
      else if (type === 'level_quit') r.quits++;
      else if (type === 'continue') r.continues++;
      else if (type === 'booster') r.b[e.booster] = (r.b[e.booster] || 0) + 1;
    }
    persist();
  }

  // ---------- State ----------
  var scene = 'menu'; // menu | play | win | fail
  var L = null;
  var particles = [], toasts = [], buttons = [];
  var time = 0, takeCounter = 0, SPEED = 1, sessionLevels = 0;
  var modal = null, modalQ = [], ad = null, overlayT = 0, lastScene = '';

  function openModal(m) { if (modal) modalQ.push(m); else { modal = m; m.t = 0; } }
  function closeModal() { var m = modal; modal = modalQ.shift() || null; if (modal) modal.t = 0; if (m && m.onClose) m.onClose(); }

  // ---------- Ads (simulated) ----------
  function showAd(kind, placement, onDone) {
    ad = { kind: kind, placement: placement, t: 0, dur: kind === 'rv' ? 4 : 3, onDone: onDone };
    an(kind === 'rv' ? (placement === 'life' ? 'ad_rv_life' : 'ad_rv') : 'ad_inter', { placement: placement });
  }
  function interEligible() {
    if (!L || L.idx < UNLOCK.inter || save.noAds || save.payer) return false;
    if (sessionLevels < 2) return false; // never after the first level of a session
    if (save.levelsSinceInter < AD.interMinLevels) return false;
    if (now() - save.lastInterTs < AD.interMinGapMs) return false;
    return true;
  }
  function maybeInter(placement, then) {
    save.levelsSinceInter++;
    if (!interEligible()) { then(); return; }
    save.levelsSinceInter = 0; save.lastInterTs = now(); save.interCount++; persist();
    showAd('inter', placement, function () {
      if (save.interCount === 1 && !save.offers.noads && !save.noAds) { save.offers.noads = true; persist(); openModal({ kind: 'offer', sku: 'noads', title: T('noAdsTitle'), sub: T('noAdsSub') }); }
      then();
    });
  }

  // ---------- Layout ----------
  var CARPET = { x: 20, y: 72, w: 360, h: 258 };
  var STRIP = { x: 20, y: 350, w: 360, h: 26 };
  var BELT = { cx: 200, cy: 452, len: 230, rad: 46 };
  var BOOST = { y: 536, h: 46 };
  var LANES = { x: 20, y: 592, w: 360, h: 180 };

  function carpetGeom() {
    var cs = Math.floor(Math.min((CARPET.w - 36) / L.cols, CARPET.h / L.rows));
    var w = cs * L.cols, h = cs * L.rows;
    return { cs: cs, x: Math.round(CARPET.x + (CARPET.w - w) / 2), y: Math.round(CARPET.y + (CARPET.h - h) / 2), w: w, h: h };
  }
  function cellCenter(r, c) { var g = carpetGeom(); return { x: g.x + c * g.cs + g.cs / 2, y: g.y + r * g.cs + g.cs / 2 }; }
  function beltPoint(u) {
    u = ((u % 1) + 1) % 1;
    var s = BELT.len, rr = BELT.rad, per = 2 * s + 2 * Math.PI * rr, d = u * per;
    var x0 = BELT.cx - s / 2, x1 = BELT.cx + s / 2;
    if (d < s) return { x: x0 + d, y: BELT.cy - rr };
    d -= s;
    if (d < Math.PI * rr) { var a = -Math.PI / 2 + d / rr; return { x: x1 + Math.cos(a) * rr, y: BELT.cy + Math.sin(a) * rr }; }
    d -= Math.PI * rr;
    if (d < s) return { x: x1 - d, y: BELT.cy + rr };
    d -= s;
    var b = Math.PI / 2 + d / rr; return { x: x0 + Math.cos(b) * rr, y: BELT.cy + Math.sin(b) * rr };
  }
  function slotPos(k) { return beltPoint(k / L.belt.length + L.beltPhase); }
  function laneW() { return LANES.w / L.lanes.length; }
  function laneFront(i) { return { x: LANES.x + laneW() * (i + 0.5), y: LANES.y + 46 }; }
  function laneSpoolY(j) { return j === 0 ? LANES.y + 46 : LANES.y + 46 + 48 + (j - 1) * 28; }

  // ---------- Game flow ----------
  function levelUnlocked(i) { return i <= save.max; }
  function requestStart(i) {
    if (!levelUnlocked(i)) { toast(T('lockedLevel')); return; }
    if (!hasLife()) { openModal({ kind: 'lives' }); return; }
    startLevel(i);
  }
  function startLevel(i) {
    L = buildLevel(i); particles = []; toasts = []; scene = 'play';
    L.t0 = now();
    if (save.streak >= 3 && i >= UNLOCK.streak) { L.belt.push(null); L.streakBonus = true; toast(T('streakBonus')); }
    if (L.def.tag === 'hard' || L.def.tag === 'veryhard' || L.def.tag === 'boss' || L.def.tag === 'bonus') L.banner = 1.8;
    if (i < 3) {
      var sol = solveLevel(i, null, 200000);
      L.tut = { path: sol.path || [], step: 0, forced: i < 2 ? 999 : 2 };
    }
    an('level_start', { tag: L.def.tag, attempt: lvRec(i + 1).curAttempt + 1, coins: save.coins, lives: save.lives, streak: save.streak });
    // intros for new mechanics / boosters (first time only)
    var intro = [];
    if (i === 5) intro.push('hidden');
    if (i === UNLOCK.slot) intro.push('slot');
    if (i === 11) intro.push('covered');
    if (i === UNLOCK.comb) intro.push('comb');
    if (i === 20) intro.push('knot');
    if (i === UNLOCK.shuttle) intro.push('shuttle');
    if (i === UNLOCK.streak) intro.push('streak');
    intro.forEach(function (k) {
      if (save.seen[k]) return;
      save.seen[k] = 1;
      if (FREE[k]) save.inv[k] = (save.inv[k] || 0) + FREE[k];
      openModal({ kind: 'intro', mech: k, booster: !!FREE[k] });
    });
    persist();
  }
  function progress() { var tot = L.rows * L.cols, w = 0; for (var r = 0; r < L.rows; r++) for (var c = 0; c < L.cols; c++) if (L.cells[r][c].state === 2) w++; return w / tot; }

  function freeSlot() { var k; for (k = 0; k < L.belt.length; k++) if (!L.belt[k]) return k; for (k = 0; k < L.belt.length; k++) if (L.belt[k].leaving) return k; return -1; }
  function weftHead() { // current shuttle head [r,c], only while it is on the active row
    if (!L.weft || L.hp >= L.wpath.length) return null;
    var hd = L.wpath[L.hp]; return hd[0] === L.active ? hd : null;
  }
  function canFire(s) {
    if (!s || s.leaving || s.left <= 0 || s.knot > 0 || L.active < 0) return false;
    for (var k = 0; k < L.belt.length; k++) { // oldest same-color spool fires first (frees its slot quickly)
      var o = L.belt[k];
      if (o && o !== s && !o.leaving && o.left > 0 && !o.knot && o.color === s.color && o.order < s.order) return false;
    }
    if (L.weft) { var hd = weftHead(); return !!hd && L.cells[hd[0]][hd[1]].color === s.color; }
    var row = L.cells[L.active];
    for (var c = 0; c < L.cols; c++) if (row[c].state === 0 && row[c].color === s.color) return true;
    return false;
  }
  function tutHint() { return (L && L.tut && L.tut.step < L.tut.forced && L.tut.step < L.tut.path.length) ? L.tut.path[L.tut.step] : -1; }

  function tapLane(i, depth) {
    if (!L || scene !== 'play' || modal || ad) return;
    var lane = L.lanes[i];
    if (!lane || !lane.length) return;
    depth = depth || 0;
    if (L.combMode) { if (depth >= lane.length) depth = lane.length - 1; }
    else depth = 0;
    var h = tutHint();
    if (h >= 0 && i !== h) { L.tutWiggle = 0.4; sBad(); return; }
    var k = freeSlot();
    if (k < 0) { L.shake = 0.35; sBad(); buzz(30); toast(T('beltFull')); return; }
    var s = lane.splice(depth, 1)[0];
    var f = { x: LANES.x + laneW() * (i + 0.5), y: laneSpoolY(Math.min(depth, 3)) };
    s.x = f.x; s.y = f.y; s.scale = 1.3; s.hidden = false;
    s.order = ++takeCounter;
    L.belt[k] = s;
    if (lane[0]) lane[0].hidden = false;
    L.failT = 0; L.taps++;
    if (L.tut) L.tut.step++;
    if (L.combMode) { L.combMode = false; consumeBooster('comb'); sMagic(); }
    else { sTap(); buzz(10); }
    burst(f.x, f.y, PAL[s.color], 6, 90);
  }

  // ---------- Boosters ----------
  function boosterUnlocked(k) { return L && L.idx >= UNLOCK[k]; }
  function canAfford(k) { return (save.inv[k] || 0) > 0 || save.coins >= PRICE[k]; }
  function consumeBooster(k) {
    var paid = 'inv';
    if ((save.inv[k] || 0) > 0) save.inv[k]--; else { save.coins -= PRICE[k]; paid = 'coins'; }
    L.boostersUsed++;
    an('booster', { booster: k, paid: paid, price: paid === 'coins' ? PRICE[k] : 0, progress: Math.round(progress() * 100) / 100 });
  }
  function useBooster(k) {
    if (!L || scene !== 'play' || modal || ad || L.done) return;
    if (!boosterUnlocked(k)) { toast(T('locked') + ' ' + (UNLOCK[k] + 1)); return; }
    if (k === 'comb' && L.combMode) { L.combMode = false; return; }
    if (!canAfford(k)) { toast(T('notEnough')); openModal({ kind: 'shop' }); an('shop_open', { from: 'booster_' + k }); return; }
    if (k === 'slot') { L.belt.push(null); consumeBooster('slot'); toast(T('slotAdded')); sMagic(); L.failT = 0; return; }
    if (k === 'comb') { L.combMode = true; toast(T('combMode')); return; }
    if (k === 'shuttle') {
      if (L.active < 0) return;
      var row = L.cells[L.active], todo = [], c;
      if (L.weft) { // weaves the next 5 stitches along the shuttle path, whatever their colour
        for (var q = L.hp; q < L.wpath.length && todo.length < 5 && L.wpath[q][0] === L.active; q++) todo.push(L.wpath[q][1]);
        L.hp += todo.length;
      } else for (c = 0; c < L.cols; c++) if (row[c].state === 0) todo.push(c);
      if (!todo.length) { toast(T('noCells')); return; }
      todo = todo.slice(0, 5);
      todo.forEach(function (cc) {
        var cell = row[cc]; cell.state = 2; cell.pop = 1;
        var ce = cellCenter(L.active, cc); burst(ce.x, ce.y, '#fff3c4', 6, 150);
        takeThread(cell.color);
      });
      consumeBooster('shuttle'); toast(T('shuttleDone')); sMagic(); L.failT = 0;
      checkRow();
    }
  }
  // remove one thread of a color from the spool furthest back in the sequence (keeps the thread budget exact)
  function takeThread(color) {
    var best = null, li = -1, where = 'lane';
    L.lanes.forEach(function (lane, i) { lane.forEach(function (s) { if (s.color === color && s.left > 0 && (!best || s.seq > best.seq)) { best = s; li = i; } }); });
    if (!best) { where = 'belt'; L.belt.forEach(function (s) { if (s && !s.leaving && s.color === color && s.left > 0 && (!best || s.seq > best.seq)) best = s; }); }
    if (!best) return;
    best.left--; best.total = Math.max(best.total - 1, best.left);
    if (best.left <= 0) {
      if (where === 'lane') { var lane = L.lanes[li]; lane.splice(lane.indexOf(best), 1); if (lane[0]) lane[0].hidden = false; }
      else best.leaving = 0.001;
    }
  }

  // ---------- Fail / continue / win ----------
  function contPrice() { return CONT[Math.min(L.continues, CONT.length - 1)]; }
  function doContinue() {
    var price = contPrice();
    if (save.coins < price) { toast(T('notEnough')); openModal({ kind: 'shop', highlight: 'rescueS' }); an('shop_open', { from: 'fail_continue' }); return; }
    save.coins -= price; L.continues++;
    save.inv.comb = (save.inv.comb || 0) + 1;
    L.belt.push(null); L.failT = 0; scene = 'play';
    an('continue', { price: price, n: L.continues, progress: Math.round(progress() * 100) / 100 });
    toast(T('slotAdded')); sMagic(); persist();
  }
  function failExit(action) {
    if (L.idx >= 3) loseLife('fail'); else save.streak = 0;
    sessionLevels++;
    var idx = L.idx;
    scene = 'menu'; particles = [];
    maybeInter('level_end_fail', function () { if (action === 'retry') requestStart(idx); });
  }
  function onFail() {
    scene = 'fail'; sFail(); buzz(80);
    an('level_fail', { progress: Math.round(progress() * 100) / 100, time: Math.round((now() - L.t0) / 1000), boosters: L.boostersUsed, continues: L.continues, beltSlots: L.belt.length });
  }
  function onWin() {
    sWin(); L.ended = true;
    var stars = L.continues ? 1 : L.boostersUsed ? 2 : 3; L.stars = stars;
    save.best[L.idx] = Math.max(save.best[L.idx] || 0, stars);
    save.max = Math.max(save.max, Math.min(L.idx + 1, LEVELS.length - 1));
    if (L.idx + 1 >= LEVELS.length) save.max = LEVELS.length - 1;
    L.reward = REWARD[L.def.tag]; save.coins += L.reward;
    save.streak++;
    an('level_win', { time: Math.round((now() - L.t0) / 1000), stars: stars, boosters: L.boostersUsed, continues: L.continues, reward: L.reward, streak: save.streak });
    sessionLevels++;
    persist();
    for (var i = 0; i < 90; i++) confetti();
    maybeInter('level_end_win', function () {
      scene = 'win';
      if (L.idx === 11 && !save.offers.starter) { save.offers.starter = true; persist(); openModal({ kind: 'offer', sku: 'starter', title: T('starterTitle'), sub: T('starterSub') }); }
      if (L.idx === 26 && !save.offers.noadsBundle && !save.noAds) { save.offers.noadsBundle = true; persist(); openModal({ kind: 'offer', sku: 'noadsBundle', title: T('noAdsTitle'), sub: T('noAdsSub') }); }
    });
  }

  // ---------- Shop (simulated) ----------
  function skuById(id) { for (var i = 0; i < SKUS.length; i++) if (SKUS[i].id === id) return SKUS[i]; return null; }
  function simulatePurchase(id) {
    var s = skuById(id); if (!s) return;
    if (s.coins) save.coins += s.coins;
    if (s.inv) for (var k in s.inv) save.inv[k] = (save.inv[k] || 0) + s.inv[k];
    if (s.infMin) save.infUntil = Math.max(now(), save.infUntil) + s.infMin * 60000;
    if (s.noAds) save.noAds = true;
    if (s.pass) { save.passUntil = Math.max(now(), save.passUntil) + 28 * 86400000; save.lives = Math.max(save.lives, PASS_LIVES); }
    if (s.oneTime) save.offers['bought_' + id] = true;
    save.payer = true;
    an('purchase_sim', { sku: id, price: s.price });
    toast(T('bought') + ': ' + s[lang]); sCoin(); persist();
  }

  // ---------- Update ----------
  function update(dt) {
    time += dt;
    for (var i = particles.length - 1; i >= 0; i--) {
      var p = particles[i]; p.t += dt;
      if (p.kind === 'fly') { var fe = clamp(p.t / p.life, 0, 1), fq = fe * fe; p.x = p.sx + (p.tx - p.sx) * fq + p.ox * Math.sin(fe * Math.PI); p.y = p.sy + (p.ty - p.sy) * fq; if (p.t > p.life) { particles.splice(i, 1); if (Math.random() < 0.5) tone(1200 + Math.random() * 300, 0.04, 'sine', 0.04); } continue; }
      p.vy += (p.g || 500) * dt; p.x += p.vx * dt; p.y += p.vy * dt; p.rot += p.vr * dt;
      if (p.t > p.life) particles.splice(i, 1);
    }
    for (i = toasts.length - 1; i >= 0; i--) { toasts[i].t += dt; if (toasts[i].t > 1.4) toasts.splice(i, 1); }
    if (ad) { ad.t += dt; return; }
    if (modal) { modal.t += dt; return; }
    if (!L || scene === 'menu') return;

    L.beltPhase += dt * 0.035;
    L.shake = Math.max(0, L.shake - dt);
    L.rowFlash = Math.max(0, L.rowFlash - dt * 1.6);
    L.banner = Math.max(0, L.banner - dt);
    L.tutWiggle = Math.max(0, (L.tutWiggle || 0) - dt);
    L.reveal = Math.max(0, (L.reveal || 0) - dt * 2);
    var r, c, k, s;
    for (r = 0; r < L.rows; r++) for (c = 0; c < L.cols; c++) L.cells[r][c].pop = Math.max(0, L.cells[r][c].pop - dt * 3);

    for (k = 0; k < L.belt.length; k++) {
      s = L.belt[k];
      if (!s) continue;
      var sp = slotPos(k), t = Math.min(1, dt * 9);
      s.x = lerp(s.x, sp.x, t); s.y = lerp(s.y, sp.y, t);
      s.scale = lerp(s.scale, 1, Math.min(1, dt * 8));
      s.kick = Math.max(0, s.kick - dt * 6);
      s.cool -= dt;
      if (s.leaving) { s.leaving += dt; if (s.leaving > 0.45) L.belt[k] = null; continue; }
      if (scene === 'play' && s.cool <= 0 && canFire(s)) {
        var row = L.cells[L.active], best = -1, bd = 1e9;
        if (L.weft) { best = L.wpath[L.hp][1]; L.hp++; }
        else for (c = 0; c < L.cols; c++) if (row[c].state === 0 && row[c].color === s.color) { var cc = cellCenter(L.active, c), d = Math.abs(cc.x - s.x); if (d < bd) { bd = d; best = c; } }
        row[best].state = 1;
        s.left--; s.pending++; s.cool = 0.11; s.kick = 1;
        L.threads.push({ s: s, r: L.active, c: best, t: 0, fx: s.x, fy: s.y, color: s.color });
      }
      if (s.left <= 0 && !s.leaving) { s.leaving = 0.001; burst(s.x, s.y, PAL[s.color], 10, 160); tone(880, 0.12, 'sine', 0.06); }
    }
    for (i = L.threads.length - 1; i >= 0; i--) {
      var th = L.threads[i];
      th.t += dt / 0.26;
      if (th.t >= 1) {
        L.threads.splice(i, 1);
        var cell = L.cells[th.r][th.c];
        cell.state = 2; cell.pop = 1; th.s.pending--;
        var ce = cellCenter(th.r, th.c);
        burst(ce.x, ce.y, PAL[th.color], 5, 120);
        sPluck(Math.min(SCALE.length - 1, (L.combo = (L.combo || 0) + 1) - 1 + (L.rows - 1 - th.r) % 3));
        checkRow();
      }
    }
    if (scene === 'play' && !L.done) {
      var usedN = 0; for (k = 0; k < L.belt.length; k++) if (L.belt[k] && !L.belt[k].leaving) usedN++;
      var freeN = L.belt.length - usedN;
      if (freeN === 1 && L.lastFree !== 1) { tone(196, 0.12, 'triangle', 0.08); tone(196, 0.12, 'triangle', 0.08, 0.16); buzz([20, 60, 20]); }
      L.lastFree = freeN;
      var full = true, any = false, laneLeft = 0;
      for (k = 0; k < L.belt.length; k++) { s = L.belt[k]; if (!s || s.leaving) full = false; if (canFire(s)) any = true; }
      L.lanes.forEach(function (l) { laneLeft += l.length; });
      var stuck = L.threads.length === 0 && !any && L.active >= 0 && (full || laneLeft === 0);
      if (stuck) { L.failT += dt; if (L.failT > 0.7) onFail(); } else L.failT = 0;
    }
    if (L.done && scene === 'play' && !L.ended) { L.winT += dt; if (L.winT > 0.9) onWin(); }
  }

  function checkRow() {
    if (L.active < 0) return;
    var row = L.cells[L.active];
    for (var c = 0; c < L.cols; c++) if (row[c].state !== 2) return;
    L.flashRow = L.active; L.rowFlash = 1; L.combo = 0;
    for (c = 0; c < L.cols; c++) { var ce = cellCenter(L.active, c); burst(ce.x, ce.y, '#fff3c4', 3, 200); }
    sRow(); buzz(20);
    // knots on the belt loosen by one each time a row is finished
    L.belt.forEach(function (s) { if (s && s.knot > 0) { s.knot--; burst(s.x, s.y, '#d8c3a0', 8, 120); } });
    L.active--;
    if (L.active >= 0 && L.cells[L.active][0].veil) L.reveal = 1;
    if (L.active < 0) { L.done = true; toast(T('done')); }
  }

  function burst(x, y, color, n, spd) {
    for (var i = 0; i < n; i++) {
      var a = Math.random() * Math.PI * 2, v = spd * (0.4 + Math.random() * 0.8);
      particles.push({ x: x, y: y, vx: Math.cos(a) * v, vy: Math.sin(a) * v - 60, g: 400, t: 0, life: 0.45 + Math.random() * 0.3, size: 2 + Math.random() * 3, color: color, rot: 0, vr: 0, kind: 'dot' });
    }
  }
  function confetti() {
    var cols = Object.keys(PAL);
    particles.push({ x: Math.random() * LW, y: -20 - Math.random() * 200, vx: (Math.random() - 0.5) * 80, vy: 80 + Math.random() * 120, g: 120,
      t: 0, life: 3 + Math.random() * 1.5, size: 5 + Math.random() * 5, color: PAL[cols[Math.floor(Math.random() * cols.length)]], rot: Math.random() * 6, vr: (Math.random() - 0.5) * 10, kind: 'rect' });
  }
  function toast(msg) { toasts.push({ msg: msg, t: 0 }); if (toasts.length > 3) toasts.shift(); }

  // ---------- Drawing primitives ----------
  function rr(x, y, w, h, r) {
    ctx.beginPath(); ctx.moveTo(x + r, y); ctx.arcTo(x + w, y, x + w, y + h, r); ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r); ctx.arcTo(x, y, x + w, y, r); ctx.closePath();
  }
  function text(str, x, y, size, color, align, weight) {
    ctx.fillStyle = color; ctx.font = (weight || '700') + ' ' + size + 'px -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif';
    ctx.textAlign = align || 'center'; ctx.textBaseline = 'middle'; ctx.fillText(str, x, y);
  }
  function fitText(str, x, y, size, color, maxW, align, weight) {
    ctx.font = (weight || '700') + ' ' + size + 'px -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif';
    while (size > 8 && ctx.measureText(str).width > maxW) { size -= 0.5; ctx.font = (weight || '700') + ' ' + size + 'px -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif'; }
    text(str, x, y, size, color, align, weight);
  }
  function wrapText(str, x, y, size, color, maxW, lh, weight) {
    ctx.font = (weight || '500') + ' ' + size + 'px -apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif';
    var words = str.split(' '), line = '', lines = [];
    words.forEach(function (w) { var t = line ? line + ' ' + w : w; if (ctx.measureText(t).width > maxW && line) { lines.push(line); line = w; } else line = t; });
    if (line) lines.push(line);
    lines.forEach(function (l, i) { text(l, x, y + i * lh, size, color, 'center', weight || '500'); });
    return lines.length;
  }
  function button(id, x, y, w, h, label, bg, fg, size, disabled) {
    buttons.push({ id: id, x: x, y: y, w: w, h: h });
    ctx.globalAlpha = disabled ? 0.5 : 1;
    ctx.fillStyle = 'rgba(0,0,0,0.25)'; rr(x, y + 4, w, h, 14); ctx.fill();
    ctx.fillStyle = bg; rr(x, y, w, h, 14); ctx.fill();
    fitText(label, x + w / 2, y + h / 2 + 1, size || 18, fg || '#fff', w - 12);
    ctx.globalAlpha = 1;
  }
  function pill(x, y, w, h, bg) { ctx.fillStyle = bg || 'rgba(0,0,0,0.35)'; rr(x, y, w, h, h / 2); ctx.fill(); }
  function drawHeart(x, y, s, color) {
    ctx.fillStyle = color; ctx.beginPath(); ctx.moveTo(x, y + s * 0.35);
    ctx.bezierCurveTo(x - s * 0.9, y - s * 0.3, x - s * 0.35, y - s * 0.95, x, y - s * 0.35);
    ctx.bezierCurveTo(x + s * 0.35, y - s * 0.95, x + s * 0.9, y - s * 0.3, x, y + s * 0.35); ctx.fill();
  }
  function drawCoin(x, y, r) { ctx.fillStyle = '#b8860b'; ctx.beginPath(); ctx.arc(x, y, r, 0, 6.3); ctx.fill(); ctx.fillStyle = '#f2c94c'; ctx.beginPath(); ctx.arc(x, y, r * 0.78, 0, 6.3); ctx.fill(); text('¢', x, y + 0.5, r * 1.1, '#8a5a00'); }
  function drawStar(x, y, r, color) {
    ctx.fillStyle = color; ctx.beginPath();
    for (var i = 0; i < 10; i++) { var a = -Math.PI / 2 + i * Math.PI / 5, rad = i % 2 ? r * 0.45 : r; ctx.lineTo(x + Math.cos(a) * rad, y + Math.sin(a) * rad); }
    ctx.closePath(); ctx.fill();
  }
  function tagInfo(tag) {
    if (tag === 'hard') return { c: '#d64532', label: T('tagHard'), banner: T('bannerHard') };
    if (tag === 'veryhard') return { c: '#8e44ad', label: T('tagVeryhard'), banner: T('bannerVeryhard') };
    if (tag === 'boss') return { c: '#7b2fd0', label: T('tagBoss'), banner: T('bannerBoss') };
    if (tag === 'bonus') return { c: '#2e9e5b', label: T('tagBonus'), banner: T('bannerBonus') };
    return null;
  }
  function drawBackground() {
    var g = ctx.createLinearGradient(0, 0, 0, LH);
    g.addColorStop(0, '#4a2c20'); g.addColorStop(1, '#22140f');
    ctx.fillStyle = g; ctx.fillRect(-offX / scale, -offY / scale, LW + 2 * offX / scale, LH + 2 * offY / scale);
    ctx.strokeStyle = 'rgba(255,220,180,0.035)'; ctx.lineWidth = 1;
    for (var i = -LH; i < LW + LH; i += 22) { ctx.beginPath(); ctx.moveTo(i, 0); ctx.lineTo(i - LH, LH); ctx.stroke(); }
  }

  // ---------- Play scene ----------
  function drawSym(kind, x, y, r, ink, alpha) {
    ctx.save(); ctx.globalAlpha *= (alpha == null ? 1 : alpha); ctx.fillStyle = ink; ctx.strokeStyle = ink; ctx.lineWidth = Math.max(1.5, r * 0.3);
    ctx.beginPath();
    if (kind === 'circle') ctx.arc(x, y, r * 0.78, 0, 6.3);
    else if (kind === 'square') ctx.rect(x - r * 0.7, y - r * 0.7, r * 1.4, r * 1.4);
    else if (kind === 'diamond') { ctx.moveTo(x, y - r); ctx.lineTo(x + r * 0.78, y); ctx.lineTo(x, y + r); ctx.lineTo(x - r * 0.78, y); }
    else if (kind === 'triangle') { ctx.moveTo(x, y - r); ctx.lineTo(x + r, y + r * 0.8); ctx.lineTo(x - r, y + r * 0.8); }
    else if (kind === 'cross') { ctx.rect(x - r * 0.28, y - r, r * 0.56, r * 2); ctx.rect(x - r, y - r * 0.28, r * 2, r * 0.56); }
    else if (kind === 'hexagon') { for (var i = 0; i < 6; i++) { var a = Math.PI / 3 * i; ctx.lineTo(x + r * Math.cos(a), y + r * Math.sin(a)); } }
    else if (kind === 'star') { for (var j = 0; j < 10; j++) { var b = -Math.PI / 2 + j * Math.PI / 5, q = j % 2 ? r * 0.45 : r; ctx.lineTo(x + q * Math.cos(b), y + q * Math.sin(b)); } }
    else if (kind === 'moon') { ctx.arc(x, y, r, Math.PI * 0.35, Math.PI * 1.65); ctx.arc(x + r * 0.45, y, r * 0.78, Math.PI * 1.45, Math.PI * 0.55, true); }
    else if (kind === 'heart') { ctx.moveTo(x, y + r * 0.9); ctx.bezierCurveTo(x - r * 1.4, y - r * 0.1, x - r * 0.6, y - r * 1.2, x, y - r * 0.35); ctx.bezierCurveTo(x + r * 0.6, y - r * 1.2, x + r * 1.4, y - r * 0.1, x, y + r * 0.9); }
    else if (kind === 'drop') { ctx.moveTo(x, y - r); ctx.bezierCurveTo(x + r * 0.9, y, x + r * 0.8, y + r, x, y + r); ctx.bezierCurveTo(x - r * 0.8, y + r, x - r * 0.9, y, x, y - r); }
    else if (kind === 'ring') { ctx.arc(x, y, r * 0.7, 0, 6.3); ctx.closePath(); ctx.stroke(); ctx.restore(); return; }
    else if (kind === 'bar') ctx.rect(x - r, y - r * 0.3, r * 2, r * 0.6);
    else if (kind === 'chevron') { ctx.moveTo(x - r, y - r * 0.4); ctx.lineTo(x, y + r * 0.5); ctx.lineTo(x + r, y - r * 0.4); ctx.lineTo(x + r, y + r * 0.2); ctx.lineTo(x, y + r); ctx.lineTo(x - r, y + r * 0.2); }
    else if (kind === 'wave') { ctx.moveTo(x - r, y); ctx.quadraticCurveTo(x - r / 2, y - r, x, y); ctx.quadraticCurveTo(x + r / 2, y + r, x + r, y); ctx.stroke(); ctx.restore(); return; }
    else ctx.arc(x, y, r * 0.4, 0, 6.3);
    ctx.closePath(); ctx.fill(); ctx.restore();
  }
  function drawCell(x, y, cs, cell, active, pulse, veiled) {
    var hex = PAL[cell.color];
    if (cell.state === 2) {
      var s = 1 + 0.35 * Math.sin(cell.pop * Math.PI) * cell.pop, w = cs * s, ox = x + (cs - w) / 2, oy = y + (cs - w) / 2;
      ctx.fillStyle = hex; ctx.fillRect(ox, oy, w + 0.5, w + 0.5);
      ctx.strokeStyle = shade(hex, -38); ctx.lineWidth = Math.max(1, cs * 0.07);
      ctx.beginPath(); var m = cs * 0.22;
      ctx.moveTo(ox + m, oy + m); ctx.lineTo(ox + w / 2, oy + w / 2); ctx.lineTo(ox + w - m, oy + m);
      ctx.moveTo(ox + m, oy + w / 2 + m * 0.4); ctx.lineTo(ox + w / 2, oy + w - m * 0.6); ctx.lineTo(ox + w - m, oy + w / 2 + m * 0.4);
      ctx.stroke();
      ctx.strokeStyle = shade(hex, 30); ctx.lineWidth = Math.max(0.6, cs * 0.03);
      ctx.beginPath(); ctx.moveTo(ox + 1, oy + 1); ctx.lineTo(ox + w - 1, oy + 1); ctx.stroke();
      if (save.cb && cs >= 14) drawSym(SYM[cell.color], ox + w / 2, oy + w / 2, cs * 0.16, inkFor(cell.color), 0.45);
    } else if (veiled) {
      ctx.fillStyle = '#5a4a40'; ctx.fillRect(x + 1, y + 1, cs - 2, cs - 2);
      ctx.fillStyle = 'rgba(255,240,220,0.18)'; ctx.beginPath(); ctx.arc(x + cs / 2, y + cs / 2, cs * 0.12, 0, 6.3); ctx.fill();
    } else {
      ctx.fillStyle = rgba(hex, active ? 0.5 + 0.2 * pulse : 0.34);
      ctx.fillRect(x + 1, y + 1, cs - 2, cs - 2);
      if (save.cb || active) drawSym(SYM[cell.color], x + cs / 2, y + cs / 2, cs * 0.22, active ? rgba(inkFor(cell.color) === '#FFFFFF' ? '#FFFFFF' : '#2B1A14', 0.75) : 'rgba(255,255,255,0.55)');
      if (active) { ctx.strokeStyle = rgba(hex, 0.9); ctx.lineWidth = 1.5; ctx.strokeRect(x + 2.5, y + 2.5, cs - 5, cs - 5); }
      if (cell.state === 1) { ctx.fillStyle = rgba(hex, 0.6); ctx.fillRect(x + cs * 0.3, y + cs * 0.3, cs * 0.4, cs * 0.4); }
    }
  }
  function inRun(r, c) { // cells of the run the shuttle is about to weave
    var hd = weftHead(); if (!hd) return false;
    var col = L.cells[hd[0]][hd[1]].color;
    for (var k = L.hp; k < L.wpath.length && L.wpath[k][0] === hd[0]; k++) { var p = L.wpath[k]; if (L.cells[p[0]][p[1]].color !== col) break; if (p[1] === c) return true; }
    return false;
  }
  function drawWeftHead(g, cs) {
    if (L.active < 0) return;
    var k = L.rows - 1 - L.active, dir = k % 2 === 0 ? 1 : -1, y = g.y + L.active * cs + cs / 2, hd = weftHead();
    if (!hd) return;
    var x = g.x + hd[1] * cs, b = 0.5 + 0.5 * Math.sin(time * 8);
    ctx.strokeStyle = 'rgba(255,233,168,' + (0.6 + 0.4 * b) + ')'; ctx.lineWidth = 3; ctx.strokeRect(x + 1.5, y - cs / 2 + 1.5, cs - 3, cs - 3);
    // little shuttle (boat shape) riding the head
    var sx = x + cs / 2 - dir * cs * 0.15, sy = y - cs / 2 - 6;
    ctx.fillStyle = '#c9a36b'; ctx.beginPath(); ctx.moveTo(sx - 12, sy); ctx.quadraticCurveTo(sx, sy - 7, sx + 12, sy); ctx.quadraticCurveTo(sx, sy + 7, sx - 12, sy); ctx.fill();
    ctx.fillStyle = PAL[L.cells[hd[0]][hd[1]].color]; ctx.beginPath(); ctx.arc(sx, sy, 3, 0, 6.3); ctx.fill();
  }
  // preview strip: the next colour runs the shuttle needs (stops at a still-covered row)
  function stripRuns() {
    var runs = [], act = L.active;
    var lim = Math.min(L.wpath.length, (Math.floor(L.hp / L.cols) + 2) * L.cols);
    for (var k = L.hp; k < lim; k++) {
      var p = L.wpath[k], r = p[0];
      if (r !== act && L.cells[r][0].veil) { runs.push({ q: true }); break; }
      var col = L.cells[r][p[1]].color, last = runs[runs.length - 1];
      if (last && !last.q && last.c === col && last.r === r) last.n++;
      else { if (last && last.r !== r) runs.push({ flip: true, r: r }); runs.push({ c: col, n: 1, r: r }); }
    }
    return runs;
  }
  function drawStrip() {
    if (!L.weft || L.active < 0) return;
    var x = STRIP.x, y = STRIP.y, h = STRIP.h;
    ctx.fillStyle = 'rgba(0,0,0,0.28)'; rr(x, y, STRIP.w, h, 13); ctx.fill();
    text(T('nextStrip'), x + 26, y + h / 2 + 0.5, 10, 'rgba(246,231,207,0.7)', 'center', '800');
    var runs = stripRuns(), cx = x + 52;
    for (var i = 0; i < runs.length; i++) {
      var ru = runs[i];
      if (cx > x + STRIP.w - 30) break;
      if (ru.flip) { text('↩', cx + 6, y + h / 2 + 1, 13, '#ffe9a8', 'center', '800'); cx += 16; continue; }
      if (ru.q) { ctx.fillStyle = 'rgba(255,255,255,0.12)'; rr(cx, y + 3, 34, h - 6, 9); ctx.fill(); text('?', cx + 17, y + h / 2 + 1, 13, '#f6e7cf', 'center', '800'); cx += 40; continue; }
      var w = 46, first = i === 0;
      ctx.fillStyle = PAL[ru.c]; rr(cx, y + 3, w, h - 6, 9); ctx.fill();
      if (first) { ctx.strokeStyle = 'rgba(255,233,168,' + (0.6 + 0.4 * Math.sin(time * 8)) + ')'; ctx.lineWidth = 2.5; rr(cx - 1, y + 2, w + 2, h - 4, 10); ctx.stroke(); }
      drawSym(SYM[ru.c], cx + 12, y + h / 2, 6, inkFor(ru.c));
      text('×' + ru.n, cx + 32, y + h / 2 + 1, 12, inkFor(ru.c), 'center', '800');
      cx += w + 6;
    }
  }
  function drawCarpet() {
    var g = carpetGeom(), cs = g.cs, r, c;
    ctx.fillStyle = '#1a0f0b'; rr(g.x - 8, g.y - 8, g.w + 16, g.h + 16, 6); ctx.fill();
    ctx.strokeStyle = '#d8c3a0'; ctx.lineWidth = 1.5;
    for (var fx = g.x; fx <= g.x + g.w; fx += 6) {
      var sw = Math.sin(time * 2 + fx * 0.3) * 1.5;
      ctx.beginPath(); ctx.moveTo(fx, g.y - 8); ctx.lineTo(fx + sw, g.y - 14); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(fx, g.y + g.h + 8); ctx.lineTo(fx - sw, g.y + g.h + 16); ctx.stroke();
    }
    ctx.fillStyle = '#efe2c8'; ctx.fillRect(g.x - 2, g.y - 2, g.w + 4, g.h + 4);
    ctx.fillStyle = '#6e5a4c'; ctx.fillRect(g.x, g.y, g.w, g.h);
    var pulse = 0.5 + 0.5 * Math.sin(time * 6);
    for (r = 0; r < L.rows; r++) for (c = 0; c < L.cols; c++) {
      var cell = L.cells[r][c];
      drawCell(g.x + c * cs, g.y + r * cs, cs, cell, r === L.active && (!L.weft || inRun(r, c)), pulse, cell.veil && r < L.active);
    }
    if (L.weft) drawWeftHead(g, cs);
    if (L.reveal > 0 && L.active >= 0) { ctx.fillStyle = 'rgba(255,240,210,' + (0.5 * L.reveal) + ')'; ctx.fillRect(g.x, g.y + L.active * cs, g.w, cs); }
    if (L.rowFlash > 0 && L.flashRow >= 0) {
      var sw = 1 - L.rowFlash, sxp = g.x - 60 + (g.w + 120) * sw, gr = ctx.createLinearGradient(sxp - 60, 0, sxp + 60, 0);
      gr.addColorStop(0, 'rgba(255,245,200,0)'); gr.addColorStop(0.5, 'rgba(255,250,220,' + (0.85 * L.rowFlash) + ')'); gr.addColorStop(1, 'rgba(255,245,200,0)');
      ctx.fillStyle = 'rgba(255,245,200,' + (0.2 * L.rowFlash) + ')'; ctx.fillRect(g.x, g.y + L.flashRow * cs, g.w, cs);
      ctx.save(); ctx.beginPath(); ctx.rect(g.x, g.y + L.flashRow * cs, g.w, cs); ctx.clip(); ctx.fillStyle = gr; ctx.fillRect(g.x, g.y + L.flashRow * cs, g.w, cs); ctx.restore();
    }
    if (L.done) {
      var wk = clamp(L.winT / 0.6, 0, 1), pad = 6 + 4 * Math.sin(time * 8);
      ctx.strokeStyle = 'rgba(242,193,78,' + wk + ')'; ctx.lineWidth = 4; ctx.strokeRect(g.x - pad, g.y - pad, g.w + 2 * pad, g.h + 2 * pad);
      for (var sp = 0; sp < 8; sp++) { var ang = time * 2 + sp * 0.785, per = (ang / 6.283 % 1 + 1) % 1, P = 2 * (g.w + g.h), d = per * P, px, py;
        if (d < g.w) { px = g.x + d; py = g.y - pad; } else if (d < g.w + g.h) { px = g.x + g.w + pad; py = g.y + d - g.w; } else if (d < 2 * g.w + g.h) { px = g.x + g.w - (d - g.w - g.h); py = g.y + g.h + pad; } else { px = g.x - pad; py = g.y + g.h - (d - 2 * g.w - g.h); }
        ctx.globalAlpha = wk; drawStar(px, py, 5, '#fff3c4'); ctx.globalAlpha = 1; }
    }
    if (L.active >= 0) { var ay = g.y + L.active * cs + cs / 2, bob = Math.sin(time * 5) * 2; if (L.weft) { if ((L.rows - 1 - L.active) % 2 === 0) drawArrow(g.x - 14 + bob, ay, 1); else drawArrow(g.x + g.w + 14 - bob, ay, -1); } else { drawArrow(g.x - 14 + bob, ay, 1); drawArrow(g.x + g.w + 14 - bob, ay, -1); } }
  }
  function drawArrow(x, y, dir) {
    ctx.save(); ctx.translate(x, y); ctx.scale(dir, 1); ctx.fillStyle = '#e0a526';
    ctx.beginPath(); ctx.moveTo(6, 0); ctx.lineTo(-6, -6); ctx.lineTo(-3, 0); ctx.lineTo(-6, 6); ctx.closePath(); ctx.fill(); ctx.restore();
  }
  function drawSpool(x, y, rad, color, left, total, hidden, kick, alpha, knot) {
    ctx.save(); ctx.globalAlpha = alpha == null ? 1 : alpha;
    ctx.translate(x, y);
    var k = 1 + 0.12 * (kick || 0); ctx.scale(k, 1 / k);
    var hex = hidden ? '#7a6a60' : PAL[color], w = rad * 1.5, h = rad * 1.9;
    var frac = total ? clamp(left / total, 0, 1) : 1, bw = w * (0.55 + 0.45 * frac);
    ctx.fillStyle = shade(hex, -30); rr(-bw / 2, -h / 2 + 4, bw, h - 8, rad * 0.35); ctx.fill();
    ctx.fillStyle = hex; rr(-bw / 2 + 2, -h / 2 + 4, bw - 4, h - 8, rad * 0.3); ctx.fill();
    ctx.strokeStyle = shade(hex, 35); ctx.lineWidth = 1.2;
    for (var i = -h / 2 + 8; i < h / 2 - 6; i += 4) { ctx.beginPath(); ctx.moveTo(-bw / 2 + 3, i); ctx.lineTo(bw / 2 - 3, i + 2); ctx.stroke(); }
    ctx.fillStyle = '#a0703f'; rr(-w / 2, -h / 2, w, 6, 3); ctx.fill(); rr(-w / 2, h / 2 - 6, w, 6, 3); ctx.fill();
    ctx.fillStyle = '#c99256'; ctx.fillRect(-w / 2 + 2, -h / 2 + 1, w - 4, 2);
    if (hidden) text('?', 0, 1, rad * 0.95, '#fff');
    else {
      drawSym(SYM[color], 0, -rad * 0.08, rad * 0.4, inkFor(color));
      var bx = w / 2 - 2, by = h / 2 - 6, br = Math.max(7, rad * 0.4);
      ctx.fillStyle = '#2B1A14'; ctx.beginPath(); ctx.arc(bx, by, br, 0, 6.3); ctx.fill();
      ctx.strokeStyle = '#F6ECD9'; ctx.lineWidth = 1.5; ctx.stroke();
      text(String(left), bx, by + 0.5, br * 1.15, '#fff', 'center', '800');
    }
    if (knot) { // rope knot across the spool
      ctx.strokeStyle = '#e9d8b4'; ctx.lineWidth = Math.max(2, rad * 0.16);
      ctx.beginPath(); ctx.moveTo(-w / 2, -h / 2 + 6); ctx.lineTo(w / 2, h / 2 - 6); ctx.moveTo(w / 2, -h / 2 + 6); ctx.lineTo(-w / 2, h / 2 - 6); ctx.stroke();
      ctx.fillStyle = '#e9d8b4'; ctx.beginPath(); ctx.arc(0, 0, rad * 0.26, 0, 6.3); ctx.fill();
      ctx.strokeStyle = '#8a6a45'; ctx.lineWidth = 1.2; ctx.beginPath(); ctx.arc(0, 0, rad * 0.26, 0, 6.3); ctx.stroke();
    }
    ctx.restore();
  }
  function drawBelt() {
    var sh = L.shake > 0 ? Math.sin(L.shake * 60) * 6 * L.shake : 0;
    ctx.save(); ctx.translate(sh, 0);
    var s = BELT.len, r = BELT.rad, x0 = BELT.cx - s / 2;
    function path() { ctx.beginPath(); ctx.moveTo(x0, BELT.cy - r); ctx.lineTo(x0 + s, BELT.cy - r); ctx.arc(x0 + s, BELT.cy, r, -Math.PI / 2, Math.PI / 2); ctx.lineTo(x0, BELT.cy + r); ctx.arc(x0, BELT.cy, r, Math.PI / 2, Math.PI * 1.5); ctx.closePath(); }
    ctx.lineCap = 'round';
    path(); ctx.strokeStyle = '#140b08'; ctx.lineWidth = 40; ctx.stroke();
    path(); ctx.strokeStyle = '#3d2a20'; ctx.lineWidth = 34; ctx.stroke();
    ctx.setLineDash([6, 10]); ctx.lineDashOffset = -L.beltPhase * 2000; path(); ctx.strokeStyle = '#6b4c3b'; ctx.lineWidth = 30; ctx.stroke(); ctx.setLineDash([]);
    var used = 0; for (var k = 0; k < L.belt.length; k++) if (L.belt[k] && !L.belt[k].leaving) used++;
    var free = L.belt.length - used, pulse = 0.5 + 0.5 * Math.sin(time * 9);
    var tcol = free <= 0 ? '#ff6b5e' : free === 1 ? '#ffb340' : 'rgba(255,230,200,0.85)';
    if (free <= 1) { ctx.fillStyle = free <= 0 ? 'rgba(255,80,60,' + (0.18 + 0.15 * pulse) + ')' : 'rgba(255,170,50,' + (0.12 + 0.12 * pulse) + ')'; rr(BELT.cx - 70, BELT.cy - 24, 140, 48, 20); ctx.fill(); }
    text(free === 1 ? T('lastSlot') : T('belt'), BELT.cx, BELT.cy - 11, free === 1 ? 12 : 11, free <= 1 ? tcol : 'rgba(255,230,200,0.45)', 'center', '800');
    text(used + ' / ' + L.belt.length, BELT.cx, BELT.cy + 10, free <= 1 ? 20 + 2 * pulse : 18, tcol, 'center', '800');
    for (k = 0; k < L.belt.length; k++) {
      var p = slotPos(k), empty = !L.belt[k] || L.belt[k].leaving;
      ctx.lineWidth = empty && free === 1 ? 3 : 2;
      ctx.strokeStyle = empty ? (free === 1 ? 'rgba(255,179,64,' + (0.5 + 0.5 * pulse) + ')' : 'rgba(255,230,200,0.45)') : 'rgba(255,230,200,0.2)';
      ctx.setLineDash(empty && free === 1 ? [] : [3, 3]); ctx.beginPath(); ctx.arc(p.x, p.y, 18, 0, Math.PI * 2); ctx.stroke(); ctx.setLineDash([]);
    }
    ctx.restore();
  }
  function drawBeltSpools() {
    for (var k = 0; k < L.belt.length; k++) {
      var s = L.belt[k]; if (!s) continue;
      var a = s.leaving ? Math.max(0, 1 - s.leaving / 0.45) : 1, yy = s.y - (s.leaving ? s.leaving * 60 : 0);
      drawSpool(s.x, yy, 22 * s.scale * (s.leaving ? 1 + s.leaving : 1), s.color, s.left, s.total, false, s.kick, a, s.knot > 0);
    }
  }
  function drawThreads() {
    for (var i = 0; i < L.threads.length; i++) {
      var th = L.threads[i], to = cellCenter(th.r, th.c), e = 1 - Math.pow(1 - th.t, 3);
      var mx = (th.fx + to.x) / 2, my = Math.min(th.fy, to.y) - 40;
      var x = (1 - e) * (1 - e) * th.fx + 2 * (1 - e) * e * mx + e * e * to.x, y = (1 - e) * (1 - e) * th.fy + 2 * (1 - e) * e * my + e * e * to.y;
      ctx.strokeStyle = rgba(PAL[th.color], 0.25); ctx.lineWidth = 7;
      ctx.beginPath(); ctx.moveTo(th.s.x, th.s.y); ctx.quadraticCurveTo(mx, my, x, y); ctx.stroke();
      ctx.strokeStyle = rgba(PAL[th.color], 0.95); ctx.lineWidth = 2.5;
      ctx.beginPath(); ctx.moveTo(th.s.x, th.s.y); ctx.quadraticCurveTo(mx, my, x, y); ctx.stroke();
      ctx.fillStyle = '#fff'; ctx.beginPath(); ctx.arc(x, y, 3, 0, Math.PI * 2); ctx.fill();
    }
  }
  function drawLanes() {
    var lw = laneW(), hint = tutHint();
    for (var i = 0; i < L.lanes.length; i++) {
      var x = LANES.x + lw * i, lane = L.lanes[i];
      var hl = L.combMode || hint === i;
      ctx.fillStyle = hl ? 'rgba(224,165,38,0.16)' : 'rgba(0,0,0,0.25)'; rr(x + 4, LANES.y, lw - 8, LANES.h, 14); ctx.fill();
      ctx.strokeStyle = hl ? 'rgba(224,165,38,0.8)' : 'rgba(255,230,200,0.12)'; ctx.lineWidth = hl ? 2.5 : 1.5; rr(x + 4, LANES.y, lw - 8, LANES.h, 14); ctx.stroke();
      var cx = x + lw / 2, wig = (hint === i && L.tutWiggle > 0) ? Math.sin(L.tutWiggle * 40) * 4 : 0;
      for (var j = Math.min(lane.length - 1, 3); j >= 1; j--) {
        var sp = lane[j];
        drawSpool(cx, laneSpoolY(j), (L.combMode ? 19 : 17) - j, sp.color, sp.left, sp.total, sp.hidden, 0, L.combMode ? 1 : 1 - j * 0.15, sp.knot > 0);
      }
      if (lane[0]) drawSpool(cx + wig, LANES.y + 46 + Math.sin(time * 4 + i) * 1.5, Math.min(28, lw * 0.36), lane[0].color, lane[0].left, lane[0].total, lane[0].hidden, 0, 1, lane[0].knot > 0);
      else text(T('empty'), cx, LANES.y + 46, 13, 'rgba(255,230,200,0.3)', 'center', '600');
      if (lane.length > 4) text('+' + (lane.length - 4), cx, LANES.y + LANES.h - 10, 12, 'rgba(255,230,200,0.6)');
    }
  }
  function drawHand(x, y) {
    var b = Math.sin(time * 6) * 6;
    ctx.save(); ctx.translate(x + 10, y + 18 + b);
    ctx.fillStyle = 'rgba(255,255,255,0.25)'; ctx.beginPath(); ctx.arc(-10, -18 - b, 26 + Math.sin(time * 6) * 4, 0, 6.3); ctx.fill();
    ctx.fillStyle = '#ffe2c4'; ctx.strokeStyle = '#5b3a29'; ctx.lineWidth = 2;
    rr(-6, -14, 12, 26, 6); ctx.fill(); ctx.stroke();
    rr(-6, 4, 26, 22, 9); ctx.fill(); ctx.stroke();
    ctx.restore();
  }
  function drawTutorial() {
    if (!L.tut) return;
    var st = L.tut.step, idx = L.idx, msg = '';
    if (idx === 0) msg = st === 0 ? T('tut1') : L.active < L.rows - 1 ? T('tut3') : T('tut2');
    else if (idx === 1) msg = st < 2 ? T('tut4') : T('tut5');
    else msg = st < 4 ? T('tut6') : '';
    if (msg && !L.done) {
      ctx.fillStyle = 'rgba(20,10,6,0.82)'; rr(30, BOOST.y - 2, 340, 46, 14); ctx.fill();
      ctx.strokeStyle = '#e0a526'; ctx.lineWidth = 2; rr(30, BOOST.y - 2, 340, 46, 14); ctx.stroke();
      fitText(msg, LW / 2, BOOST.y + 21, 15, '#fff3dd', 320);
    }
    var h = tutHint();
    if (h >= 0) { var f = laneFront(h); drawHand(f.x, f.y); }
  }
  function drawBoosters() {
    if (L.tut && L.idx < 3) return;
    var defs = [['slot', T('bSlot')], ['comb', T('bComb')], ['shuttle', T('bShuttle')]], w = 112, gap = 12, x0 = (LW - 3 * w - 2 * gap) / 2;
    defs.forEach(function (d, i) {
      var k = d[0], x = x0 + i * (w + gap), y = BOOST.y, unlocked = boosterUnlocked(k), on = k === 'comb' && L.combMode;
      buttons.push({ id: 'b_' + k, x: x, y: y, w: w, h: BOOST.h });
      ctx.fillStyle = on ? '#e0a526' : unlocked ? 'rgba(61,42,32,0.95)' : 'rgba(40,28,22,0.7)'; rr(x, y, w, BOOST.h, 12); ctx.fill();
      ctx.strokeStyle = unlocked ? 'rgba(224,165,38,0.6)' : 'rgba(255,255,255,0.1)'; ctx.lineWidth = 1.5; rr(x, y, w, BOOST.h, 12); ctx.stroke();
      drawBoosterIcon(k, x + 22, y + BOOST.h / 2, unlocked ? 1 : 0.35);
      fitText(d[1], x + 70, y + 15, 13, unlocked ? (on ? '#2b1a14' : '#f6e7cf') : 'rgba(246,231,207,0.4)', 76);
      var sub;
      if (!unlocked) sub = T('locked') + ' ' + (UNLOCK[k] + 1);
      else if ((save.inv[k] || 0) > 0) sub = '×' + save.inv[k];
      else sub = fmtCoins(PRICE[k]) + ' ¢';
      fitText(sub, x + 70, y + 32, 12, unlocked ? (on ? '#2b1a14' : '#e0a526') : 'rgba(246,231,207,0.35)', 76);
    });
  }
  function drawBoosterIcon(k, x, y, a) {
    ctx.save(); ctx.globalAlpha = a; ctx.translate(x, y);
    if (k === 'slot') { ctx.strokeStyle = '#f6e7cf'; ctx.lineWidth = 2; ctx.setLineDash([3, 3]); ctx.beginPath(); ctx.arc(0, 0, 12, 0, 6.3); ctx.stroke(); ctx.setLineDash([]); text('+', 0, 1, 18, '#e0a526', 'center', '800'); }
    else if (k === 'comb') { ctx.fillStyle = '#c99256'; ctx.fillRect(-12, -10, 24, 7); for (var i = 0; i < 6; i++) ctx.fillRect(-11 + i * 4.4, -3, 2.4, 13); }
    else { ctx.fillStyle = '#c99256'; ctx.beginPath(); ctx.moveTo(-14, 0); ctx.quadraticCurveTo(0, -10, 14, 0); ctx.quadraticCurveTo(0, 10, -14, 0); ctx.fill(); ctx.fillStyle = '#e0a526'; ctx.fillRect(-4, -2, 8, 4); }
    ctx.restore();
  }
  function drawHUD() {
    button('menu', 10, 10, 46, 46, '‹', 'rgba(0,0,0,0.35)', '#fff', 26);
    button('restart', LW - 56, 10, 46, 46, '↻', 'rgba(0,0,0,0.35)', '#fff', 22);
    var ti = tagInfo(L.def.tag), rowNow = Math.min(L.rows, L.rows - L.active), parts = [];
    ctx.font = '800 20px sans-serif';
    var lvl = T('level') + ' ' + (L.idx + 1), rowTxt = T('row') + ' ' + Math.max(1, rowNow) + '/' + L.rows;
    var tagW = 0; if (ti) { ctx.font = '800 11px sans-serif'; tagW = Math.max(46, ctx.measureText(ti.label).width + 16); }
    ctx.font = '800 20px sans-serif'; var lw0 = ctx.measureText(lvl).width; ctx.font = '700 15px sans-serif'; var rw0 = ctx.measureText(rowTxt).width;
    var gap = 10, total = lw0 + (ti ? gap + tagW : 0) + gap + 12 + rw0, x = LW / 2 - total / 2;
    text(lvl, x, 33, 20, '#f6e7cf', 'left', '800'); x += lw0 + gap;
    if (ti) { pill(x, 24, tagW, 18, ti.c); text(ti.label, x + tagW / 2, 33.5, 11, '#fff', 'center', '800'); x += tagW + gap; }
    text('·', x + 3, 33, 18, 'rgba(246,231,207,0.5)', 'center', '800'); x += 12;
    text(rowTxt, x, 33.5, 15, '#e0a526', 'left', '700');
    var na = clamp(2.6 - (now() - L.t0) / 1000, 0, 1);
    if (na > 0) { ctx.globalAlpha = na; text(L.def.name[lang], LW / 2, 54, 11, 'rgba(246,231,207,0.7)', 'center', '600'); ctx.globalAlpha = 1; }
    if (L.streakBonus) fitText('★ ' + save.streak, LW - 70, 54, 10, '#e0a526', 60, 'right', '800');
  }
  function drawBanner() {
    if (!L.banner) return;
    var ti = tagInfo(L.def.tag); if (!ti) return;
    var t = L.banner, a = t > 1.5 ? (1.8 - t) / 0.3 : Math.min(1, t / 0.4);
    ctx.globalAlpha = clamp(a, 0, 1);
    ctx.fillStyle = ti.c; ctx.fillRect(0, 196, LW, 64);
    ctx.fillStyle = 'rgba(0,0,0,0.25)'; ctx.fillRect(0, 256, LW, 4);
    text(ti.banner, LW / 2, 228, 30, '#fff', 'center', '800');
    ctx.globalAlpha = 1;
  }
  function drawResult(kind) {
    ctx.fillStyle = 'rgba(15,8,5,0.72)'; ctx.fillRect(-offX / scale, -offY / scale, LW + 2 * offX / scale, LH + 2 * offY / scale);
    var y0 = 200, t = Math.min(1, overlayT / 0.4), s = easeOutBack(t);
    ctx.save(); ctx.translate(LW / 2, y0 + 170); ctx.scale(s, s); ctx.translate(-LW / 2, -(y0 + 170));
    var ph = kind === 'win' ? 340 : 372; if (kind !== 'win') y0 = 186;
    ctx.fillStyle = '#f3e6cf'; rr(36, y0, 328, ph, 22); ctx.fill();
    ctx.strokeStyle = kind === 'win' ? '#3e7c59' : '#c0392b'; ctx.lineWidth = 6; rr(44, y0 + 8, 312, ph - 16, 18); ctx.stroke();
    if (kind === 'win') {
      text(T('winTitle'), LW / 2, y0 + 46, 28, '#5b3a29');
      var stars = L.stars || 3;
      for (var i = 0; i < 3; i++) {
        var st = 0.35 + i * 0.18, k = clamp((overlayT - st) / 0.22, 0, 1), sx = LW / 2 + (i - 1) * 56;
        drawStar(sx, y0 + 100, 22, '#cbb89a');
        if (i < stars && k > 0) {
          if (!L.starSnd) L.starSnd = 0;
          if (L.starSnd <= i) { L.starSnd = i + 1; tone(660 * Math.pow(1.26, i), 0.16, 'triangle', 0.09); burst(sx, y0 + 100, '#ffe9a8', 10, 160); }
          ctx.save(); ctx.translate(sx, y0 + 100); var sc = easeOutBack(k) * (1 + 0.25 * Math.max(0, 1 - k * 2)); ctx.scale(sc, sc); drawStar(0, 0, 22, '#e0a526'); ctx.restore();
        }
      }
      var rw = L.reward * (L.x2 ? 2 : 1), ck = clamp((overlayT - 0.95) / 0.6, 0, 1);
      if (ck > 0 && !L.coinFly) { L.coinFly = 1; for (var q = 0; q < 10; q++) particles.push({ kind: 'fly', sx: LW / 2 - 40, sy: y0 + 148, tx: LW / 2 + 52, ty: 30, x: LW / 2 - 40, y: y0 + 148, vx: 0, vy: 0, g: 0, rot: 0, vr: 0, t: -q * 0.05, life: 0.6, size: 6, color: '#f2c14e', ox: (Math.random() - 0.5) * 60 }); }
      drawCoin(LW / 2 - 40, y0 + 148, 11); text('+' + Math.round(rw * ck), LW / 2 - 24, y0 + 148, 22, '#5b3a29', 'left', '800');
      var by = y0 + 176;
      if (L.idx >= UNLOCK.rv2x && !L.x2) {
        var left = AD.rv2xPerDay - daily().rv2x;
        if (left > 0) { button('x2', 70, by, 260, 46, T('x2') + ' · ' + left + T('perDay'), '#e0a526', '#2b1a14', 15); by += 56; }
      }
      button('next', 90, by, 220, 50, T('next'), '#3e7c59');
      if (overlayT < 1.6) text(T('tapSkip'), LW / 2, y0 + 296, 11, '#a08a70', 'center', '600');
      if (save.streak > 0 && L.idx >= UNLOCK.streak) text(T('streak') + ': ' + save.streak + (save.streak >= 3 ? ' · +1' : ' / 3'), LW / 2, y0 + 316, 13, '#7a5a40', 'center', '700');
    } else {
      var pr = progress(), tot = L.rows * L.cols, leftN = Math.round(tot * (1 - pr)), close = pr >= 0.8 || leftN <= 12;
      button('giveup', 312, y0 + 14, 40, 40, '✕', 'rgba(0,0,0,0.12)', '#7a5a40', 20);
      text(close ? T('soClose') : T('failTitle'), LW / 2, y0 + 42, 26, '#7a2e22');
      text(leftN + ' ' + (leftN === 1 ? T('left1') : T('leftN')), LW / 2, y0 + 72, 17, '#5b3a29', 'center', '800');
      // mini rug preview: woven cells solid, remaining cells outlined + highlighted
      var mcs = Math.floor(Math.min(150 / L.cols, 84 / L.rows)), mw = mcs * L.cols, mh = mcs * L.rows, mx = LW / 2 - mw / 2, my = y0 + 90;
      ctx.fillStyle = '#2B1A14'; rr(mx - 4, my - 4, mw + 8, mh + 8, 6); ctx.fill();
      for (var r = 0; r < L.rows; r++) for (var c = 0; c < L.cols; c++) {
        var cl = L.cells[r][c];
        if (cl.state === 2) { ctx.fillStyle = PAL[cl.color]; ctx.fillRect(mx + c * mcs, my + r * mcs, mcs - 0.5, mcs - 0.5); }
        else { ctx.fillStyle = rgba(PAL[cl.color], 0.3); ctx.fillRect(mx + c * mcs, my + r * mcs, mcs - 0.5, mcs - 0.5); ctx.strokeStyle = 'rgba(255,200,80,' + (0.45 + 0.35 * Math.sin(time * 5)) + ')'; ctx.lineWidth = 1; ctx.strokeRect(mx + c * mcs + 0.5, my + r * mcs + 0.5, mcs - 1.5, mcs - 1.5); }
      }
      var price = contPrice(), afford = save.coins >= price, by2 = my + mh + 16;
      button('cont', 56, by2, 288, 60, '', afford ? '#3e7c59' : '#8a7a6a', '#fff', 15);
      fitText(T('cont'), LW / 2, by2 + 20, 16, '#fff', 270);
      drawCoin(LW / 2 - 30, by2 + 42, 8); text(fmtCoins(price), LW / 2 - 18, by2 + 42, 15, '#ffe9a8', 'left', '800');
      var free = L.idx < 3;
      button('retry', 96, by2 + 72, 208, 40, free ? T('retryFree') : T('retry'), 'rgba(91,58,41,0.14)', '#5b3a29', 15);
      var ly = by2 + 128;
      if (save.streak > 0 && !free) { text(T('streakLost') + ': ' + save.streak + ' → 0', LW / 2, ly, 12, '#a04030', 'center', '700'); ly += 16; }
      if (L.continues < CONT.length - 1) text(T('nextCont') + ': ' + fmtCoins(CONT[Math.min(L.continues + 1, CONT.length - 1)]), LW / 2, ly, 11, '#8a6a55', 'center', '600');
      drawCoin(66, y0 + 348, 7); text(fmtCoins(save.coins), 78, y0 + 348, 12, '#5b3a29', 'left', '700');
    }
    ctx.restore();
    if (kind === 'win') {
      var landed = 0; for (var pi = 0; pi < particles.length; pi++) if (particles[pi].kind === 'fly') landed++;
      var rw2 = L.reward * (L.x2 ? 2 : 1), shown = save.coins - Math.round(rw2 * (1 - clamp((overlayT - 1.05) / 0.7, 0, 1)));
      var pop = landed && overlayT > 1.05 ? 1 + 0.08 * Math.sin(time * 30) : 1;
      ctx.save(); ctx.translate(LW / 2 + 52, 30); ctx.scale(pop, pop);
      ctx.fillStyle = 'rgba(0,0,0,0.45)'; rr(-48, -15, 96, 30, 15); ctx.fill(); drawCoin(-30, 0, 9); text(fmtCoins(shown), 6, 1, 14, '#ffe9a8', 'center', '800'); ctx.restore();
    }
  }

  // ---------- Menu ----------
  function topBar() {
    livesTick();
    pill(12, 12, 118, 34);
    drawHeart(32, 30, 12, '#e5484d');
    text(infActive() ? '∞' : String(save.lives), 32, 29, 11, '#fff', 'center', '800');
    var lt = infActive() ? T('infLives') : save.lives >= maxLives() ? T('full') : mmss(nextLifeMs());
    text(lt, 50, 29, 13, '#f6e7cf', 'left', '700');
    pill(138, 12, 112, 34); drawCoin(156, 29, 9); text(fmtCoins(save.coins), 170, 29, 14, '#f6e7cf', 'left', '700');
    buttons.push({ id: 'shop', x: 138, y: 12, w: 112, h: 34 });
  }
  function drawMenu() {
    topBar();
    button('lang', LW - 82, 12, 70, 34, lang === 'en' ? 'EN · tr' : 'en · TR', 'rgba(0,0,0,0.35)', '#f6e7cf', 13);
    text(T('title'), LW / 2, 80, 40, '#f6e7cf', 'center', '800');
    text(T('sub'), LW / 2, 110, 13, '#e0a526', 'center', '700');
    text(VARIANT ? T('vRegion') : T('region'), LW / 2, 138, 13, 'rgba(246,231,207,0.75)', 'center', '700');
    var pulse = 0.5 + 0.5 * Math.sin(time * 4);
    for (var i = 0; i < LEVELS.length; i++) {
      var col = i % 5, row = Math.floor(i / 5), x = 72 + col * 64, y = 182 + row * 66, d = LEVELS[i], un = levelUnlocked(i), ti = tagInfo(d.tag);
      buttons.push({ id: 'lvl' + i, x: x - 28, y: y - 28, w: 56, h: 60 });
      if (i === save.max) { ctx.strokeStyle = 'rgba(224,165,38,' + (0.4 + 0.5 * pulse) + ')'; ctx.lineWidth = 3; ctx.beginPath(); ctx.arc(x, y, 28, 0, 6.3); ctx.stroke(); }
      ctx.fillStyle = 'rgba(0,0,0,0.3)'; ctx.beginPath(); ctx.arc(x, y + 3, 24, 0, 6.3); ctx.fill();
      ctx.fillStyle = !un ? (ti ? rgba(ti.c, 0.32) : '#2e221c') : ti ? ti.c : '#5b3a29'; ctx.beginPath(); ctx.arc(x, y, 24, 0, 6.3); ctx.fill();
      ctx.strokeStyle = un ? 'rgba(255,230,200,0.35)' : 'rgba(255,230,200,0.08)'; ctx.lineWidth = 2; ctx.beginPath(); ctx.arc(x, y, 24, 0, 6.3); ctx.stroke();
      text(String(i + 1), x, y - (ti ? 3 : 0), d.tag === 'boss' ? 17 : 16, un ? '#fff' : 'rgba(255,255,255,0.3)', 'center', '800');
      if (ti) fitText(ti.label, x, y + 11, 7, un ? '#fff' : 'rgba(255,255,255,0.3)', 42, 'center', '800');
      var b = save.best[i] || 0;
      if (b) for (var s = 0; s < 3; s++) drawStar(x - 12 + s * 12, y + 29, 5, s < b ? '#e0a526' : 'rgba(255,255,255,0.18)');
    }
    if (!VARIANT) text(T('nextRegion'), LW / 2, 556, 11, 'rgba(246,231,207,0.45)', 'center', '600');
    if (save.max >= UNLOCK.streak && save.streak > 0) text(T('streak') + ': ' + save.streak + (save.streak >= 3 ? ' · +1 ' + T('bSlot').replace('+1 ', '') : ' / 3'), LW / 2, 578, 12, '#e0a526', 'center', '700');
    button('play', 70, 594, 260, 58, T('play') + ' · ' + T('level') + ' ' + (save.max + 1), '#3e7c59', '#fff', 20);
    button('shop', 40, 668, 152, 44, T('shop'), '#8a6a55', '#fff', 16);
    button('debug', 208, 668, 152, 44, T('debug'), 'rgba(0,0,0,0.4)', '#e0a526', 15);
    button('cb', 100, 712, 200, 30, save.cb ? T('cbOn') : T('cbOff'), save.cb ? '#3e7c59' : 'rgba(0,0,0,0.3)', '#f6e7cf', 12);
    if (passActive()) text(T('passOn'), LW / 2, 750, 11, '#e0a526', 'center', '700');
    text(T('note'), LW / 2, 768, 9.5, 'rgba(246,231,207,0.45)', 'center', '500');
  }

  // ---------- Modals ----------
  function panel(x, y, w, h) {
    var t = Math.min(1, (modal ? modal.t : 1) / 0.3), s = easeOutBack(t);
    ctx.translate(x + w / 2, y + h / 2); ctx.scale(s, s); ctx.translate(-(x + w / 2), -(y + h / 2));
    ctx.fillStyle = '#f3e6cf'; rr(x, y, w, h, 22); ctx.fill();
    ctx.strokeStyle = '#c0392b'; ctx.lineWidth = 5; rr(x + 7, y + 7, w - 14, h - 14, 17); ctx.stroke();
  }
  function dim() { ctx.fillStyle = 'rgba(15,8,5,0.75)'; ctx.fillRect(-offX / scale, -offY / scale, LW + 2 * offX / scale, LH + 2 * offY / scale); }
  function drawModal() {
    var m = modal; dim(); ctx.save();
    if (m.kind === 'intro') {
      panel(40, 180, 320, 360);
      var info = MECH[m.mech][lang];
      text(m.booster ? T('newBooster') : T('newMech'), LW / 2, 218, 15, '#c0392b', 'center', '800');
      text(info[0], LW / 2, 248, 26, '#5b3a29', 'center', '800');
      ctx.fillStyle = '#3a2419'; rr(130, 272, 140, 100, 14); ctx.fill();
      if (m.mech === 'hidden') { drawSpool(175, 322, 22, 'R', 4, 4, true, 0, 1); drawSpool(225, 322, 22, 'Y', 4, 4, false, 0, 1); }
      else if (m.mech === 'covered') { for (var r = 0; r < 3; r++) for (var c = 0; c < 4; c++) drawCell(160 + c * 20, 292 + r * 20, 20, { color: 'RCNY'[c], state: r === 2 ? 2 : 0, pop: 0 }, r === 1, 0.5, r === 0); }
      else if (m.mech === 'knot') drawSpool(200, 322, 26, 'N', 5, 5, false, 0, 1, true);
      else if (m.mech === 'streak') { for (var s = 0; s < 3; s++) drawStar(165 + s * 35, 322, 14, '#e0a526'); }
      else { ctx.save(); ctx.translate(200, 322); ctx.scale(2, 2); drawBoosterIcon(m.mech, 0, 0, 1); ctx.restore(); }
      wrapText(info[1], LW / 2, 400, 15, '#5b3a29', 270, 21, '600');
      if (m.booster) text('×' + FREE[m.mech] + ' ' + T('free'), LW / 2, 462, 15, '#3e7c59', 'center', '800');
      button('m_ok', 110, 478, 180, 46, T('ok'), '#3e7c59');
    } else if (m.kind === 'offer') {
      var sku = skuById(m.sku);
      panel(40, 210, 320, 300);
      text(m.title, LW / 2, 252, 22, '#5b3a29', 'center', '800');
      text(sku[lang], LW / 2, 284, 15, '#c0392b', 'center', '800');
      wrapText(m.sub, LW / 2, 318, 14, '#5b3a29', 260, 20, '600');
      button('m_buy_' + sku.id, 80, 392, 240, 50, fmtPrice(sku.price), '#3e7c59');
      button('m_close', 120, 452, 160, 38, T('close'), '#8a6a55', '#fff', 14);
    } else if (m.kind === 'confirm') {
      var sk = skuById(m.sku);
      panel(40, 230, 320, 280);
      text(sk[lang], LW / 2, 272, 20, '#5b3a29', 'center', '800');
      text(fmtPrice(sk.price), LW / 2, 304, 24, '#3e7c59', 'center', '800');
      wrapText(T('protoBuy'), LW / 2, 340, 14, '#7a2e22', 260, 20, '700');
      button('m_sim_' + sk.id, 60, 380, 280, 50, T('simBuy'), '#3e7c59', '#fff', 15);
      button('m_close', 120, 442, 160, 38, T('cancel'), '#8a6a55', '#fff', 14);
    } else if (m.kind === 'shop') {
      panel(16, 40, 368, 700);
      text(T('shop'), LW / 2, 76, 24, '#5b3a29', 'center', '800');
      text(T('protoBuy'), LW / 2, 100, 11, '#7a2e22', 'center', '700');
      SKUS.forEach(function (s, i) {
        var y = 116 + i * 46, owned = (s.oneTime && save.offers['bought_' + s.id]) || (s.noAds && !s.coins && save.noAds);
        if (m.highlight === s.id) { ctx.fillStyle = 'rgba(224,165,38,0.25)'; rr(30, y - 2, 340, 42, 10); ctx.fill(); }
        fitText(s[lang], 40, y + 12, 14, '#5b3a29', 200, 'left', '800');
        var desc = [];
        if (s.coins) desc.push(fmtCoins(s.coins) + ' ¢');
        if (s.inv) desc.push(T('bSlot') + '/' + T('bComb') + '/' + T('bShuttle'));
        if (s.noAds) desc.push(lang === 'tr' ? 'reklamsız' : 'no ads');
        if (s.pass) desc.push(lang === 'tr' ? '8 can, ödül yolu' : '8 lives, reward track');
        if (s.infMin) desc.push(s.infMin + (lang === 'tr' ? ' dk sınırsız can' : ' min ∞ lives'));
        fitText(desc.join(' · '), 40, y + 29, 10, '#8a6a55', 220, 'left', '600');
        button(owned ? 'm_owned' : 'm_buy_' + s.id, 268, y + 2, 92, 34, owned ? '✓' : fmtPrice(s.price), owned ? '#9a8a7a' : '#3e7c59', '#fff', 14);
      });
      drawCoin(44, 676, 8); text(fmtCoins(save.coins), 56, 676, 13, '#5b3a29', 'left', '800');
      text(T('bSlot') + ' ×' + (save.inv.slot || 0) + '  ' + T('bComb') + ' ×' + (save.inv.comb || 0) + '  ' + T('bShuttle') + ' ×' + (save.inv.shuttle || 0), 360, 676, 11, '#5b3a29', 'right', '700');
      button('m_close', 120, 692, 160, 36, T('close'), '#8a6a55', '#fff', 14);
    } else if (m.kind === 'lives') {
      panel(40, 200, 320, 360);
      for (var h = 0; h < 5; h++) drawHeart(120 + h * 40, 252, 14, h < save.lives ? '#e5484d' : '#cbb89a');
      text(T('outLives'), LW / 2, 292, 22, '#5b3a29', 'center', '800');
      text(T('outLivesSub') + ' ' + mmss(nextLifeMs()), LW / 2, 322, 15, '#7a5a40', 'center', '700');
      button('m_refill', 60, 344, 280, 50, T('refill') + ' · ' + fmtCoins(PRICE.refill) + ' ¢', save.coins >= PRICE.refill ? '#3e7c59' : '#9a8a7a', '#fff', 16);
      var lft = AD.rvLifePerDay - daily().rvLife;
      button('m_adlife', 60, 404, 280, 46, lft > 0 ? T('adLife') + ' · ' + lft + T('perDay') : T('limitReached'), lft > 0 ? '#e0a526' : '#9a8a7a', '#2b1a14', 15);
      button('m_shop', 60, 460, 280, 40, T('shop'), '#8a6a55', '#fff', 15);
      button('m_close', 120, 508, 160, 36, T('close'), '#8a6a55', '#fff', 14);
    } else if (m.kind === 'quit') {
      panel(50, 260, 300, 230);
      text(T('quitTitle'), LW / 2, 304, 22, '#5b3a29', 'center', '800');
      text(T('quitSub'), LW / 2, 336, 15, '#7a2e22', 'center', '700');
      button('m_stay', 75, 362, 250, 48, T('stay'), '#3e7c59');
      button('m_quit', 115, 422, 170, 40, T('quit'), '#c0392b', '#fff', 15);
    }
    ctx.restore();
  }
  function drawAd() {
    ctx.fillStyle = '#0b0b0f'; ctx.fillRect(-offX / scale, -offY / scale, LW + 2 * offX / scale, LH + 2 * offY / scale);
    text(T('adTitle'), LW / 2, 300, 26, '#fff', 'center', '800');
    text(T('adSub') + ': ' + (ad.kind === 'rv' ? 'rewarded · ' : 'interstitial · ') + ad.placement, LW / 2, 338, 13, '#aaa', 'center', '600');
    var left = ad.dur - ad.t;
    if (left > 0) text(T('adWait') + ' ' + Math.ceil(left), LW / 2, 400, 18, '#e0a526', 'center', '700');
    else button('ad_close', 120, 380, 160, 48, T('adClose'), '#e0a526', '#111', 16);
    ctx.strokeStyle = '#333'; ctx.lineWidth = 6; ctx.beginPath(); ctx.arc(LW / 2, 480, 30, -Math.PI / 2, -Math.PI / 2 + 6.283 * clamp(ad.t / ad.dur, 0, 1)); ctx.stroke();
  }
  function drawParticles() {
    for (var i = 0; i < particles.length; i++) {
      var p = particles[i], a = 1 - p.t / p.life;
      if (p.kind === 'fly') { if (p.t >= 0) drawCoin(p.x, p.y, 6); continue; }
      ctx.globalAlpha = clamp(a, 0, 1); ctx.fillStyle = p.color;
      if (p.kind === 'rect') { ctx.save(); ctx.translate(p.x, p.y); ctx.rotate(p.rot); ctx.fillRect(-p.size / 2, -p.size / 4, p.size, p.size / 2); ctx.restore(); }
      else { ctx.beginPath(); ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2); ctx.fill(); }
    }
    ctx.globalAlpha = 1;
  }
  function drawToasts() {
    for (var i = 0; i < toasts.length; i++) {
      var t = toasts[i], a = t.t < 0.15 ? t.t / 0.15 : Math.max(0, 1 - (t.t - 1.0) / 0.4);
      ctx.globalAlpha = a; var y = 300 - t.t * 20 - i * 40;
      ctx.fillStyle = 'rgba(0,0,0,0.7)'; rr(LW / 2 - 140, y - 17, 280, 34, 17); ctx.fill();
      fitText(t.msg, LW / 2, y, 15, '#fff', 264);
      ctx.globalAlpha = 1;
    }
  }
  function render() {
    ctx.setTransform(dpr * scale, 0, 0, dpr * scale, dpr * offX, dpr * offY);
    buttons = [];
    drawBackground();
    if (scene === 'menu') { drawMenu(); drawParticles(); drawToasts(); }
    else {
      drawHUD(); drawCarpet(); drawStrip(); drawBelt(); drawThreads(); drawLanes(); drawBoosters(); drawBeltSpools(); drawTutorial(); drawParticles(); drawBanner(); drawToasts();
      if (scene === 'win' || scene === 'fail') { buttons = []; drawResult(scene); drawParticles(); }
    }
    if (modal) { buttons = []; drawModal(); }
    if (ad) { buttons = []; drawAd(); }
  }

  // ---------- Input ----------
  function toLogical(e) { var t = e.changedTouches ? e.changedTouches[0] : e; return { x: (t.clientX - offX) / scale, y: (t.clientY - offY) / scale }; }
  function onDown(e) {
    if (dbgOpen) return;
    e.preventDefault(); audio();
    var p = toLogical(e);
    for (var i = buttons.length - 1; i >= 0; i--) {
      var b = buttons[i];
      if (p.x >= b.x && p.x <= b.x + b.w && p.y >= b.y && p.y <= b.y + b.h) { press(b.id); return; }
    }
    if (scene === 'win' && !modal && !ad && overlayT < 1.6) { overlayT = 1.6; return; }
    if (scene === 'play' && !modal && !ad && p.y >= LANES.y - 10 && p.y <= LANES.y + LANES.h && p.x >= LANES.x && p.x <= LANES.x + LANES.w) {
      var ry = p.y - LANES.y, depth = ry < 76 ? 0 : Math.min(3, 1 + Math.floor((ry - 80) / 28));
      tapLane(Math.floor((p.x - LANES.x) / laneW()), depth);
    }
  }
  function press(id) {
    sTap();
    if (ad) { if (id === 'ad_close' && ad.t >= ad.dur) { var cb = ad.onDone; ad = null; if (cb) cb(); } return; }
    if (modal) {
      if (id === 'm_ok' || id === 'm_close' || id === 'm_stay') { closeModal(); return; }
      if (id.indexOf('m_buy_') === 0) { var sid = id.slice(6); an('shop_tap', { sku: sid }); modal = null; modalQ = []; openModal({ kind: 'confirm', sku: sid }); return; }
      if (id.indexOf('m_sim_') === 0) { simulatePurchase(id.slice(6)); closeModal(); return; }
      if (id === 'm_refill') { if (save.coins >= PRICE.refill) { save.coins -= PRICE.refill; save.lives = maxLives(); an('lives_refill', { price: PRICE.refill }); persist(); closeModal(); } else toast(T('notEnough')); return; }
      if (id === 'm_adlife') { var d = daily(); if (d.rvLife < AD.rvLifePerDay) { modal = null; showAd('rv', 'life', function () { d.rvLife++; save.lives = Math.min(maxLives(), save.lives + 1); persist(); }); } return; }
      if (id === 'm_shop') { modal = null; openModal({ kind: 'shop' }); an('shop_open', { from: 'lives' }); return; }
      if (id === 'm_quit') { modal = null; loseLife('quit'); an('level_quit', { progress: Math.round(progress() * 100) / 100 }); scene = 'menu'; particles = []; return; }
      return;
    }
    if (id === 'lang') { lang = lang === 'en' ? 'tr' : 'en'; save.lang = lang; persist(); return; }
    if (id === 'cb') { save.cb = !save.cb; persist(); sTap(); return; }
    if (id === 'shop') { openModal({ kind: 'shop' }); an('shop_open', { from: scene }); return; }
    if (id === 'debug') { openDebug(); return; }
    if (id === 'play') { requestStart(save.max); return; }
    if (id.indexOf('lvl') === 0) { requestStart(parseInt(id.slice(3), 10)); return; }
    if (id.indexOf('b_') === 0) { useBooster(id.slice(2)); return; }
    if (id === 'menu') {
      if (scene === 'play' && L && L.taps > 0 && L.idx >= 3 && !L.ended) { openModal({ kind: 'quit' }); return; }
      scene = 'menu'; particles = []; return;
    }
    if (id === 'restart') {
      if (L && L.taps > 0 && L.idx >= 3 && !L.ended) { loseLife('restart'); an('level_quit', { progress: Math.round(progress() * 100) / 100, restart: true }); }
      requestStart(L.idx); return;
    }
    if (id === 'cont') { doContinue(); return; }
    if (id === 'retry') { failExit('retry'); return; }
    if (id === 'giveup') { failExit('menu'); return; }
    if (id === 'x2') { var dd = daily(); if (dd.rv2x < AD.rv2xPerDay && !L.x2) showAd('rv', 'win_x2', function () { dd.rv2x++; L.x2 = true; save.coins += L.reward; toast(T('x2Done')); sCoin(); persist(); }); return; }
    if (id === 'next') { if (L.idx + 1 < LEVELS.length) requestStart(L.idx + 1); else { scene = 'menu'; } return; }
  }
  cv.addEventListener('touchstart', onDown, { passive: false });
  cv.addEventListener('mousedown', function (e) { if (!('ontouchstart' in window)) onDown(e); });

  // ---------- Debug panel (DOM) ----------
  var dbgOpen = false, dbg = document.createElement('div');
  dbg.style.cssText = 'display:none;position:fixed;inset:0;z-index:10;background:rgba(20,12,8,0.97);color:#f6e7cf;overflow:auto;-webkit-overflow-scrolling:touch;font:12px -apple-system,Segoe UI,Roboto,sans-serif;padding:12px 10px 40px;touch-action:auto;user-select:text;-webkit-user-select:text';
  document.body.appendChild(dbg);
  function exportData() {
    return {
      app: 'Rugtopia prototype', version: 'v2', exportedAt: new Date().toISOString(),
      settings: { reward: REWARD, price: PRICE, continue: CONT, unlock: UNLOCK, ads: AD, refillMinutes: REFILL_MS / 60000, targets: TARGET },
      levels: LEVELS.map(function (d, i) { return { level: i + 1, name: d.name.en, tag: d.tag, size: d.p[0].length + 'x' + d.p.length, belt: d.belt, lanes: d.lanes, cap: d.cap, shuffle: d.shuffle, seed: d.seed, hidden: d.hidden, covered: d.covered, knots: d.knots, target: TARGET[d.tag] }; }),
      perLevel: save.an.lv,
      state: { coins: save.coins, lives: save.lives, inv: save.inv, streak: save.streak, max: save.max + 1, noAds: save.noAds, payer: save.payer, passActive: passActive(), interCount: save.interCount },
      events: save.an.ev
    };
  }
  function esc(s) { return String(s).replace(/[&<>]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;' }[c]; }); }
  function openDebug() {
    dbgOpen = true; livesTick();
    var ev = save.an.ev, cnt = function (t) { return ev.filter(function (e) { return e.type === t; }).length; };
    var spend = ev.filter(function (e) { return e.type === 'purchase_sim'; }).reduce(function (a, e) { return a + e.price; }, 0);
    var h = '<div style="display:flex;justify-content:space-between;align-items:center"><b style="font-size:16px">Rugtopia debug · analytics</b><button data-a="close">Close</button></div>';
    h += '<p>Coins ' + save.coins + ' · Lives ' + save.lives + '/' + maxLives() + ' · Inv slot ' + save.inv.slot + ' comb ' + save.inv.comb + ' shuttle ' + save.inv.shuttle + ' · Streak ' + save.streak + ' · NoAds ' + save.noAds + ' · Payer ' + save.payer + '</p>';
    h += '<p>Ads: rewarded ' + (cnt('ad_rv') + cnt('ad_rv_life')) + ' · interstitial ' + cnt('ad_inter') + ' · Simulated purchases ' + cnt('purchase_sim') + ' ($' + spend.toFixed(2) + ') · Events ' + ev.length + '</p>';
    h += '<div style="display:flex;flex-wrap:wrap;gap:6px;margin:8px 0">' + ['export|Export JSON', 'copy|Copy JSON', 'unlock|Unlock all', 'coins|+1000 coins', 'refill|Refill lives', 'empty|Lives → 0', 'skip|Time +30 min', 'econ|Reset economy', 'resetan|Reset analytics', 'reset|Reset save'].map(function (b) { var p = b.split('|'); return '<button data-a="' + p[0] + '">' + p[1] + '</button>'; }).join('') + '</div>';
    h += '<table style="border-collapse:collapse;width:100%;font-size:11px"><tr style="text-align:right;color:#e0a526"><th style="text-align:left">Lv</th><th style="text-align:left">Tag</th><th>Starts</th><th>Wins</th><th>Fails</th><th>Fail%</th><th>Target</th><th>1st-try win%</th><th>Near-miss</th><th>Avg win s</th><th>+Slot</th><th>Comb</th><th>Shuttle</th><th>Cont.</th><th>Quits</th></tr>';
    LEVELS.forEach(function (d, i) {
      var r = save.an.lv[i + 1] || { starts: 0, wins: 0, fails: 0, nearMiss: 0, quits: 0, continues: 0, b: {}, winTime: 0, firstTryWins: 0 };
      var fr = r.starts ? Math.round(100 * r.fails / r.starts) + '%' : '–', tg = TARGET[d.tag];
      h += '<tr style="text-align:right;border-top:1px solid #3d2a20"><td style="text-align:left">' + (i + 1) + '</td><td style="text-align:left">' + d.tag + '</td><td>' + r.starts + '</td><td>' + r.wins + '</td><td>' + r.fails + '</td><td>' + fr + '</td><td>' + tg[0] + '–' + tg[1] + '%</td><td>' + (r.wins ? Math.round(100 * r.firstTryWins / Math.max(1, r.wins)) + '%' : '–') + '</td><td>' + r.nearMiss + '</td><td>' + (r.wins ? Math.round(r.winTime / r.wins) : '–') + '</td><td>' + (r.b.slot || 0) + '</td><td>' + (r.b.comb || 0) + '</td><td>' + (r.b.shuttle || 0) + '</td><td>' + r.continues + '</td><td>' + r.quits + '</td></tr>';
    });
    h += '</table><p style="color:#bba">Last 30 events:</p><pre style="white-space:pre-wrap;font-size:10px;color:#cdb">' + esc(ev.slice(-30).reverse().map(function (e) { return JSON.stringify(e); }).join('\n')) + '</pre>';
    h += '<textarea id="dbgjson" style="display:none;width:100%;height:160px"></textarea>';
    dbg.innerHTML = h; dbg.style.display = 'block';
    Array.prototype.forEach.call(dbg.querySelectorAll('button'), function (b) {
      b.style.cssText = 'background:#3e7c59;color:#fff;border:0;border-radius:8px;padding:8px 10px;font-weight:700';
      b.onclick = function () { debugAction(b.getAttribute('data-a')); };
    });
  }
  function debugAction(a) {
    if (a === 'close') { dbgOpen = false; dbg.style.display = 'none'; return; }
    if (a === 'export') {
      var json = JSON.stringify(exportData(), null, 2);
      try { var url = URL.createObjectURL(new Blob([json], { type: 'application/json' })), el = document.createElement('a'); el.href = url; el.download = 'rugtopia-analytics-' + new Date().toISOString().slice(0, 19).replace(/:/g, '') + '.json'; document.body.appendChild(el); el.click(); el.remove(); } catch (e) {}
      var ta = document.getElementById('dbgjson'); ta.style.display = 'block'; ta.value = json; return;
    }
    if (a === 'copy') { var j = JSON.stringify(exportData()); try { navigator.clipboard.writeText(j); } catch (e) {} var t2 = document.getElementById('dbgjson'); t2.style.display = 'block'; t2.value = j; t2.select(); return; }
    if (a === 'unlock') save.max = LEVELS.length - 1;
    if (a === 'coins') save.coins += 1000;
    if (a === 'refill') { save.lives = maxLives(); save.livesTs = now(); }
    if (a === 'empty') { save.lives = 0; save.livesTs = now(); save.infUntil = 0; }
    if (a === 'skip') save.timeOffset = (save.timeOffset || 0) + REFILL_MS;
    if (a === 'resetan') save.an = { lv: {}, ev: [] };
    if (a === 'econ') { // back to a non-paying player's economy; keeps level progress, stars and analytics
      var fs0 = freshSave(); save.coins = fs0.coins; save.inv = fs0.inv; save.payer = false; save.noAds = false; save.passUntil = 0; save.infUntil = 0;
      save.lives = fs0.lives; save.livesTs = Date.now(); save.offers = {}; an('debug_econ_reset', {});
    }
    if (a === 'reset') { var keepLang = lang; save = freshSave(); save.lang = keepLang; }
    persist(); openDebug();
  }

  // ---------- Loop ----------
  var last = 0;
  function frame(ts) {
    var dt = Math.min(0.05, (ts - last) / 1000 || 0) * SPEED; last = ts;
    if (scene !== lastScene) { overlayT = 0; lastScene = scene; }
    overlayT += dt;
    update(dt); render();
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);

  // Test hook (automated tests + offline tuner)
  window.__rug = {
    LEVELS: LEVELS, SRC: SRC, TARGET: TARGET, pattern: pattern, buildLevel: buildLevel, solveLevel: solveLevel, botFailRate: botFailRate,
    mFromLevel: mFromLevel, botRun: botRun, botSkilledRun: botSkilledRun, botSkilledStats: botSkilledStats, botNewStats: botNewStats, NEW_BOT: NEW_BOT, mVisible: mVisible, rng: rng, startLevel: startLevel, requestStart: requestStart, tapLane: tapLane, useBooster: useBooster, press: press,
    state: function () { return { scene: scene, L: L, modal: modal, ad: ad, save: save }; }, setLang: function (l) { lang = l; }, T: T,
    setSpeed: function (s) { SPEED = s; }, exportData: exportData, openDebug: openDebug, debugAction: debugAction,
    closeOverlays: function () { while (modal) closeModal(); if (ad) { var cb = ad.onDone; ad = null; if (cb) cb(); } }
  };
})();
