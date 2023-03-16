let lastRequestTime = 0;
window.updateCookieExpiration = async () => {
    const currentTime = Date.now();
    if (currentTime - lastRequestTime > 500) {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                // Check for redirect
                let response = JSON.parse(xhr.response);
                if (response.expired) {
                    window.location.href = window.location.href;
                }
            }
        };
        xhr.open('GET', '/api/auth/keepAlive');
        xhr.send();
        lastRequestTime = currentTime;
    }
};


window.checkForExpiredSession = async () => {
    const currentTime = Date.now();
    if (currentTime - lastRequestTime > 500) {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                // Check for redirect
                let response = JSON.parse(xhr.response);
                if (response.expired) {
                    window.location.href = window.location.href;
                }
            }
        };
        xhr.open('GET', '/api/auth/sessionAlive');
        xhr.send();
        lastRequestTime = currentTime;
    }
};