using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SecurityInnovation.TeamMentor.Website.App_Code.Controllers
{
    public class InfoController : Controller
    {
        public ViewResult Index()
        {
            return View();
        }

        public string Ping()
        {
            return "pong";
        }
    }
}