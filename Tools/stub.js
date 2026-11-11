// minimal DOM stub to load game.js in node
const fs=require('fs');
function ctxStub(){return new Proxy({}, {get:(t,k)=>{ if(k in t) return t[k]; if(k==='measureText') return ()=>({width:10}); if(k==='createLinearGradient'||k==='createRadialGradient') return ()=>({addColorStop(){}}); return ()=>{}; }, set:(t,k,v)=>{t[k]=v;return true;}});}
const el=()=>({style:{},getContext:()=>ctxStub(),addEventListener(){},appendChild(){},querySelectorAll:()=>[],setAttribute(){},remove(){},click(){}});
global.window=global; global.document={getElementById:()=>el(),createElement:()=>el(),body:el(),addEventListener(){}};
global.innerWidth=400; global.innerHeight=780; global.devicePixelRatio=1; global.addEventListener=()=>{};
global.localStorage={_d:{},getItem(k){return this._d[k]||null},setItem(k,v){this._d[k]=v},removeItem(k){delete this._d[k]}};
global.requestAnimationFrame=()=>{}; global.navigator={vibrate(){}};
global.AudioContext=undefined;
module.exports=function(path){ new Function(fs.readFileSync(path||'/workspace/ilmek-prototype/game.js','utf8'))(); return global.__rug; };
