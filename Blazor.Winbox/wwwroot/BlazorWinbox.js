window.WinBoxWindowManager =
{
    OpenWindow: (title, windowOptions, componentRef, mountElement) => {

        let winBoxElement = null;
        let closed = false;

        const invoke = async (methodName, ...params) => {
            try {
                return await WinBoxWindowManager
                    .CallMethodWithParameters(componentRef, methodName, ...params);
            } catch (error) {
                // callbacks can still be in flight while the window is closing and blazor
                // already disposed the window instance, that is not an error worth reporting
                if (!closed) {
                    throw error;
                }
            }
        };

        windowOptions.onclose = (force) => {
            if (force === true) {
                // the window is destroyed right after this callback, release everything it holds
                closed = true;
                WinBoxDocking.detach(winBoxElement);
                WinBoxFullscreen.detach(winBoxElement);
                WinBoxToolbar.detach(winBoxElement);
                invoke("OnXClose");
                return false;
            } else {
                invoke("CloseAsync");
                return true;
            }
        };

        windowOptions.onresize = (w, h) => invoke(windowOptions.onResizeHandlerName, w, h);
        windowOptions.onmove = (x, y) => invoke(windowOptions.onMoveHandlerName, x, y);
        windowOptions.onshow = () => invoke(windowOptions.onShowHandlerName, null);
        windowOptions.onhide = () => invoke(windowOptions.onHideHandlerName, null);
        windowOptions.onfocus = () => invoke(windowOptions.onFocusHandlerName, null);
        windowOptions.onblur = () => invoke(windowOptions.onBlurHandlerName, null);
        windowOptions.onfullscreen = () => invoke(windowOptions.onFullScreenHandlerName, null);
        windowOptions.onmaximize = () => invoke(windowOptions.onMaximizeHandlerName, null);
        windowOptions.onminimize = () => invoke(windowOptions.onMinimizeHandlerName, null);
        windowOptions.onrestore = () => invoke(windowOptions.onRestoreHandlerName);
        windowOptions.mount = mountElement;
        winBoxElement = new WinBox(windowOptions, title);

        if (windowOptions.docking) {
            WinBoxDocking.attach(winBoxElement, windowOptions.docking);
        }

        WinBoxFullscreen.attach(winBoxElement);
        WinBoxToolbar.attach(winBoxElement, windowOptions, componentRef);

        return winBoxElement;
    },
    CallMethodWithParameters: async (dotnetHelper, methodName, ...params) => {
        return await dotnetHelper.invokeMethodAsync(methodName, ...params);
    }

}

