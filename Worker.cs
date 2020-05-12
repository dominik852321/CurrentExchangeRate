using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;

namespace ExchangeRate
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var client = new HttpClient())
                {
                    var url = "http://data.fixer.io/api/latest?access_key=970418e105ffa7bfa3f3d70599585de0&format=1";
                    HttpResponseMessage responseMessage = await client.GetAsync(url);

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var context = await responseMessage.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<Rates>(context);
                        _logger.LogInformation("Euro = " + data.rates["PLN"].ToString() + " PLN");
                        _logger.LogInformation("Euro = " + data.rates["USD"].ToString() + " USD");
                        _logger.LogInformation("Euro = " + data.rates["GBP"].ToString() + " GBP");
                        _logger.LogInformation("Euro = " + data.rates["CNY"].ToString() + " CNY");
                        SendMail(data.rates);
                    }
                }
                await Task.Delay(3600000, stoppingToken);
            }
        }

        public void SendMail(JToken rates)
        {
            string subject = "Kurs walut";
            string body = "Kursy \n" + "Euro = " + rates["PLN"] + " Z³otych\n" + "Euro = " + rates["USD"] + " Dolarów amerykañskich\n" + "Euro = " + rates["GBP"] + " Funtów brytyjskich \n" + "Euro = " + rates["CNY"] + " Yuanów chiñskich \n";

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("domiorzechowsky@gmail.com");
            mailMessage.To.Add("domiorzechowsky@gmail.com");
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("domiorzechowsky@gmail.com", "******");
                client.Send(mailMessage);
                Console.WriteLine("Email send");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("{ex}", ex);
            }


        }
    }
}
public class Rates
{
    public bool success { get; set; }
    public string timestamp { get; set; }
    public JToken rates { get; set; }
    public string Base { get; set; }
    public DateTime date { get; set; }
}

