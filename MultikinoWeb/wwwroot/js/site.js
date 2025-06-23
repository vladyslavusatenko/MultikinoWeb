// Improved Multikino JavaScript

// Ensure dropdowns work properly
document.addEventListener('DOMContentLoaded', function () {
    // Fix dropdown z-index issues
    const dropdowns = document.querySelectorAll('.dropdown-menu');
    dropdowns.forEach(dropdown => {
        dropdown.style.zIndex = '1055';
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert:not(.alert-persistent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            if (alert.parentNode) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });

    // Smooth scroll for anchor links
    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    anchorLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Confirm dialogs for delete/cancel actions
    const dangerousActions = document.querySelectorAll('[data-confirm]');
    dangerousActions.forEach(element => {
        element.addEventListener('click', function (e) {
            const message = this.getAttribute('data-confirm');
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // Loading states for forms
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="loading me-2"></span>Przetwarzanie...';

                // Restore button after 10 seconds (fallback)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 10000);
            }
        });
    });

    // Tooltips initialization (if Bootstrap tooltips are used)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Card hover effects enhancement
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });

    // Navigation active state
    const currentLocation = location.pathname;
    const menuItems = document.querySelectorAll('.navbar-nav .nav-link');
    menuItems.forEach(item => {
        if (item.getAttribute('href') === currentLocation) {
            item.classList.add('active');
            item.style.fontWeight = 'bold';
        }
    });

    // Dynamic countdown for booking cancellation
    const countdownElements = document.querySelectorAll('[data-countdown]');
    countdownElements.forEach(element => {
        const targetTime = new Date(element.getAttribute('data-countdown')).getTime();

        function updateCountdown() {
            const now = new Date().getTime();
            const timeLeft = targetTime - now;

            if (timeLeft > 0) {
                const hours = Math.floor(timeLeft / (1000 * 60 * 60));
                const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
                element.textContent = `${hours}h ${minutes}m`;
            } else {
                element.textContent = 'Czas minął';
                element.classList.add('text-danger');
            }
        }

        updateCountdown();
        setInterval(updateCountdown, 60000); // Update every minute
    });

    // Print functionality
    const printButtons = document.querySelectorAll('[onclick*="print"]');
    printButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            window.print();
        });
    });

    // Seat selection functionality (if on booking page)
    if (typeof toggleSeat === 'function') {
        // Seat selection is handled in the booking page specific script
        console.log('Seat selection functionality loaded');
    }

    // Dynamic search functionality
    const searchInputs = document.querySelectorAll('input[type="search"], input[name*="search"]');
    searchInputs.forEach(input => {
        let timeout;
        input.addEventListener('input', function () {
            clearTimeout(timeout);
            const searchTerm = this.value;

            if (searchTerm.length >= 2) {
                timeout = setTimeout(() => {
                    // You can implement live search here
                    console.log('Searching for:', searchTerm);
                }, 500);
            }
        });
    });

    // Image lazy loading fallback
    const images = document.querySelectorAll('img');
    images.forEach(img => {
        img.addEventListener('error', function () {
            if (!this.src.includes('placeholder')) {
                this.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZGRkIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkJyYWsgcG9zdGVyYTwvdGV4dD48L3N2Zz4=';
                this.alt = 'Brak postera';
            }
        });
    });

    // Mobile menu improvements
    const navbarToggler = document.querySelector('.navbar-toggler');
    if (navbarToggler) {
        navbarToggler.addEventListener('click', function () {
            const isExpanded = this.getAttribute('aria-expanded') === 'true';
            this.setAttribute('aria-expanded', !isExpanded);
        });
    }

    // Connection status checker
    function checkConnectionStatus() {
        const statusElement = document.querySelector('.badge:contains("Połączono")');
        if (statusElement) {
            if (navigator.onLine) {
                statusElement.className = 'badge bg-success ms-1';
                statusElement.innerHTML = '<i class="fas fa-database me-1"></i>Połączono (SQL Server)';
            } else {
                statusElement.className = 'badge bg-danger ms-1';
                statusElement.innerHTML = '<i class="fas fa-exclamation-triangle me-1"></i>Brak połączenia';
            }
        }
    }

    // Check connection status on load and periodically
    checkConnectionStatus();
    window.addEventListener('online', checkConnectionStatus);
    window.addEventListener('offline', checkConnectionStatus);

    // Form validation improvements
    const formControls = document.querySelectorAll('.form-control');
    formControls.forEach(control => {
        control.addEventListener('invalid', function () {
            this.classList.add('is-invalid');
        });

        control.addEventListener('input', function () {
            if (this.validity.valid) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
            }
        });
    });
});

// Global functions for backward compatibility
function showAlert(message, type = 'info') {
    const alertContainer = document.querySelector('main') || document.body;
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show temp-alert`;
    alertDiv.style.zIndex = '1065'; // Higher than dropdown
    alertDiv.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'danger' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    alertContainer.insertBefore(alertDiv, alertContainer.firstChild);

    // Auto-hide after 5 seconds
    setTimeout(() => {
        if (alertDiv.parentNode) {
            const bsAlert = new bootstrap.Alert(alertDiv);
            bsAlert.close();
        }
    }, 5000);
}

// Utility function for formatting currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('pl-PL', {
        style: 'currency',
        currency: 'PLN'
    }).format(amount);
}

// Utility function for formatting dates
function formatDate(date, options = {}) {
    const defaultOptions = {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };

    return new Intl.DateTimeFormat('pl-PL', { ...defaultOptions, ...options }).format(new Date(date));
}

// Debug mode (only in development)
if (window.location.hostname === 'localhost') {
    console.log('Multikino Debug Mode Active');

    // Add debug info to console
    console.log('User Session:', {
        isLoggedIn: document.querySelector('.navbar .dropdown-toggle') !== null,
        currentPage: location.pathname
    });
}