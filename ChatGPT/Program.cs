// See https://aka.ms/new-console-template for more information

using Autofac;
using ChatGPT.Enums;
using ChatGPT.Extensions;
using DALChatGPT.Contexts;
using DALChatGPT.Models;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "rabbit",
            Password = "P@ssw0rd!"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "EmailTemplate", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += Consumer_Received;
        channel.BasicConsume(queue: "EmailTemplate", autoAck: true, consumer: consumer);

        Console.WriteLine("Listening to the mail template queue . . . ");
        Console.WriteLine("Press [enter] to exist");
        Console.ReadLine();
    }

    private static void Consumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        var body = e.Body.ToArray();
        var data = Encoding.UTF8.GetString(body);
        MailTemplate mailTemplate = JsonConvert.DeserializeObject<MailTemplate>(data);
        Console.WriteLine($"[X] Received mail template ID: [{mailTemplate.Id}]");

        var gpt3 = new OpenAIService(new OpenAiOptions
        {
            ApiKey = "<Open AI Key here>"
        });

        foreach (LanguageTypes languageTypes in Enum.GetValues(typeof(LanguageTypes)))
        {
            if (languageTypes == LanguageTypes.English) continue;
            TranslateTextByLanguage(gpt3, mailTemplate.MailText, languageTypes, mailTemplate.Id);
        }
    }

    private static void TranslateTextByLanguage(OpenAIService gpt3, string? mailTemplateMailText, LanguageTypes languageTypes, int parentId)
    {
        var completionResult = gpt3.Completions.CreateCompletion(new CompletionCreateRequest
        {
            Prompt = $"Could you please help me to translate this to {languageTypes}? '{mailTemplateMailText}'",
            Model = Models.TextDavinciV3,
            Temperature = 0.7F,
            MaxTokens = 512,
            TopP = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        });

        if (completionResult.Result.Successful)
        {
            var resultText = completionResult.Result.Choices.FirstOrDefault().Text.Trim();

            var data = new MailTemplate
            {
                MailText = resultText,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                ParentId = parentId,
                LanguageId = (int)languageTypes
            };
            var container = AutofacExtension.AutofacContainerConfigure();
            using var scope = container.BeginLifetimeScope();
            var context = scope.Resolve<IChatGPTContext>();

            context.Set<MailTemplate>().Add(data);
            context.SaveChanges();
            Console.WriteLine($"Mail template was save... Language: [{languageTypes}], ID: [{data.Id}]");
        }
        else
        {
            if (completionResult.Result.Error == null)
            {
                throw new Exception("Unknown Error.");
            }

            Console.WriteLine($"{completionResult.Result.Error.Code}: {completionResult.Result.Error.Message}");
        }
    }
}