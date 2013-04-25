using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using O2.DotNetWrappers.ExtensionMethods;

namespace TeamMentor.CoreLib
{
    [Serializable] 
    public class UserActivity
    {
        [XmlAttribute] public string	Action		{ get; set; }
        [XmlAttribute] public string	Detail		{ get; set; }
        [XmlAttribute] public string	Who		    { get; set; }
        [XmlAttribute] public string	IPAddress	{ get; set; }
        [XmlAttribute] public long      When		{ get; set; }        
    }

    public static class UserActivities_Ex
    {
        public static string ToDateTime(this long when)
        {
            var result = DateTime.FromFileTimeUtc(ConvertFileTimeToLocalTime(when));
            return string.Format("{0:G}", @result);
        }

        private static long ConvertFileTimeToLocalTime(long fileTime) {
            return fileTime + ((long)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMilliseconds * 10000);
        }

        public static string ReplaceZeroWith(this DateTime dateTime, string replacement)
        {
            return dateTime < DateTime.Parse("1 JAN 1970") 
                ? replacement 
                : string.Format("{0:G}", dateTime);
        }
    }

    public class UserActivities
    {
        public static UserActivities Current { get; set; }

        public List<UserActivity> ActivitiesLog { get; set; }



        static UserActivities()
        {
            Current = new UserActivities();
        }
        public UserActivities()
        {
            ActivitiesLog = new List<UserActivity>();
        }

        [LogTo_GoogleAnalytics]
        public UserActivity LogUserActivity(TMUser tmUser , UserActivity userActivity)
        {
            if (tmUser.notNull() && tmUser.ID != Guid.Empty)
            {                
                tmUser.UserActivities.Add(userActivity);
                tmUser.saveTmUser();
            }  
            ActivitiesLog.Add(userActivity);			
            return userActivity;
        }


    }

    public static class UserActivities_ExtensionMethods
    {
        public static UserActivity LogUserActivity(this TM_WebServices tm_WebServices, string name, string detail)
        {
            var currentUser = tm_WebServices.Current_User();
            if (currentUser.notNull())
            {
                currentUser.UserName.tmUser().logUserActivity(name, detail);
            }
            return null;

        }

        public static UserActivity logUserActivity(this TMUser tmUser , string action, string detail)
        {
            var userActivites = UserActivities.Current;
            if (userActivites.notNull())
            {
                var userActivity = new UserActivity
                    {
                        Action    = action, 
                        Detail    = detail, 
                        Who       = tmUser.notNull() ? tmUser.UserName :"[NoUser]",
                        When      = DateTime.Now.ToFileTimeUtc(),
                        IPAddress = HttpContextFactory.Request.UserHostAddress
                    };
                return userActivites.LogUserActivity(tmUser , userActivity);
            }
            return null;
        }

        public static UserActivities reset(this UserActivities userActivites)
        {
            userActivites.ActivitiesLog.clear();
            return userActivites;
        }

    }
}
