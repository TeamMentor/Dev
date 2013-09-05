using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using FluentSharp.CoreLib;
using FluentSharp.CoreLib.API;

namespace TeamMentor.CoreLib
{    
    public class SendEmails
    {   
        public static List<EmailMessage> Sent_EmailMessages { get; set; }
        public static string             TM_Server_URL      { get; set; }

        public string From          { get; set; }
        public string Smtp_Server   { get; set; }
        public string Smtp_UserName { get; set; }
        public string Smtp_Password { get; set; }

        static SendEmails()
        {
            Sent_EmailMessages = new List<EmailMessage>();
            try
            {
                TM_Server_URL = HttpContextFactory.Context.serverUrl();
            }
            catch (Exception ex)
            {
                ex.log("[SendEmails] static ctor");
            }
        }


        public SendEmails()
        {
            From = TMConfig.Current.TMSecurity.Default_AdminEmail;
            if (TM_Xml_Database.Current.notNull())
            { 
                var secretData = TM_Xml_Database.Current.UserData.SecretData;
                if (secretData.notNull())
                {
                    Smtp_Server = secretData.SMTP_Server;
                    Smtp_UserName = secretData.SMTP_UserName;
                    Smtp_Password = secretData.SMTP_Password;
                }
            }
        }
        public SendEmails(string smtpServer, string smtpUserName, string smtpPassword ) : this()
        {
            Smtp_Server     = smtpServer;
            Smtp_UserName   = smtpUserName;
            Smtp_Password   = smtpPassword;
        }
        

        public bool send_TestEmail()
        {
            return send("Test Email", "From TeamMentor");
        }        
        
        public bool send(string subject, string message)
        {
            return send(From, subject, message, false);
        }
        public bool send(string to, string subject, string message)
        {
            return send(to, subject, message, false);
        }

        public bool send(string to, string subject, string message, bool htmlMessage)
        {
            var emailMessage = new EmailMessage 
                                    {
                                        To = to, 
                                        From = this.From, 
                                        Subject = subject, 
                                        Message = message, 
                                        HtmlMessage = htmlMessage
                                    };
            return send(emailMessage);
        }
        
        //Refactor into SMTP class
        public bool send(EmailMessage_Post emailMessagePost)
        {
            var emailMessage = new EmailMessage(emailMessagePost);
            return send(emailMessage);
        }
        public bool send(EmailMessage emailMessage)
        {
            emailMessage.Message += TMConsts.EMAIL_DEFAULT_FOOTER;
            Sent_EmailMessages.Add(emailMessage);
            try
            {
                if (this.serverNotConfigured())
                {                    
                    emailMessage.SentStatus = SentStatus.NoConfig;
                    return false;   
                }
                if (emailMessage.From.notValid())
                    emailMessage.From = this.From;
                emailMessage.SentStatus = SentStatus.Sending;
                "Sending email:\n  to: {0}\n  from: {0}\n  subject: {0} ".info(emailMessage.To, emailMessage.Subject, emailMessage.Message);
                var mailMsg = new MailMessage();                
                // To
                mailMsg.To.Add(new MailAddress(emailMessage.To));
                // From
                mailMsg.From = new MailAddress(emailMessage.From);
                // Subject and multipart/alternative Body
                mailMsg.Subject = emailMessage.Subject;                
                if (emailMessage.HtmlMessage)
                {
                    mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(emailMessage.Message, null, MediaTypeNames.Text.Plain));
                    mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(emailMessage.Message, null, MediaTypeNames.Text.Html));
                }
                else
                {
                    mailMsg.Body = emailMessage.Message;
                }
                // Init SmtpClient and send
                var smtpClient = new SmtpClient(Smtp_Server, 587);
                var credentials = new System.Net.NetworkCredential(Smtp_UserName, Smtp_Password);
                smtpClient.Credentials = credentials;

                smtpClient.Send(mailMsg);
                
                emailMessage.SentStatus = SentStatus.Sent;
                emailMessage.Sent_Date = DateTime.Now.ToFileTimeUtc();
                return true;
            }
            catch (Exception ex)
            {
                ex.log("[Error sending email");
                emailMessage.SentStatus = SentStatus.Error;
                return false;
            }
        }        

        [Assert_Admin]
        public static bool SendPasswordReminderToUser(TMUser tmUser, Guid passwordResetToken)
        {
            try
            {                 
var userMessage =
@"Hi {0}, a password reminder was requested for your account.

You can change the password of your {1} account using {3}/passwordReset/{1}/{2}

If you didn't make this request, please let us know at support@securityinnovation.com.
             ".format(tmUser.fullName(), tmUser.UserName, passwordResetToken,TM_Server_URL);
             
                SendEmailToEmail(tmUser.EMail, "TeamMentor Password Reset", userMessage);
             
                //userMessage = "(sent to: {0})\n\n{1}".format(tmUser.EMail, userMessage);
                //SendEmailToTM("(user email) TeamMentor Password Reset", userMessage);
                return true;
            }
            catch (Exception ex)
            {
                ex.log();
            }
            return false;

        }

        public static Thread SendEmailToTM(string subject, string message)
        {
            return O2Thread.mtaThread(
                ()=>{
                        try
                        {
                            new SendEmails().send(subject, message);
                        }
                        catch (Exception ex)
                        {
                            ex.log("in SendEmailToTM");
                        }
                    });
        }
        public static Thread SendEmailToEmail(string to, string subject, string message)
        {
            return O2Thread.mtaThread(
                ()=>{
                        try
                        {
                            new SendEmails().send(to, subject, message);
                        }
                        catch (Exception ex)
                        {
                            ex.log("in SendEmailToTM");
                        }
                    });
        }
        
    }

    public static class SendEmail_ExtensionMethods
    {
        public static Thread email_NewUser_Welcome(this TMUser tmUser)
        {
            var email = tmUser.EMail;
            var userName = tmUser.UserName;
            var serverUrl = SendEmails.TM_Server_URL;
            if (email.notValid())
                "[SendNewUserEmails] can't sent email because email value is not set".error();
            else if (userName.notValid())
                "[SendNewUserEmails] can't sent email because userName value is not set".error();
            else if (serverUrl.notValid())
                "[SendNewUserEmails] can't sent email because server Url value is not set".error();
            else
            {
                var subject = TMConsts.EMAIL_SUBJECT_NEW_USER_WELCOME;
                var fullName = tmUser.fullName();
                var userMessage = TMConsts.EMAIL_BODY_NEW_USER_WELCOME.format(fullName, tmUser.UserName, serverUrl);
                return SendEmails.SendEmailToEmail(email, subject, userMessage);
            }
            return null;
        }

        public static bool serverNotConfigured(this SendEmails sendEmails)
        {
            return sendEmails.Smtp_Server.notValid() ||
                   sendEmails.Smtp_UserName.notValid() ||
                   sendEmails.Smtp_Password.notValid();
        }
    }
}
