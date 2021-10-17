using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.MatrixParameter.ModelBinders
{
    internal static class ModelBinderExtensions
    {
        public static ModelBindingResult CreateResult(this ModelBindingContext bindingContext, IEnumerable<string>? values)
        {
            if (values is null)
            {
                return ModelBindingResult.Failed();
            }
            
            try
            {
                if (bindingContext.ModelType.IsArray)
                {
                    return ConvertArrayValue(bindingContext, values);
                }
            
                var value = values.FirstOrDefault();
                if (value is null)
                {
                    return ModelBindingResult.Failed();
                }
                
                return ConvertValue(bindingContext, value);
            }
            catch (Exception exc)
            {
                AddModelError(bindingContext, bindingContext.ModelName, exc);
                return ModelBindingResult.Failed();
            }
        }

        private static void AddModelError(ModelBindingContext bindingContext, string modelName, Exception exc)
        {
            var targetInvocationException = exc as TargetInvocationException;
            if (targetInvocationException?.InnerException != null)
            {
                exc = targetInvocationException.InnerException;
            }

            // Don't add an error message if a binding error has already occurred for this property.
            var modelState = bindingContext.ModelState;
            var validationState = modelState.GetFieldValidationState(modelName);
            if (validationState == ModelValidationState.Unvalidated)
            {
                modelState.AddModelError(modelName, exc, bindingContext.ModelMetadata);
            }
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