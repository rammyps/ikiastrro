import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

const APP = process.env.APP_ORIGIN ?? 'http://localhost:5160';
const CDP = process.env.CDP_PORT ?? '9232';
const PERSON = process.env.PERSON_ID ?? '88';
const tabs = await (await fetch(`http://127.0.0.1:${CDP}/json/list`)).json();
const socket = new WebSocket(tabs.find(tab => tab.type === 'page').webSocketDebuggerUrl);
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
    for (let n = 0; n < 60; n++) {
        if (await evaluate(expression)) return;
        await delay(250);
    }
    throw new Error(`Timed out: ${expression}`);
};

try {
    await fs.mkdir('reports/theme-verification', { recursive: true });
    await send('Runtime.enable');
    await send('Page.enable');
    await send('Emulation.setDeviceMetricsOverride', { width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false });
    await send('Page.navigate', { url: `${APP}/charts` });
    await waitFor(`document.querySelector('.sp-menu-btn')`);
    await delay(1500);

    await evaluate(`document.querySelector('.sp-menu-btn').click()`);
    await waitFor(`[...document.querySelectorAll('.sp-menu-item')].some(x => x.textContent.trim() === 'PREFERENCES')`);
    await evaluate(`[...document.querySelectorAll('.sp-menu-item')].find(x => x.textContent.trim() === 'PREFERENCES').click()`);
    await waitFor(`document.querySelector('.sp-prefs-body')`);
    const settingsText = await evaluate(`document.querySelector('.sp-prefs-body').innerText`);
    assert.match(settingsText, /Default Light/);

    const expected = {
        'default-light': ['rgb(250, 245, 234)', 'rgb(15, 32, 65)'],
        'cosmic-light': ['rgb(248, 250, 252)', 'rgb(23, 32, 51)'],
        'cosmic-dark': ['rgb(11, 15, 25)', 'rgb(248, 250, 252)'],
    };

    const results = [];
    for (const mode of Object.keys(expected)) {
        await evaluate(`(() => { localStorage.setItem('ikiastrro-theme', '${mode}'); location.href='${APP}/key-inference/${PERSON}'; return true; })()`);
        await waitFor(`document.documentElement.dataset.theme === '${mode}' && document.querySelector('.ik-page')`);
        await delay(750);

        const state = await evaluate(`(() => {
            const body = getComputedStyle(document.body);
            const root = getComputedStyle(document.documentElement);
            const table = document.querySelector('table');
            const tableStyle = table ? getComputedStyle(table) : null;
            const chart = document.querySelector('svg.ntw, .pgls-chart, .sid-grid, .si-grid, [class*="south-indian"]');
            return {
                mode: document.documentElement.dataset.theme,
                background: body.backgroundColor,
                foreground: body.color,
                surface: root.getPropertyValue('--brand-surface').trim(),
                line: root.getPropertyValue('--brand-line').trim(),
                gridStroke: root.getPropertyValue('--grid-stroke').trim(),
                wheelRing: root.getPropertyValue('--wheel-ring').trim(),
                tableBackground: tableStyle?.backgroundColor ?? null,
                chartFound: Boolean(chart),
                horizontalOverflow: document.documentElement.scrollWidth > innerWidth + 1,
            };
        })()`);

        assert.equal(state.mode, mode);
        assert.equal(state.background, expected[mode][0]);
        assert.equal(state.foreground, expected[mode][1]);
        assert.ok(state.surface);
        assert.ok(state.line);
        assert.ok(state.gridStroke);
        assert.ok(state.wheelRing);
        assert.equal(state.horizontalOverflow, false);
        results.push(state);

        const shot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
        await fs.writeFile(`reports/theme-verification/${mode}.png`, Buffer.from(shot.data, 'base64'));
    }

    assert.deepEqual(errors, []);
    console.log(JSON.stringify({ result: 'passed', settingsText, modes: results }, null, 2));
} finally {
    socket.close();
}
