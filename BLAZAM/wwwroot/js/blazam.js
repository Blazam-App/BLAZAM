let lastRequestTime = 0;
window.updateCookieExpiration = async () => {
    const currentTime = Date.now();
    //Only upadte at least 500ms intervals
    if (currentTime - lastRequestTime > 500) {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                // Check for expiration
                let response = JSON.parse(xhr.response);
                if (response.expired == 'true') {
                    //refresh current page
                    window.location.href = window.location.href;
                }
            }
        };
        xhr.open('GET', '/api/auth/keepAlive');
        xhr.send();
        lastRequestTime = currentTime;
    }
};

window.attemptSignIn = async (loginReq) => {
    formData = new FormData();
    for (var key in loginReq) {
        formData.append(key, loginReq[key]);
    }

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
        xhr.send(formData);
    });
    return response;
};

window.playAudio = async (path) => {
    var audio = new Audio(path);
    audio.play();

};

window.printPage = async () => {
    window.print();

};
window.scrollToBottom = async (id) => {
    const element = document.getElementById(id);
    element.scrollTop = element.scrollHeight;
};

var dialGauges = {};

window.createGauge = async (id,maxValue) => {
    dialGauges[id] = Gauge(document.getElementById(id), {
        max: maxValue,
        // custom label renderer
        label: function (value) {
            return Math.round(value) + "/" + this.max;
        },
        value: 0,
        // Custom dial colors (Optional)
        color: function (value) {
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
    //console.log(dialGauges);
}

window.setGaugeValue = async (id, val, time) => {
    dialGauges[id].setValueAnimated(val, time);
}
//const listener = (e) => {
//    e.preventDefault();
//    e.returnValue = '';
//    return false;
//}

//window.warnOnNavigation = async (active) => {
//    if (active) {
//        window.addEventListener('beforeunload', listener);
//        //window.popstate = listener;
//        //window.addEventListener('popstate', listener);
//    } else {
//        window.removeEventListener('beforeunload', listener);
//        //window.removeEventListener('beforepopstate', listener);

//    }

   
//}
