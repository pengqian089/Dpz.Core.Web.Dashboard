/**
 * MainLayout Dropdown Manager
 * 管理移动端下拉菜单的点击交互
 */

/**
 * 检测是否为移动设备
 * @returns {boolean} 是否为移动设备
 */
function isMobile() {
    return window.innerWidth <= 768;
}

/**
 * 下拉菜单管理器类
 */
class DropdownManager {
    constructor() {
        this.dropdowns = [];
        this.clickHandler = null;
        this.resizeTimer = null;
    }

    /**
     * 初始化所有下拉菜单
     */
    init() {
        this.dropdowns = Array.from(document.querySelectorAll('.dropdown'));
        
        if (this.dropdowns.length === 0) {
            return;
        }

        this.dropdowns.forEach(dropdown => {
            const trigger = dropdown.querySelector('.btn-icon, .user-profile');
            const menu = dropdown.querySelector('.dropdown-menu');
            
            if (!trigger || !menu) {
                return;
            }
            
            // 移动端：点击切换
            if (isMobile()) {
                this.attachMobileEvents(dropdown, trigger);
            }
        });
        
        // 点击外部关闭下拉菜单
        this.attachOutsideClickHandler();
        
        // 点击菜单项后关闭
        this.attachMenuItemClickHandler();
        
        // 响应式处理
        this.attachResizeHandler();
    }

    /**
     * 为触发器附加移动端事件
     * @param {HTMLElement} dropdown 下拉菜单容器
     * @param {HTMLElement} trigger 触发器元素
     */
    attachMobileEvents(dropdown, trigger) {
        trigger.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            // 关闭其他下拉菜单
            this.dropdowns.forEach(d => {
                if (d !== dropdown) {
                    d.classList.remove('active');
                }
            });
            
            // 切换当前下拉菜单
            dropdown.classList.toggle('active');
        });
    }

    /**
     * 附加点击外部关闭处理器
     */
    attachOutsideClickHandler() {
        if (this.clickHandler) {
            document.removeEventListener('click', this.clickHandler);
        }

        this.clickHandler = (e) => {
            if (isMobile()) {
                const clickedInsideDropdown = e.target.closest('.dropdown');
                if (!clickedInsideDropdown) {
                    this.closeAll();
                }
            }
        };

        document.addEventListener('click', this.clickHandler);
    }

    /**
     * 附加菜单项点击处理器
     */
    attachMenuItemClickHandler() {
        this.dropdowns.forEach(dropdown => {
            const items = dropdown.querySelectorAll('.dropdown-item');
            items.forEach(item => {
                item.addEventListener('click', () => {
                    if (isMobile()) {
                        dropdown.classList.remove('active');
                    }
                });
            });
        });
    }

    /**
     * 附加窗口大小变化处理器
     */
    attachResizeHandler() {
        window.addEventListener('resize', () => {
            if (this.resizeTimer) {
                clearTimeout(this.resizeTimer);
            }
            this.resizeTimer = setTimeout(() => {
                this.refresh();
            }, 250);
        });
    }

    /**
     * 关闭所有下拉菜单
     */
    closeAll() {
        this.dropdowns.forEach(dropdown => {
            dropdown.classList.remove('active');
        });
    }

    /**
     * 刷新下拉菜单
     */
    refresh() {
        this.dispose();
        this.init();
    }

    /**
     * 清理资源
     */
    dispose() {
        if (this.clickHandler) {
            document.removeEventListener('click', this.clickHandler);
            this.clickHandler = null;
        }

        if (this.resizeTimer) {
            clearTimeout(this.resizeTimer);
            this.resizeTimer = null;
        }

        this.dropdowns = [];
    }
}

let manager = null;

/**
 * 初始化下拉菜单管理器
 * @export
 */
export function initDropdowns() {
    if (!manager) {
        manager = new DropdownManager();
    }
    manager.init();
}

/**
 * 刷新下拉菜单
 * @export
 */
export function refreshDropdowns() {
    if (manager) {
        manager.refresh();
    }
}

/**
 * 清理资源
 * @export
 */
export function dispose() {
    if (manager) {
        manager.dispose();
        manager = null;
    }
}