window.WinBoxDocking = (() => {
    const RESIZE_HANDLES = ["wb-n", "wb-s", "wb-e", "wb-w", "wb-nw", "wb-ne", "wb-se", "wb-sw"];
    const DRAG_HANDLE = "wb-drag";
    const DEFAULT_SNAP_DISTANCE = 10;
    const DEFAULT_GUIDE_COLOR = "rgba(96, 165, 250, .95)";
    const DEFAULT_GUIDE_THICKNESS = 1;
    const EDGE_TOLERANCE = 0.5;

    let stylesInjected = false;
    let verticalGuide = null;
    let horizontalGuide = null;

    const injectStyles = () => {
        if (stylesInjected) return;
        stylesInjected = true;

        const style = document.createElement("style");
        style.id = "winbox-docking-styles";
        style.textContent =
            ".winbox-docking-guide{position:fixed;z-index:2147483646;pointer-events:none;display:none;margin:0;padding:0}" +
            ".winbox-docking-guide-vertical{top:0;width:0;height:100vh;border-left:1px dashed rgba(96,165,250,.95)}" +
            ".winbox-docking-guide-horizontal{left:0;width:100vw;height:0;border-top:1px dashed rgba(96,165,250,.95)}";
        document.head.appendChild(style);
    };

    const ensureGuides = () => {
        injectStyles();

        if (!verticalGuide) {
            verticalGuide = document.createElement("div");
            verticalGuide.className = "winbox-docking-guide winbox-docking-guide-vertical";
            document.body.appendChild(verticalGuide);
        }

        if (!horizontalGuide) {
            horizontalGuide = document.createElement("div");
            horizontalGuide.className = "winbox-docking-guide winbox-docking-guide-horizontal";
            document.body.appendChild(horizontalGuide);
        }
    };

    const hideGuides = () => {
        if (verticalGuide) verticalGuide.style.display = "none";
        if (horizontalGuide) horizontalGuide.style.display = "none";
    };

    const drawGuides = (controller, x, y) => {
        ensureGuides();

        if (x === null) {
            verticalGuide.style.display = "none";
        } else {
            verticalGuide.style.left = Math.round(x) + "px";
            verticalGuide.style.borderLeftWidth = controller.guideThickness + "px";
            verticalGuide.style.borderLeftColor = controller.guideColor;
            verticalGuide.style.display = "block";
        }

        if (y === null) {
            horizontalGuide.style.display = "none";
        } else {
            horizontalGuide.style.top = Math.round(y) + "px";
            horizontalGuide.style.borderTopWidth = controller.guideThickness + "px";
            horizontalGuide.style.borderTopColor = controller.guideColor;
            horizontalGuide.style.display = "block";
        }
    };

    class DockingController
    {
        constructor(win, options) {
            const settings = options || {};

            this.win = win;
            this.snapDistance = typeof settings.snapDistance === "number" && settings.snapDistance > 0
                ? settings.snapDistance
                : DEFAULT_SNAP_DISTANCE;
            this.showGuides = settings.showGuides !== false;
            this.guideColor = typeof settings.guideColor === "string" && settings.guideColor
                ? settings.guideColor
                : DEFAULT_GUIDE_COLOR;
            this.guideThickness = typeof settings.guideThickness === "number" && settings.guideThickness > 0
                ? settings.guideThickness
                : DEFAULT_GUIDE_THICKNESS;
            this.group = typeof settings.group === "string" && settings.group ? settings.group : null;
            this.activeEdge = null;
            this.suppressed = false;
            this.pending = null;
            this.originalOnMove = win.onmove;
            this.originalOnResize = win.onresize;

            win.onmove = (x, y) => this.handleMove(x, y);
            win.onresize = (width, height) => this.handleResize(width, height);

            this.pointerDown = (event) => this.handlePointerDown(event);
            this.pointerUp = () => this.handlePointerUp();

            win.g.addEventListener("mousedown", this.pointerDown, true);
            win.g.addEventListener("touchstart", this.pointerDown, true);
        }

        destroy() {
            const win = this.win;

            if (win && win.g) {
                win.g.removeEventListener("mousedown", this.pointerDown, true);
                win.g.removeEventListener("touchstart", this.pointerDown, true);
                win.onmove = this.originalOnMove;
                win.onresize = this.originalOnResize;
            }

            this.handlePointerUp();

            if (win) {
                win.__docking = null;
            }
        }

        handlePointerDown(event) {
            const target = event.target;

            if (!target || !target.closest) return;

            const handle = target.closest("." + DRAG_HANDLE + "," + RESIZE_HANDLES.map(name => "." + name).join(","));

            if (!handle || !this.win.g.contains(handle)) return;

            this.pending = null;

            if (handle.classList.contains(DRAG_HANDLE)) {
                this.activeEdge = "move";
            } else {
                this.activeEdge = RESIZE_HANDLES
                    .filter(name => handle.classList.contains(name))
                    .map(name => name.substring(3))[0] || null;
            }

            if (!this.activeEdge) return;

            document.addEventListener("mouseup", this.pointerUp, true);
            document.addEventListener("touchend", this.pointerUp, true);
        }

        handlePointerUp() {
            document.removeEventListener("mouseup", this.pointerUp, true);
            document.removeEventListener("touchend", this.pointerUp, true);
            this.activeEdge = null;
            hideGuides();
            this.commit();
        }

        commit() {
            const pending = this.pending;
            this.pending = null;

            if (!pending) return;

            const win = this.win;

            if (!win || !win.g) return;

            if (pending.x !== null) win.x = pending.x;
            if (pending.y !== null) win.y = pending.y;
            if (pending.width !== null) win.width = pending.width;
            if (pending.height !== null) win.height = pending.height;

            this.suppressed = true;
            try {
                if (pending.x !== null || pending.y !== null) {
                    win.move(win.x, win.y, true);
                }
                if (pending.width !== null || pending.height !== null) {
                    win.resize(win.width, win.height, true);
                }
            } finally {
                this.suppressed = false;
            }
        }

        handleMove(x, y) {
            if (this.suppressed) return;

            const win = this.win;
            let guideX = null;
            let guideY = null;

            if (this.activeEdge === "move") {
                const snap = this.snapPosition(x, y);

                if (snap) {
                    x = snap.x !== null ? snap.x : x;
                    y = snap.y !== null ? snap.y : y;
                    guideX = snap.guideX;
                    guideY = snap.guideY;
                    this.pending = { x: x, y: y, width: null, height: null, guideX: guideX, guideY: guideY };

                    this.suppressed = true;
                    try {
                        win.move(x, y, true);
                    } finally {
                        this.suppressed = false;
                    }
                } else {
                    this.pending = null;
                }
            } else if (this.pending && (this.pending.x !== null || this.pending.y !== null)) {
                // winbox moves the window back to the raw position after resizing from its top/left edges
                x = this.pending.x !== null ? this.pending.x : x;
                y = this.pending.y !== null ? this.pending.y : y;

                this.suppressed = true;
                try {
                    win.move(x, y, true);
                } finally {
                    this.suppressed = false;
                }

                this.renderGuides(this.pending.guideX, this.pending.guideY);

                if (this.originalOnMove) {
                    this.originalOnMove.call(win, x, y);
                }

                return;
            }

            this.renderGuides(guideX, guideY);

            if (this.originalOnMove) {
                this.originalOnMove.call(win, x, y);
            }
        }

        handleResize(width, height) {
            if (this.suppressed) return;

            const win = this.win;
            let guideX = null;
            let guideY = null;

            if (this.activeEdge) {
                const snap = this.snapSize(width, height);

                if (snap) {
                    width = snap.width !== null ? snap.width : width;
                    height = snap.height !== null ? snap.height : height;
                    guideX = snap.guideX;
                    guideY = snap.guideY;
                    this.pending = { x: snap.x, y: snap.y, width: snap.width, height: snap.height, guideX: guideX, guideY: guideY };

                    this.suppressed = true;
                    try {
                        if (snap.x !== null || snap.y !== null) {
                            win.move(snap.x !== null ? snap.x : win.x, snap.y !== null ? snap.y : win.y, true);
                        }
                        win.resize(width, height, true);
                    } finally {
                        this.suppressed = false;
                    }
                } else {
                    this.pending = null;
                }
            }

            this.renderGuides(guideX, guideY);

            if (this.originalOnResize) {
                this.originalOnResize.call(win, width, height);
            }
        }

        renderGuides(x, y) {
            if (!this.showGuides || !this.activeEdge || (x === null && y === null)) {
                hideGuides();
                return;
            }

            drawGuides(this, x, y);
        }

        snapPosition(x, y) {
            const win = this.win;
            const targets = this.collectTargets();

            if (!targets.length) return null;

            const width = win.width;
            const height = win.height;
            const bounds = this.positionBounds();
            const xCandidates = [];
            const yCandidates = [];

            targets.forEach(target => {
                xCandidates.push({ value: target.left - width, guide: target.left });
                xCandidates.push({ value: target.right, guide: target.right });
                xCandidates.push({ value: target.left, guide: target.left });
                xCandidates.push({ value: target.right - width, guide: target.right });

                yCandidates.push({ value: target.top - height, guide: target.top });
                yCandidates.push({ value: target.bottom, guide: target.bottom });
                yCandidates.push({ value: target.top, guide: target.top });
                yCandidates.push({ value: target.bottom - height, guide: target.bottom });
            });

            const xSnap = this.choose(x, xCandidates, bounds.minX, bounds.maxX);
            const ySnap = this.choose(y, yCandidates, bounds.minY, bounds.maxY);

            if (!xSnap && !ySnap) return null;

            return {
                x: xSnap ? xSnap.value : null,
                y: ySnap ? ySnap.value : null,
                guideX: xSnap ? xSnap.guide : null,
                guideY: ySnap ? ySnap.guide : null
            };
        }

        snapSize(width, height) {
            const win = this.win;
            const targets = this.collectTargets();

            if (!targets.length) return null;

            const edge = this.activeEdge;
            const resizesEast = edge.indexOf("e") >= 0;
            const resizesWest = edge.indexOf("w") >= 0;
            const resizesSouth = edge.indexOf("s") >= 0;
            const resizesNorth = edge.indexOf("n") >= 0;
            const left = win.x;
            const top = win.y;
            const right = left + width;
            const bottom = top + height;
            const maxWidth = Math.min(win.l, document.documentElement.clientWidth - left - (win.right || 0));
            const maxHeight = Math.min(win.j, document.documentElement.clientHeight - top - (win.bottom || 0));
            const widthCandidates = [];
            const heightCandidates = [];

            targets.forEach(target => {
                if (resizesEast) {
                    widthCandidates.push({ value: target.left - left, guide: target.left });
                    widthCandidates.push({ value: target.right - left, guide: target.right });
                }
                if (resizesWest) {
                    widthCandidates.push({ value: right - target.right, guide: target.right, origin: target.right });
                    widthCandidates.push({ value: right - target.left, guide: target.left, origin: target.left });
                }
                if (resizesSouth) {
                    heightCandidates.push({ value: target.top - top, guide: target.top });
                    heightCandidates.push({ value: target.bottom - top, guide: target.bottom });
                }
                if (resizesNorth) {
                    heightCandidates.push({ value: bottom - target.bottom, guide: target.bottom, origin: target.bottom });
                    heightCandidates.push({ value: bottom - target.top, guide: target.top, origin: target.top });
                }
            });

            const widthSnap = this.choose(width, widthCandidates, win.o, maxWidth);
            const heightSnap = this.choose(height, heightCandidates, win.m, maxHeight);

            if (!widthSnap && !heightSnap) return null;

            return {
                width: widthSnap ? widthSnap.value : null,
                height: heightSnap ? heightSnap.value : null,
                x: widthSnap && widthSnap.origin !== undefined ? widthSnap.origin : null,
                y: heightSnap && heightSnap.origin !== undefined ? heightSnap.origin : null,
                guideX: widthSnap ? widthSnap.guide : null,
                guideY: heightSnap ? heightSnap.guide : null
            };
        }

        choose(current, candidates, minimum, maximum) {
            let best = null;
            let bestDistance = Infinity;

            candidates.forEach(candidate => {
                if (candidate.value < minimum - EDGE_TOLERANCE || candidate.value > maximum + EDGE_TOLERANCE) return;

                const distance = Math.abs(current - candidate.value);

                if (distance <= this.snapDistance && distance < bestDistance) {
                    best = candidate;
                    bestDistance = distance;
                }
            });

            return best;
        }

        positionBounds() {
            const win = this.win;

            return {
                minX: win.left || 0,
                maxX: document.documentElement.clientWidth - win.width - (win.right || 0),
                minY: win.top || 0,
                maxY: document.documentElement.clientHeight - win.height - (win.bottom || 0)
            };
        }

        collectTargets() {
            const targets = [];
            const nodes = document.querySelectorAll(".winbox");
            const root = this.win.g.parentNode;

            for (let i = 0; i < nodes.length; i++) {
                const node = nodes[i];
                const other = node.winbox;

                if (!other || other === this.win || other.g !== node) continue;
                if (other.hidden || other.min || other.max || other.full) continue;
                if (node.parentNode !== root) continue;
                if ((other.__docking ? other.__docking.group : null) !== this.group) continue;

                targets.push({
                    left: other.x,
                    top: other.y,
                    right: other.x + other.width,
                    bottom: other.y + other.height
                });
            }

            return targets;
        }
    }

    return {
        attach: (win, options) => {
            if (!win || !win.g) return null;

            if (win.__docking) {
                win.__docking.destroy();
            }

            const settings = options || {};

            if (settings.enabled === false) return null;

            const controller = new DockingController(win, settings);
            win.__docking = controller;

            return controller;
        },
        detach: (win) => {
            if (win && win.__docking) {
                win.__docking.destroy();
            }
        }
    };
})();

