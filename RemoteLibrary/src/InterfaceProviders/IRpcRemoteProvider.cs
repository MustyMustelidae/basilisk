namespace RemoteLibrary.InterfaceProviders
{
    /// <summary>
    /// Provides proxy objects representing remote objects
    /// </summary>
    public interface IRpcRemoteProvider
    {
        /// <summary>
        /// Gets a proxy object of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        T GetProxy<T>() where T : class;
    }
}