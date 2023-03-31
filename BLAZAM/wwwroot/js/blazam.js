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
    var form = document.querySelector("form");
    var formData = new FormData();

    for (var x = 0; x < form.length; x++) {
        console.log(form[x].name);
        console.log(form[x].value);
        formData.append(form[x].name,form[x].value)
    }
    var data = Array.from(formData);
    var xhr = new XMLHttpRequest();
    console.log(data);
    var response = await new Promise((resolve, reject) => {
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                resolve(xhr.response);
            } else if (this.readyState == 4 && this.status != 200) {
                reject(new Error('Request failed'));
            }
        };

        xhr.open('POST', '/signin');
       // xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        xhr.send(formData);
    });
    return response;
};

window.playAudio = async (path) => {
    var audio = new Audio(path);
    audio.play();

};
