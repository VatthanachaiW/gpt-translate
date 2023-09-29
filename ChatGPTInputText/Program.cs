// See https://aka.ms/new-console-template for more information

using Autofac;
using ChatGPTInputText.Enums;
using ChatGPTInputText.Extensions;
using DALChatGPT.Contexts;
using DALChatGPT.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        var container = AutofacExtension.AutofacContainerConfigure();
        using var scope = container.BeginLifetimeScope();
        var context = scope.Resolve<IChatGPTContext>();

        Console.WriteLine("Write translate Email Template: ");
        var textTemplate = Console.ReadLine();
        try
        {
            var data = new MailTemplate
            {
                MailText = textTemplate,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                ParentId = 0,
                LanguageId = (int)LanguageTypes.English,
            };

            await context.Set<MailTemplate>().AddAsync(data);
            await context.SaveChangesAsync();

            Console.WriteLine($"Mail template was saved, ID: [{data.Id}]");

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "rabbit",
                Password = "P@ssw0rd!"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "EmailTemplate", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var templateData = JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(templateData);

            channel.BasicPublish(exchange: "", routingKey: "EmailTemplate", basicProperties: null, body: body);

            Console.WriteLine($"Mail template add to the Queue.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: [{ex.Message}]");
        }
    }
}