using System;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;

namespace Function
{
    public class FunctionHandler
    {
        public void Handle(string input) {

            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentNullException(nameof(input));

            var newEmail = JsonConvert.DeserializeObject<NewEmail>(input);        
            var smtpSettings = GetSmtpSettings();

            string result;

            try
            {
                var from = new MailAddress(smtpSettings.SmtpUsername, newEmail.Author);
                var to = new MailAddress(newEmail.Receiver);

                var newEmailMessage = new MailMessage(from, to)
                {
                    Priority = MailPriority.High,
                    Body = newEmail.Content,
                    Subject = newEmail.Title
                };

                using (var smtp = new SmtpClient(smtpSettings.SmtpHost, smtpSettings.SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpSettings.SmtpUsername, smtpSettings.SmtpPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(newEmailMessage);
                }

                result = $"Succesfully send a message, payload: {JsonConvert.SerializeObject(newEmail)}";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            Console.WriteLine(result);
        }

        private static SmtpSettings GetSmtpSettings()
        {
            int GetSmtpPort(string port) => port == null ? 587 : Convert.ToInt32(port);

            var smtpHost = Environment.GetEnvironmentVariable("SmtpHost");
            var smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword");
            var smtpPort = GetSmtpPort(Environment.GetEnvironmentVariable("SmtpPort"));
            var smtpUsername = Environment.GetEnvironmentVariable("SmtpUsername");

            return new SmtpSettings
            {
                SmtpHost = smtpHost,
                SmtpPassword = smtpPassword,
                SmtpPort = smtpPort,
                SmtpUsername = smtpUsername
            };
        }
    }
}
 