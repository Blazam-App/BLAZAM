let lastRequestTime = 0;
window.updateCookieExpiration = async () => {
    const currentTime = Date.now();
    //Only update at least 500ms intervals
    if (currentTime - lastRequestTime > 500) {
        let xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200) {
                // Check for expiration
                let response = JSON.parse(xhr.response);
                if (response.expired == 'true') {
                    //refresh current page
                    location.reload();
                }
            }
        };
        xhr.open('GET', '/api/auth/keepAlive');
        xhr.send();
        lastRequestTime = currentTime;
    }
};

window.attemptSignIn = async (loginReq) => {
    let formData = new FormData();
    for (let key in loginReq) {
        formData.append(key, loginReq[key]);
    }

    let xhr = new XMLHttpRequest();
    let response = await new Promise((resolve, reject) => {
        xhr.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200) {
                resolve(xhr.response);
            } else if (this.readyState == 4 && this.status != 200) {
                reject(new Error('Request failed'));
            }
        };
        xhr.open('POST', '/signin');
        xhr.send(formData);
    });
    return response;
};

window.playAudio = async (path) => {
    let audio = new Audio(path);
    audio.play();
};

window.printPage = async () => {
    window.print();
};

window.scrollToBottom = async (id) => {
    const element = document.getElementById(id);
    element.scrollTop = element.scrollHeight;
};

const dialGauges = {};

window.createGauge = async (id, maxValue) => {
    dialGauges[id] = Gauge(document.getElementById(id), {
        max: maxValue,
        // custom label renderer
        label: function(value) {
            return Math.round(value) + "/" + this.max;
        },
        value: 0,
        // Custom dial colors (Optional)
        color: function(value) {
            if (value < 20) {
                return "#5ee432"; // green
            } else if (value < 40) {
                return "#fffa50"; // yellow
            } else if (value < 60) {
                return "#f7aa38"; // orange
            } else {
                return "#ef4655"; // red
            }
        }
    });
};

window.setGaugeValue = async (id, val, time) => {
    dialGauges[id].setValueAnimated(val, time);
};

window.customAnalyticsEvent = async (eventName, jsonData) => {
    gtag('event', eventName, {
        jsonData
    });
};

window.blazam_stripHtml = (html) => {
    const tmp = document.createElement('div');
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || '';
};

window.pwaNotificationsEnabled = async () => {
    return localStorage.getItem('pwaNotificationsEnabled') === 'true';
}

window.blazam = {
    pollingInterval: null,
    lastNotificationId: 0,

    subscribeToPushNotifications: async () => {
        if ('serviceWorker' in navigator) {
            const permission = await Notification.requestPermission();
            if (permission === 'granted') {
                console.log('Notification permission granted.');
                await navigator.serviceWorker.register('/');
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
        return localStorage.getItem('pwaNotificationsEnabled') === 'true';
    },

    startPolling: () => {
        if (window.blazam.getPushNotificationSubscriptionState() && !window.blazam.pollingInterval) {
            window.blazam.lastNotificationId = Number(localStorage.getItem('lastNotificationId'));
            if (window.blazam.lastNotificationId > 0 == false) {
                localStorage.setItem('lastNotificationId', 0);
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

