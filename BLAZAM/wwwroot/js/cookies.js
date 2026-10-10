// Generic cookie helpers
window.cookieHelper = {
    set: (name, value, days = null, path = '/', sameSite = 'Lax', secure = false, domain = null) => {
        try {
            const stringValue = typeof value === 'object'
                ? JSON.stringify(value)
                : String(value);

            let cookieString = `${encodeURIComponent(name)}=${encodeURIComponent(stringValue)}; path=${path}; SameSite=${sameSite}`;

            if (days !== null && days !== undefined) {
                const date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                cookieString += `; expires=${date.toUTCString()}`;
            }

            if (secure || location.protocol === 'https:') {
                cookieString += '; Secure';
            }

            if (domain) {
                cookieString += `; domain=${domain}`;
            }

            document.cookie = cookieString;
        } catch (error) {
            console.error('Error setting cookie:', error);
        }
    },

    get: (name, defaultValue = null) => {
        try {
            const nameEQ = encodeURIComponent(name) + '=';
            const ca = document.cookie.split(';');
            for (let i = 0; i < ca.length; i++) {
                let c = ca[i].trim();
                if (c.indexOf(nameEQ) === 0) {
                    return decodeURIComponent(c.substring(nameEQ.length));
                }
            }
            return defaultValue;
        } catch (error) {
            console.error('Error getting cookie:', error);
            return defaultValue;
        }
    },

    getBoolean: (name, defaultValue = false) => {
        const value = window.cookieHelper.get(name);
        return value === 'true' ? true : value === 'false' ? false : defaultValue;
    },

    getNumber: (name, defaultValue = 0) => {
        const value = window.cookieHelper.get(name);
        if (value === null) return defaultValue;
        const parsed = Number(value);
        return Number.isNaN(parsed) ? defaultValue : parsed;
    },

    getObject: (name, defaultValue = null) => {
        try {
            const value = window.cookieHelper.get(name);
            return value ? JSON.parse(value) : defaultValue;
        } catch (error) {
            console.error('Error parsing cookie object:', error);
            return defaultValue;
        }
    },

    getAll: () => {
        try {
            const cookies = {};
            if (!document.cookie) return cookies;
            const pairs = document.cookie.split(';');
            for (let i = 0; i < pairs.length; i++) {
                const pair = pairs[i].trim();
                const separatorIndex = pair.indexOf('=');
                if (separatorIndex > 0) {
                    const key = decodeURIComponent(pair.substring(0, separatorIndex));
                    const val = decodeURIComponent(pair.substring(separatorIndex + 1));
                    cookies[key] = val;
                }
            }
            return cookies;
        } catch (error) {
            console.error('Error getting all cookies:', error);
            return {};
        }
    },

    delete: (name, path = '/', domain = null) => {
        try {
            let cookieString = `${encodeURIComponent(name)}=; path=${path}; expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Lax`;
            if (domain) {
                cookieString += `; domain=${domain}`;
            }
            document.cookie = cookieString;
        } catch (error) {
            console.error('Error deleting cookie:', error);
        }
    },

    remove: (name, path = '/', domain = null) => {
        window.cookieHelper.delete(name, path, domain);
    },

    exists: (name) => {
        return window.cookieHelper.get(name) !== null;
    }
};