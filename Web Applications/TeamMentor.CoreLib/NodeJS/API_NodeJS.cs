//from the O2.Platform.Scripts API_NodeJS file

using FluentSharp.CoreLib;
using TeamMentor.CoreLib;

namespace FluentSharp
{
    [Admin]
    public class API_NodeJS
    {
        public static string nodeJS_ExeLocation = @"node.exe";
        public static string nodeJS_VirtualPath = @"bin\NodeJS";

        public string Executable { get; set; }
        
        public API_NodeJS()
        {
            this.update_NodeJs_ExeFolder(TMConfig.WebRoot);
        }
    }

    public static class NodeJS_ExtensionMethods
    {
        public static string update_NodeJs_ExeFolder(this API_NodeJS nodeJs, string exeFolder)
        {
            return nodeJs.Executable = exeFolder.pathCombine(API_NodeJS.nodeJS_VirtualPath)
                                                .pathCombine(API_NodeJS.nodeJS_ExeLocation);            
        }

        public static bool isInstalled(this API_NodeJS nodeJS)
        {
            return nodeJS.Executable.fileExists();
        }

        public static string execute(this API_NodeJS nodeJS, string arguments)
        {
            return nodeJS.Executable.startProcess_getConsoleOut(arguments);
        }

/*        public static Process execute(this API_NodeJS nodeJS, string arguments, Action<string> onConsoleOut)
        {
            return nodeJS.Executable.startProcess(arguments, onConsoleOut);
        } */

        public static string help(this API_NodeJS nodeJS)
        {
            return nodeJS.execute("-h");
        }

/*        public static Process eval(this API_NodeJS nodeJS, string evalScript, Action<string> onConsoleOut)
        {
            return nodeJS.execute("-p -e {0}".format(evalScript), onConsoleOut);
        }*/

        public static string eval(this API_NodeJS nodeJS, string evalScript)
        {
            return nodeJS.execute("-p -e {0}".format(evalScript)).trim();
        }
    }
}