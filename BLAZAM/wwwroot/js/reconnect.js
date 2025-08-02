const reconnectModal = document.getElementById('components-reconnect-modal');

let reloadIntervalId = null;
let stuckTimerId = null; // Timer to detect a stuck modal

async function attemptReload() {
    try {
        await fetch(window.location.href, { method: 'HEAD', cache: 'no-store' });
        window.location.reload();
    } catch (error) {
        console.log('Auto-reload: Server still unreachable.');
    }
}

const observer = new MutationObserver(() => {
    const isVisible = !reconnectModal.classList.contains('components-reconnect-hide');

    if (isVisible) {
        // If the modal is visible and we aren't already trying to reload...
        if (reloadIntervalId === null) {
            // ...start a timer. If this timer finishes, we'll assume the modal is stuck.
            // We give it 5 seconds to resolve itself normally.
            stuckTimerId = setTimeout(() => {
                console.log('Modal is stuck. Forcing reload attempts...');
                attemptReload(); // Try once immediately
                reloadIntervalId = setInterval(attemptReload, 10000);
            }, 5000); // 5-second delay
        }
    } else {
        // Modal is hidden, so connection is good. Clear all timers.
        if (stuckTimerId) {
            clearTimeout(stuckTimerId);
            stuckTimerId = null;
        }
        if (reloadIntervalId) {
            clearInterval(reloadIntervalId);
            reloadIntervalId = null;
        }
    }
});

observer.observe(reconnectModal, { attributes: true }); // We only need to watch attributes (the 'class') for this logic