using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rayiah.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class SaveAttribute : Attribute {

    }
}
