# Blazor.Bindable
Source Generator Project to add Bindable Attribute that creates binding for blazor parameters.

## How to use
If you write
```C#
[Parameter, Bindable] public string TestPropertyName { get; set; }
```

It will append the generate the following code in the project's analyzer (allowing you to use Roslyn intellisense for properties and whatnot).
```C#
[Microsoft.AspNetCore.Components.Parameter] public Microsoft.AspNetCore.Components.EventCallback<string> TestPropertyNameChanged { get; set; }

private string CurrentTestPropertyName 
{
    get 
    {
        return this.TestPropertyName;
    }
    set
    {
        var hasChanged = value != this.TestPropertyName;
        if (hasChanged)
        {
            this.TestPropertyName = value;
            _ = this.TestPropertyNameChanged.InvokeAsync(this.TestPropertyName);
        }
    }
}
``` 
This allows you to easily bind to a Blazor parameter without rewriting boilerplate code.
