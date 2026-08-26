import http from 'node:http';
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const dataDirectory = path.join(here, 'data');
const dataFile = path.join(dataDirectory, 'store.json');
const port = Number(process.env.PORT || 3000);
const adminEmail = process.env.ADMIN_EMAIL || 'admin@site.local';
const adminPassword = process.env.ADMIN_PASSWORD || 'admin123!';
const tokenSecret = process.env.TOKEN_SECRET || 'replace-this-development-secret';

fs.mkdirSync(dataDirectory, { recursive: true });
if (!fs.existsSync(dataFile)) {
  fs.writeFileSync(dataFile, JSON.stringify({ posts: [], subscribers: [] }, null, 2));
}

function readStore() {
  return JSON.parse(fs.readFileSync(dataFile, 'utf8'));
}

function writeStore(store) {
  fs.writeFileSync(dataFile, JSON.stringify(store, null, 2));
}

function json(response, status, body) {
  response.writeHead(status, {
    'Content-Type': 'application/json; charset=utf-8',
    'Access-Control-Allow-Origin': process.env.CORS_ORIGIN || '*',
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS'
  });
  response.end(JSON.stringify(body));
}

function slugify(value) {
  return value.toLocaleLowerCase('tr-TR').trim()
    .replace(/[ç]/g, 'c').replace(/[ğ]/g, 'g').replace(/[ı]/g, 'i')
    .replace(/[ö]/g, 'o').replace(/[ş]/g, 's').replace(/[ü]/g, 'u')
    .replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
}

function createToken() {
  const expiresAt = Date.now() + 8 * 60 * 60 * 1000;
  const payload = Buffer.from(JSON.stringify({ expiresAt })).toString('base64url');
  const signature = crypto.createHmac('sha256', tokenSecret).update(payload).digest('base64url');
  return `${payload}.${signature}`;
}

function authenticated(request) {
  const token = request.headers.authorization?.replace(/^Bearer\s+/i, '');
  if (!token) return false;
  const [payload, signature] = token.split('.');
  const expected = crypto.createHmac('sha256', tokenSecret).update(payload).digest('base64url');
  if (!signature || signature.length !== expected.length || !crypto.timingSafeEqual(Buffer.from(signature), Buffer.from(expected))) return false;
  try { return JSON.parse(Buffer.from(payload, 'base64url')).expiresAt > Date.now(); } catch { return false; }
}

function body(request) {
  return new Promise((resolve, reject) => {
    let raw = '';
    request.on('data', chunk => { raw += chunk; if (raw.length > 1_000_000) request.destroy(); });
    request.on('end', () => { try { resolve(raw ? JSON.parse(raw) : {}); } catch { reject(new Error('Geçersiz JSON.')); } });
    request.on('error', reject);
  });
}

function publicPost(post) {
  const { id, title, slug, excerpt, content, coverImage, status, createdAt, updatedAt, publishedAt } = post;
  return { id, title, slug, excerpt, content, coverImage, status, createdAt, updatedAt, publishedAt };
}

const server = http.createServer(async (request, response) => {
  const url = new URL(request.url, `http://${request.headers.host}`);
  const route = url.pathname;
  if (request.method === 'OPTIONS') return json(response, 204, {});
  if (request.method === 'GET' && route === '/api/health') return json(response, 200, { ok: true });

  if (request.method === 'POST' && route === '/api/auth/login') {
    try {
      const input = await body(request);
      if (input.email !== adminEmail || input.password !== adminPassword) return json(response, 401, { error: 'E-posta veya parola hatalı.' });
      return json(response, 200, { token: createToken(), expiresIn: 28800 });
    } catch (error) { return json(response, 400, { error: error.message }); }
  }

  const store = readStore();
  store.subscribers ||= [];
  if (request.method === 'POST' && route === '/api/subscriptions') {
    try {
      const input = await body(request);
      const email = input.email?.trim().toLowerCase();
      if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return json(response, 422, { error: 'Geçerli bir e-posta adresi girin.' });
      if (!store.subscribers.some(item => item.email === email)) {
        store.subscribers.push({ id: crypto.randomUUID(), email, createdAt: new Date().toISOString() });
        writeStore(store);
      }
      return json(response, 201, { message: 'Aboneliğiniz kaydedildi.' });
    } catch (error) { return json(response, 400, { error: error.message }); }
  }
  if (request.method === 'GET' && route === '/api/posts') {
    const posts = store.posts.filter(post => post.status === 'published').sort((a, b) => new Date(b.publishedAt) - new Date(a.publishedAt));
    return json(response, 200, { posts: posts.map(publicPost) });
  }
  if (request.method === 'GET' && route.startsWith('/api/posts/')) {
    const key = decodeURIComponent(route.slice('/api/posts/'.length));
    const post = store.posts.find(item => (item.id === key || item.slug === key) && item.status === 'published');
    return post ? json(response, 200, { post: publicPost(post) }) : json(response, 404, { error: 'Yazı bulunamadı.' });
  }

  if (!route.startsWith('/api/admin/')) return json(response, 404, { error: 'Endpoint bulunamadı.' });
  if (!authenticated(request)) return json(response, 401, { error: 'Yönetici oturumu gerekli.' });
  if (request.method === 'GET' && route === '/api/admin/posts') return json(response, 200, { posts: store.posts.sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt)), subscribers: store.subscribers.length });

  if (request.method === 'POST' && route === '/api/admin/posts') {
    try {
      const input = await body(request);
      if (!input.title?.trim() || !input.content?.trim()) return json(response, 422, { error: 'Başlık ve içerik zorunludur.' });
      const now = new Date().toISOString();
      const baseSlug = slugify(input.slug || input.title);
      const slug = `${baseSlug || 'yazi'}-${crypto.randomUUID().slice(0, 6)}`;
      const post = { id: crypto.randomUUID(), title: input.title.trim(), slug, excerpt: input.excerpt?.trim() || '', content: input.content.trim(), coverImage: input.coverImage?.trim() || '', status: input.status === 'published' ? 'published' : 'draft', createdAt: now, updatedAt: now, publishedAt: input.status === 'published' ? now : null };
      store.posts.push(post); writeStore(store); return json(response, 201, { post });
    } catch (error) { return json(response, 400, { error: error.message }); }
  }

  const id = route.replace('/api/admin/posts/', '');
  const index = store.posts.findIndex(post => post.id === id);
  if (index < 0) return json(response, 404, { error: 'Yazı bulunamadı.' });
  if (request.method === 'DELETE') { store.posts.splice(index, 1); writeStore(store); return json(response, 204, {}); }
  if (request.method === 'PUT') {
    try {
      const input = await body(request);
      const previous = store.posts[index];
      const status = input.status === 'published' ? 'published' : 'draft';
      const post = { ...previous, ...input, id: previous.id, slug: input.slug ? slugify(input.slug) : previous.slug, status, updatedAt: new Date().toISOString(), publishedAt: status === 'published' ? (previous.publishedAt || new Date().toISOString()) : null };
      store.posts[index] = post; writeStore(store); return json(response, 200, { post });
    } catch (error) { return json(response, 400, { error: error.message }); }
  }
  return json(response, 405, { error: 'Bu endpoint için yöntem desteklenmiyor.' });
});

server.listen(port, () => console.log(`API http://localhost:${port}/api/health adresinde hazır.`));
