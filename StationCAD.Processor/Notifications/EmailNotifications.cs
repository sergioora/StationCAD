﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using RestSharp;
using RestSharp.Authenticators;

using StationCAD.Model.Notifications.Mailgun;
using NLog;

namespace StationCAD.Processor.Notifications
{
    public static class Email
    {
        public static int SendEmailMessage(string recipient, string subject, string textBody, string htmlBody)
        {
            Send("StationCAD", "donotreply", recipient, subject, textBody, htmlBody);
            return 1;
        }

        public static string SendEmailMessage(EmailNotification email)
        {
            return SendAPIMessage(email.OrganizationName, email.OrganizationEmail, email.Recipient, email.MessageSubject, email.MessageBody);
        }

        public static string SendEmailMessage(SMSEmailNotification smsEmail)
        {
            return SendAPIMessage(smsEmail.OrganizationName, smsEmail.OrganizationEmail, smsEmail.SMSEmailRecipient, smsEmail.MessageSubject, smsEmail.MessageBody);
        }

        private static string SendAPIMessage(string orgName, string orgEmail, string recipient, string subject, string body)
        {
            return Send(orgName, orgEmail, recipient, subject, body, string.Empty);
        }

        private static string Send(string fromName, string fromEmailPrefix, string recipient, string subject, string textBody, string htmlBody)
        {
            string statusResult = string.Empty;
            try
            {
                string mailgunKey = ConfigurationManager.AppSettings["mailgunKey"];
                if (mailgunKey == null)
                    throw new ApplicationException("Unable to find MailGun API Key");
                string mailgunDomain = ConfigurationManager.AppSettings["mailgunDomain"];
                if (mailgunDomain == null)
                    throw new ApplicationException("Unable to find MailGun Domain Key");
                string mailgunAPIUri = ConfigurationManager.AppSettings["mailgunAPIUri"];
                if (mailgunAPIUri == null)
                    throw new ApplicationException("Unable to find MailGun API Uri");

                RestClient client = new RestClient();
                client.BaseUrl = new Uri(mailgunAPIUri);
                client.Authenticator = new HttpBasicAuthenticator("api", mailgunKey);

                RestRequest request = new RestRequest();
                request.AddParameter("domain", mailgunDomain, ParameterType.UrlSegment);
                request.Resource = string.Format("{0}/messages", mailgunDomain);

                request.AddParameter("from", string.Format("{0} <{1}@{2}>", fromName, fromEmailPrefix, mailgunDomain));
                request.AddParameter("to", recipient);
                request.AddParameter("subject", subject);
                request.AddParameter("text", textBody);
                if (htmlBody.Length > 0)
                    request.AddParameter("html", htmlBody);
                    request.Method = Method.POST;

                IRestResponse result = client.Execute(request);
                statusResult = result.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("An error occurred in Email.SendAPIMessage(). Exception: {0}", ex.Message);
                LogException(errMsg, ex);
                throw ex;
            }
            return statusResult;
        }

        public static string CreateRoute(string orgName, string orgTag)
        {
            string statusResult = string.Empty;
            try
            {
                string mailgunKey = ConfigurationManager.AppSettings["mailgunKey"];
                if (mailgunKey == null)
                    throw new ApplicationException("Unable to find MailGun API Key");
                string mailgunDomain = ConfigurationManager.AppSettings["mailgunDomain"];
                if (mailgunDomain == null)
                    throw new ApplicationException("Unable to find MailGun Domain Key");
                string mailgunAPIUri = ConfigurationManager.AppSettings["mailgunAPIUri"];
                if (mailgunAPIUri == null)
                    throw new ApplicationException("Unable to find MailGun API Uri");

                RestClient client = new RestClient();
                client.BaseUrl = new Uri(mailgunAPIUri);
                client.Authenticator = new HttpBasicAuthenticator("api", mailgunKey);

                RestRequest request = new RestRequest();
                request.AddParameter("domain", mailgunDomain, ParameterType.UrlSegment);
                request.Resource = "routes";
                request.AddParameter("priority", 0);
                string routeName = string.Format("{0} - {1} Incident Email Route", orgTag, orgName);
                request.AddParameter("description", routeName); 
                string recipient = string.Format("{0}@{1}", orgTag, mailgunDomain);
                request.AddParameter("expression",
                                     string.Format("match_recipient('{0}')", recipient));
                request.AddParameter("action",
                                     "forward('http://stationcad.graphitegear.com/event/Process/')");
                request.AddParameter("action", "stop()");
                request.Method = Method.POST;
                IRestResponse result = client.Execute(request);
                statusResult = result.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("An error occurred in Email.CreateRoute(). Exception: {0}", ex.Message);
                LogException(errMsg, ex);
                throw ex;
            }
            return statusResult;
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void LogInfo(string message)
        {
            logger.Log(LogLevel.Info, message);
        }

        private static void LogException(string message, Exception ex)
        {
            logger.Log(LogLevel.Error, ex, message);
        }

    }
}
