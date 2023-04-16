using System;
using System.Net.Http;
using System.Net.Mail;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.IO;

namespace KapNews
{
    public class Function1
    {
        [FunctionName("KapNews")]
        public void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            string s= null;
            int i = 0;

            if (File.Exists("LastKapId.txt"))
                s = File.ReadAllText("LastKapId.txt");

            if (!string.IsNullOrWhiteSpace(s))
                i = Convert.ToInt32(s.Trim());

            Console.WriteLine(i);
            var httpClient = new HttpClient();
            var result = httpClient.GetStringAsync("https://www.kap.org.tr/tr/api/disclosures").Result;
            JArray arr = JArray.Parse(result);
            string[] mailList = new[] { "tanjuileri92@gmail.com", "cenkfidan93@gmail.com", "rumeysaakgunn@gmail.com" };

            foreach (var jobj in arr)
            {
                var obj = jobj["basic"].Value<JObject>();

                if (obj["title"].Value<string>() == "Yeni Ýþ Ýliþkisi" || obj["summary"].Value<string>() == "Yeni Ýþ Ýliþkisi")
                {
                    var client = new SmtpClient("smtp.office365.com", 587)
                    {
                        Credentials = new NetworkCredential("kaphaberleri@hotmail.com", "kaphaber!."),
                        EnableSsl = true
                    };

                    MailAddress from = new MailAddress("kaphaberleri@hotmail.com", "kaphaberleri");

                    foreach (var item in mailList)
                    {
                        MailAddress to = new MailAddress(item);
                        MailMessage myMail = new MailMessage(from, to);
                        myMail.Subject = $"{obj["stockCodes"]?.Value<string>()} --> {obj["companyName"]?.Value<string>()} --> {obj["summary"]?.Value<string>()}";
                        myMail.Body = $"Bu mail Tanju ve Cenk'in FAKÝRLERE hayratýdýr.";
                        client.Send(myMail);
                    }
                }
            }

            File.WriteAllText("LastKapId.txt", (i + 1).ToString());

        }
    }
}
