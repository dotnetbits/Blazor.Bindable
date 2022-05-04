using System;
namespace Microsoft.AspNetCore.Components
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class BindableAttribute : Attribute
    {
        public BindableAttribute()
        {
        }
    }
}