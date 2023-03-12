using BLAZAM.Common;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using System.Collections;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection;

namespace BLAZAM.Server.Data.Services.Update
{
    public class UpdateServiceBase
    {


     
        private MediaTypeFormatter m_formatter = new VssJsonMediaTypeFormatter();

        protected Timer _updateCheckTimer;
       
        protected async Task<T> ReadContentAsAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            CheckForDisposed();
            bool flag = IsJsonResponse(response);
            bool isMismatchedContentType = false;
            try
            {
                if (flag && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()) && !typeof(byte[]).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()) && !typeof(JObject).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo()))
                {
                    return (await ReadJsonContentAsync<VssJsonCollectionWrapper<T>>(response, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Value;
                }

                if (flag)
                {
                    return await ReadJsonContentAsync<T>(response, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            catch (JsonReaderException)
            {
                isMismatchedContentType = true;
            }

            if (HasContent(response))
            {
                return await HandleInvalidContentType<T>(response, isMismatchedContentType).ConfigureAwait(continueOnCapturedContext: false);
            }

            return default;
        }
        protected virtual async Task<T> ReadJsonContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            return await response.Content.ReadAsAsync<T>(new MediaTypeFormatter[1] { m_formatter }, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        private void CheckForDisposed()
        {

        }

        private async Task<T> HandleInvalidContentType<T>(HttpResponseMessage response, bool isMismatchedContentType)
        {
            string responseType = response.Content?.Headers?.ContentType?.MediaType ?? "Unknown";
            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
            using StreamReader streamReader = new StreamReader(responseStream);
            char[] contentBuffer = new char[4096];
            int contentLength = 0;
            for (int i = 0; i < 4; i++)
            {
                int num = await streamReader.ReadAsync(contentBuffer, i * 1024, 1024).ConfigureAwait(continueOnCapturedContext: false);
                contentLength += num;
                if (num < 1024)
                {
                    break;
                }
            }

            throw new VssServiceResponseException(message: !isMismatchedContentType ? $"Invalid response content type: {responseType} Response Content: {new string(contentBuffer, 0, contentLength)}" : $"Mismatched response content type. {responseType} Response Content: {new string(contentBuffer, 0, contentLength)}", code: response.StatusCode, innerException: null);
        }
        private bool HasContent(HttpResponseMessage response)
        {
            if (response != null && response.StatusCode != HttpStatusCode.NoContent && response.RequestMessage?.Method != HttpMethod.Head && response.Content?.Headers != null && (!response.Content.Headers.ContentLength.HasValue || response.Content.Headers.ContentLength.HasValue && response.Content.Headers.ContentLength != 0))
            {
                return true;
            }

            return false;
        }
        private bool IsJsonResponse(HttpResponseMessage response)
        {
            if (HasContent(response) && response.Content.Headers != null && response.Content.Headers.ContentType != null && !string.IsNullOrEmpty(response.Content.Headers.ContentType!.MediaType))
            {
                return string.Compare("application/json", response.Content.Headers.ContentType!.MediaType, StringComparison.OrdinalIgnoreCase) == 0;
            }

            return false;
        }

       

      
    }
}