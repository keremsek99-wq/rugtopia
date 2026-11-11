const puppeteer=require('puppeteer-core');
(async()=>{const b=await puppeteer.launch({executablePath:'/usr/bin/google-chrome',headless:'new',args:['--no-sandbox']});
const p=await b.newPage();await p.setViewport({width:390,height:844,deviceScaleFactor:2,isMobile:true,hasTouch:true});
const w=ms=>new Promise(r=>setTimeout(r,ms));const U=process.argv[2]||'file:///workspace/ilmek-prototype/index.html';
await p.goto(U+'?variant=weft');await p.evaluate(()=>localStorage.clear());await p.reload();await w(500);
console.log('variant route levels',await p.evaluate(()=>__rug.LEVELS.length));
await p.evaluate(()=>{__rug.startLevel(2);__rug.closeOverlays();});await w(400);
const path=await p.evaluate(()=>__rug.state().L.tut.path);
for(let k=0;k<2;k++){await p.evaluate(l=>__rug.tapLane(l),path[k]);await w(900);}
await p.screenshot({path:'/workspace/ilmek-prototype/screenshot-tutorial-l3.png'});
await p.evaluate(()=>{__rug.startLevel(1);__rug.closeOverlays();});await w(400);
const p2=await p.evaluate(()=>__rug.state().L.tut.path);
for(let k=0;k<3;k++){await p.evaluate(l=>__rug.tapLane(l),p2[k]);await w(700);}
await p.screenshot({path:'/workspace/ilmek-prototype/screenshot-tutorial-l2.png'});
await b.close();})();
