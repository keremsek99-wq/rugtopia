const puppeteer=require('puppeteer-core');
const OUT='/workspace/ilmek-prototype/';
const URL=process.argv[2]||'file:///workspace/ilmek-prototype/index.html';
(async()=>{
  const b=await puppeteer.launch({executablePath:'/usr/bin/google-chrome',headless:'new',args:['--no-sandbox']});
  const p=await b.newPage();
  const errs=[]; p.on('pageerror',e=>errs.push(e.message)); p.on('console',m=>{if(m.type()==='error')errs.push(m.text())});
  await p.setViewport({width:390,height:844,deviceScaleFactor:2,isMobile:true,hasTouch:true});
  await p.goto(URL); await p.evaluate(()=>localStorage.clear()); await p.reload();
  const wait=ms=>new Promise(r=>setTimeout(r,ms));
  const R=[]; const log=s=>{R.push(s);console.log(s);};
  await wait(700);
  const g=await p.evaluate(()=>({w:innerWidth,h:innerHeight})); const sc=Math.min(g.w/400,g.h/780), ox=(g.w-400*sc)/2, oy=(g.h-780*sc)/2;
  const tap=(x,y)=>p.touchscreen.tap(ox+x*sc,oy+y*sc);
  const st=()=>p.evaluate(()=>{const s=__rug.state();return {scene:s.scene,modal:s.modal&&s.modal.kind,ad:s.ad&&s.ad.kind,idx:s.L&&s.L.idx,coins:s.save.coins,lives:s.save.lives,inv:s.save.inv,streak:s.save.streak,max:s.save.max,belt:s.L&&s.L.belt.length};});
  await p.screenshot({path:OUT+'screenshot-menu.png'});
  await tap(353,29); await wait(250);
  log('lang toggle -> '+await p.evaluate(()=>__rug.T('play')));
  await p.screenshot({path:OUT+'screenshot-menu-tr.png'});
  await tap(353,29); await wait(250);
  log('lang back -> '+await p.evaluate(()=>__rug.T('play')));
  // Real touch: Play -> L1 tutorial
  await tap(200,623); await wait(400);
  let s=await st(); log('touch Play -> '+s.scene+' L'+(s.idx+1));
  await p.evaluate(()=>__rug.setSpeed(4));
  const idle=async()=>{for(let k=0;k<200;k++){const ok=await p.evaluate(()=>{const S=__rug.state();if(S.ad||S.modal)return 'block';const L=S.L;if(!L||S.scene!=='play')return true;if(L.threads.length)return false;
    const hd=L.hp<L.wpath.length?L.wpath[L.hp]:null;return !L.belt.some(sp=>sp&&!sp.leaving&&sp.left>0&&!sp.knot&&hd&&hd[0]===L.active&&L.cells[hd[0]][hd[1]].color===sp.color);});if(ok===true)return true;if(ok==='block')return 'block';await wait(25);}return false;};
  // tutorial: a wrong tap must be rejected, then follow the hand with real touches
  const tut=await p.evaluate(()=>__rug.state().L.tut); log('L1 tutorial path '+JSON.stringify(tut.path));
  await wait(300); await p.screenshot({path:OUT+'screenshot-tutorial.png'});
  const lw=await p.evaluate(()=>360/__rug.state().L.lanes.length);
  const wrong=tut.path[0]===0?1:0; await tap(20+lw*(wrong+0.5),632); await wait(100);
  log('tutorial wrong-lane tap rejected: '+(await p.evaluate(()=>__rug.state().L.taps))===0);
  log('tutorial wrong tap taps='+await p.evaluate(()=>__rug.state().L.taps));
  // closes overlays (ads/modals) by real "press" ids
  const clear=async(tag)=>{for(let k=0;k<200;k++){const S=await st();if(!S.ad&&!S.modal)return;
    if(S.ad){await wait(60);await p.evaluate(()=>{const s=__rug.state();if(s.ad&&s.ad.t>=s.ad.dur)__rug.press('ad_close');});log(tag+': ad '+S.ad+' shown');await wait(30);continue;}
    if(S.modal){log(tag+': modal '+S.modal+(S.modal==='intro'?':'+await p.evaluate(()=>__rug.state().modal.mech):S.modal==='offer'?':'+await p.evaluate(()=>__rug.state().modal.sku):''));
      if(tag==='L21'&&S.modal==='intro') await p.screenshot({path:OUT+'screenshot-intro.png'});
      await p.evaluate(()=>{const m=__rug.state().modal.kind;__rug.press(m==='quit'?'m_stay':m==='intro'?'m_ok':'m_close');});await wait(30);}}};
  const playLevel=async(i,shots)=>{
    s=await st(); if(s.scene!=='play'||s.idx!==i){await p.evaluate(i=>__rug.press('lvl'+i),i);await wait(100);}
    await clear('L'+(i+1));
    const path=await p.evaluate(()=>{const L=__rug.state().L;return L.tut?L.tut.path:__rug.solveLevel(L.idx).path;});
    for(let k=0;k<path.length;k++){
      await idle();
      if(i===0){await tap(20+lw*(path[k]+0.5),632);} else await p.evaluate(l=>__rug.tapLane(l),path[k]);
      if(shots&&shots[k]){await wait(250);await p.screenshot({path:OUT+shots[k]});}
      await wait(30);
    }
    let sc2='';for(let k=0;k<200;k++){await clear('L'+(i+1)+' end');sc2=(await st()).scene;if(sc2==='win'||sc2==='fail')break;await wait(40);}
    return sc2;
  };
  const results=[];
  for(let i=0;i<30;i++){
    const shots={}; if(i===20)shots[6]='screenshot.png'; if(i===29)shots[12]='screenshot-boss.png'; if(i===14) shots[5]='screenshot-hard.png';
    const r=await playLevel(i,shots); results.push((i+1)+':'+r);
    if(i===29){await wait(500);await p.screenshot({path:OUT+'screenshot-win.png'});}
    if(r==='win'&&i<29){ await p.evaluate(()=>__rug.press('next')); await wait(60); await clear('after L'+(i+1)); }
  }
  log('play-through: '+results.join(' '));
  s=await st(); log('after 30: coins='+s.coins+' streak='+s.streak+' lives='+s.lives+' max='+(s.max+1)+' inv='+JSON.stringify(s.inv));
  // x2 rewarded on win screen
  const c0=(await st()).coins; await p.evaluate(()=>__rug.press('x2')); await wait(100); await clear('x2'); log('x2 rewarded: coins '+c0+' -> '+(await st()).coins);
  // Boosters on L26
  await p.evaluate(()=>{__rug.state().save.streak=0;__rug.press('lvl25');}); await wait(100); await clear('boost');
  const bb=await st();
  await p.evaluate(()=>__rug.useBooster('slot')); await wait(50);
  await p.evaluate(()=>__rug.useBooster('comb')); await wait(50);
  const comb=await p.evaluate(()=>__rug.state().L.combMode); 
  const deep=await p.evaluate(()=>{const L=__rug.state().L;const lane=L.lanes.findIndex(l=>l.length>2);const sp=L.lanes[lane][2];__rug.tapLane(lane,2);return L.belt.includes(sp);});
  await wait(300); await p.evaluate(()=>__rug.useBooster('shuttle')); await wait(300);
  const ba=await st(); log('boosters: belt '+bb.belt+' -> '+ba.belt+', combMode='+comb+' deepPick='+deep+', inv '+JSON.stringify(bb.inv)+' -> '+JSON.stringify(ba.inv)+' coins '+bb.coins+'->'+ba.coins);
  const brec=await p.evaluate(()=>__rug.state().save.an.lv[26].b); log('analytics boosters L26 '+JSON.stringify(brec));
  // Fail flow on L15 (hard): tap wrong-colour spools until jammed
  const doFail=async()=>{for(let k=0;k<40;k++){const sc3=(await st()).scene;if(sc3==='fail')return true;
      await p.evaluate(()=>{const L=__rug.state().L;if(!L||L.active<0)return;const row=L.cells[L.active];
        let pick=L.lanes.findIndex(l=>l[0]&&!row.some(c=>c.state===0&&c.color===l[0].color)); if(pick<0) pick=L.lanes.findIndex(l=>l.length); if(pick>=0)__rug.tapLane(pick);});await wait(120);}
    for(let k=0;k<80;k++){if((await st()).scene==='fail')return true;await wait(50);}return false;};
  await p.evaluate(()=>__rug.press('lvl14')); await wait(100); await clear('fail');
  log('fail reached: '+await doFail()); await wait(500); await p.screenshot({path:OUT+'screenshot-fail.png'});
  const f0=await st(); await p.evaluate(()=>__rug.press('cont')); await wait(100); const f1=await st();
  log('continue: scene '+f0.scene+' -> '+f1.scene+', belt '+f0.belt+' -> '+f1.belt+', coins '+f0.coins+' -> '+f1.coins+(f1.modal?' modal '+f1.modal:''));
  if(f1.modal) await clear('cont');
  if((await st()).scene==='play'){ log('2nd fail: '+await doFail()); }
  const l0=(await st()).lives; await p.evaluate(()=>__rug.press('giveup')); await wait(100); await clear('giveup'); const l1=await st();
  log('give up: lives '+l0+' -> '+l1.lives+', scene '+l1.scene);
  // Out of lives
  await p.evaluate(()=>{__rug.debugAction('empty');__rug.debugAction('close');}); await wait(50);
  await p.evaluate(()=>__rug.press('play')); await wait(200); s=await st(); log('no lives -> scene '+s.scene+' modal '+s.modal);
  await p.screenshot({path:OUT+'screenshot-lives.png'});
  await p.evaluate(()=>__rug.press('m_adlife')); await wait(100); await clear('adlife'); log('after rewarded life: lives '+(await st()).lives);
  // Shop + simulated purchase
  await p.evaluate(()=>__rug.press('shop')); await wait(400); await p.screenshot({path:OUT+'screenshot-shop.png'});
  const k0=(await st()).coins; await p.evaluate(()=>__rug.press('m_buy_coinM')); await wait(100); log('confirm modal: '+(await st()).modal);
  await p.evaluate(()=>__rug.press('m_sim_coinM')); await wait(100); log('simulated coinM: coins '+k0+' -> '+(await st()).coins);
  // Debug panel + export
  await p.evaluate(()=>__rug.press('debug')); await wait(200); await p.screenshot({path:OUT+'screenshot-debug.png'});
  const ex=await p.evaluate(()=>{__rug.debugAction('export');const t=document.getElementById('dbgjson').value;const j=JSON.parse(t);return {levels:j.levels.length,perLevel:Object.keys(j.perLevel).length,events:j.events.length,types:[...new Set(j.events.map(e=>e.type))].join(',')};});
  log('export JSON: '+JSON.stringify(ex));
  const lvsum=await p.evaluate(()=>{const lv=__rug.state().save.an.lv;return [1,15,26,30].map(n=>n+':'+JSON.stringify({s:lv[n].starts,w:lv[n].wins,f:lv[n].fails,c:lv[n].continues,q:lv[n].quits,nm:lv[n].nearMiss}));});
  log('analytics sample '+lvsum.join(' '));
  await p.evaluate(()=>__rug.debugAction('close'));
  // TR menu after progress for screenshot
  log('errors: '+JSON.stringify(errs));
  require('fs').writeFileSync('/tmp/pt/test2.log',R.join('\n'));
  await b.close();
})();
