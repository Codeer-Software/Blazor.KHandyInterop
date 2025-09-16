function ensure() {
    if (typeof globalThis === "undefined" || typeof globalThis.KJS === "undefined") {
        throw new Error('KJS is not loaded. Ensure <script src="/js/kjs-modules.js"> is included by the host app.');
    }
}
function getKjs() { ensure(); return globalThis.KJS; }

export function call(path, ...args) {
    const fn = path.split(".").reduce((o, k) => (o == null ? undefined : o[k]), getKjs());
    if (typeof fn !== "function") throw new Error(`KJS function not found: ${path}`);
    return fn(...args);
}

export function isReady() { try { ensure(); return true; } catch { return false; } }
export function startVibrator(onMs, offMs, repeatCount) { return call("Notification.startVibrator", onMs, offMs, repeatCount); }
export function stopVibrator() { return call("Notification.stopVibrator"); }
export function startLed(color, onMs, offMs, repeatCount) { return call("Notification.startLed", color, onMs, offMs, repeatCount); }
export function stopLed() { return call("Notification.stopLed"); }
export function startBuzzer(tone, onMs, offMs, repeatCount) { return call("Notification.startBuzzer", tone, onMs, offMs, repeatCount); }
export function stopBuzzer() { return call("Notification.stopBuzzer"); }

export function scannerStartRead() { return call("Scanner.startRead"); }
export function scannerStopRead() { return call("Scanner.stopRead"); }
export function scannerIsReading() { return call("Scanner.isReading"); }
export function scannerStartMultipleRead() { return call("Scanner.startMultipleRead"); }
export function scannerStopMultipleRead() { return call("Scanner.stopMultipleRead"); }

export function setScannerReadCallback(dotNetRef, method = "OnScan") {
    if (!dotNetRef || typeof dotNetRef.invokeMethodAsync !== "function") throw new Error("Invalid DotNetObjectReference.");
    call("Scanner.setReadCallback", (data) => dotNetRef.invokeMethodAsync(method, data));
}
export function clearScannerReadCallback() { return call("Scanner.clearReadCallback"); }
export function setScannerMultipleReadCallback(dotNetRef, method = "OnScan") {
    if (!dotNetRef || typeof dotNetRef.invokeMethodAsync !== "function") throw new Error("Invalid DotNetObjectReference.");
    call("Scanner.setMultipleReadCallback", (data) => dotNetRef.invokeMethodAsync(method, data));
}
export function clearScannerMultipleReadCallback() { return call("Scanner.clearMultipleReadCallback"); }
