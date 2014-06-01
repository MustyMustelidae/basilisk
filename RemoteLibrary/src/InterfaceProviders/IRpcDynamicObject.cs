using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace RemoteLibrary.InterfaceProviders
{
    /// <summary>
    /// Interface IDynamicRemoteProxyObject
    /// </summary>
    public interface IRpcDynamicObject
    {
        /// <summary>
        /// Tries the get member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool TryGetMember(GetMemberBinder binder, out object result);
        /// <summary>
        /// Tries the invoke member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result);
        /// <summary>
        /// Tries the name of the invoke member by.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool TryInvokeMemberByName(string memberName, IEnumerable<object> args, out object result);
        /// <summary>
        /// Gets the meta object.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>DynamicMetaObject.</returns>
        DynamicMetaObject GetMetaObject(Expression parameter);
    }
}