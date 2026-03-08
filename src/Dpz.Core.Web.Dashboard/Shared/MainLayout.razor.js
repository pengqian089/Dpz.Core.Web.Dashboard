/**
 * MainLayout
 */

let resizeTimer = null;
let outsideClickHandler = null;
let bound = false;

/**
 * 检测是否为移动设备
 * @returns {boolean} 是否为移动设备
 */
function isMobile() {
    return window.innerWidth <= 768;
}

function getDropdowns() {
    return Array.from(document.querySelectorAll('.dropdown'));
}

function closeAll() {
    getDropdowns().forEach((dropdown) => {
        dropdown.classList.remove('active');
    });
}

function bindMobileEvents() {
    const dropdowns = getDropdowns();
    if (dropdowns.length === 0) {
        return;
    }

    dropdowns.forEach((dropdown) => {
        const trigger = dropdown.querySelector('.btn-icon, .user-profile');
        if (!trigger) {
            return;
        }

        trigger.onclick = (e) => {
            if (!isMobile()) {
                return;
            }

            e.preventDefault();
            e.stopPropagation();

            dropdowns.forEach((d) => {
                if (d !== dropdown) {
                    d.classList.remove('active');
                }
            });

            dropdown.classList.toggle('active');
        };

        dropdown.querySelectorAll('.dropdown-item').forEach((item) => {
            item.onclick = () => {
                if (isMobile()) {
                    dropdown.classList.remove('active');
                }
            };
        });
    });

    outsideClickHandler = (e) => {
        if (!isMobile()) {
            return;
        }

        if (!e.target.closest('.dropdown')) {
            closeAll();
        }
    };

    document.addEventListener('click', outsideClickHandler);
}

function handleResize() {
    if (resizeTimer) {
        clearTimeout(resizeTimer);
    }

    resizeTimer = setTimeout(() => {
        closeAll();
        if (isMobile()) {
            bindMobileEvents();
        }
    }, 250);
}

export function initDropdowns() {
    if (bound) {
        return;
    }

    bound = true;

    if (isMobile()) {
        bindMobileEvents();
    }

    window.addEventListener('resize', handleResize);
}

export function dispose() {
    if (!bound) {
        return;
    }

    bound = false;

    if (outsideClickHandler) {
        document.removeEventListener('click', outsideClickHandler);
        outsideClickHandler = null;
    }

    if (resizeTimer) {
        clearTimeout(resizeTimer);
        resizeTimer = null;
    }

    window.removeEventListener('resize', handleResize);
    closeAll();
}
