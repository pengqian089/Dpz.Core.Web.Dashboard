/**
 * Dialog Interop Module
 * 对话框交互模块，负责管理对话框打开/关闭时的行为
 * 使用 JS 隔离 (JS Isolation) 来避免全局命名空间污染
 */

let isDialogOpen = false;

/**
 * 检查对话框是否打开
 * @returns {boolean}
 */
export function isOpen() {
    return isDialogOpen;
}

/**
 * 禁用 body 滚动（对话框打开时调用）
 * @param {boolean} disableScroll - 是否禁用滚动（默认 true）
 */
export function disableBodyScroll(disableScroll = true) {
    isDialogOpen = true;

    if (disableScroll) {
        // Pause DOM change manager to avoid performance issues
        const domManager = window.appDOMManager;
        if (domManager) {
            domManager.pause();
        }
        document.body.style.overflow = 'hidden';
    }
}

/**
 * 启用 body 滚动（对话框关闭时调用）
 */
export function enableBodyScroll() {
    isDialogOpen = false;
    document.body.style.overflow = '';
    
    // Resume DOM change manager after layout settled
    const domManager = window.appDOMManager;
    if (domManager) {
        domManager.resume();
    }
}

/**
 * 初始化全局按键监听
 * @param {DotNetObjectReference} dotNetHelper 
 */
export function initKeyboardListener(dotNetHelper) {
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            dotNetHelper.invokeMethodAsync('HandleGlobalEsc');
        }
    });
}

/**
 * 手动更新 LazyLoad（对话框打开后调用，因为 DOM 管理器已暂停）
 */
export function updateLazyLoad() {
    // TODO: Implement lazy load update logic if needed
}

