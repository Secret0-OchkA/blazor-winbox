window.WinBoxWindowManager =
{
    OpenWindow: (title, windowOptions, componentRef) => {

        windowOptions.onclose = (force) => {
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
                    windowOptions.onShowHandlerName, null);
        };
        windowOptions.onhide = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onHideHandlerName, null);
        };
        windowOptions.onfocus = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onFocusHandlerName, null);
        };
        windowOptions.onblur = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onBlurHandlerName, null);
        };
        windowOptions.onfullscreen = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onFullScreenHandlerName, null);
        };
        windowOptions.onmaximize = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onMaximizeHandlerName, null);
        };
        windowOptions.onminimize = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onMinimizeHandlerName, null);
        };
        windowOptions.onrestore = async () => {
            await WinBoxWindowManager
                .CallMethodWithParameters(windowOptions.blazorWindowInstanceReference,
                    windowOptions.onRestoreHandlerName);
        };
        let winBoxElement = new WinBox(windowOptions, title);

        if (windowOptions.docking) {
            WinBoxDocking.attach(winBoxElement, windowOptions.docking);
        }

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