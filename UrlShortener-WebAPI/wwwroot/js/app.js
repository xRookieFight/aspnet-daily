"use strict";

const API = "/api/links";

const state = {
    links: []
};

const els = {
    form: document.getElementById("new-link"),
    url: document.getElementById("new-url"),
    alias: document.getElementById("new-alias"),
    list: document.getElementById("link-list"),
    empty: document.getElementById("empty"),
    summary: document.getElementById("summary"),
    error: document.getElementById("error"),
    template: document.getElementById("link-template")
};

async function request(url, options = {}) {
    const response = await fetch(url, {
        headers: { "Content-Type": "application/json" },
        ...options
    });

    if (!response.ok) {
        throw new Error(await errorMessage(response));
    }

    return response.status === 204 ? null : response.json();
}

async function errorMessage(response) {
    try {
        const body = await response.json();
        if (body.errors) {
            return Object.values(body.errors).flat().join(" ");
        }
        return body.title || `İstek başarısız (${response.status})`;
    } catch {
        return `İstek başarısız (${response.status})`;
    }
}

function showError(err) {
    els.error.textContent = err ? err.message : "";
    els.error.hidden = !err;
}

async function run(action) {
    try {
        showError(null);
        await action();
    } catch (err) {
        showError(err);
    }
}

async function load() {
    state.links = await request(API);
    render();
}

function render() {
    els.list.replaceChildren(...state.links.map(renderLink));
    els.empty.hidden = state.links.length > 0;

    const clicks = state.links.reduce((sum, l) => sum + l.clicks, 0);
    els.summary.textContent = `${state.links.length} link, ${clicks} tıklama`;
}

function renderLink(link) {
    const node = els.template.content.firstElementChild.cloneNode(true);

    const short = node.querySelector(".short");
    short.href = link.shortUrl;
    short.textContent = link.shortUrl;
    node.querySelector(".target").textContent = link.targetUrl;
    node.querySelector(".clicks").textContent = `${link.clicks} tıklama`;

    node.querySelector(".copy").addEventListener("click", () => run(async () => {
        await navigator.clipboard.writeText(link.shortUrl);
    }));

    node.querySelector(".delete").addEventListener("click", () => run(async () => {
        await request(`${API}/${encodeURIComponent(link.code)}`, { method: "DELETE" });
        await load();
    }));

    return node;
}

els.form.addEventListener("submit", (event) => {
    event.preventDefault();
    run(async () => {
        const alias = els.alias.value.trim();
        await request(API, {
            method: "POST",
            body: JSON.stringify({ url: els.url.value.trim(), alias: alias || null })
        });
        els.form.reset();
        await load();
    });
});

document.addEventListener("visibilitychange", () => {
    if (!document.hidden) {
        run(load);
    }
});

run(load);
