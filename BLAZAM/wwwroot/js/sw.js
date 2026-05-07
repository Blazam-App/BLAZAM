const cacheName = 'site-cache-v1';
const assetsToCache = [
    '/'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(cacheName).then((cache) => {
            return cache.addAll(assetsToCache);
        })
    );
});

self.addEventListener('message', event => {
    // Verify the message origin matches the service worker's origin
    if (event.origin !== self.location.origin) {
        console.warn('Message received from unauthorized origin:', event.origin);
        return;
    }

    if (event.data && event.data.type === 'show-notification') {
        const notification = event.data.notification;
        self.registration.showNotification(notification.title, {
            body: notification.message,
            icon: '/icon-192.png'
        });
    }
});