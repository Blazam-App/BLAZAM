using BLAZAM.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages
{
    /// <summary>
    /// Represents a page model that serves static assets based on the specified method and data parameters.
    /// </summary>
    /// <remarks>This model is designed to handle HTTP GET requests and dynamically serve static assets, such
    /// as images or icons,  based on the provided <see cref="Method"/> and <see cref="Data"/> properties. It also sets
    /// appropriate HTTP headers  for caching responses.</remarks>
    public class StaticModel : PageModel
    {
        /// <summary>
        /// Gets or sets the HTTP method used for the current request.
        /// </summary>
        /// <remarks>This property supports binding from query strings or route data when handling GET
        /// requests.</remarks>
        [BindProperty(SupportsGet = true)]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the data associated with the current request.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Data { get; set; }

        /// <summary>
        /// Gets the database context used for interacting with the underlying data store.
        /// </summary>
        protected IDatabaseContext Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticModel"/> class using the specified user database factory.
        /// </summary>
        /// <param name="factory">The factory used to create the database context. Cannot be <see langword="null"/>.</param>
        public StaticModel(IUserDatabaseFactory factory)
        {
            Context = factory.CreateDbContext();

        }

        /// <summary>
        /// Handles HTTP GET requests and returns the appropriate response based on the specified method and data.
        /// </summary>
        /// <remarks>This method sets caching headers for the response, including "Cache-Control" and
        /// "Expires", to indicate that the response is cacheable for 24 hours. The behavior of the method is determined
        /// by the value of the <c>Method</c> property, which specifies the type of operation to perform.</remarks>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation. Returns a specific response based
        /// on the <c>Method</c> value, or a <see cref="NotFoundResult"/> if the method is not recognized.</returns>
        public async Task<IActionResult> OnGet()
        {
            return await Task.Run(() =>
            {
                var expires = DateTime.UtcNow.AddDays(1);
                Response.Headers.Append("Cache-Control", "public,max-age=86400");
                Response.Headers.Append("Expires", expires.ToString("R"));

                switch (Method.ToLower())
                {
                    case "img":
                        return GetImg(Data);

                }
                return NotFound();
            });



        }

        /// <summary>
        /// Returns an image file based on the specified file name.
        /// </summary>
        /// <remarks>The method supports retrieving specific static assets. For "appicon.png", the image
        /// is returned as a PNG file.  For "favicon.ico", the image is returned as an ICO file. Ensure the input string
        /// matches one of the supported  file names (case-insensitive).</remarks>
        /// <param name="data">The name of the image file to retrieve. Supported values are "appicon.png" and "favicon.ico".</param>
        /// <returns>An <see cref="IActionResult"/> containing the requested image file with the appropriate MIME type,  or <see
        /// langword="null"/> if the specified file name is not recognized.</returns>
        public IActionResult GetImg(string data)
        {
            switch (data.ToLower())
            {
                case "appicon.png":
                    return File(StaticAssets.AppIcon(), "image/png");
                case "favicon.ico":
                    return File(StaticAssets.AppIcon(100), "image/x-icon");
            }

            return null;
        }

    }
}
