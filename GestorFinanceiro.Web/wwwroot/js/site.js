document.addEventListener('DOMContentLoaded', function () {
    var menu = document.getElementById('avatarMenu');
    var btn = document.getElementById('avatarBtn');
    var dropdown = document.getElementById('avatarDropdown');

    if (menu && btn && dropdown) {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var open = menu.classList.toggle('is-open');
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
        });

        document.addEventListener('click', function (e) {
            if (!menu.contains(e.target)) {
                menu.classList.remove('is-open');
                btn.setAttribute('aria-expanded', 'false');
            }
        });
    }

    document.querySelectorAll('.table-row-clickable[data-href]').forEach(function (row) {
        function navigate() {
            window.location = row.dataset.href;
        }

        row.addEventListener('click', function (e) {
            if (e.target.closest('a, button')) return;
            navigate();
        });

        row.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                navigate();
            }
        });
    });
});
