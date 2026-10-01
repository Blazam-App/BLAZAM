using Microsoft.JSInterop;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods for <see cref="IJSRuntime"/> to manage browser cookies via JavaScript interop.
    /// </summary>
    public static class CookieHelpers
    {
        /// <summary>
        /// Sets a cookie with the specified name, value, and options.
        /// </summary>
        /// <typeparam name="T">The type of the value to store.</typeparam>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie. Objects will be serialized to JSON.</param>
        /// <param name="days">Optional expiration time in days. If null, creates a session cookie.</param>
        /// <param name="path">The cookie path. Defaults to "/".</param>
        /// <param name="sameSite">The SameSite policy (e.g. "Lax", "Strict", "None"). Defaults to "Lax".</param>
        /// <param name="secure">Whether the cookie requires HTTPS. Defaults to false (auto-detected in JS on HTTPS).</param>
        /// <param name="domain">Optional domain attribute for the cookie.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SetCookieAsync<T>(
            this IJSRuntime jsRuntime,
            string name,
            T value,
            int? days = null,
            string path = "/",
            string sameSite = "Lax",
            bool secure = false,
            string? domain = null)
        {
            await jsRuntime.InvokeVoidAsync("cookieHelper.set", name, value, days, path, sameSite, secure, domain);
        }

        /// <summary>
        /// Gets the string value of a cookie by name.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="defaultValue">The fallback value to return if the cookie does not exist.</param>
        /// <returns>The string value of the cookie, or the <paramref name="defaultValue"/> if not found.</returns>
        public static async Task<string?> GetCookieAsync(
            this IJSRuntime jsRuntime,
            string name,
            string? defaultValue = null)
        {
            return await jsRuntime.InvokeAsync<string?>("cookieHelper.get", name, defaultValue);
        }

        /// <summary>
        /// Gets a boolean cookie value by name.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="defaultValue">The fallback boolean value if the cookie does not exist or cannot be parsed.</param>
        /// <returns>The boolean value of the cookie, or <paramref name="defaultValue"/> if not found.</returns>
        public static async Task<bool> GetCookieBooleanAsync(
            this IJSRuntime jsRuntime,
            string name,
            bool defaultValue = false)
        {
            return await jsRuntime.InvokeAsync<bool>("cookieHelper.getBoolean", name, defaultValue);
        }

        /// <summary>
        /// Gets a numeric cookie value by name.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="defaultValue">The fallback numeric value if the cookie does not exist or cannot be parsed.</param>
        /// <returns>The numeric value of the cookie as a double, or <paramref name="defaultValue"/> if not found.</returns>
        public static async Task<double> GetCookieNumberAsync(
            this IJSRuntime jsRuntime,
            string name,
            double defaultValue = 0)
        {
            return await jsRuntime.InvokeAsync<double>("cookieHelper.getNumber", name, defaultValue);
        }

        /// <summary>
        /// Gets and deserializes a JSON object stored in a cookie.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the cookie value into.</typeparam>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="defaultValue">The fallback value if the cookie does not exist or deserialization fails.</param>
        /// <returns>The deserialized object instance, or <paramref name="defaultValue"/> if not found.</returns>
        public static async Task<T?> GetCookieObjectAsync<T>(
            this IJSRuntime jsRuntime,
            string name,
            T? defaultValue = default)
        {
            return await jsRuntime.InvokeAsync<T?>("cookieHelper.getObject", name, defaultValue);
        }

        /// <summary>
        /// Retrieves all cookies as a dictionary of key-value pairs.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <returns>A dictionary containing all cookie keys and their decoded string values.</returns>
        public static async Task<Dictionary<string, string>> GetAllCookiesAsync(this IJSRuntime jsRuntime)
        {
            return await jsRuntime.InvokeAsync<Dictionary<string, string>>("cookieHelper.getAll")
                   ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Deletes a cookie by name with the matching path and domain.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie to delete.</param>
        /// <param name="path">The cookie path used when set. Defaults to "/".</param>
        /// <param name="domain">Optional domain used when the cookie was set.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task DeleteCookieAsync(
            this IJSRuntime jsRuntime,
            string name,
            string path = "/",
            string? domain = null)
        {
            await jsRuntime.InvokeVoidAsync("cookieHelper.delete", name, path, domain);
        }

        /// <summary>
        /// Checks whether a cookie with the specified name exists.
        /// </summary>
        /// <param name="jsRuntime">The <see cref="IJSRuntime"/> instance.</param>
        /// <param name="name">The name of the cookie to check.</param>
        /// <returns>True if the cookie exists; otherwise, false.</returns>
        public static async Task<bool> CookieExistsAsync(
            this IJSRuntime jsRuntime,
            string name)
        {
            return await jsRuntime.InvokeAsync<bool>("cookieHelper.exists", name);
        }
    }
}