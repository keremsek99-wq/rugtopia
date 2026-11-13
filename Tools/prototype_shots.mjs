// Headless captures of the HTML prototype for the side-by-side comparison.
import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';

const chrome = '/usr/local/bin/google-chrome';
const port = 9333;
const outDir = new URL('../docs/screenshots/prototype-match/', import.meta.url);

mkdirSync(outDir, { recursive: true });
const proc = spawn(chrome, [
  '--headless=new', '--disable-gpu', '--no-sandbox', '--hide-scrollbars',
  `--remote-debugging-port=${port}`, '--window-size=400,780', 'about:blank'
], { stdio: 'ignore' });

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
async function json(path) {
  const res = await fetch(`http://127.0.0.1:${port}${path}`);
  return res.json();
}
let ws;
for (let i = 0; i < 40; i++) {
  try {
    const list = await json('/json/list');
    const page = list.find((t) => t.type === 'page');
    if (page) { ws = page.webSocketDebuggerUrl; break; }
  } catch { /* chrome still booting */ }
  await sleep(150);
}
if (!ws) { proc.kill(); throw new Error('chrome did not start'); }

const sock = new WebSocket(ws);
let id = 0;
const pending = new Map();
sock.addEventListener('message', (ev) => {
  const msg = JSON.parse(ev.data);
  if (msg.id && pending.has(msg.id)) pending.get(msg.id)(msg);
});
await new Promise((r) => sock.addEventListener('open', r, { once: true }));
function send(method, params = {}) {
  const msgId = ++id;
  return new Promise((resolve, reject) => {
    pending.set(msgId, (msg) => {
      pending.delete(msgId);
      if (msg.error) reject(new Error(JSON.stringify(msg.error)));
      else resolve(msg.result);
    });
    sock.send(JSON.stringify({ id: msgId, method, params }));
  });
}
await send('Page.enable');
await send('Runtime.enable');
await send('Emulation.setDeviceMetricsOverride', { width: 400, height: 780, deviceScaleFactor: 2, mobile: true });
await send('Page.navigate', { url: 'file:///workspace/prototype/index.html' });
await sleep(600);
async function evalJs(expression) {
  const result = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true });
  if (result.exceptionDetails) throw new Error(JSON.stringify(result.exceptionDetails));
  return result.result && result.result.value;
}
await evalJs(`window.__rug.setLang('en'); window.__rug.startLevel(0); true`);
await sleep(500);
async function shot(name) {
  const { data } = await send('Page.captureScreenshot', { format: 'png' });
  writeFileSync(new URL(name, outDir), Buffer.from(data, 'base64'));
  console.log('wrote', name);
}
await shot('js-playing.png');
const taps = await evalJs(`(() => {
  const sol = window.__rug.solveLevel(0, null, 200000);
  return (sol && sol.path) ? sol.path : [];
})()`);
const mid = Math.floor(taps.length / 2);
for (let i = 0; i < taps.length; i++) {
  await evalJs(`window.__rug.tapLane(${taps[i]}, 0); true`);
  await sleep(i + 1 === mid ? 900 : 700);
  if (i + 1 === mid) await shot('js-mid.png');
}
await sleep(1600);
await shot('js-win.png');
sock.close();
proc.kill();
console.log('taps', taps.length);
