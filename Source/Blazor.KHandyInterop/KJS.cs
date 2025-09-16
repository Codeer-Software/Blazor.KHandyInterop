using Microsoft.JSInterop;

namespace Blazor.KHandyInterop
{
    public class KJS
    {
        private readonly IJSRuntime _js;
        private IJSObjectReference? _module;
        private DotNetObjectReference<CallbackSink>? _singleReadSink;
        private DotNetObjectReference<CallbackSink>? _multiReadSink;

        private const string ModulePath = $"/_content/Blazor.KHandyInterop/js/blazor-kjs-bridge.js";

        public KJS(IJSRuntime js) => _js = js;

        private async Task<IJSObjectReference> GetModuleAsync()
            => _module ??= await _js.InvokeAsync<IJSObjectReference>("import", ModulePath);

        public async Task<bool> IsReadyAsync()
        {
            try { return await (await GetModuleAsync()).InvokeAsync<bool>("isReady"); }
            catch { return false; }
        }

        // ---- Notification ----
        public async Task StartVibratorAsync(int onMs, int offMs, int repeatCount)
            => await (await GetModuleAsync()).InvokeVoidAsync("startVibrator", onMs, offMs, repeatCount);

        public async Task StopVibratorAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("stopVibrator");

        public async Task StartLedAsync(int color, int onMs, int offMs, int repeatCount)
            => await (await GetModuleAsync()).InvokeVoidAsync("startLed", color, onMs, offMs, repeatCount);

        public async Task StopLedAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("stopLed");

        public async Task StartBuzzerAsync(int tone, int onMs, int offMs, int repeatCount)
            => await (await GetModuleAsync()).InvokeVoidAsync("startBuzzer", tone, onMs, offMs, repeatCount);

        public async Task StopBuzzerAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("stopBuzzer");

        // ---- Scanner ----
        public async Task<bool> ScannerStartReadAsync()
            => await (await GetModuleAsync()).InvokeAsync<bool>("scannerStartRead");

        public async Task ScannerStopReadAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("scannerStopRead");

        public async Task<bool> ScannerIsReadingAsync()
            => await (await GetModuleAsync()).InvokeAsync<bool>("scannerIsReading");

        public async Task ScannerStartMultipleReadAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("scannerStartMultipleRead");

        public async Task ScannerStopMultipleReadAsync()
            => await (await GetModuleAsync()).InvokeVoidAsync("scannerStopMultipleRead");

        // ---- Callbacks ----
        public async Task SetScannerReadCallbackAsync(Func<string, Task> onRead)
        {
            await ClearScannerReadCallbackAsync();
            _singleReadSink = DotNetObjectReference.Create(new CallbackSink(onRead));
            await (await GetModuleAsync()).InvokeVoidAsync("setScannerReadCallback", _singleReadSink, "OnScan");
        }

        public async Task ClearScannerReadCallbackAsync()
        {
            try { await (await GetModuleAsync()).InvokeVoidAsync("clearScannerReadCallback"); } catch { }
            _singleReadSink?.Dispose();
            _singleReadSink = null;
        }

        public async Task SetScannerMultipleReadCallbackAsync(Func<string, Task> onRead)
        {
            await ClearScannerMultipleReadCallbackAsync();
            _multiReadSink = DotNetObjectReference.Create(new CallbackSink(onRead));
            await (await GetModuleAsync()).InvokeVoidAsync("setScannerMultipleReadCallback", _multiReadSink, "OnScan");
        }

        public async Task ClearScannerMultipleReadCallbackAsync()
        {
            try { await (await GetModuleAsync()).InvokeVoidAsync("clearScannerMultipleReadCallback"); } catch { }
            _multiReadSink?.Dispose();
            _multiReadSink = null;
        }

        public async Task DisposeAsync()
        {
            await ClearScannerReadCallbackAsync();
            await ClearScannerMultipleReadCallbackAsync();
            if (_module is not null)
            {
                try { await _module.DisposeAsync(); } catch { }
                _module = null;
            }
            GC.SuppressFinalize(this);
        }

        private sealed class CallbackSink
        {
            private readonly Func<string, Task> _onRead;
            public CallbackSink(Func<string, Task> onRead) => _onRead = onRead;

            [JSInvokable] public Task OnScan(string data) => _onRead?.Invoke(data) ?? Task.CompletedTask;
        }
    }
}
