namespace TeamMentor.CoreLib
{
	public class TMConsts
	{
        public static string DEFAULT_ARTICLE_FOLDER_NAME      = "Articles";
		public static string VIRTUAL_PATH_MAPPING			  = "..\\..";					  
		public static string XML_DATABASE_VIRTUAL_PATH		  = "TeamMentor";
		public static string XML_DATABASE_VIRTUAL_PATH_LEGACY = "Library_Data\\XmlDatabase";  // for legacy support (pre 3.3)
        public static string TM_SERVER_FILENAME               = "TMServer.config";
        public static string TM_CONFIG_FILENAME               = "TMConfig.config";
        public static string DEFAULT_ERROR_PAGE_REDIRECT      = "/error";
        public static string EMAIL_SUBJECT_NEW_USER_WELCOME   = "Welcome to TeamMentor";

        public static string EMAIL_BODY_NEW_USER_WELCOME      =
@"Hi {0}, welcome to TeamMentor.

You can login with your '{1}' account at {2}

                ";
        public static string EMAIL_DEFAULT_FOOTER             =
@"
Sent by TeamMentor. 

";
	}
}