// Manual browser smoke for the v2 Transit landing page (docs/ui/components/transit.md).
// Not wired to CI. Run the app on :5160 with a remote-debug Chrome, then:
//   CDP_PORT=9222 PERSON_ID=88 node scripts/verify-transit-ui.mjs
// Rewritten for v2 (two-tab D1 Birth / Current Transit table); the v1 version drove the
// old natal↔transit selector page.

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
    await send('Page.navigate', { url: `${APP}/transit-wheel/${PERSON}` });

    await waitFor(`document.querySelector('h1.transit-heading')?.textContent.trim() === 'TRANSIT - D1 BIRTH CHART'`);
    await delay(500);

    // Shell: HOME pill hard-left, the three per-person tabs visible once a person is open.
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
    assert.deepEqual(shell.tabs, ['TRANSIT', 'ALL CHARTS', 'KEY INFERENCE']);
    assert.ok(shell.person && shell.person.length > 0);

    // D1 Birth tab — persisted rows, Lagna first.
    const d1 = await evaluate(`(() => {
        const rows = [...document.querySelectorAll('.tw-table tbody tr')];
        return {
            count: rows.length,
            firstPlanet: rows[0]?.children[1]?.textContent.trim(),
            headers: [...document.querySelectorAll('.tw-table thead th')].map(th => th.textContent.trim()),
        };
    })()`);
    console.log(JSON.stringify(d1));
    assert.ok(d1.count >= 8, `expected >= 8 D1 rows, got ${d1.count}`);
    assert.match(d1.firstPlanet, /Ascendant|Lagna/);
    assert.deepEqual(d1.headers, ['House', 'Planet', 'Motion', 'Degree', 'Sign', 'Nakṣatra', 'Nak. Pad']);

    // Switch to Current Transit — either a table or the documented CLI-hint empty state.
    await evaluate(`[...document.querySelectorAll('.tw-tabs .mud-tab')].find(t => /Current Transit/i.test(t.textContent)).click()`);
    await delay(400);
    const transit = await evaluate(`(() => {
        const alert = document.querySelector('.tw-alert')?.textContent.trim();
        const headers = [...document.querySelectorAll('.tw-table thead th')].map(th => th.textContent.replace(/\\s+/g, ' ').trim());
        return { alert, headers, rows: document.querySelectorAll('.tw-table tbody tr').length };
    })()`);
    console.log(JSON.stringify(transit));
    assert.ok(transit.alert?.includes('backfill-planet-transits') || transit.headers.includes('House from D1'),
        'Current Transit shows neither the table nor the backfill hint');

    // Layout: square wheel placement, no horizontal page scroll.
    const desktop = await evaluate(`({
        scroll: document.documentElement.scrollWidth, width: innerWidth,
        wheel: (() => { const r = document.querySelector('.tw-wheel img').getBoundingClientRect(); return { w: r.width, h: r.height }; })(),
    })`);
    console.log(JSON.stringify(desktop));
    assert.ok(desktop.scroll <= desktop.width, `horizontal overflow: ${desktop.scroll} > ${desktop.width}`);
    assert.ok(Math.abs(desktop.wheel.w - desktop.wheel.h) < 2, 'wheel placement is not square');
    await fs.mkdir('reports', { recursive: true });
    await fs.writeFile('reports/transit-landing-desktop.jpg',
        Buffer.from((await send('Page.captureScreenshot', { format: 'jpeg', quality: 45 })).data, 'base64'));

    // Narrow viewport: columns stack, still no horizontal scroll.
    await send('Emulation.setDeviceMetricsOverride', { width: 390, height: 844, deviceScaleFactor: 1, mobile: false });
    await delay(400);
    const mobile = await evaluate(`({
        scroll: document.documentElement.scrollWidth, width: innerWidth,
        cols: getComputedStyle(document.querySelector('.tw-grid')).gridTemplateColumns.split(' ').length,
    })`);
    console.log(JSON.stringify(mobile));
    assert.ok(mobile.scroll <= mobile.width + 1, `mobile horizontal overflow: ${mobile.scroll} > ${mobile.width}`);
    assert.equal(mobile.cols, 1, 'columns did not stack at 390px');
    await fs.writeFile('reports/transit-landing-mobile.jpg',
        Buffer.from((await send('Page.captureScreenshot', { format: 'jpeg', quality: 45, captureBeyondViewport: true })).data, 'base64'));

    assert.deepEqual(errors, [], 'JS exceptions were thrown');
    console.log(JSON.stringify({
        result: 'passed',
        checks: ['HOME pill hard-left', 'per-person tabs revealed', 'context band person', 'D1 Birth persisted rows',
                 'D1 Birth Lagna first', 'Current Transit table or backfill hint', 'square wheel placement',
                 'no horizontal overflow (1440 + 390)', 'columns stack at 390', 'no JS exceptions'],
    }, null, 2));
} finally {
    socket.close();
}
