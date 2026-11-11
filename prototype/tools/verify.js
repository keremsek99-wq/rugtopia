const fs=require('fs');
const R=require(__dirname+'/stub.js')(process.argv[2]||'/workspace/ilmek-prototype/game.js');
const {LEVELS,TARGET}=R;
const TAP_FAST=0.6, TAP_AVG=1.0, OVER=2;
const rows=[];
LEVELS.forEach((d,i)=>{
  const s=R.solveLevel(i,null,400000), L=R.buildLevel(i), sp=L.lanes.reduce((a,l)=>a+l.length,0), cells=L.rows*L.cols;
  const sk=R.botSkilledStats(i,null,2000), ex=R.botSkilledStats(i,null,1000,0), sl=R.botSkilledStats(i,null,1000,0.3);
  const casual=R.botFailRate(i,null,1000,0.6);
  const taps=sk.taps||sp, tF=taps*TAP_FAST+OVER, tA=taps*TAP_AVG+OVER, f=sk.fail, tg=TARGET[d.tag];
  rows.push({level:i+1,tag:d.tag,size:L.cols+'x'+L.rows,belt:d.belt,lanes:d.lanes,cap:d.cap,shuffle:d.shuffle,spools:sp,
    obstacles:[d.hidden?'hidden':'',d.covered?'covered:'+d.covered:'',d.knots?'knots:'+d.knots:''].filter(Boolean).join(' '),
    solvable:s.ok,solverNodes:s.nodes,target:tg,skilledFail:+(f*100).toFixed(1),expertFail:+(ex.fail*100).toFixed(1),sloppyFail:+(sl.fail*100).toFixed(1),casualFail:+(casual*100).toFixed(1),
    inBand:f*100>=tg[0]&&f*100<=tg[1],taps:+taps.toFixed(1),estWinSecFast:Math.round(tF),estWinSecAvg:Math.round(tA),estSecWithRetriesAvg:Math.round(tA/Math.max(0.05,1-f))});
});
console.table(rows.map(r=>({L:r.level,tag:r.tag,size:r.size,b:r.belt,ln:r.lanes,cap:r.cap,sp:r.spools,ok:r.solvable,tgt:r.target.join('-'),skilled:r.skilledFail,expert:r.expertFail,sloppy:r.sloppyFail,inBand:r.inBand,tAvg:r.estWinSecAvg,tRetry:r.estSecWithRetriesAvg})));
const byTag={};rows.forEach(r=>{(byTag[r.tag]=byTag[r.tag]||[]).push(r.skilledFail);});
const tagSum=Object.keys(byTag).map(t=>({tag:t,levels:byTag[t].length,target:TARGET[t].join('-')+'%',mean:+(byTag[t].reduce((a,b)=>a+b,0)/byTag[t].length).toFixed(1),min:Math.min(...byTag[t]),max:Math.max(...byTag[t])}));
console.table(tagSum);
const allOk=rows.every(r=>r.solvable), inb=rows.filter(r=>r.inBand).length;
const sess=[0,1,2].map(k=>rows.slice(10+k*3,13+k*3).reduce((a,r)=>a+r.estSecWithRetriesAvg,0));
console.log('all solvable without boosters:',allOk,' in band:',inb+'/30',' avg-player time, 3 consecutive normal-zone levels (s):',sess.join(', '));
fs.writeFileSync('/workspace/ilmek-prototype/tools/difficulty-report.json',JSON.stringify({version:'v3.1 (Weft Path core rule)',generated:new Date().toISOString(),
  bot:{skilled:'depth-2 lookahead over lane choices, reads the NEXT preview strip (12 stitches, not into covered rows), 15% random slips',expert:'same, 0% slips',sloppy:'same, 30% slips',casual:'v2 bot: 60% picks a lane whose front fits the shuttle tip'},
  timeModel:{secPerTapFast:TAP_FAST,secPerTapAvg:TAP_AVG,overheadSec:OVER,note:'recalibrated v3.1 from Kerem\'s v3 playtest (L1-12 win times 5-23 s; best fit 0.84 s/tap overall, about 0.5 s/tap on L11-12). Weaving overlaps with tapping, so time is modelled per tap only. fast = Kerem-like 0.6 s/tap, avg = new player 1.0 s/tap (estimate); with-retries = avg / (1 - skilled fail)'},
  tags:tagSum,levels:rows},null,1));
