// Manual browser smoke for the Key Inference "D1 / Transit" step
// (docs/ui/components/key-inference.md). Not wired to CI. Run the app on :5160 with a
// remote-debug Chrome, then:
//   CDP_PORT=9222 PERSON_ID=88 node scripts/verify-transit-ui.mjs
// The standalone /transit-wheel landing page (docs/ui/components/spec_Natal_Transit_Comp_Wheel.md)
// was folded into this page's "Current Transit" tab — the wheel + date/dasha selector + Gochara
// table now render there instead of on their own route.

import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

const APP = process.env.APP_ORIGIN ?? 'http://localhost:5160';
const CDP = process.env.CDP_PORT ?? '9222';
const PERSON = process.env.PERSON_ID ?? '88';

const list = await (await fetch(`http://127.0.0.1:${CDP}/json/list`)).json();
const socket = new WebSocket(list.find(t => t.type === 'page').webSocketDebuggerUrl);
await new Promise(resolve => socket.addEventListener('open', resolve, { once: true }));

let sequence = 0;
const pending = new Map();
const errors = [];
socket.addEventListener('message', ({ data }) => {
    const message = JSON.parse(data);
    if (message.method === 'Runtime.exceptionThrown') errors.push(message.params.exceptionDetails.text);
    if (!message.id) return;
    const request = pending.get(message.id);
    pending.delete(message.id);
    if (message.error) request.reject(new Error(JSON.stringify(message.error)));
    else request.resolve(message.result);
});
const send = (method, params = {}) => new Promise((resolve, reject) => {
    const id = ++sequence;
    pending.set(id, { resolve, reject });
    socket.send(JSON.stringify({ id, method, params }));
});
const evaluate = async expression => {
    const result = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
    if (result.exceptionDetails) throw new Error(JSON.stringify(result.exceptionDetails));
    return result.result.value;
};
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
const waitFor = async expression => {
    for (let n = 0; n < 40; n++) {
        if (await evaluate(expression)) return;
        await delay(250);
    }
    throw new Error('Timed out: ' + expression);
};

