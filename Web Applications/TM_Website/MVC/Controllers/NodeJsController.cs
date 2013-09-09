using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentSharp;

namespace TeamMentor.Website
{
    public class NodeJsController : Controller
    {
        public API_NodeJS NodeJS { get; set; }

        public NodeJsController()
        {
            NodeJS = new API_NodeJS();
        }

        public string eval(string evalText)
        {
            return NodeJS.eval(evalText);
        }
    }
}