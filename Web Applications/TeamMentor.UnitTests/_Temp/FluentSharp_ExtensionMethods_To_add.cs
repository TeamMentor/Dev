﻿using O2.DotNetWrappers.ExtensionMethods;
using O2.XRules.Database.Utils;

namespace TeamMentor.UnitTests
{
    public static class FluentSharp_ExtensionMethods_To_add
    {
        public static ascx_Simple_Script_Editor script_Me_WaitForClose(this object objectToScript)
        {
            return objectToScript.script_Me()
                                 .waitForClose();
        }
        public static ascx_Simple_Script_Editor script_Me_WaitForClose(this object objectToScript, string objectName)
        {
            return objectToScript.script_Me(objectName)
                                 .waitForClose();
        }
    }
}
