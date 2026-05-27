
window.blazam_stripHtml = (html) => {
    const tmp = document.createElement('div');
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || '';
};

window.pwaNotificationsEnabled = async () => {
    return window.localStorageHelper.getBoolean('pwaNotificationsEnabled');
}

window.blazam = {
    pollingInterval: null,
    lastNotificationId: 0,

    subscribeToPushNotifications: async () => {
        if ('serviceWorker' in navigator) {
            const permission = await Notification.requestPermission();
            if (permission === 'granted') {
                console.log('Notification permission granted.');
                await navigator.serviceWorker.register('/js/sw.js');
                localStorage.setItem('pwaNotificationsEnabled', 'true');
                window.blazam.startPolling();
                return true;
            } else {
                console.error('Notification permission denied.');
                localStorage.setItem('pwaNotificationsEnabled', 'false');
                return false;
            }
        } else {
            console.error('Service workers are not supported.');
            return false;
        }
    },

    unsubscribeFromPushNotifications: async () => {
        if ('serviceWorker' in navigator) {
            const registration = await navigator.serviceWorker.getRegistration('/js/sw.js');
            if (registration) {
                await registration.unregister();
                console.log('Service worker unregistered.');
            }
        }
        localStorage.setItem('pwaNotificationsEnabled', 'false');
        window.blazam.stopPolling();
    },

    getPushNotificationSubscriptionState: () => {
        return window.pwaNotificationsEnabled();
    },

    startPolling: () => {
        if (window.blazam.getPushNotificationSubscriptionState() && !window.blazam.pollingInterval) {
            window.blazam.lastNotificationId = window.localStorageHelper.getNumber('lastNotificationId', 0);
            if (window.blazam.lastNotificationId > 0 == false) {
                window.localStorageHelper.setItem('lastNotificationId', 0);
                window.blazam.lastNotificationId = 0;
            }
            window.blazam.pollingInterval = setInterval(window.blazam.pollForNotifications, 30000);
        }
    },

    stopPolling: () => {
        if (window.blazam.pollingInterval) {
            clearInterval(window.blazam.pollingInterval);
            window.blazam.pollingInterval = null;
        }
    },

    pollForNotifications: async () => {
        const response = await fetch('/api/unread-notifications');
        if (response.ok) {
            const notifications = await response.json();
            if (notifications && notifications.length > 0) {
                const latestNotification = notifications[0];
                if (latestNotification.id > window.blazam.lastNotificationId) {
                    navigator.serviceWorker.ready.then((registration) => {
                        registration.showNotification(latestNotification.notification.title || "Notification", {
                            body: window.blazam_stripHtml(latestNotification.notification.message || ""),
                            icon: latestNotification.icon || "/icon-192.png",
                            tag: latestNotification.tag || "blazam-notification",
                        });
                    });
                }
                window.blazam.lastNotificationId = latestNotification.id;
                localStorage.setItem('lastNotificationId', latestNotification.id);

            }
        }
    }
};

// Start polling if the user is already subscribed
window.blazam.startPolling();