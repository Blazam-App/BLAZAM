using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection;

namespace BLAZAM.Server.Data.Services.Update
{
    public class UpdateServiceBase
    {


        protected string organization = "branchburg";
        protected string projectGUID = "40e01d64-a275-43b6-b071-e4b7339a9b8c";
        protected string personalAccessToken = "iu4ayxl65otldw5b3cggezcaqufdusfsyb5ile226bhkq3gwrdfa";


        private MediaTypeFormatter m_formatter = new VssJsonMediaTypeFormatter();

        protected Timer _updateCheckTimer;
        protected BuildHttpClient BuildClient
        {
            get
            {
                VssCredentials credentials = new VssBasicCredential(string.Empty, personalAccessToken);
                VssConnection connection = new VssConnection(new Uri($"https://dev.azure.com/{organization}"), credentials);
                return connection.GetClient<BuildHttpClient>();

            }
        }
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

        public async Task<ApplicationVersion> GetLatestVersion()
        {
            var versionString = (await GetLatestBuild()).BuildNumber;
           

            return new ApplicationVersion(versionString);

        }

        public async Task<Build?> GetLatestBuild()
        {
            BuildHttpClient buildClient = BuildClient;

            var builds = await buildClient.GetBuildsAsync(projectGUID);
            if (builds.Count > 0)
            {
                return builds.Where(r => r.Status == BuildStatus.Completed && !r.Deleted && r.Result == BuildResult.Succeeded).OrderByDescending(r => r.FinishTime).First();

            }
            return null;
        }

        public async Task<BuildArtifact?> GetLatestBuildArtifact()
        {
            try
            {
                var latestBuild = await GetLatestBuild();
                var artifacts = await BuildClient.GetArtifactsAsync(projectGUID, latestBuild.Id);
                if (artifacts.Count > 0)
                {
                    return artifacts.First();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}