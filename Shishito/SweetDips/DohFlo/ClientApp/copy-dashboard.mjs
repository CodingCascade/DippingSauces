import { access, cp, mkdir, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, join, resolve } from 'node:path';

const root = dirname(fileURLToPath(import.meta.url));
const source = join(root, 'dist/dohflo-dashboard/browser');
const app = resolve(root, '../DohFlo');
const assets = join(app, 'wwwroot/dashboard');
const metadata = join(app, 'DashboardAssets');

// Read Angular's generated entry page, so hashed filenames stay automatic.
const indexHtml = await readFile(join(source, 'index.html'), 'utf8');

function assetName(url, extension) {
  const name = url.replace(/^\/dashboard\//, '').replace(/^\.\//, '');

  if (!new RegExp(`^[A-Za-z0-9._-]+\\.${extension}$`).test(name)) {
    throw new Error(`Unexpected dashboard asset path: ${url}`);
  }

  return name;
}

const scripts = [...indexHtml.matchAll(/<script\b[^>]*\bsrc="([^"]+)"[^>]*>/gi)]
  .map(match => assetName(match[1], 'js'));
const stylesheets = [...indexHtml.matchAll(/<link\b[^>]*>/gi)]
  .map(match => match[0])
  .filter(tag => /\brel="stylesheet"/i.test(tag))
  .map(tag => {
    const href = tag.match(/\bhref="([^"]+)"/i);

    if (!href) {
      throw new Error('A dashboard stylesheet is missing its href.');
    }

    return assetName(href[1], 'css');
  });

if (scripts.length === 0) {
  throw new Error('No Angular entry scripts were found. Build Angular first.');
}

const manifest = {
  Scripts: [...new Set(scripts)],
  Stylesheets: [...new Set(stylesheets)]
};

// Check the entry files before replacing the previous generated output.
for (const name of [...manifest.Scripts, ...manifest.Stylesheets]) {
  await access(join(source, name));
}

const entries = await readdir(source);
await rm(assets, { recursive: true, force: true });
await mkdir(assets, { recursive: true });
await mkdir(metadata, { recursive: true });

for (const name of entries) {
  if (name !== 'index.html') {
    await cp(join(source, name), join(assets, name), { recursive: true });
  }
}

await writeFile(
  join(metadata, 'dashboard-assets.json'),
  JSON.stringify(manifest, null, 2),
  'utf8'
);

// The MVC view now supplies the page shell.
await rm(join(metadata, 'index.html'), { force: true });
console.log('Copied dashboard assets and generated dashboard-assets.json.');
