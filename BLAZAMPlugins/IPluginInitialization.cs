namespace BLAZAM.Plugins
{
    /// <summary>
    /// Defines the contract for initializing a plugin within the application.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for performing any setup or configuration
    /// required to initialize a plugin. This may include tasks such as loading resources, validating dependencies, or
    /// registering services.</remarks>
    public interface IPluginInitialization
    {
        /// <summary>
        /// Initializes the component, preparing it for use.
        /// </summary>
        /// <remarks>This method will be called before using the component if the <see cref="IPluginInitialization"/>
        /// interface is used.
        /// </remarks>
        void Initialize();
    }
}
