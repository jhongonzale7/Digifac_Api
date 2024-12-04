using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Digifac
{
    internal class Api
    {
        private static readonly string AuthUrl = "https://certest.digifact.com.sv/sv.com.apinucv2/api/login/get_token";
        private static readonly string ApiUrlJson = "https://certest.digifact.com.sv/sv.com.apinucv2/api/v2/transform/nuc_json?TAXID=06142409071059&USERNAME=06142409071059&FORMAT=JSON";

        /// <summary>
        /// Método para obtener el token 
        /// </summary>
        /// <param name="username">Parámetro del nombre de usuario</param>
        /// <param name="password">Clave</param>
        /// <returns>Retorna el token</returns>
        /// <exception cref="Exception"></exception>
        internal async Task<string> GetTokenAsync(string username, string password)
        {
            using HttpClient client = new HttpClient();

            var credentials = new
            {
                Username = username,
                Password = password
            };

            string jsonCredentials = JsonConvert.SerializeObject(credentials);
            var authContent = new StringContent(jsonCredentials, Encoding.UTF8, "application/json");

            HttpResponseMessage authResponse = await client.PostAsync(AuthUrl, authContent);

            if (!authResponse.IsSuccessStatusCode)
            {
                string errorResponse = await authResponse.Content.ReadAsStringAsync();
                throw new Exception($"Error de autenticación: {authResponse.StatusCode} - {errorResponse}");
            }

            string authResponseBody = await authResponse.Content.ReadAsStringAsync();
            var authData = JsonConvert.DeserializeObject<Dictionary<string, object>>(authResponseBody);

            if (!authData.TryGetValue("Token", out var tokenObj))
            {
                throw new Exception("Error: La respuesta de autenticación no contiene un Token.");
            }

            return tokenObj.ToString();
        }

        internal async Task<string> SendRequestAsync(string jsonFilePath, string token, string taxid, string username, string format, bool useJson = true)
        {
            using HttpClient client = new HttpClient();
            SetupClientHeaders(client, token);

            string jsonString = await ReadJsonFileAsync(jsonFilePath);
            ValidateJson(jsonString);

            string contentType = useJson ? "application/json" : "application/xml";

            // Construir la URL con los parámetros
            string requestUrl = $"{ApiUrlJson}?TAXID={taxid}&USERNAME={username}&FORMAT={format}";

            var content = new StringContent(jsonString, Encoding.UTF8, contentType);

            Console.WriteLine("URL de la solicitud: " + requestUrl);
            Console.WriteLine("Contenido de la solicitud:");
            Console.WriteLine(jsonString);
            Console.WriteLine("Enviando solicitud a la API...");

            HttpResponseMessage response = await client.PostAsync(requestUrl, content);
            await HandleResponseAsync(response);

            return await response.Content.ReadAsStringAsync();
        }

        private void SetupClientHeaders(HttpClient client, string token)
        {
            // Se elimina "Bearer"
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<string> ReadJsonFileAsync(string jsonFilePath)
        {
            try
            {
                return await File.ReadAllTextAsync(jsonFilePath);
            }
            catch (Exception e)
            {
                throw new Exception($"Error al leer el archivo JSON: {e.Message}");
            }
        }

        private void ValidateJson(string jsonString)
        {
            try
            {
                JToken.Parse(jsonString);
            }
            catch (JsonReaderException e)
            {
                throw new Exception("El contenido del archivo JSON no es válido.", e);
            }
        }

        private async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la solicitud: {response.StatusCode} - {errorResponse}");
            }
        }
    }
}
