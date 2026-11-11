const fs=require('fs');
const GAME='/workspace/ilmek-prototype/game.js';
const R=require(__dirname+'/stub.js')(GAME);
const {LEVELS,TARGET,solveLevel,botSkilledStats}=R;
const only=process.argv[2]?process.argv[2].split(',').map(Number):null;
function centre(i,tag){ if(tag==='normal'){const p=(i%10)/9;return 19+4*p;} if(tag==='ftue')return i<3?0:4; if(tag==='bonus')return 1; const t=TARGET[tag];return (t[0]+t[1])/2; }
const prev=(()=>{const m=fs.readFileSync(GAME,'utf8').match(/\/\*TUNED\*\/var TUNED = (.*?);\/\*END\*\//);return m&&m[1]!=='null'?JSON.parse(m[1]):null;})();
const out=[];
for(let i=0;i<LEVELS.length;i++){
  if(only&&!only.includes(i+1)){out.push(prev?prev[i]:null);continue;}
  const d=LEVELS[i],tag=d.tag,tg=TARGET[tag],c=centre(i,tag),cells=d.p.length*d.p[0].length;
  const belts=i<3?[3,4]:[3,4,5,6], lanesA=i<3?[2]:[2,3,4,5], caps=i<3?[4,5,6]:[3,4,5,6,7], shufs=i<3?[0]:[0,0.3,0.6,1], seeds=i<3?[0,1,2]:[0,1,2,3,4,5,6,7];
  const cands=[];
  for(const belt of belts)for(const lanes of lanesA)for(const cap of caps)for(const shuffle of shufs)for(const seed of seeds){
    const over=Object.assign({},d,{belt,lanes,cap,shuffle,seed});
    const st=botSkilledStats(i,over,60);
    // prefer more decisions (lower cap) a little: spools ≈ cells/cap
    cands.push({over,fr:st.fail*100,dist:Math.abs(st.fail*100-c)+ (cap>5?1.5:0)});
  }
  cands.sort((a,b)=>a.dist-b.dist);
  const top=cands.slice(0,50);
  for(const cd of top){const st=botSkilledStats(i,cd.over,500);cd.fr2=st.fail*100;cd.taps=st.taps;cd.dist2=Math.abs(cd.fr2-c)+(cd.fr2<tg[0]||cd.fr2>tg[1]?50:0)+(cd.over.cap>5?1.5:0);}
  top.sort((a,b)=>a.dist2-b.dist2);
  let pick=null;
  for(const cd of top){const s=solveLevel(i,cd.over,400000);if(s.ok){pick=cd;pick.solve=s;break;}}
  if(!pick){console.log('L'+(i+1),'NO SOLVABLE');out.push(null);continue;}
  const o=pick.over,t={belt:o.belt,lanes:o.lanes,cap:o.cap,shuffle:o.shuffle,seed:o.seed};out.push(t);
  console.log(JSON.stringify({L:i+1,tag,band:tg,c:+c.toFixed(1),fail:+pick.fr2.toFixed(1),inBand:pick.fr2>=tg[0]&&pick.fr2<=tg[1],taps:+pick.taps.toFixed(1),cells,...t}));
}
const src=fs.readFileSync(GAME,'utf8').replace(/\/\*TUNED\*\/var TUNED = .*?;\/\*END\*\//,'/*TUNED*/var TUNED = '+JSON.stringify(out)+';/*END*/');
fs.writeFileSync(GAME,src);
