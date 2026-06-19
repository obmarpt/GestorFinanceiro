document.addEventListener('DOMContentLoaded', function () {
    var viewMode = document.getElementById('profileView');
    var editMode = document.getElementById('profileEdit');
    var btnEdit = document.getElementById('btnEditProfile');
    var btnCancel = document.getElementById('btnCancelEdit');
    var card = document.getElementById('profileCard');

    if (!viewMode || !editMode) return;

    function showEdit() {
        viewMode.classList.add('profile-mode--hidden');
        editMode.classList.remove('profile-mode--hidden');
        if (card) card.classList.add('is-editing');
    }

    function showView() {
        editMode.classList.add('profile-mode--hidden');
        viewMode.classList.remove('profile-mode--hidden');
        if (card) card.classList.remove('is-editing');
    }

    if (btnEdit) btnEdit.addEventListener('click', showEdit);
    if (btnCancel) {
        btnCancel.addEventListener('click', function () {
            editMode.querySelectorAll('[data-profile-field]').forEach(function (input) {
                input.value = input.dataset.originalValue || '';
            });
            showView();
        });
    }

    if (card && card.dataset.editMode === 'true') showEdit();
});
