(function () {
    // Same-origin by default (pagina e servită direct de WebAPI pe 5000).
    // Pentru port separat, setează window.API_BASE = 'http://localhost:5000' într-un <script> înainte.
    const API = (window.API_BASE || '') + '/api/simulator';
    const REFRESH_MS = 1000;
    const MAX_PRESSURE = 12.0;
    const HISTORY_ROWS = 30;

    const $ = (id) => document.getElementById(id);
    const els = {
        conn: $('conn'),
        updatedAt: $('updatedAt'),
        pressure: $('pressure'),
        pressureBar: $('pressureBar'),
        setpoint: $('setpoint'),
        mode: $('mode'),
        modeHint: $('modeHint'),
        alarmDot: $('alarmDot'),
        alarmText: $('alarmText'),
        consumption: $('consumption'),
        pumps: $('pumps'),
        historyBody: $('historyBody'),
        historyCount: $('historyCount'),
    };

    function renderPumps(state) {
        const pumps = state?.Pumps || [false, false, false, false];
        const lamps = state?.Lamps || [false, false, false, false];
        const cards = [];
        for (let i = 0; i < 4; i++) {
            const running = !!pumps[i];
            const available = !!lamps[i];
            const motorClasses = ['pump-motor'];
            if (running) motorClasses.push('running');
            if (!available) motorClasses.push('unavailable');
            cards.push(`
                <div class="pump-card">
                    <div class="pump-label">M${i + 1}</div>
                    <div class="${motorClasses.join(' ')}">
                        <div class="pump-motor-icon">⚙</div>
                    </div>
                    <div class="pump-lamp">
                        <span class="pump-lamp-dot${available ? ' lit' : ''}"></span>
                        <span>P${i + 1} ${available ? 'disponibilă' : 'indisponibilă'}</span>
                    </div>
                    <div class="pump-status ${running ? 'on' : ''}">${
                        !available ? 'indisponibilă' : (running ? 'pornită' : 'oprită')
                    }</div>
                </div>
            `);
        }
        els.pumps.innerHTML = cards.join('');
    }

    function fmtBits(arr) {
        if (!arr) return '— — — —';
        return arr.map(b => b ? '●' : '○').join(' ');
    }

    function renderLatest(state) {
        if (!state) {
            els.pressure.textContent = '—';
            els.pressureBar.style.width = '0%';
            els.setpoint.textContent = '—';
            els.mode.textContent = '—';
            els.modeHint.textContent = 'fără date';
            els.alarmDot.classList.remove('alarmed');
            els.alarmText.classList.remove('alarmed');
            els.alarmText.textContent = '—';
            els.consumption.textContent = '—';
            renderPumps(null);
            return;
        }
        const pressure = Number(state.Pressure) || 0;
        const setpoint = Number(state.Setpoint) || 0;
        els.pressure.textContent = pressure.toFixed(2);
        els.setpoint.textContent = setpoint.toFixed(1) + ' bar';
        const pct = Math.max(0, Math.min(100, (pressure / MAX_PRESSURE) * 100));
        els.pressureBar.style.width = pct + '%';

        let color;
        if (pressure >= 10.5) color = 'var(--danger)';
        else if (pressure < setpoint - 0.15) color = 'var(--warn)';
        else color = 'var(--ok)';
        els.pressure.style.color = color;
        els.pressureBar.style.background = color;

        const mode = state.Mode ?? 'Off';
        els.mode.textContent = String(mode).toUpperCase();
        els.mode.style.color = mode === 'Auto' || mode === 1 ? 'var(--accent)' : 'var(--muted)';
        els.modeHint.textContent = mode === 'Auto' || mode === 1
            ? 'control automat activ'
            : 'sistem oprit';

        const alarm = !!state.Alarm;
        els.alarmDot.classList.toggle('alarmed', alarm);
        els.alarmText.classList.toggle('alarmed', alarm);
        els.alarmText.textContent = alarm ? 'ALARMĂ B1' : 'Normal';

        const cons = Number(state.ConsumptionRate);
        els.consumption.textContent = isNaN(cons) ? '—' : cons.toFixed(1);

        renderPumps(state);
    }

    function renderHistory(events) {
        if (!events || events.length === 0) {
            els.historyBody.innerHTML = '<tr><td colspan="7" class="empty">— niciun eveniment încă —</td></tr>';
            els.historyCount.textContent = '';
            return;
        }
        const slice = events.slice(-HISTORY_ROWS).reverse();
        const rows = slice.map(ev => {
            const time = new Date(ev.StateChangedDate);
            const timeStr = isNaN(time.getTime()) ? '—'
                : time.toLocaleTimeString('ro-RO', { hour12: false }) + '.' + String(time.getMilliseconds()).padStart(3, '0');
            const alarm = !!ev.Alarm;
            return `<tr class="${alarm ? 'alarmed' : ''}">
                <td>${timeStr}</td>
                <td>${Number(ev.Pressure).toFixed(2)} bar</td>
                <td>${Number(ev.Setpoint).toFixed(1)}</td>
                <td>${String(ev.Mode).toUpperCase()}</td>
                <td>${fmtBits(ev.Pumps)}</td>
                <td>${fmtBits(ev.Lamps)}</td>
                <td>${alarm ? '⚠ DA' : '—'}</td>
            </tr>`;
        }).join('');
        els.historyBody.innerHTML = rows;
        els.historyCount.textContent = `· ${events.length} evenimente (afișate ultimele ${slice.length})`;
    }

    function setOnline(ok) {
        if (ok) {
            els.conn.textContent = 'online';
            els.conn.classList.remove('badge-warn', 'badge-danger');
            els.conn.classList.add('badge-ok');
        } else {
            els.conn.textContent = 'offline';
            els.conn.classList.remove('badge-ok');
            els.conn.classList.add('badge-warn');
        }
    }

    async function tick() {
        try {
            const res = await fetch(API, { cache: 'no-store' });
            if (!res.ok) throw new Error('HTTP ' + res.status);
            const events = await res.json();
            const latest = events.length ? events[events.length - 1] : null;
            renderLatest(latest);
            renderHistory(events);
            setOnline(true);
            els.updatedAt.textContent = 'actualizat ' + new Date().toLocaleTimeString('ro-RO', { hour12: false });
        } catch (err) {
            setOnline(false);
            els.updatedAt.textContent = 'eroare conexiune';
            console.warn('fetch failed:', err.message);
        }
    }

    renderPumps(null);
    tick();
    setInterval(tick, REFRESH_MS);
})();
