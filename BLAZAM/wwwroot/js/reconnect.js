const reconnectModal = document.getElementById('components-reconnect-modal');

// Wait until a 'reload' button appears
new MutationObserver((mutations, observer) => {
   // let refreshButton = document.querySelector('#components-reconnect-modal .rejected #reconnect-rejected-refresh-button');
    let reconnectButton = document.querySelector('#components-reconnect-modal .failed #reconnect-failed-button');
    const isVisible = !reconnectModal.classList.contains('components-reconnect-hide');
    if (isVisible) {
        if (reconnectButton.offsetParent !=null) {
            // Now every 10 seconds, see if the server appears to be back, and if so, reload
            async function attemptReload() {
                await fetch(''); // Check the server really is back
                location.reload();
            }
            observer.disconnect();
            attemptReload();
            setInterval(attemptReload, 10000);
        } else {
            console.log("Refresh not visible");
        }
    }
}).observe(reconnectModal, { attributes: true, childList: true, subtree: true });
