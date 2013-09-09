namespace TeamMentor.CoreLib
{
	public static class MarkdownDeep_ExtensionMethods
	{
        public static bool SafeMode { get; set; } //true;

		public static string markdown_transform(this string markdown)
		{						
		    var md = new MarkdownDeep.Markdown
		                 {
                             SafeMode = SafeMode,       // was false in the MarkdownDeep demo app
                             ExtraMode = true,      
                             AutoHeadingIDs = true, 
                             MarkdownInHtml = true, 
                             NewWindowForExternalLinks = true,
                             
		                 };
            return md.Transform(markdown);            
		}
	}
}
