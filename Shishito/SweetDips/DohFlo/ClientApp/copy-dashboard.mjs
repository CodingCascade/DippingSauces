import { cp, mkdir, readdir, rm } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { dirname, join, resolve } from 'node:path';

const root = dirname(fileURLToPath(import.meta.url));
const source = join(root, 'dist/dohflo-dashboard/browser');
const app = resolve(root, '../DohFlo');
const assets = join(app, 'wwwroot/dashboard');
const shell = join(app, 'DashboardAssets');

let entries;
try {
  entries = await readdir(source);
} catch (error) {
  throw new Error('Angular output was not found. Run ng build first.', { cause: error });
}
if (!entries.includes('index.html')) {
  throw new Error('Angular index.html was not found. Run ng build first.');
}

await rm(assets, { recursive: true, force: true });
await mkdir(assets, { recursive: true });
await mkdir(shell, { recursive: true });
for (const name of entries) {
  if (name !== 'index.html') {
    await cp(join(source, name), join(assets, name), { recursive: true });
  }
}
await cp(join(source, 'index.html'), join(shell, 'index.html'));
console.log('Copied dashboard assets and protected HTML.');
