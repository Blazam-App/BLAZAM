window.updateCookieExpiration = async () => {
    await DotNet.invokeMethodAsync('BLAZAM','UpdateCookieExpiration');
};