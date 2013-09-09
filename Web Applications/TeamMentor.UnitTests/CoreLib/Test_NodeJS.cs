using FluentSharp;
using NUnit.Framework;
using FluentSharp.CoreLib;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.CoreLib
{
    [TestFixture][Assert_Admin]
    public class Test_NodeJS
    {
        public API_NodeJS apiNodeJs;
        public string     nodeJsExeFolder;
        public string     nodeJS_Exe;

        public Test_NodeJS()
        {
            apiNodeJs = new API_NodeJS();
            nodeJsExeFolder = this.WebSite_Root_OnDisk();
            nodeJS_Exe = apiNodeJs.update_NodeJs_ExeFolder(nodeJsExeFolder);
        }

        [Test] public void Check_Install_and_Config()
        {
            Assert.IsNotNull(apiNodeJs);
            Assert.IsTrue   (nodeJsExeFolder.dirExists(), "virtualPath folder not found: {0}".format(nodeJsExeFolder));
            Assert.IsTrue   (nodeJS_Exe.fileExists(), "exeFromUpdate file didn't exist: {0}".format(nodeJS_Exe));
            Assert.IsTrue   (apiNodeJs.Executable.fileExists());
            Assert.AreEqual (nodeJS_Exe, apiNodeJs.Executable);
            Assert.IsTrue   (apiNodeJs.isInstalled());
        }

        [Test] public void Simple_Executions()
        {
            var helpText = apiNodeJs.help();
            Assert.IsNotNull(helpText);
            Assert.IsTrue   (helpText.contains("Usage: node [options] [ -e script | script.js ] [arguments]"));

            Assert.AreEqual("v0.6.18\r\n", apiNodeJs.execute("-v"));   // check the version

            var evalText = "2+2";
            var evalExpected = "4";
            var evalResult = apiNodeJs.eval(evalText);
            Assert.AreEqual(evalExpected, evalResult);

            var script = "var a = 40+2; console.log(42);";
            var scriptExpected = "42\n";
            var scriptFile = ".js".tempFile();
            script.saveAs(scriptFile);            
            var scriptResult = apiNodeJs.execute(scriptFile);
            "scriptFile: {0}".debug(scriptFile);
            "scriptResult: {0}".debug(scriptResult);

            Assert.AreEqual(scriptExpected, scriptResult);
        }
    }
}
