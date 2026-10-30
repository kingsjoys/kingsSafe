import { cp, mkdir, readdir, rm, access } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
const root = fileURLToPath(new URL('../', import.meta.url));
const source = path.join(root, 'src');
const projectDocs = path.resolve(root, '../docs');
const publicDocs = new Set(['overview.md','integration.md','protection.md','exercises.md','architecture.md','validation.md','sources.md']);
try {
  await access(projectDocs);
  await mkdir(path.join(source, 'content'), {recursive:true});
  for (const entry of await readdir(projectDocs)) {
    if (publicDocs.has(entry)) await cp(path.join(projectDocs, entry), path.join(source, 'content', entry));
  }
} catch (error) {
  if (error.code !== 'ENOENT') throw error;
  await access(path.join(source, 'content/overview.md'));
}
for (const entry of await readdir(path.join(source, 'content'))) {
  if (entry.endsWith('.md') && !publicDocs.has(entry)) await rm(path.join(source, 'content', entry));
}
await rm(path.join(root, 'dist'), {recursive:true, force:true});
await cp(source, path.join(root, 'dist'), {recursive:true});
console.log('Built kingsSafe website to dist/. Project documentation synchronized.');
