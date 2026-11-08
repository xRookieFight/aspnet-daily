"use strict";

const API = "/api/todo";

const state = {
    todos: [],
    filter: "all",
    editingId: null
};

const els = {
    form: document.getElementById("new-todo"),
    title: document.getElementById("new-title"),
    description: document.getElementById("new-description"),
    list: document.getElementById("todo-list"),
    empty: document.getElementById("empty"),
    summary: document.getElementById("summary"),
    error: document.getElementById("error"),
    template: document.getElementById("todo-template"),
    filters: document.querySelectorAll(".filters button")
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
        // hmm, the ID mismatch case sends plain text, not json
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
    state.todos = await request(API);
    render();
}

async function create(title, description) {
    const todo = await request(API, {
        method: "POST",
        body: JSON.stringify({ title, description: description || null, isCompleted: false })
    });
    state.todos.push(todo);
    render();
}

async function update(todo) {
    await request(`${API}/${todo.id}`, {
        method: "PUT",
        body: JSON.stringify(todo)
    });
    state.todos = state.todos.map(t => (t.id === todo.id ? todo : t));
    render();
}

async function remove(id) {
    await request(`${API}/${id}`, { method: "DELETE" });
    state.todos = state.todos.filter(t => t.id !== id);
    render();
}

function visibleTodos() {
    switch (state.filter) {
        case "active": return state.todos.filter(t => !t.isCompleted);
        case "done": return state.todos.filter(t => t.isCompleted);
        default: return state.todos;
    }
}

function render() {
    const todos = visibleTodos();
    els.list.replaceChildren(...todos.map(renderTodo));
    els.empty.hidden = todos.length > 0;

    const left = state.todos.filter(t => !t.isCompleted).length;
    els.summary.textContent = `${left} aktif / ${state.todos.length} toplam`;

    els.filters.forEach(btn => btn.classList.toggle("active", btn.dataset.filter === state.filter));
}

function renderTodo(todo) {
    const li = els.template.content.firstElementChild.cloneNode(true);
    const toggle = li.querySelector(".toggle");
    const body = li.querySelector(".body");
    const editBtn = li.querySelector(".edit");
    const deleteBtn = li.querySelector(".delete");

    li.classList.toggle("done", todo.isCompleted);
    toggle.checked = todo.isCompleted;

    if (state.editingId === todo.id) {
        renderEditor(body, todo);
        editBtn.textContent = "Vazgeç";
        editBtn.addEventListener("click", () => stopEditing());
    } else {
        body.querySelector(".title").textContent = todo.title;
        body.querySelector(".description").textContent = todo.description ?? "";
        editBtn.addEventListener("click", () => startEditing(todo.id));
    }

    toggle.addEventListener("change", () =>
        run(() => update({ ...todo, isCompleted: toggle.checked })));

    deleteBtn.addEventListener("click", () => {
        if (confirm(`"${todo.title}" silinsin mi?`)) {
            run(() => remove(todo.id));
        }
    });

    return li;
}

function renderEditor(body, todo) {
    const title = document.createElement("input");
    title.type = "text";
    title.maxLength = 200;
    title.value = todo.title;

    const description = document.createElement("input");
    description.type = "text";
    description.maxLength = 1000;
    description.placeholder = "Açıklama";
    description.value = todo.description ?? "";

    const save = () => {
        const newTitle = title.value.trim();
        if (!newTitle) {
            showError(new Error("Başlık boş olamaz."));
            title.focus();
            return;
        }
        run(async () => {
            await update({ ...todo, title: newTitle, description: description.value.trim() || null });
            stopEditing();
        });
    };

    for (const input of [title, description]) {
        input.addEventListener("keydown", e => {
            if (e.key === "Enter") save();
            if (e.key === "Escape") stopEditing();
        });
    }

    body.replaceChildren(title, description);
    requestAnimationFrame(() => title.focus());
}

function startEditing(id) {
    state.editingId = id;
    render();
}

function stopEditing() {
    state.editingId = null;
    render();
}

els.form.addEventListener("submit", e => {
    e.preventDefault();
    const title = els.title.value.trim();
    if (!title) return;

    run(async () => {
        await create(title, els.description.value.trim());
        els.form.reset();
        els.title.focus();
    });
});

els.filters.forEach(btn => btn.addEventListener("click", () => {
    state.filter = btn.dataset.filter;
    render();
}));

run(load);
