
const COLORS = {
    paynesGray: '#4f6d7a',
    columbiaBlue: '#c0d6df',
    aliceBlue: '#dbe9ee',
    trueBlue: '#4a6fa5',
    lapisLazuli: '#166088',
    paynesGrayLight: '#6b8490',
    paynesGrayDark: '#3d5560',
    trueBlueDark: '#3a5a85',
    lapisLazuliLight: '#1f7ba8'
};

document.addEventListener('DOMContentLoaded', function () {
    const dropdowns = document.querySelectorAll('.dropdown-menu');
    dropdowns.forEach(dropdown => {
        dropdown.style.zIndex = '1055';
    });

    const alerts = document.querySelectorAll('.alert:not(.alert-persistent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            if (alert.parentNode) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });

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

    const dangerousActions = document.querySelectorAll('[data-confirm]');
    dangerousActions.forEach(element => {
        element.addEventListener('click', function (e) {
            const message = this.getAttribute('data-confirm');
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

   
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="loading me-2"></span>Przetwarzanie...';


                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 10000);
            }
        });
    });


    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });


    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
            this.style.boxShadow = `0 8px 25px ${COLORS.paynesGray}33`;
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = `0 4px 6px ${COLORS.paynesGray}1A`;
        });
    });

    const currentLocation = location.pathname;
    const menuItems = document.querySelectorAll('.navbar-nav .nav-link');
    menuItems.forEach(item => {
        if (item.getAttribute('href') === currentLocation) {
            item.classList.add('active');
            item.style.fontWeight = 'bold';
            item.style.color = COLORS.aliceBlue + ' !important';
        }
    });

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
                element.style.color = COLORS.trueBlue;
            } else {
                element.textContent = 'Czas minął';
                element.classList.add('text-danger');
                element.style.color = '#dc3545';
            }
        }

        updateCountdown();
        setInterval(updateCountdown, 60000); 
    });

    const printButtons = document.querySelectorAll('[onclick*="print"]');
    printButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            window.print();
        });
    });


    if (typeof toggleSeat === 'function') {
        console.log('Seat selection functionality loaded');
    }


    const searchInputs = document.querySelectorAll('input[type="search"], input[name*="search"]');
    searchInputs.forEach(input => {
        let timeout;
        input.addEventListener('input', function () {
            clearTimeout(timeout);
            const searchTerm = this.value;

            if (searchTerm.length >= 2) {
                timeout = setTimeout(() => {
                    console.log('Searching for:', searchTerm);
                }, 500);
            }
        });
    });


    const images = document.querySelectorAll('img');
    images.forEach(img => {
        img.addEventListener('error', function () {
            if (!this.src.includes('placeholder')) {
                this.src = `data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIke0NPTE9SUy5jb2x1bWJpYUJsdWV9Ii8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIxNCIgZmlsbD0iJHtDT0xPUlMucGF5bmVzR3JheX0iIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5CcmFrIHBvc3RlcmE8L3RleHQ+PC9zdmc+`;
                this.alt = 'Brak postera';
            }
        });
    });

    const navbarToggler = document.querySelector('.navbar-toggler');
    if (navbarToggler) {
        navbarToggler.addEventListener('click', function () {
            const isExpanded = this.getAttribute('aria-expanded') === 'true';
            this.setAttribute('aria-expanded', !isExpanded);
        });
    }


    function checkConnectionStatus() {
        const statusElement = document.querySelector('.badge:contains("Połączono")');
        if (statusElement) {
            if (navigator.onLine) {
                statusElement.className = 'badge bg-success ms-1';
                statusElement.innerHTML = '<i class="fas fa-database me-1"></i>Połączono (SQL Server)';
                statusElement.style.backgroundColor = COLORS.lapisLazuli;
            } else {
                statusElement.className = 'badge bg-danger ms-1';
                statusElement.innerHTML = '<i class="fas fa-exclamation-triangle me-1"></i>Brak połączenia';
            }
        }
    }

    checkConnectionStatus();
    window.addEventListener('online', checkConnectionStatus);
    window.addEventListener('offline', checkConnectionStatus);

    const formControls = document.querySelectorAll('.form-control');
    formControls.forEach(control => {
        control.addEventListener('invalid', function () {
            this.classList.add('is-invalid');
            this.style.borderColor = '#dc3545';
        });

        control.addEventListener('input', function () {
            if (this.validity.valid) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
                this.style.borderColor = COLORS.lapisLazuli;
            } else {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
                this.style.borderColor = '#dc3545';
            }
        });


        control.addEventListener('focus', function () {
            this.style.borderColor = COLORS.trueBlue;
            this.style.boxShadow = `0 0 0 0.2rem ${COLORS.trueBlue}40`;
        });

        control.addEventListener('blur', function () {
            if (!this.classList.contains('is-invalid') && !this.classList.contains('is-valid')) {
                this.style.borderColor = COLORS.columbiaBlue;
                this.style.boxShadow = 'none';
            }
        });
    });
});


