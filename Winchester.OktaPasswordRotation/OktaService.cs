using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Winchester.OktaPasswordRotation
{
    public interface IOktaService
    {
        /// <summary>
        /// Verifies a password.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <param name="password">The password of the account.</param>
        /// <returns></returns>
        Task<string> VerifyPassword(string username, string password);

        /// <summary>
        /// Changes a user account's password.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <param name="currentPassword">The current password of the account.</param>
        /// <param name="newPassword">The password that the account should be changed to.</param>
        /// <returns></returns>
        Task<string> ExecutePasswordChange(string username, string oldPassword, string newPassword);
    }

    internal class OktaService : IOktaService
    {
        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromMinutes(60) };
        private readonly OktaConfiguration _configuration;

        public OktaService(IOptions<OktaConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public async Task<string> VerifyPassword(string username, string password)
        {
            string result = "Password is invalid";
            object data = new
            {
                username = username,
                password = password
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync($"{_configuration.BaseURL}/api/v1/authn", data);
            
            if (response.IsSuccessStatusCode)
            {
                result = "Password is valid";
            }

            return result;
        }

        public async Task<string> ExecutePasswordChange(string username, string currentPassword, string newPassword)
        {
            string result = "Password change failed";
            string userID = string.Empty;
            object data = new
            {
                username = username,
                password = currentPassword
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync($"{_configuration.BaseURL}/api/v1/authn", data);

            if (response.IsSuccessStatusCode)
            {
                JObject res = JObject.Parse(await response.Content.ReadAsStringAsync());
                #pragma warning disable IDE0059 // Unnecessary assignment of a value
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                // For the time, we're just going to assume that if the response is sucessful this value will always be present.
                userID = res["_embedded"]["user"]["id"].ToString();
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
                #pragma warning restore IDE0059 // Unnecessary assignment of a value
            }
            else
            {
                throw new Exception("User ID retrieval failed.");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("SSWS", _configuration.APIToken);
            data = new
            {
                oldPassword = new
                {
                    value = currentPassword
                },
                newPassword = new
                {
                    value = newPassword
                }
            };
            response = await _client.PostAsJsonAsync($"{_configuration.BaseURL}/api/v1/users/{userID}/credentials/change_password", data);
            var stuff = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode)
            {
                result = "Password successfully changed";
            }
            else
            {
                // TODO: Log error/display to console
                var error = JObject.Parse(await response.Content.ReadAsStringAsync())["errorCauses"].FirstOrDefault()["errorSummary"].ToString();
            }

            return result;
        }
    }
}