try {
    await send('Runtime.enable');
    await send('Page.enable');
    await send('Emulation.setAutoDarkModeOverride', { enabled: false });
    await send('Emulation.setDeviceMetricsOverride', { width: 1440, height: 900, deviceScaleFactor: 1, mobile: false });
    await send('Page.navigate', { url: `${APP}/key-inference/${PERSON}` });

    await waitFor(`document.querySelector('h1.ki-heading')?.textContent.trim() === 'D1-TRANSIT'`);
    await delay(500);

    // Shell: HOME pill hard-left, the two per-person tabs visible once a person is open
    // (the former standalone TRANSIT tab is gone — its content lives inside KEY INFERENCE now).
    const shell = await evaluate(`(() => {
        const tabs = [...document.querySelectorAll('.ik-tabs .ik-tab')].map(a => a.textContent.trim());
        return {
            home: document.querySelector('.ik-home')?.textContent.trim(),
            homeFirst: document.querySelector('.mud-toolbar')?.firstElementChild?.classList.contains('ik-home'),
            tabs,
            person: document.querySelector('.ik-person-name')?.textContent.trim(),
        };
    })()`);
    console.log(JSON.stringify(shell));
    assert.equal(shell.home, 'HOME');
    assert.equal(shell.homeFirst, true);
    assert.deepEqual(shell.tabs, ['ALL CHARTS', 'KEY INFERENCE']);
    assert.ok(shell.person && shell.person.length > 0);

    // D1 Birth tab (active by default) — persisted rows via PlanetPositionsTable, Lagna first.
    const d1 = await evaluate(`(() => {
        const rows = [...document.querySelectorAll('.ppt tbody tr')];
        return {
            count: rows.length,
            firstPlanet: rows[0]?.children[0]?.textContent.trim(),
            headers: [...document.querySelectorAll('.ppt thead th')].map(th => th.textContent.replace(/\\s+/g, ' ').trim()),
        };
    })()`);
    console.log(JSON.stringify(d1));
    assert.ok(d1.count >= 8, `expected >= 8 D1 rows, got ${d1.count}`);
    assert.match(d1.firstPlanet, /Ascendant|Lagna/);
    assert.ok(d1.headers.includes('Graha') && d1.headers.includes('Nakshatra'),
        `unexpected D1 Birth headers: ${d1.headers.join(', ')}`);

    // Switch to Current Transit — wheel + date/dasha selector + a table or the documented
    // CLI-hint empty state, all migrated in from the old /transit-wheel page.
    await evaluate(`[...document.querySelectorAll('.ki-tabs .mud-tab')].find(t => /Current Transit/i.test(t.textContent)).click()`);
    await delay(400);
    const transit = await evaluate(`(() => {
        const alert = document.querySelector('.ki-alert')?.textContent.trim();
        const headers = [...document.querySelectorAll('.ki-table thead th')].map(th => th.textContent.replace(/\\s+/g, ' ').trim());
        const fields = [...document.querySelectorAll('.ki-controls .ki-field span')].map(s => s.textContent.trim());
        return { alert, headers, fields, rows: document.querySelectorAll('.ki-table tbody tr').length };
    })()`);
    console.log(JSON.stringify(transit));
    assert.deepEqual(transit.fields, ['Current transit date', 'Mahadasha', 'Antardasha', 'Pratyantar']);
    assert.ok(transit.alert?.includes('backfill-planet-transits') || transit.headers.includes('House from D1'),
        'Current Transit shows neither the table nor the backfill hint');

    // Layout: square wheel placement, no horizontal page scroll.
    const desktop = await evaluate(`({
        scroll: document.documentElement.scrollWidth, width: innerWidth,
        wheel: (() => { const r = document.querySelector('.ki-wheel svg.ntw').getBoundingClientRect(); return { w: r.width, h: r.height }; })(),
    })`);
    console.log(JSON.stringify(desktop));
    assert.ok(desktop.scroll <= desktop.width, `horizontal overflow: ${desktop.scroll} > ${desktop.width}`);
    assert.ok(Math.abs(desktop.wheel.w - desktop.wheel.h) < 2, 'wheel placement is not square');
    await fs.mkdir('reports', { recursive: true });
    await fs.writeFile('reports/transit-landing-desktop.jpg',
        Buffer.from((await send('Page.captureScreenshot', { format: 'jpeg', quality: 45 })).data, 'base64'));

    // Narrow viewport: the date/dasha selector stacks to one column, still no horizontal scroll.
    await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 844, deviceScaleFactor: 1, mobile: false });
    await delay(400);
    const mobile = await evaluate(`({
        scroll: document.documentElement.scrollWidth, width: innerWidth,
        cols: getComputedStyle(document.querySelector('.ki-controls')).gridTemplateColumns.split(' ').length,
    })`);
    console.log(JSON.stringify(mobile));
    assert.ok(mobile.scroll <= mobile.width + 1, `mobile horizontal overflow: ${mobile.scroll} > ${mobile.width}`);
    assert.equal(mobile.cols, 1, 'selector columns did not stack at 390px');
    await fs.writeFile('reports/transit-landing-mobile.jpg',
        Buffer.from((await send('Page.captureScreenshot', { format: 'jpeg', quality: 45, captureBeyondViewport: true })).data, 'base64'));

    assert.deepEqual(errors, [], 'JS exceptions were thrown');
    console.log(JSON.stringify({
        result: 'passed',
        checks: ['HOME pill hard-left', 'per-person tabs revealed', 'context band person', 'D1 Birth persisted rows',
                 'D1 Birth Lagna first', 'Current Transit selector + table or backfill hint', 'square wheel placement',
                 'no horizontal overflow (1440 + 390)', 'selector columns stack at 390', 'no JS exceptions'],
    }, null, 2));
} finally {
    socket.close();
}
