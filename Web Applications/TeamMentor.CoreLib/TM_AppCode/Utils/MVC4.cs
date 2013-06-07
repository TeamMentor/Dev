using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace TeamMentor.CoreLib
{
    public class MVC4
    {        
        public static void MapDefaultRoutes()
        {
            //AreaRegistration.RegisterAllAreas();
            RouteTable.Routes.MapRoute("Default",
                                       "{controller}/{action}/{id}",
                                       new { action = "Index", id = UrlParameter.Optional });
        }


    }
}
