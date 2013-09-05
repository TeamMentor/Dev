using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FluentSharp;
using NUnit.Framework;
using FluentSharp.CoreLib;
using TeamMentor.CoreLib;
using TeamMentor.Website;

namespace TeamMentor.UnitTests.TM_Website
{
    [TestFixture]
    public class Test_WebSite_Controllers : TM_XmlDatabase_InMemory
    {
        [Test]
        public void Info_Controller()
        {            
            var infoController = new InfoController();
            Assert.IsNotNull(infoController);
            var result_Index =  infoController.Index();
            var result_Ping  = infoController.Ping();

            Assert.AreEqual("System.Web.Mvc.ViewResult", result_Index.str());
            Assert.IsInstanceOf<ViewResult>(result_Index);
            Assert.AreEqual("pong", result_Ping);                        
        }

        [Test][Assert_Editor]
        public void Markdown_Controller()
        {
            var markdownController = new MarkdownController();
            var article = markdownController.getArticle(Guid.Empty.str());
            Assert.IsNull(article);
        }

        [Test] [Assert_Admin]
        public void NodeJs_Controller()
        {
            var nodeJsController = new NodeJsController();
            var nodeJsExeFolder = this.WebSite_Root_OnDisk();
            nodeJsController.NodeJS.update_NodeJs_ExeFolder(nodeJsExeFolder);
            "nodeJs: {0}".debug(nodeJsController.NodeJS.Executable);
            Assert.IsTrue(nodeJsController.NodeJS.Executable.fileExists());
            var evalText     = "40+2";
            var evalExpected = "42";
            var evalResponse = nodeJsController.eval(evalText);
            Assert.AreEqual(evalExpected, evalResponse);
        }
    }
}
