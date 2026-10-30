const documents = {
  overview: '产品介绍', integration: '接入与构建', protection: '三项核心防护',
  exercises: '攻防演练', architecture: '技术方案', validation: '评测方法', sources: '来源索引'
};
const article = document.getElementById('doc-article');
let requestId = 0;
function escapeHtml(value) {
  return value.replace(/[&<>"']/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
}
function inline(value) {
  let safe = escapeHtml(value);
  const saved = [];
  safe = safe.replace(/`([^`]+)`/g, (_, code) => {
    const index = saved.push('<code>' + code + '</code>') - 1;
    return '\u0001' + index + '\u0002';
  });
  safe = safe.replace(/\[([^\]]+)\]\(([^)]+)\)/g, (_, label, url) => {
    const decoded = url.replace(/&amp;/g, '&');
    if (!/^(https?:\/\/|[\w.-]+\.md(?:#[^ ]*)?$|#[^ ]*)/.test(decoded)) return label;
    if (!decoded.startsWith('http') && !decoded.startsWith('#')) {
      const name = decoded.split('#')[0].replace(/\.md$/, '');
      if (!documents[name]) return label;
      url = 'docs.html?doc=' + name;
    }
    const external = decoded.startsWith('http');
    return '<a href="' + url + '"' + (external ? ' target="_blank" rel="noopener noreferrer"' : '') + '>' + label + '</a>';
  });
  safe = safe.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
  return safe.replace(/\u0001(\d+)\u0002/g, (_, index) => saved[Number(index)]);
}
function renderMarkdown(markdown) {
  const lines = markdown.replace(/\r\n/g, '\n').split('\n');
  const output = [];
  let index = 0;
  while (index < lines.length) {
    const line = lines[index];
    if (!line.trim()) {index++; continue;}
    if (line.startsWith('```')) {
      const language = line.slice(3).trim();
      const code = [];
      index++;
      while (index < lines.length && !lines[index].startsWith('```')) code.push(lines[index++]);
      index++;
      if (language === 'mermaid') output.push('<p><small>以下为技术流程的文本定义，可下载原始文档查看。</small></p>');
      output.push('<pre><code>' + escapeHtml(code.join('\n')) + '</code></pre>');
      continue;
    }
    const heading = line.match(/^(#{1,6})\s+(.+)$/);
    if (heading) {
      const level = heading[1].length;
      const id = heading[2].toLowerCase().replace(/[^\p{L}\p{N}\s-]/gu, '').replace(/\s+/g, '-');
      output.push('<h' + level + ' id="' + escapeHtml(id) + '">' + inline(heading[2]) + '</h' + level + '>');
      index++; continue;
    }
    if (line.startsWith('|') && lines[index + 1] && /^\|[\s:|-]+\|$/.test(lines[index + 1])) {
      const cells = row => row.trim().replace(/^\||\|$/g, '').split('|').map(cell => cell.trim());
      const header = cells(line);
      index += 2;
      const rows = [];
      while (index < lines.length && lines[index].startsWith('|')) rows.push(cells(lines[index++]));
      output.push('<div class="doc-table-wrap"><table><thead><tr>' + header.map(cell => '<th scope="col">' + inline(cell) + '</th>').join('') + '</tr></thead><tbody>' + rows.map(row => '<tr>' + row.map(cell => '<td>' + inline(cell) + '</td>').join('') + '</tr>').join('') + '</tbody></table></div>');
      continue;
    }
    if (/^(- |\d+\. )/.test(line)) {
      const ordered = /^\d+\./.test(line);
      const tag = ordered ? 'ol' : 'ul';
      const rows = [];
      while (index < lines.length && (ordered ? /^\d+\. / : /^- /).test(lines[index])) rows.push('<li>' + inline(lines[index++].replace(/^(- |\d+\. )/, '')) + '</li>');
      output.push('<' + tag + '>' + rows.join('') + '</' + tag + '>');
      continue;
    }
    const paragraph = [];
    while (index < lines.length && lines[index].trim() && !/^(#{1,6} |```|\||- |\d+\. )/.test(lines[index])) paragraph.push(lines[index++]);
    if (paragraph.length) output.push('<p>' + inline(paragraph.join(' ')) + '</p>');
    else {output.push('<p>' + inline(lines[index++]) + '</p>');}
  }
  return output.join('\n');
}
async function loadDocument(name, updateHistory = false) {
  if (!documents[name]) name = 'overview';
  const currentRequest = ++requestId;
  article.setAttribute('aria-busy', 'true');
  document.getElementById('doc-label').textContent = documents[name];
  document.getElementById('doc-download').href = 'content/' + name + '.md';
  document.getElementById('doc-select').value = name;
  document.querySelectorAll('[data-doc]').forEach(link => {
    const active = link.dataset.doc === name;
    link.classList.toggle('active', active);
    if (active) link.setAttribute('aria-current', 'page');
    else link.removeAttribute('aria-current');
  });
  document.title = documents[name] + ' — kingsSafe';
  if (updateHistory) history.pushState({doc:name}, '', 'docs.html?doc=' + name);
  try {
    const response = await fetch('content/' + name + '.md');
    if (!response.ok) throw new Error('Document unavailable');
    const markdown = await response.text();
    if (currentRequest !== requestId) return;
    article.innerHTML = renderMarkdown(markdown);
  } catch {
    if (currentRequest !== requestId) return;
    article.innerHTML = '<div class="doc-error">文档暂时无法加载。请刷新页面，或使用上方的原始文档下载链接。</div>';
  } finally {
    if (currentRequest === requestId) article.setAttribute('aria-busy', 'false');
  }
}
document.querySelectorAll('[data-doc]').forEach(link => link.addEventListener('click', event => {
  if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
  event.preventDefault();
  loadDocument(link.dataset.doc, true);
  window.scrollTo({top:0, behavior:'instant'});
}));
document.getElementById('doc-select').addEventListener('change', event => {
  loadDocument(event.target.value, true);
  window.scrollTo({top:0, behavior:'instant'});
});
window.addEventListener('popstate', () => loadDocument(new URLSearchParams(location.search).get('doc')));
loadDocument(new URLSearchParams(location.search).get('doc'));
