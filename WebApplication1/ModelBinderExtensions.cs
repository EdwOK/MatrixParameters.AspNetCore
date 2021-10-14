using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1
{
    internal static class ModelBinderExtensions
    {
        public static ModelBindingResult CreateResult(this ModelBindingContext bindingContext, params string[] values)
        {
            if (!bindingContext.ModelType.IsArray)
            {
                return ConvertValue(bindingContext, values[0]);
            }

            return ConvertArrayValue(bindingContext, values);
        }

        private static ModelBindingResult ConvertArrayValue(ModelBindingContext bindingContext, IEnumerable<string> values)
        {
            var valueType = bindingContext.ModelType.GetElementType() ?? bindingContext.ModelType;
            var valueConverter = TypeDescriptor.GetConverter(valueType);
            
            var convertedValues = values.Select(v => valueConverter.ConvertFromInvariantString(v)).ToArray();
            var resultValues = Array.CreateInstance(valueType, convertedValues.Length);
            Array.Copy(convertedValues, resultValues, convertedValues.Length);

            return ModelBindingResult.Success(resultValues);
        }

        private static ModelBindingResult ConvertValue(ModelBindingContext bindingContext, string value)
        {
            var valueConverter = TypeDescriptor.GetConverter(bindingContext.ModelType);
            return ModelBindingResult.Success(valueConverter.ConvertFromInvariantString(value));
        }
    }
}