function showAlert(message, type = 'info') {
    const alertContainer = document.querySelector('main') || document.body;
    const alertDiv = document.createElement('div');


    const alertColors = {
        'success': COLORS.lapisLazuli,
        'danger': '#dc3545',
        'warning': '#ffc107',
        'info': COLORS.trueBlue
    };

    const iconMap = {
        'success': 'check-circle',
        'danger': 'exclamation-triangle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };

    alertDiv.className = `alert alert-${type} alert-dismissible fade show temp-alert`;
    alertDiv.style.zIndex = '1065'; 
    alertDiv.style.borderLeftColor = alertColors[type] || COLORS.trueBlue;
    alertDiv.innerHTML = `
        <i class="fas fa-${iconMap[type] || 'info-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    alertContainer.insertBefore(alertDiv, alertContainer.firstChild);


    setTimeout(() => {
        if (alertDiv.parentNode) {
            const bsAlert = new bootstrap.Alert(alertDiv);
            bsAlert.close();
        }
    }, 5000);
}


function formatCurrency(amount) {
    return new Intl.NumberFormat('pl-PL', {
        style: 'currency',
        currency: 'PLN'
    }).format(amount);
}

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

function applyColorTheme(element) {
    const primaryButtons = element.querySelectorAll('.btn-primary');
    primaryButtons.forEach(btn => {
        btn.style.background = `linear-gradient(135deg, ${COLORS.trueBlue} 0%, ${COLORS.trueBlueDark} 100%)`;
    });

    const successButtons = element.querySelectorAll('.btn-success');
    successButtons.forEach(btn => {
        btn.style.background = `linear-gradient(135deg, ${COLORS.lapisLazuli} 0%, ${COLORS.lapisLazuliDark} 100%)`;
    });


    const cards = element.querySelectorAll('.card');
    cards.forEach(card => {
        card.style.boxShadow = `0 4px 6px ${COLORS.paynesGray}1A`;
    });


    const formControls = element.querySelectorAll('.form-control');
    formControls.forEach(control => {
        control.style.borderColor = COLORS.columbiaBlue;
    });
}


function addButtonEffects() {
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mousedown', function () {
            this.style.transform = 'translateY(1px)';
        });

        button.addEventListener('mouseup', function () {
            this.style.transform = 'translateY(0)';
        });

        button.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}


document.addEventListener('DOMContentLoaded', addButtonEffects);

if (window.location.hostname === 'localhost') {
    console.log('Multikino Debug Mode Active');
    console.log('Color Palette:', COLORS);

    console.log('User Session:', {
        isLoggedIn: document.querySelector('.navbar .dropdown-toggle') !== null,
        currentPage: location.pathname
    });
}

window.MULTIKINO_COLORS = COLORS;




class MultikinoParticles {
    constructor() {
        this.particlesContainer = null;
        this.heroContainer = null;
        this.particleTypes = ['bubble', 'star', 'film-reel'];
        this.maxParticles = 25; // ZMIANA: było 15, teraz 25
        this.particleCreationInterval = 1500; // ZMIANA: było 2000, teraz 1500
        this.heroParticleCount = 30; // ZMIANA: było 20, teraz 30
    
        this.isInitialized = false;
        this.animationId = null;

        this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        this.isMobile = window.innerWidth <= 768;

        this.init();
    }

    init() {
        if (this.isInitialized || this.reducedMotion) return;

        this.createContainers();
        this.createBackgroundParticles();
        this.createHeroParticles();
        this.setupEventListeners();
        this.isInitialized = true;

        console.log('Multikino Particles System initialized');
    }

    createContainers() {
        if (!document.getElementById('particles-bg')) {
            this.particlesContainer = document.createElement('div');
            this.particlesContainer.id = 'particles-bg';
            document.body.appendChild(this.particlesContainer);
        } else {
            this.particlesContainer = document.getElementById('particles-bg');
        }

        const heroSection = document.querySelector('.hero-section');
        if (heroSection) {
            this.heroContainer = heroSection.querySelector('.hero-particles');
            if (!this.heroContainer) {
                this.heroContainer = document.createElement('div');
                this.heroContainer.className = 'hero-particles';
                heroSection.appendChild(this.heroContainer);
            }
        }
    }

    createBackgroundParticles() {
        if (!this.particlesContainer || this.isMobile) return;

        const createParticle = () => {
            if (document.querySelectorAll('.particle').length >= this.maxParticles) return;

            const particle = document.createElement('div');
            const type = this.particleTypes[Math.floor(Math.random() * this.particleTypes.length)];

            particle.className = `particle ${type}`;
            particle.style.left = Math.random() * 100 + '%';
            particle.style.animationDuration = (Math.random() * 10 + 10) + 's';
            particle.style.animationDelay = Math.random() * 2 + 's';

            if (type === 'film-reel') {
                particle.style.fontSize = '0.8rem';
                particle.innerHTML = '🎬';
                particle.style.background = 'transparent';
                particle.style.border = 'none';
            } else if (type === 'star') {
                particle.innerHTML = '✨';
                particle.style.background = 'transparent';
                particle.style.fontSize = '0.6rem';
            }

            this.particlesContainer.appendChild(particle);

            setTimeout(() => {
                if (particle.parentNode) {
                    particle.parentNode.removeChild(particle);
                }
            }, 25000);
        };

        this.particleInterval = setInterval(createParticle, this.particleCreationInterval);

        for (let i = 0; i < 3; i++) {
            setTimeout(createParticle, i * 500);
        }
    }

    createHeroParticles() {
        if (!this.heroContainer) return;

        this.heroContainer.innerHTML = '';

        const particleCount = this.isMobile ? 10 : this.heroParticleCount;

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'hero-particle';
            particle.style.left = Math.random() * 100 + '%';
            particle.style.top = Math.random() * 100 + '%';
            particle.style.animationDelay = Math.random() * 8 + 's';
            particle.style.animationDuration = (Math.random() * 4 + 6) + 's';

            if (Math.random() > 0.8) {
                particle.style.width = '6px';
                particle.style.height = '6px';
                particle.style.background = 'rgba(255, 255, 255, 0.5)';
            }

            this.heroContainer.appendChild(particle);
        }
    }

    setupEventListeners() {
        let ticking = false;

        const updateParallax = () => {
            if (!this.heroContainer) return;

            const scrolled = window.pageYOffset;
            const heroParticles = this.heroContainer.querySelectorAll('.hero-particle');

            heroParticles.forEach((particle, index) => {
                const speed = (index % 3 + 1) * 0.3;
                particle.style.transform = `translateY(${scrolled * speed}px)`;
            });

            ticking = false;
        };

        const requestParallaxUpdate = () => {
            if (!ticking) {
                requestAnimationFrame(updateParallax);
                ticking = true;
            }
        };

        window.addEventListener('scroll', requestParallaxUpdate, { passive: true });

        window.addEventListener('resize', () => {
            this.handleResize();
        });

        document.addEventListener('visibilitychange', () => {
            this.handleVisibilityChange();
        });

        const motionQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
        motionQuery.addListener((e) => {
            if (e.matches) {
                this.destroy();
            } else {
                this.init();
            }
        });
    }

    handleResize() {
        const newIsMobile = window.innerWidth <= 768;

        if (newIsMobile !== this.isMobile) {
            this.isMobile = newIsMobile;

            if (this.isMobile) {
                this.removeAllParticles();
            } else {
                this.createBackgroundParticles();
            }

            this.createHeroParticles();
        }
    }

    handleVisibilityChange() {
        if (document.hidden) {
            this.pauseAnimations();
        } else {
            this.resumeAnimations();
        }
    }

    pauseAnimations() {
        const particles = document.querySelectorAll('.particle, .hero-particle');
        particles.forEach(particle => {
            particle.style.animationPlayState = 'paused';
        });

        if (this.particleInterval) {
            clearInterval(this.particleInterval);
        }
    }

    resumeAnimations() {
        const particles = document.querySelectorAll('.particle, .hero-particle');
        particles.forEach(particle => {
            particle.style.animationPlayState = 'running';
        });

        if (!this.isMobile && !this.reducedMotion) {
            this.createBackgroundParticles();
        }
    }

    removeAllParticles() {
        if (this.particlesContainer) {
            this.particlesContainer.innerHTML = '';
        }

        if (this.particleInterval) {
            clearInterval(this.particleInterval);
        }
    }

    addCustomParticle(type, x, y, options = {}) {
        if (!this.particlesContainer || this.reducedMotion) return;

        const particle = document.createElement('div');
        particle.className = `particle ${type}`;
        particle.style.left = x + '%';
        particle.style.top = y + '%';

        if (options.duration) {
            particle.style.animationDuration = options.duration;
        }
        if (options.delay) {
            particle.style.animationDelay = options.delay;
        }
        if (options.size) {
            particle.style.width = options.size;
            particle.style.height = options.size;
        }

        this.particlesContainer.appendChild(particle);

        setTimeout(() => {
            if (particle.parentNode) {
                particle.parentNode.removeChild(particle);
            }
        }, 15000);
    }

    createBurstEffect(x, y, count = 5) {
        if (this.reducedMotion) return;

        for (let i = 0; i < count; i++) {
            setTimeout(() => {
                this.addCustomParticle('star',
                    x + (Math.random() - 0.5) * 10,
                    y + (Math.random() - 0.5) * 10,
                    {
                        duration: '2s',
                        delay: Math.random() * 0.5 + 's',
                        size: '12px'
                    }
                );
            }, i * 100);
        }
    }

    destroy() {
        this.removeAllParticles();

        if (this.particleInterval) {
            clearInterval(this.particleInterval);
        }

        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }

        if (this.particlesContainer && this.particlesContainer.parentNode) {
            this.particlesContainer.parentNode.removeChild(this.particlesContainer);
        }

        this.isInitialized = false;
        console.log('Multikino Particles System destroyed');
    }
}

class CounterAnimation {
    constructor() {
        this.observers = [];
        this.init();
    }

    init() {
        this.setupCounters();
    }

    setupCounters() {
        const counters = document.querySelectorAll('.stat-counter');

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !entry.target.dataset.animated) {
                    this.animateCounter(entry.target);
                    entry.target.dataset.animated = 'true';
                }
            });
        }, { threshold: 0.5 });

        counters.forEach(counter => {
            observer.observe(counter);
        });

        this.observers.push(observer);
    }

    animateCounter(counter) {
        const target = parseFloat(counter.getAttribute('data-count'));
        const duration = 2000; // 2 seconds
        const increment = target / (duration / 16); // 60fps
        let current = 0;

        const isInteger = Number.isInteger(target);

        const updateCounter = () => {
            if (current < target) {
                current += increment;

                if (isInteger) {
                    counter.textContent = Math.floor(current).toLocaleString();
                } else {
                    counter.textContent = (Math.floor(current * 10) / 10).toFixed(1);
                }

                requestAnimationFrame(updateCounter);
            } else {
                if (isInteger) {
                    counter.textContent = Math.floor(target).toLocaleString();
                } else {
                    counter.textContent = target.toFixed(1);
                }
            }
        };

        updateCounter();
    }

    destroy() {
        this.observers.forEach(observer => observer.disconnect());
        this.observers = [];
    }
}

class PageAnimations {
    constructor() {
        this.observers = [];
        this.init();
    }

    init() {
        this.setupScrollAnimations();
        this.setupInteractiveElements();
    }

    setupScrollAnimations() {
        const animatedElements = document.querySelectorAll('.fade-in-up, .fade-in-left, .fade-in-right');

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.animationPlayState = 'running';
                    entry.target.style.opacity = '1';
                }
            });
        }, { threshold: 0.1 });

        animatedElements.forEach(el => {
            el.style.animationPlayState = 'paused';
            el.style.opacity = '0';
            observer.observe(el);
        });

        this.observers.push(observer);
    }

    setupInteractiveElements() {
        document.querySelectorAll('.screening-time').forEach(time => {
            time.addEventListener('click', this.createClickEffect.bind(this));
        });

        document.querySelectorAll('.movie-card').forEach(card => {
            card.addEventListener('mouseenter', this.enhanceCardHover.bind(this));
        });
    }

    createClickEffect(event) {
        const element = event.target;
        element.style.transform = 'scale(0.95)';

        setTimeout(() => {
            element.style.transform = 'scale(1.05)';
            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 150);
        }, 150);

        if (window.multikinoParticles) {
            const rect = element.getBoundingClientRect();
            const x = ((rect.left + rect.width / 2) / window.innerWidth) * 100;
            const y = ((rect.top + rect.height / 2) / window.innerHeight) * 100;
            window.multikinoParticles.createBurstEffect(x, y, 3);
        }
    }

    enhanceCardHover(event) {
        const card = event.target.closest('.movie-card');
        if (!card) return;

        card.style.boxShadow = '0 15px 40px rgba(74, 111, 165, 0.25), 0 0 0 1px rgba(74, 111, 165, 0.1)';
    }

    destroy() {
        this.observers.forEach(observer => observer.disconnect());
        this.observers = [];
    }
}

function subscribeNewsletter() {
    const email = document.getElementById('newsletterEmail');
    if (!email) return;

    const emailValue = email.value.trim();
    if (emailValue && emailValue.includes('@')) {
        showAlert('Dziękujemy za zapisanie się do newslettera!', 'success');
        email.value = '';

        if (window.multikinoParticles) {
            window.multikinoParticles.createBurstEffect(50, 30, 8);
        }
    } else {
        showAlert('Proszę wprowadzić prawidłowy adres email.', 'warning');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    try {
        window.multikinoParticles = new MultikinoParticles();
        window.counterAnimation = new CounterAnimation();
        window.pageAnimations = new PageAnimations();

        console.log('All Multikino animation systems loaded successfully');
    } catch (error) {
        console.warn('Error initializing Multikino animations:', error);
    }
});

window.addEventListener('beforeunload', function () {
    if (window.multikinoParticles) {
        window.multikinoParticles.destroy();
    }
    if (window.counterAnimation) {
        window.counterAnimation.destroy();
    }
    if (window.pageAnimations) {
        window.pageAnimations.destroy();
    }
});

window.MultikinoParticles = MultikinoParticles;
window.CounterAnimation = CounterAnimation;
window.PageAnimations = PageAnimations;