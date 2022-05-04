using Microsoft.CodeAnalysis;

namespace Blazor.Bindable
{
    internal static class BindableHelpers
    {
        public static string EventCallbackText(string propertyName, ITypeSymbol propertyType)
        {
            return $@"
        [Microsoft.AspNetCore.Components.Parameter] public Microsoft.AspNetCore.Components.EventCallback<{propertyType}> {propertyName}Changed {{ get; set; }}
";
        }

        public static string CurrentInstanceText(string propertyName, ITypeSymbol propertyType)
        {
            return $@"
        private {propertyType} Current{propertyName} 
        {{
            get 
            {{
                return this.{propertyName};
            }}
            set
            {{
                var hasChanged = value != this.{propertyName};
                if (hasChanged)
                {{
                    this.{propertyName} = value;
                    _ = this.{propertyName}Changed.InvokeAsync(this.{propertyName});
                }}
            }}
        }}
";
        }
    }
}