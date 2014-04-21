using System;

namespace RemoteLibrary.Properties
{
    /// <summary>
    ///     ASP.NET MVC attribute. Allows disabling all inspections
    ///     for MVC views within a class or a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AspMvcSupressViewErrorAttribute : Attribute
    {
    }
}