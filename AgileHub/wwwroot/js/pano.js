// Kanban panosunda sürükle-bırak. JavaScript kapalıyken kartlar ◀ / ▶ düğmeleriyle taşınır.
// Bırakılan kart, düğmelerin kullandığı formla aynı action'a (Gorev/DurumGuncelle) fetch ile gönderilir.
(() => {
    const pano = document.querySelector('.kanban');
    if (!pano) {
        return;
    }

    const hataKutusu = document.getElementById('kanban-hata');
    let suruklenen = null;
    let bekliyor = false;

    pano.querySelectorAll('.kanban-kart').forEach(kart => {
        kart.draggable = true;

        kart.addEventListener('dragstart', olay => {
            // Önceki taşıma bitmeden yenisi başlamasın.
            if (bekliyor) {
                olay.preventDefault();
                return;
            }

            suruklenen = kart;
            olay.dataTransfer.effectAllowed = 'move';
            // Firefox sürüklemeyi yalnızca veri verilince başlatır.
            olay.dataTransfer.setData('text/plain', '');
            kart.classList.add('kanban-suruklenen');
        });

        kart.addEventListener('dragend', () => {
            kart.classList.remove('kanban-suruklenen');
            pano.querySelectorAll('.kanban-hedef').forEach(s => s.classList.remove('kanban-hedef'));
            suruklenen = null;
        });
    });

    pano.querySelectorAll('.kanban-sutun').forEach(sutun => {
        sutun.addEventListener('dragover', olay => {
            if (!suruklenen || sutun.contains(suruklenen)) {
                return;
            }

            olay.preventDefault();
            sutun.classList.add('kanban-hedef');
        });

        sutun.addEventListener('dragleave', olay => {
            if (!sutun.contains(olay.relatedTarget)) {
                sutun.classList.remove('kanban-hedef');
            }
        });

        sutun.addEventListener('drop', olay => {
            if (!suruklenen || sutun.contains(suruklenen)) {
                return;
            }

            olay.preventDefault();
            tasi(suruklenen, sutun);
        });
    });

    async function tasi(kart, hedefSutun) {
        const eskiListe = kart.parentElement;
        const eskiSonraki = kart.nextElementSibling;
        const form = kart.querySelector('form');

        // Formdaki alanlar: id, donusAdresi ve anti-forgery token.
        const veri = new FormData(form);
        veri.set('durum', hedefSutun.dataset.durum);

        bekliyor = true;
        hataKutusu.classList.add('d-none');
        hedefSutun.querySelector('.kanban-liste').appendChild(kart);

        try {
            const yanit = await fetch(form.getAttribute('action'), {
                method: 'POST',
                headers: { 'X-Requested-With': 'fetch' },
                body: veri
            });

            // Yalnızca 204 başarıdır; ör. oturum düşmüşse giriş sayfası 200 ile döner.
            if (yanit.status !== 204) {
                throw new Error(`Beklenmeyen yanıt: ${yanit.status}`);
            }

            // Sayılar, SP toplamları ve ok düğmeleri sunucudaki son durumdan yeniden çizilir.
            window.location.reload();
        } catch {
            eskiListe.insertBefore(kart, eskiSonraki);
            hataKutusu.textContent = 'Kart taşınamadı. Sayfayı yenileyip tekrar deneyin.';
            hataKutusu.classList.remove('d-none');
            hataKutusu.scrollIntoView({ block: 'nearest' });
            bekliyor = false;
        }
    }
})();
