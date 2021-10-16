using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.MatrixParameter
{
    /// <summary>
    /// Used to bind the segment prefix value from the route.
    /// </summary>
    /// <example>
    /// If [Route["{fruits}/{location}"] is specified and the incoming uri's relative path is
    /// "/apples:color=red,green/washington;rate=good", then in the action's argument list,
    /// <c>[SegmentPrefix] string fruits</c> will have fruits = apples
    /// but <c>string location</c> without this attribute will have location = washington;rate=good.
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SegmentPrefixAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.Path;

        /// <inheritdoc />
        public string? Name { get; set; }
    }
}
