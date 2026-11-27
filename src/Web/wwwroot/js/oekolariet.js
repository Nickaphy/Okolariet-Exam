// Small browser helpers called from Blazor via IJSRuntime.
window.oekolariet = {
    // Opens (or re-focuses) the kiosk window for a project.
    // Returns false when the browser's pop-up blocker stopped it.
    openKioskWindow(url, windowName) {
        const win = window.open(url, windowName, "popup=yes,width=1280,height=800");
        if (!win) return false;
        win.focus();
        return true;
    },

    // Locks the kiosk window down for guests: no right-click / long-press menu, no dragging pictures out,
    // no selecting text, and the usual "open developer tools / view source / save / print" shortcuts are swallowed.
    // NB: a web page can't block everything (the browser's own menus, Alt+F4 ...). On the real touchscreen,
    // also start the browser in kiosk mode and turn the developer tools off with the browser policy
    // DeveloperToolsAvailability = 2 (see the README). Called once by KioskStart.
    lockKiosk() {
        if (window.__oekolarietLocked) return;   // only once, also if the page is rendered again
        window.__oekolarietLocked = true;

        const block = e => { e.preventDefault(); e.stopPropagation(); return false; };

        document.addEventListener("contextmenu", block, { capture: true });   // right-click and long-press
        document.addEventListener("dragstart", block, { capture: true });     // dragging an image or link out
        document.addEventListener("selectstart", e => {
            if (!e.target.closest || !e.target.closest("input, textarea")) block(e);
        }, { capture: true });

        document.addEventListener("keydown", e => {
            const key = (e.key || "").toUpperCase();
            const ctrl = e.ctrlKey || e.metaKey;
            const devTools = key === "F12"
                || (ctrl && e.shiftKey && ["I", "J", "C", "K"].includes(key))   // inspect, console, picker
                || (e.metaKey && e.altKey && ["I", "J", "C"].includes(key));    // the same on a Mac
            const pageTools = ctrl && ["U", "S", "P", "F", "G"].includes(key);  // source, save, print, find
            if (devTools || pageTools) block(e);
        }, { capture: true });
    },

    // Requests full screen. Only works when called from a tap/click.
    async enterFullscreen() {
        const el = document.documentElement;
        if (!el.requestFullscreen || document.fullscreenElement) return !!document.fullscreenElement;
        try {
            await el.requestFullscreen({ navigationUI: "hide" });
            return true;
        } catch {
            return false;
        }
    }
};
