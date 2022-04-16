using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers
{
    public static class SendGridStatic
    {
        public static async Task Execute(string sSendGridKey, string sFromEmail, string sFromName, string sSubject, string sToEmail, string sToName, string sPlainTextContent = "", string sHTMLContent = "")
        {
            var apiKey = sSendGridKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(sFromEmail, sFromName);
            var subject = sSubject;
            var to = new EmailAddress(sToEmail, sToName);
            var plainTextContent = sPlainTextContent;
            var htmlContent = sHTMLContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
