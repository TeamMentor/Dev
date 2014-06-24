using System;
using Algorim.CreoleWiki;
using FluentSharp.CoreLib;

namespace TeamMentor.CoreLib.TM_AppCode.ExtensionMethods
{
    public static class CreoleWiki_ExtensionMethods
    {
        public static string wikiText_Transform(this string wikiText)
        {
            try
            {
                return new CreoleParser().Parse(wikiText);
            }
            catch (Exception ex)
            {
                ex.log("[wikiText_transform]");
                return "";
            }
        }
    }
}
