namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// A searcher class for BitLocker objects in an <see cref="IActiveDirectoryContext"/>
    /// </summary>
    public interface IADBitLockerSearcher
    {
        /// <summary>
        /// Finds BitLocker recovery information by recovery ID synchronously.
        /// </summary>
        /// <param name="searchTerm">The recovery ID to search for.</param>
        /// <returns>A list of <see cref="IADBitLockerRecovery"/> objects matching the search term.</returns>
        List<IADBitLockerRecovery> FindByRecoveryId(string searchTerm);

        /// <summary>
        /// Finds BitLocker recovery information by recovery ID asynchronously.
        /// </summary>
        /// <param name="searchTerm">The recovery ID to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="IADBitLockerRecovery"/> objects matching the search term.</returns>
        Task<List<IADBitLockerRecovery>> FindByRecoveryIdAsync(string searchTerm);

        /// <summary>
        /// Finds BitLocker recovery information for a specific computer synchronously.
        /// </summary>
        /// <param name="computer">The <see cref="IADComputer"/> to search for BitLocker recovery information.</param>
        /// <returns>A list of <see cref="IADBitLockerRecovery"/> objects associated with the computer.</returns>
        List<IADBitLockerRecovery> FindByComputer(IADComputer computer);

        /// <summary>
        /// Finds BitLocker recovery information for a specific computer asynchronously.
        /// </summary>
        /// <param name="computer">The <see cref="IADComputer"/> to search for BitLocker recovery information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="IADBitLockerRecovery"/> objects associated with the computer.</returns>
        Task<List<IADBitLockerRecovery>> FindByComputerAsync(IADComputer computer);
    }
}