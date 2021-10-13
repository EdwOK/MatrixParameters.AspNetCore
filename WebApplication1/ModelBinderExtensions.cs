using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1
{
    internal static class ModelBinderExtensions
    {
        public static void SetResult(this ModelBindingContext bindingContext, object value)
        {
            try
            {
                var valueConverter = TypeDescriptor.GetConverter(bindingContext.ModelType);
                bindingContext.Result = ModelBindingResult.Success(valueConverter.ConvertFrom(value));
            }
            catch (Exception exc)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exc.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}