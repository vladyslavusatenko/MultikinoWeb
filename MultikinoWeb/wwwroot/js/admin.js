// Fix for form submission confirmations
document.addEventListener('DOMContentLoaded', function () {
    // Handle delete confirmations
    document.querySelectorAll('form[onsubmit*="confirm"]').forEach(form => {
        form.addEventListener('submit', function (e) {
            const confirmText = this.getAttribute('onsubmit');
            const message = confirmText.match(/confirm\('([^']+)'/)[1];

            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
        });
    });

    // Handle ban/unban buttons
    document.querySelectorAll('button[title="Zablokuj"], button[title="Odblokuj"]').forEach(button => {
        button.closest('form').addEventListener('submit', function (e) {
            const action = button.title === 'Zablokuj' ? 'zablokować' : 'odblokować';
            if (!confirm(`Czy na pewno chcesz ${action} tego użytkownika?`)) {
                e.preventDefault();
                return false;
            }
        });
    });
});