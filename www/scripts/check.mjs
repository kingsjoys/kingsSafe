import { readFile, access, readdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
const root = fileURLToPath(new URL('../', import.meta.url));
const dist = path.join(root, 'dist');
let failures = 0;
for (const name of ['index.html','docs.html']) {
  const html = await readFile(path.join(dist,name),'utf8');
  for (const match of html.matchAll(/(?:href|src)="([^"]+)"/g)) {
    const url = match[1];
    if (/^(https?:|#|mailto:)/.test(url)) continue;
    const pathname = url.split('?')[0].split('#')[0];
    try {await access(path.resolve(dist, pathname === './' ? 'index.html' : pathname));}
    catch {console.error('Broken resource:',name,url);failures++;}
  }
  for (const match of html.matchAll(/href="#([^"]+)"/g)) {
    if (!html.includes('id="' + match[1] + '"')) {console.error('Missing anchor',name,match[1]);failures++;}
  }
  if (!html.includes('lang="zh-CN"') || !html.includes('name="viewport"')) failures++;
}
for (const name of ['app.js','docs.js']) {
  const result = spawnSync(process.execPath,['--check',path.join(dist,name)],{encoding:'utf8'});
  if (result.status !== 0) {console.error(result.stderr);failures++;}
}
const docs=await readdir(path.join(dist,'content'));
if(docs.filter(name=>name.endsWith('.md')).length !== 7) {console.error('Expected 7 documentation pages'); failures++;}
if(failures) process.exit(1);
console.log('Passed: static resources, homepage anchors, JavaScript syntax and 7 documentation pages.');
