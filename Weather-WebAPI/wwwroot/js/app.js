"use strict";

const API = "/api/weather";

const els = {
    form: document.getElementById("search"),
    city: document.getElementById("city"),
    days: document.getElementById("days"),
    error: document.getElementById("error"),
    result: document.getElementById("result"),
    place: document.getElementById("place"),
    current: document.getElementById("current"),
    details: document.getElementById("details"),
    daily: document.getElementById("daily"),
    template: document.getElementById("day-template")
};

const dateFormat = new Intl.DateTimeFormat("tr-TR", { weekday: "long", day: "numeric", month: "long" });

async function request(url) {
    const response = await fetch(url, { headers: { Accept: "application/json" } });

    if (!response.ok) {
        throw new Error(await errorMessage(response));
    }

    return response.json();
}

async function errorMessage(response) {
    if (response.status === 404) {
        return "Şehir bulunamadı.";
    }
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

function temp(value) {
    return value == null ? "-" : `${Math.round(value)}°C`;
}

function render(report) {
    const { location, current, daily } = report;

    els.place.textContent = [location.name, location.region, location.country]
        .filter(Boolean)
        .join(", ");
    els.current.textContent = `${temp(current.temperature)}, ${current.description}`;
    els.details.textContent =
        `Hissedilen ${temp(current.apparentTemperature)} · Nem %${current.humidity} · Rüzgar ${Math.round(current.windSpeed)} km/s`;

    els.daily.replaceChildren(...daily.map(renderDay));
    els.result.hidden = false;
}

function renderDay(day) {
    const node = els.template.content.firstElementChild.cloneNode(true);

    node.querySelector(".date").textContent = dateFormat.format(new Date(`${day.date}T12:00:00`));
    node.querySelector(".desc").textContent = day.description ?? "-";
    node.querySelector(".min").textContent = temp(day.minTemperature);
    node.querySelector(".max").textContent = temp(day.maxTemperature);
    node.querySelector(".rain").textContent = day.precipitationProbability == null ? "-" : `%${day.precipitationProbability}`;

    return node;
}

els.form.addEventListener("submit", (event) => {
    event.preventDefault();
    run(async () => {
        const params = new URLSearchParams({ city: els.city.value.trim(), days: els.days.value });
        render(await request(`${API}?${params}`));
    });
});
