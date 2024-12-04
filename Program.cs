using System;
using System.Threading.Tasks;

namespace Digifac
{
    class Program
    {
        private const string JsonFilePath = @"C:\Archivo JSON\NUC 1-FAC.json";

        static async Task Main(string[] args)
        {

            try
            {
                Api api = new Api();

                // Componer correctamente el username
                string username = "SV.06142409071059.06142409071059";
                string password = "Digifact23*";

                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"Password: {password}");

                string token = await api.GetTokenAsync(username, password);
                Console.WriteLine("Token: " + token);

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Error: El token obtenido está vacío.");
                }

                string taxid = "06142409071059";
                string format = "JSON";
                string usernameapi = "06142409071059";

                string response = await api.SendRequestAsync(JsonFilePath, token , taxid, usernameapi, format, useJson: true);
                Console.WriteLine("Respuesta: " + response);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
