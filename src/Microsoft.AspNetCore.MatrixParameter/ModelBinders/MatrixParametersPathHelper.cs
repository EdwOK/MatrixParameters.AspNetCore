using System;

namespace Microsoft.AspNetCore.MatrixParameter.ModelBinders
{
    internal static class MatrixParametersPathHelper
    {
        public static string? GetMatrixParameter(string pathString, string key)
        {
            if (string.IsNullOrEmpty(pathString) || pathString == ";")
            {
                return null;
            }

            var scanIndex = 0;
            if (pathString[0] == ';')
            {
                scanIndex = 1;
            }

            var textLength = pathString.Length;
            var equalIndex = pathString.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }

            while (scanIndex < textLength)
            {
                var ampersandIndex = pathString.IndexOf(';', scanIndex);
                if (ampersandIndex == -1)
                {
                    ampersandIndex = textLength;
                }

                if (equalIndex < ampersandIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(pathString[scanIndex]))
                    {
                        ++scanIndex;
                    }
                    
                    var name = pathString[scanIndex..equalIndex];
                    var value = pathString.Substring(equalIndex + 1, ampersandIndex - equalIndex - 1);
                    var processedName = Uri.UnescapeDataString(name.Replace('+', ' '));
                    if (string.Equals(processedName, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(value);
                    }

                    equalIndex = pathString.IndexOf('=', ampersandIndex);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                else
                {
                    if (ampersandIndex > scanIndex)
                    {
                        var value = pathString[scanIndex..ampersandIndex];
                        if (string.Equals(value, key, StringComparison.OrdinalIgnoreCase))
                        {
                            return string.Empty;
                        }
                    }
                }

                scanIndex = ampersandIndex + 1;
            }

            return null;
        }
    }
}