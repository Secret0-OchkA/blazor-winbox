window.WinBoxWindowManager =
{
    OpenWindow: (title, windowOptions, componentRef) => {

        windowOptions.onclose = (force) => {
            console.log('onclose');
            if (force === true) {
                WinBoxWindowManager.CallMethodWithParameters(windowOptions.blazorWindowInstanceReference, windowOptions.xCloseHandlerName);
                return false;
            } else {
                WinBoxWindowManager.CallMethodWithParameters(windowOptions.blazorWindowInstanceReference, windowOptions.onCloseHandlerName);
                return true;
            }
        };

        windowOptions.onresize = async (w, h) => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onResizeHandlerName,
                    w, h);
        };
        windowOptions.onmove = async (x, y) => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onMoveHandlerName,
                    x, y);
        };
        windowOptions.onshow = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onShowHandlerName);
        };
        windowOptions.onhide = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onHideHandlerName);
        };
        windowOptions.onfocus = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onFocusHandlerName, null);
        };
        windowOptions.onblur = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onBlurHandlerName);
        };
        windowOptions.onfullscreen = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onFullScreenHandlerName);
        };
        windowOptions.onmaximize = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onMaximizeHandlerName);
        };
        windowOptions.onminimize = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onMinimizeHandlerName);
        };
        windowOptions.onrestore = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onRestoreHandlerName);
        };
        let winBoxElement = new WinBox(windowOptions, title);
        return winBoxElement;
    },
    CallMethodWithParameters: async (dotnetHelper, methodName, ...params) => {
        return await dotnetHelper.invokeMethodAsync(methodName, ...params);
    }

}

function dockWindow(win, x, y) {
    let snapDistance = 10; // Порог для прилипания
    let windows = document.querySelectorAll(".winbox"); // Найти все окна

    windows.forEach(w => {
        if (w === win.body) return; // Не проверять само себя

        let rect = w.getBoundingClientRect();
        let winRect = win.body.getBoundingClientRect();

        // Проверяем, приближается ли окно к границам другого окна
        if (Math.abs(winRect.right - rect.left) < snapDistance) {
            win.move(rect.left - winRect.width, y);
        }
        if (Math.abs(winRect.left - rect.right) < snapDistance) {
            win.move(rect.right, y);
        }
        if (Math.abs(winRect.bottom - rect.top) < snapDistance) {
            win.move(x, rect.top - winRect.height);
        }
        if (Math.abs(winRect.top - rect.bottom) < snapDistance) {
            win.move(x, rect.bottom);
        }
    });
}