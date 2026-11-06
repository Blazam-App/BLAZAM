// Import necessary namespaces for various functionalities
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Services;
using MudBlazor;

namespace BLAZAM.Pages
{
    /// <summary>
    /// Represents a search component that facilitates searching for directory entries based on various parameters and
    /// filters.
    /// </summary>
    /// <remarks>This component integrates with the <see cref="SearchService"/> to manage search parameters
    /// and performs searches using the <see cref="ADSearch"/> class. It supports initializing search terms from the
    /// URI, handling cascading parameters, and managing search results.</remarks>
    public partial class Search : SearchPageBase
    {

    }
}