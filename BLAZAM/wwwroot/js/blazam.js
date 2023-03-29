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

window.attemptSignIn = async () => {
    //TODO get form data accurately
    var form = document.querySelector("form");
    var formData = new FormData(form);
    var data = Array.from(new FormData(form));
    var xhr = new XMLHttpRequest();
    var response = await new Promise((resolve, reject) => {
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                resolve(xhr.response);
            } else if (this.readyState == 4 && this.status != 200) {
                reject(new Error('Request failed'));
            }
        };

        xhr.open('POST', '/signin');
        xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        xhr.send(formData);
    });
    return response;
};