window.WinBoxFullscreen = (() => {
    const FULL_CLASS = "blazor-winbox-fullscreen";

    const viewportSize = () => ({
        width: document.documentElement.clientWidth || window.innerWidth,
        height: document.documentElement.clientHeight || window.innerHeight
    });

    const fullscreenBounds = (win) => {
        const left = win.left || 0;
        const top = win.top || 0;
        const right = win.right || 0;
        const bottom = win.bottom || 0;
        const viewport = viewportSize();

        return {
            x: left,
            y: top,
            width: Math.max(viewport.width - left - right, 0),
            height: Math.max(viewport.height - top - bottom, 0)
        };
    };

    const applyBounds = (win, bounds) => {
        win.move(bounds.x, bounds.y);
        win.resize(bounds.width, bounds.height);
    };

    const enter = (win) => {
        if (!win || !win.g || win.__blazorFullscreen) return win;

        if (win.min || win.max) {
            win.restore();
        }

        const bounds = {
            x: win.x,
            y: win.y,
            width: win.width,
            height: win.height
        };
        const resizeHandler = () => applyBounds(win, fullscreenBounds(win));

        win.__blazorFullscreen = { bounds, resizeHandler };

        win.focus();
        win.g.classList.add(FULL_CLASS);
        win.full = true;

        applyBounds(win, fullscreenBounds(win));
        window.addEventListener("resize", win.__blazorFullscreen.resizeHandler);

        if (win.onfullscreen) {
            win.onfullscreen();
        }

        return win;
    };

    const cleanup = (win) => {
        const state = win && win.__blazorFullscreen;

        if (!state) return;

        window.removeEventListener("resize", state.resizeHandler);
        win.__blazorFullscreen = null;
        win.full = false;

        if (win.g) win.g.classList.remove(FULL_CLASS);
    };

    const exit = (win) => {
        const state = win && win.__blazorFullscreen;

        if (!state) return win;

        cleanup(win);
        applyBounds(win, state.bounds);

        if (win.onrestore) {
            win.onrestore();
        }

        return win;
    };

    return {
        attach: (win) => {
            if (!win || !win.g || win.__blazorFullscreenAttached) return;

            win.__blazorFullscreenAttached = true;

            win.blazorIsFullscreen = () => !!win.__blazorFullscreen;

            win.fullscreen = function (state) {
                const next = state === undefined || state === null ? !this.full : state !== false;

                return next ? enter(this) : exit(this);
            };

            const restoreWindow = win.restore;

            win.restore = function () {
                if (this.__blazorFullscreen) {
                    exit(this);

                    if (this.max || this.min) {
                        return restoreWindow.call(this);
                    }

                    return this;
                }

                return restoreWindow.call(this);
            };

            const maximizeWindow = win.maximize;

            win.maximize = function (state) {
                if (this.__blazorFullscreen && state !== false) {
                    exit(this);
                }

                return maximizeWindow.call(this, state);
            };

            const minimizeWindow = win.minimize;

            win.minimize = function (state) {
                if (this.__blazorFullscreen && state !== false) {
                    exit(this);
                }

                return minimizeWindow.call(this, state);
            };

            const closeWindow = win.close;

            win.close = function (force) {
                exit(this);
                return closeWindow.call(this, force);
            };
        },
        detach: (win) => {
            cleanup(win);
        },
        isFullscreen: (win) => !!(win && win.__blazorFullscreen)
    };
})();

