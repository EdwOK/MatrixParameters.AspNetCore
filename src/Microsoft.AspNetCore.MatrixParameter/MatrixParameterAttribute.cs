using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.MatrixParameter
{
    /// <summary>
    /// Used to bind matrix parameter values from the URI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MatrixParameterAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixParameterAttribute"/> class.
        /// </summary>
        /// <param name="required">
        /// Indicates that parameter is required for model binding, default value is false.
        /// </param>
        /// <example>
        /// <c>[MatrixParameter] string[] color</c> will match all color values from the whole path.
        /// </example>
        public MatrixParameterAttribute(bool required = false) 
            : this(null, required)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixParameterAttribute"/> class.
        /// </summary>
        /// <param name="segment">
        /// Can be empty, a target prefix value, or a general route name embedded in "{" and "}".
        /// </param>
        /// <param name="required">
        /// Indicates that parameter is required for model binding, default value is false.
        /// </param>
        /// <example>
        /// <c>[MatrixParam("")] string[] color</c> will match all color values from the whole path.
        /// <c>[MatrixParam("oranges")] string[] color</c> will match color only from the segment starting
        /// with "oranges" like .../oranges;color=red/...
        /// <c>[MatrixParam("{fruits}")] string[] color</c> will match color only from the route .../{fruits}/...
        /// </example>
        public MatrixParameterAttribute(string? segment, bool required = false) 
            => (Segment, Required) = (segment, required);

        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.Path;
        
        /// <inheritdoc />
        public string? Name { get; set; }

        /// <summary>
        /// Can be empty, a target prefix value, or a route parameter name embedded in "{" and "}".
        /// </summary>
        /// <example>
        /// <c>[MatrixParameter("")] string[] color</c> will match all color values from the whole path.
        /// <c>[MatrixParameter("oranges")] string[] color</c> will match color only from the segment starting
        /// with "oranges" like .../oranges;color=red/...
        /// <c>[MatrixParameter("{fruits}")] string[] color</c> will match color only from the route .../{fruits}/...
        /// </example>
        public string? Segment { get; set; }

        /// <summary>
        /// Indicates that parameter is required for model binding.
        /// </summary>
        public bool Required { get; set; }
    }
}
