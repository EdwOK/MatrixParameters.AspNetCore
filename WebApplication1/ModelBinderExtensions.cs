using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1
{
    internal static class ModelBinderExtensions
    {
        public static void SetResult(this ModelBindingContext bindingContext, string value)
        {
            try
            {
                var valueConverter = TypeDescriptor.GetConverter(bindingContext.ModelType);
                bindingContext.Result = ModelBindingResult.Success(valueConverter.ConvertFromInvariantString(value));
            }
            catch (Exception exc)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exc.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
        
        public static void SetResult(this ModelBindingContext bindingContext, string[] values)
        {
            try
            {
                var elementType = bindingContext.ModelType.GetElementType() ?? bindingContext.ModelType;
                
                var valueConverter = TypeDescriptor.GetConverter(elementType);
                var convertedValues = values.Select(x => valueConverter.ConvertFromInvariantString(x)).ToArray();
                
                var resultValues = Array.CreateInstance(elementType, convertedValues.Length);
                Array.Copy(values, resultValues, values.Length);
                
                bindingContext.Result = ModelBindingResult.Success(resultValues);
            }
            catch (Exception exc)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exc.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}