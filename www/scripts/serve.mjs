import http from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
const root = path.resolve(fileURLToPath(new URL('../dist/', import.meta.url)));
const types = {'.html':'text/html; charset=utf-8','.css':'text/css; charset=utf-8','.js':'text/javascript; charset=utf-8','.svg':'image/svg+xml','.md':'text/plain; charset=utf-8','.json':'application/json; charset=utf-8'};
let port = Number(process.env.PORT || 4173);
const server = http.createServer(async (request, response) => {
  try {
    if (!['GET', 'HEAD'].includes(request.method)) {response.writeHead(405, {'Allow':'GET, HEAD'}); response.end(); return;}
    const parsed = new URL(request.url, 'http://localhost');
    const pathname = decodeURIComponent(parsed.pathname);
    const target = path.resolve(root, '.' + (pathname.endsWith('/') ? pathname + 'index.html' : pathname));
    if (!target.startsWith(root + path.sep)) {response.writeHead(403); response.end(); return;}
    if (!(await stat(target)).isFile()) throw new Error('Not a file');
    const content = await readFile(target);
    response.writeHead(200, {'Content-Type':types[path.extname(target)] || 'application/octet-stream','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});
    response.end(request.method === 'HEAD' ? undefined : content);
  } catch {response.writeHead(404, {'Content-Type':'text/plain; charset=utf-8'}); response.end('页面未找到');}
});
server.on('error', error => {
  if (error.code === 'EADDRINUSE' && port < 4200) {port++; server.listen(port, '127.0.0.1');}
  else {console.error(error.message);process.exit(1);}
});
server.listen(port, '127.0.0.1', () => console.log('Local: http://127.0.0.1:' + port));
process.on('SIGTERM', () => server.close(() => process.exit(0)));
process.on('SIGINT', () => server.close(() => process.exit(0)));
