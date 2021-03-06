using RemoteLibrary.Exceptions;

namespace RemoteLibrary.Util.InterfaceProviders
{
    public interface IGenericRpcCallInstanceResolver
    {
        /// <summary>
        ///     Register a new instance of an interface
        /// </summary>
        /// <typeparam name="T">The type of the interface the object represents</typeparam>
        /// <param name="instance">The object representing the given interface</param>
        void RegisterLocalProxyInstance<T>(T instance) where T : class;

        /// <summary>
        ///     Get the locally registered instance of an inteface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InstanceNotDefinedException">
        ///     Derived types should throw this exception if there is no instance defined
        ///     for the given type
        /// </exception>
        T GetLocalProxyInstance<T>() where T : class;

        bool IsProxyInstanceRegistered<T>();
    }
}