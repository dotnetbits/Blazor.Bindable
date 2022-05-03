using System;
namespace Microsoft.AspNetCore.Components
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class BindableAttribute : Attribute
    {
        public BindableAttribute()
        {
        }
    }
}