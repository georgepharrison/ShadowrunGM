(function () {
    function onUpdateFound(registration, dotnet) {
        const installing = registration.installing;
        if (!installing) return;

        installing.addEventListener('statechange', () => {
            // New worker installed and waiting to take over
            if (installing.state === 'installed' && navigator.serviceWorker.controller) {
                dotnet.invokeMethodAsync('NotifyUpdateAvailable');
            }
        });
    }

    async function register(dotnet) {
        if (!('serviceWorker' in navigator)) return;

        // Detect already-waiting worker (e.g., page reloaded after an update)
        const reg = await navigator.serviceWorker.getRegistration();
        if (reg?.waiting) {
            dotnet.invokeMethodAsync('NotifyUpdateAvailable');
        }

        // Listen for new updates
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            // Optional: you already reload in index.html; no-op here
        });

        const registration = await navigator.serviceWorker.register('service-worker.js');
        registration.addEventListener('updatefound', () => onUpdateFound(registration, dotnet));
    }

    async function applyUpdate() {
        const reg = await navigator.serviceWorker.getRegistration();
        if (reg?.waiting) {
            reg.waiting.postMessage({ type: 'SKIP_WAITING' });
            // Page will reload via your controllerchange handler in index.html
        }
    }

    window.SwUpdates = { register, applyUpdate };
})();