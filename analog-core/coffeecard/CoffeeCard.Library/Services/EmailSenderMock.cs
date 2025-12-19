
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;

namespace CoffeeCard.Library.Services
{
    public class EmailSenderMock : IEmailSender
    {
        public Task SendEmailAsync(MimeMessage email)
        {
            Console.WriteLine($"Email mock sending to: {email.To.Aggregate("", (agg, next) =>
                $"{agg}, {next}")}");
            email.Body.WriteTo(Console.OpenStandardOutput());
            return Task.CompletedTask;
        }
    }
}