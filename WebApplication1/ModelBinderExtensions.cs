using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1
{
    internal static class ModelBinderExtensions
    {
        public static ModelBindingResult CreateResult(this ModelBindingContext bindingContext, IEnumerable<string>? values)
        {
            if (values is null)
            {
                return new ModelBindingResult();
            }

            if (bindingContext.ModelType.IsArray)
            {
                return ConvertArrayValue(bindingContext, values);
            }
            
            var value = values.FirstOrDefault();
            if (value is null)
            {
                return new ModelBindingResult();
            }
                
            return ConvertValue(bindingContext, value);
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
            var resultValue = valueConverter.ConvertFromInvariantString(value);
            return ModelBindingResult.Success(resultValue);
        }
    }
}