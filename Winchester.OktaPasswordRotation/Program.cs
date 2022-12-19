using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Winchester.OktaPasswordRotation;

#region DI/Config Building
IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

IHost host = Host.CreateDefaultBuilder().ConfigureServices(services => {
    services.AddScoped<IOktaService, OktaService>();
    services.Configure<OktaConfiguration>(options => Configuration.GetSection(nameof(OktaConfiguration)).Bind(options));
}).Build();
#endregion

IOktaService oktaService = host.Services.GetRequiredService<IOktaService>();

string userInput = string.Empty;
do
{
    Console.WriteLine("Enter:");
    Console.WriteLine("\t<1> to validate your password");
    Console.WriteLine("\t<2> to change your password");
    Console.WriteLine("\t<exit> to exit");
    Console.Write("Input: ");
    userInput = Console.ReadLine() ?? string.Empty;
    
    if (userInput.Equals("1"))
    {
        (string username, string password) = PromptValidatePassword();
        Console.WriteLine(await oktaService.VerifyPassword(username, password));
    }
    else if (userInput.Equals("2"))
    {
        (string username, string currentPassword, string newPassword) = PromptChangePassword();
        try
        {
            Console.WriteLine(await oktaService.ExecutePasswordChange(username, currentPassword, newPassword));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    else if (!userInput.Equals("exit"))
    {
        Console.WriteLine("Invalid input.");
    }

    Console.WriteLine("\n\n");
} 
while (!userInput.ToLower().Equals("exit"));

(string, string) PromptValidatePassword()
{
    Console.Write("Enter username: ");
    string username = Console.ReadLine() ?? string.Empty;
    Console.Write("Enter password: ");
    string password = Console.ReadLine() ?? string.Empty;

    return (username, password);
}

(string, string, string) PromptChangePassword()
{
    Console.Write("Enter username: ");
    string username = Console.ReadLine() ?? string.Empty;
    Console.Write("Enter current password: ");
    string currentpassword = Console.ReadLine() ?? string.Empty;
    Console.Write("Enter new password: ");
    string newPassword = Console.ReadLine() ?? string.Empty;

    return (username, currentpassword, newPassword);
}