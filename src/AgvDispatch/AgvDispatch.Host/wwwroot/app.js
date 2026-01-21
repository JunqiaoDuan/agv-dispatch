// AGV Dispatch Web App JavaScript

window.appHelpers = {
    // 重定向到指定URL
    redirectTo: function (url) {
        window.location.href = url;
    }
};

// 地图工作室 Interop
window.mapStudioInterop = {
    observers: new WeakMap(),

    init: function (element, dotNetRef) {
        if (!element) return { width: 800, height: 600 };

        // 获取初始尺寸
        const rect = element.getBoundingClientRect();

        // 使用 ResizeObserver 监听容器大小变化
        const observer = new ResizeObserver(entries => {
            for (const entry of entries) {
                const { width, height } = entry.contentRect;
                dotNetRef.invokeMethodAsync('OnContainerResize', width, height);
            }
        });

        observer.observe(element);
        this.observers.set(element, observer);

        // 返回初始尺寸
        return { width: rect.width, height: rect.height };
    },

    dispose: function (element) {
        if (!element) return;

        const observer = this.observers.get(element);
        if (observer) {
            observer.disconnect();
            this.observers.delete(element);
        }
    }
};
