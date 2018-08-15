using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;

namespace Function
{
    public class FunctionHandler
    {
        private const string SecretsLocation = "/var/openfaas/secrets";

        public string Handle(string input)
        {
            if (!Authorize()) return "Unauthorized";

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

            return result;
        }

        private static bool Authorize()
        {
            var secret = ReadSecret("api-key");
            var headerAuth = Environment.GetEnvironmentVariable("Http_Authorization");

            bool result;

            if (headerAuth == null || headerAuth != "Bearer " + secret) {
                result = false;
            } else {
                result = true;
            }

            return result;
        }

        private static SmtpSettings GetSmtpSettings()
        {
            int GetSmtpPort(string port) => port == null ? 587 : Convert.ToInt32(port);

            var smtpHost = Environment.GetEnvironmentVariable("SmtpHost");
            var smtpPassword = ReadSecret("smtp-password");
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

        private static string ReadSecret(string secretName)
        {
            return File.ReadAllText($"{SecretsLocation}/{secretName}").Trim();
        }
    }
}
 