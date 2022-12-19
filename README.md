# okta-console-implementation
Simple console application for verification of Okta credentials and changing of password.

1. You will need at least .NET 6.0.401 installed (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. You will need to get your SSWS token for the Okta API
3. Add said SSWS token to appsettings.json under the OktaConfiguration.APIToken property.
4. At this point, you should be able to run the project in Visual Studio.

There are currently 2 supported actions
1. Verifying credentials - Follow the prompts and enter your username/password
2. Changign credentials = Follow the prompts and enter your username/current password/new password
