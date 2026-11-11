const R=require(__dirname+'/stub.js')('/workspace/ilmek-prototype/game.js');
R.LEVELS.forEach((d,i)=>{const t=Date.now();const s=R.solveLevel(i,null,200000);const b=R.botSkilledStats(i,null,200);const b0=R.botSkilledStats(i,null,200,0);
const L=R.buildLevel(i);const n=L.lanes.reduce((a,l)=>a+l.length,0);
console.log(i+1,d.tag,d.p[0].length+'x'+d.p.length,'spools',n,'solv',s.ok,s.nodes,'skilled',(b.fail*100).toFixed(0),'expert',(b0.fail*100).toFixed(0),'taps',b.taps.toFixed(0),(Date.now()-t)+'ms');});