window.WinBoxToolbar = (() => {
    const BUTTON_CLASS = "wb-blazor-toolbar-button";
    const ICON_CLASS = "wb-blazor-toolbar-icon";
    const HIDDEN_CLASS = "wb-blazor-toolbar-button-hidden";
    const DISABLED_CLASS = "wb-blazor-toolbar-button-disabled";
    const ICON_CONTAINER_CLASS = "wb-icon";
    const DEFAULT_ICON_SIZE = "16px 16px";
    const CONTROL_CLASS_NAMES = {
        minimize: "wb-min",
        maximize: "wb-max",
        fullscreen: "wb-full",
        close: "wb-close"
    };
    const CONTROL_SELECTOR = ".wb-min,.wb-max,.wb-full,.wb-close";

    let stylesInjected = false;

    const injectStyles = () => {
        if (stylesInjected) return;
        stylesInjected = true;

        const style = document.createElement("style");
        style.id = "blazor-winbox-toolbar-styles";
        style.textContent =
            ".wb-blazor-toolbar-button{display:inline-flex!important;align-items:center;justify-content:center;vertical-align:top;background-position:center;background-repeat:no-repeat}" +
            ".wb-blazor-toolbar-icon{display:inline-block;width:auto!important;height:auto!important;background:none!important;font-size:16px;line-height:1;vertical-align:middle;pointer-events:none}" +
            ".wb-blazor-toolbar-button-hidden{display:none!important}" +
            ".wb-blazor-toolbar-button-disabled{opacity:.45;cursor:default}" +
            ".wb-blazor-toolbar-button:focus-visible{outline:2px solid rgba(255,255,255,.75);outline-offset:-2px}";
        document.head.appendChild(style);
    };

    const getIconContainer = (win) => {
        if (!win || !win.g) return null;
        return win.g.getElementsByClassName(ICON_CONTAINER_CLASS)[0] || null;
    };

    const cssUrl = (value) => 'url("' + String(value).replace(/["\\]/g, "\\$&") + '")';

    const hasBackgroundSize = (style) => /(^|;)\s*background-size\s*:/i.test(style || "");

    const buildClassName = (button) => {
        const classes = [BUTTON_CLASS];

        if (button.cssClass) classes.push(button.cssClass);
        if (button.visible === false) classes.push(HIDDEN_CLASS);
        if (button.disabled === true) classes.push(DISABLED_CLASS);

        return classes.join(" ");
    };

    const applyStyle = (element, button) => {
        element.removeAttribute("style");

        if (button.style) element.style.cssText = button.style;
        if (button.color) element.style.color = button.color;
        if (button.background) element.style.backgroundColor = button.background;

        if (button.icon) {
            element.style.backgroundImage = cssUrl(button.icon);
            element.style.backgroundRepeat = "no-repeat";
            element.style.backgroundPosition = "center";

            if (button.iconSize) {
                element.style.backgroundSize = button.iconSize;
            } else if (!hasBackgroundSize(button.style)) {
                element.style.backgroundSize = DEFAULT_ICON_SIZE;
            }
        }
    };

    const applyIconClass = (element, button) => {
        let icon = null;

        for (let i = 0; i < element.children.length; i++) {
            if (element.children[i].classList.contains(ICON_CLASS)) {
                icon = element.children[i];
                break;
            }
        }

        if (button.iconClass) {
            if (!icon) {
                icon = document.createElement("span");
                icon.className = ICON_CLASS;
                element.appendChild(icon);
            }

            icon.className = ICON_CLASS + " " + button.iconClass;
            icon.setAttribute("aria-hidden", "true");
        } else if (icon) {
            icon.remove();
        }
    };

    const applyState = (element, button) => {
        element.className = buildClassName(button);

        if (button.tooltip) {
            element.setAttribute("title", button.tooltip);
            element.setAttribute("aria-label", button.tooltip);
        } else {
            element.removeAttribute("title");
            element.removeAttribute("aria-label");
        }

        element.dataset.blazorToolbarDisabled = button.disabled === true ? "true" : "false";

        if (button.disabled === true) {
            element.setAttribute("aria-disabled", "true");
            element.setAttribute("tabindex", "-1");
        } else {
            element.removeAttribute("aria-disabled");
            element.setAttribute("tabindex", "0");
        }

        applyStyle(element, button);
        applyIconClass(element, button);
    };

    const findButton = (win, id) => {
        if (!win || !win.g) return null;

        const buttons = win.g.getElementsByClassName(BUTTON_CLASS);

        for (let i = 0; i < buttons.length; i++) {
            if (buttons[i].dataset.blazorToolbarId === id) return buttons[i];
        }

        return null;
    };

    const resolveIndex = (container, button, customCount) => {
        if (typeof button.index === "number" && button.index >= 0) {
            return Math.min(button.index, container.children.length);
        }

        return Math.min(customCount, container.children.length);
    };

    const onClick = (componentRef, buttonId) => {
        if (!componentRef) {
            return;
        }

        try {
            Promise.resolve(
                WinBoxWindowManager.CallMethodWithParameters(
                    componentRef,
                    "OnToolbarButtonClick",
                    buttonId)
            ).catch(() => { });
        } catch (error) { }
    };

    const createButton = (win, onClickButton, button) => {
        const container = getIconContainer(win);
        if (!container) return null;

        const element = document.createElement("span");
        element.dataset.blazorToolbarId = button.id;
        element.setAttribute("role", "button");

        element.addEventListener("click", (event) => {
            event.preventDefault();
            event.stopPropagation();

            if (element.dataset.blazorToolbarDisabled === "true") return;

            onClickButton(button.id);
        });

        element.addEventListener("keydown", (event) => {
            if (event.key !== "Enter" && event.key !== " ") return;

            event.preventDefault();
            event.stopPropagation();

            if (element.dataset.blazorToolbarDisabled === "true") return;

            onClickButton(button.id);
        });

        const customCount = container.getElementsByClassName(BUTTON_CLASS).length;
        const index = resolveIndex(container, button, customCount);

        container.insertBefore(element, container.children[index] || null);

        return element;
    };

    const upsert = (win, onClickButton, button) => {
        let element = findButton(win, button.id);

        if (!element) element = createButton(win, onClickButton, button);

        if (element) applyState(element, button);
    };

    const getControlElement = (win, name) => {
        const className = CONTROL_CLASS_NAMES[name];

        if (!className || !win || !win.g) return null;

        return win.g.getElementsByClassName(className)[0] || null;
    };

    const getControlName = (element) => {
        if (!element || !element.classList) return null;

        return Object.keys(CONTROL_CLASS_NAMES)
            .filter(name => element.classList.contains(CONTROL_CLASS_NAMES[name]))[0] || null;
    };

    const applyControl = (win, name, config) => {
        const element = getControlElement(win, name);

        if (!element) return;

        const previousClasses = (element.dataset.blazorControlClasses || "").split(" ").filter(Boolean);
        previousClasses.forEach(cssClass => element.classList.remove(cssClass));

        element.dataset.blazorControlClasses = "";
        element.removeAttribute("style");
        element.removeAttribute("title");
        element.classList.remove(HIDDEN_CLASS, DISABLED_CLASS);
        applyIconClass(element, {});

        win.__blazorControlState = win.__blazorControlState || {};
        win.__blazorControlState[name] = { disabled: !!(config && config.disabled === true) };

        if (!config) return;

        const classes = (config.cssClass || "").split(/\s+/).filter(Boolean);
        classes.forEach(cssClass => element.classList.add(cssClass));
        element.dataset.blazorControlClasses = classes.join(" ");

        if (config.style) element.style.cssText = config.style;
        if (config.color) element.style.color = config.color;
        if (config.background) element.style.backgroundColor = config.background;

        if (config.icon) {
            element.style.backgroundImage = cssUrl(config.icon);
            element.style.backgroundRepeat = "no-repeat";
            element.style.backgroundPosition = "center";

            if (config.iconSize) {
                element.style.backgroundSize = config.iconSize;
            } else if (!hasBackgroundSize(config.style)) {
                element.style.backgroundSize = DEFAULT_ICON_SIZE;
            }
        } else if (config.iconClass) {
            element.style.backgroundImage = "none";
        }

        if (config.tooltip) element.setAttribute("title", config.tooltip);
        if (config.visible === false) element.classList.add(HIDDEN_CLASS);
        if (config.disabled === true) element.classList.add(DISABLED_CLASS);

        applyIconClass(element, config);
    };

    const applyControls = (win, options) => {
        applyControl(win, "minimize", options.minimize);
        applyControl(win, "maximize", options.maximize);
        applyControl(win, "fullscreen", options.fullscreen);
        applyControl(win, "close", options.close);
    };

    // Stops clicks on disabled controls in the capture phase, before the built-in handler runs.
    const blockDisabledControlClick = (win, event) => {
        const target = event.target;

        if (!target || !target.closest || !win.g) return;

        const control = target.closest(CONTROL_SELECTOR);

        if (!control || !win.g.contains(control)) return;

        const name = getControlName(control);
        const state = name && win.__blazorControlState ? win.__blazorControlState[name] : null;

        if (!state || !state.disabled) return;

        event.preventDefault();
        event.stopPropagation();
    };

    const removeButtons = (win) => {
        if (!win || !win.g) return;

        const buttons = win.g.getElementsByClassName(BUTTON_CLASS);

        while (buttons.length) {
            buttons[0].remove();
        }
    };

    return {
        attach: (win, instanceOptions, componentRef) => {
            if (!win || !win.g) return;

            injectStyles();

            const options = instanceOptions || {};
            const onClickButton = (buttonId) => onClick(componentRef, buttonId);

            applyControls(win, options);

            const blockedClickHandler = (event) => blockDisabledControlClick(win, event);
            win.g.addEventListener("click", blockedClickHandler, true);
            win.__blazorControlClickHandler = blockedClickHandler;

            win.blazorUpdateControlButton = (name, config) => {
                const key = String(name || "").toLowerCase();

                if (!CONTROL_CLASS_NAMES[key]) return;

                applyControl(win, key, config || null);
            };
            win.blazorAddToolbarButton = (button) => {
                if (button) upsert(win, onClickButton, button);
            };
            win.blazorUpdateToolbarButton = (button) => {
                if (button) upsert(win, onClickButton, button);
            };
            win.blazorRemoveToolbarButton = (id) => {
                const element = findButton(win, id);

                if (element) element.remove();
            };
            win.blazorSetToolbarButtons = (buttons) => {
                removeButtons(win);

                (buttons || []).forEach(button => {
                    if (button) upsert(win, onClickButton, button);
                });
            };

            (options.toolbar || []).forEach(button => {
                if (button) upsert(win, onClickButton, button);
            });
        },
        detach: (win) => {
            if (!win || !win.g) return;

            removeButtons(win);

            if (win.__blazorControlClickHandler) {
                win.g.removeEventListener("click", win.__blazorControlClickHandler, true);
                win.__blazorControlClickHandler = null;
            }

            applyControls(win, {});

            delete win.blazorUpdateControlButton;
            delete win.blazorAddToolbarButton;
            delete win.blazorUpdateToolbarButton;
            delete win.blazorRemoveToolbarButton;
            delete win.blazorSetToolbarButtons;
        }
    };
})();
