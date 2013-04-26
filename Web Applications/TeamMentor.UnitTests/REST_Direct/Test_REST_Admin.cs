using FluentSharp;
using NUnit.Framework;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.ExtensionMethods;
using O2.Kernel;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.REST
{
    [TestFixture]
    public class Test_REST_Admin : TM_Rest_Direct
    {
        public Test_REST_Admin()
        {
            UserGroup.Admin.setThreadPrincipalWithRoles();
        }

        //O2 Script Library
        [Test] public void CompileAllScripts()
        {
            PublicDI.log.writeToDebug(true);
            CompileEngine.clearCompilationCache();
            foreach (var method in typeof (O2_Script_Library).methods())
            {
                var code = method.invoke().str();
                var assembly = code.compileCodeSnippet();
                Assert.IsNotNull(assembly, "Failed for compile {0} with code: \n\n {1}".format(method.Name, code));
                "Compiled OK: {0}".info(method.Name);                
            }
        }

        [Test] public void Invoke_O2_Script_Library()
        {
            var result = TmRest.Admin_InvokeScript("AAAAAAAAAA");            
            Assert.AreEqual("script not found", result);
            result = TmRest.Admin_InvokeScript("ping");            
            Assert.AreEqual("pong", result);
        }

        [Test] public void SendEmail()
        {            
            var emailsSent_Before = SendEmails.Sent_EmailMessages.size();
            const string to      = "tm@si.com";
            const string subject = "a subject";
            const string message = "a message";

            var emailMessagePost  = new EmailMessage_Post 
                                        {
                                            To = to,
                                            Subject = subject,
                                            Message = message
                                        };

            TmRest.SendEmail(emailMessagePost);

            var sentMessages     = SendEmails.Sent_EmailMessages;
            var emailsSent_After = sentMessages.size();
            var lastMessage      = sentMessages.last();
   
            Assert.IsTrue           (new SendEmails().serverNotConfigured());
            Assert.GreaterOrEqual   (emailsSent_Before, 0);
            Assert.AreNotEqual      (emailsSent_Before   , emailsSent_After);
            Assert.AreEqual         (lastMessage.To      , emailMessagePost.To);
            Assert.AreEqual         (lastMessage.Subject , emailMessagePost.Subject);
            Assert.AreEqual         (lastMessage.Message , emailMessagePost.Message);
        }

        [Test] public void SendFeedback()
        {
            var emailsSent_Before = SendEmails.Sent_EmailMessages.size();
            var from              = "not-tm@not-si.com";
            var subject           = "feedback subject";
            var message           = "feedback message";

            var feedbackMessagePost = new FeedbackMessage_Post
            {
                From = from,
                Subject = subject,
                Message = message
            };

            TmRest.SendFeedback(feedbackMessagePost);

            var sentMessages = SendEmails.Sent_EmailMessages;
            var emailsSent_After = sentMessages.size();
            var lastMessage = sentMessages.last();

            Assert.IsTrue           (new SendEmails().serverNotConfigured());
            Assert.GreaterOrEqual   (emailsSent_Before, 0);
            Assert.AreNotEqual      (emailsSent_Before, emailsSent_After);
            Assert.AreEqual         (lastMessage.From, feedbackMessagePost.From);
            Assert.AreEqual         (lastMessage.Subject, feedbackMessagePost.Subject);
            Assert.AreEqual         (lastMessage.Message, feedbackMessagePost.Message);
        }
    }
}
