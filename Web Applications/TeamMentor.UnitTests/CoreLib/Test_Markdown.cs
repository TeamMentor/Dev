using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentSharp.CoreLib;
using TeamMentor.CoreLib;
using TeamMentor.UnitTests;

namespace TeamMentor.UnitTests.CoreLib
{
    [TestFixture]
    public class Test_Markdown
    {
        [Test] public void HtmlTransform()
        {
            var markdown1      = "#A header";
            var markdown2      = "#A header\na paragraph";
            var expectedHtml1  = "<h1 id=\"a-header\">A header</h1>\n";
            var expectedHtml2  = "<h1 id=\"a-header\">A header</h1>\n<p>a paragraph</p>\n";
            var html1 = markdown1.markdown_transform();
            var html2 = markdown2.markdown_transform();

            Assert.IsNotEmpty(html1);
            Assert.IsNotEmpty(html2);
            Assert.AreEqual(html1, expectedHtml1);
            Assert.AreEqual(html2, expectedHtml2);

        }
    }
}
