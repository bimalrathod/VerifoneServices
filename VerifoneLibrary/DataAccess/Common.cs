using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VerifoneLibrary.DataObject
{
    class Common
    {
    }


    [AttributeUsage(AttributeTargets.Property,
                Inherited = false,
                AllowMultiple = false)]
    internal sealed class OptionalAttribute : Attribute
    {
    }

}
