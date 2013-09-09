using System.Collections.Generic;
using System.Web.Mvc;
using FluentSharp.CoreLib;
using FluentSharp.ExtensionMethods;
using FluentSharp.Git;
using TeamMentor.CoreLib;

namespace TeamMentor.Website
{
    public class MarkdownController : Controller
    {        
        public static TM_Xml_Database   tmDatabase;

        static MarkdownController()
        {             
             tmDatabase = TM_Xml_Database.Current;
        }

        public TeamMentor_Article getArticle(string articleId)
        {
            var article = tmDatabase.tmGuidanceItem(articleId.guid());            
            return article;
        }

        //view article
        public ActionResult Viewer(string articleId)
        {
            if (UserRole.ReadArticles.currentUserHasRole().isFalse())
                return View(@"~/MVC/Views/Loggin_Needed.cshtml");
            var markdownToRender = getArticle(articleId).Content.Data_Json;
            ViewData["Content"] = markdownToRender.markdown_transform();
            ViewData["ArticleId"] = articleId;
            return View(@"~/MVC/Views/MarkDown_Viewer.cshtml");
        }

        //show article editor        
        public ActionResult Editor(string articleId)
        {
            if (UserRole.EditArticles.currentUserHasRole().isFalse())
                return View(@"~/MVC/Views/Loggin_Needed.cshtml");

            var article = getArticle(articleId);
            if (article.notNull())
                ViewData["Content"] = getArticle(articleId).Content.Data_Json;
            else
                ViewData["Content"] = "NO ARTICLE With GUID: {0}".format(articleId);
            ViewData["ArticleId"] = articleId;

            /*
            
            var path= @"E:\TeamMentor\TM_Dev Repos\TM_Dev_Dinis\Library_Data\XmlDatabase\TM_Libraries\Lib_Top_Vulnerabilities";
            var articlePath = tmDatabase.getXmlFilePathForGuidanceId(articleId.guid()).remove(path).replace(@"\","/").removeFirstChar();
            var nGit = path.git_Open();

            //var gitData_Repository = nGit.gitData_Repository();
            var previousVersions = articlePath.file_Commits(nGit);

             var previousVersion = new List<string>();
            //var previousVersion = nGit.files();//new List<string>();
            previousVersion.add("before");
            
            previousVersion.add(previousVersions.size().str());
            previousVersion.add("after");
            previousVersion.add(articlePath);

            ViewData["PreviousVersions"]  = previousVersions;
            */
            return View(@"~/MVC/Views/MarkDown_Editor.cshtml");
        }

        //receive article content and update article
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)] 
        public ActionResult SaveContent(string articleId, string content)
        {
            var article = getArticle(articleId);
            article.Content.Data.Value = content;
            article.xmlDB_Save_Article(tmDatabase);

            return Redirect("/article/{0}".format(articleId));

            //return Redirect("Viewer?articleId={0}".format(articleId));
        }
            
    }
}