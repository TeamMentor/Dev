using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSharp.CoreLib;
using System.Web;

namespace TeamMentor.CoreLib
{
    public static class Extra_HttpContextBase_ExtensionMethods
    {
        public static HttpRequestBase request(this HttpContextBase httpContext)
        {
            try
            {
                return httpContext.notNull() ? httpContext.Request : null;
            }
            catch
            {
                return null;
            }

        }
    }
}
