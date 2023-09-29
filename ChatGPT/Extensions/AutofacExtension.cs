using Autofac;
using DALChatGPT.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ChatGPT.Extensions;

public static class AutofacExtension
{
    public static IContainer AutofacContainerConfigure()
    {
        var builder = new ContainerBuilder();

        builder.Register((com, _) =>
        {
            var strConnection = "Data Source=ai-pc;Initial Catalog=ChatGPT;Persist Security Info=True;User ID=sa;Password=P@ssw0rd!;TrustServerCertificate=True";

            var dbContextOptions = new DbContextOptionsBuilder<ChatGPTContext>()
                .UseSqlServer(strConnection, options =>
                {
                    options.EnableRetryOnFailure(5);
                    options.CommandTimeout(200);
                });

            return new ChatGPTContext(dbContextOptions.Options);
        }).As<IChatGPTContext>().InstancePerLifetimeScope();


        return builder.Build();
    }
}