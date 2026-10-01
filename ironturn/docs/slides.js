const slides    = document.querySelectorAll('.slide');
const dotsEl    = document.getElementById('dots');
const slideIdEl = document.getElementById('slide-id');
let cur = 0;

// Build navigation dots
slides.forEach((_, i) => {
    const d = document.createElement('div');
    d.className = 'nav-dot' + (i === 0 ? ' active' : '');
    d.onclick = () => goTo(i);
    dotsEl.appendChild(d);
});

function goTo(n) {
    slides[cur].classList.remove('active');
    cur = Math.max(0, Math.min(n, slides.length - 1));
    slides[cur].classList.add('active');
    document.querySelectorAll('.nav-dot').forEach((d, i) => {
        d.classList.toggle('active', i === cur);
    });
    slideIdEl.textContent = 'slide_' + String(cur + 1).padStart(2, '0');
}

function go(dir) { goTo(cur + dir); }

document.addEventListener('keydown', e => {
    if (e.key === 'ArrowRight' || e.key === 'ArrowDown' || e.key === ' ') go(1);
    if (e.key === 'ArrowLeft'  || e.key === 'ArrowUp')                    go(-1);
});