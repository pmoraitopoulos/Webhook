using System;
using System.Net;

class Program
{
    static void Main()
    {
        // Get the machine's IP address
        string ipAddress = GetLocalIPAddress();
        Console.WriteLine("Declare Port");
        string port = Console.ReadLine();
        string url = $"http://{ipAddress}:{port}/login/";

        using (HttpListener listener = new HttpListener())
        {
            listener.Prefixes.Add(url);
            listener.Start();

            Console.WriteLine($"Listening for requests on {url}");

            int i = 1;
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n-----------------------------------------------");
                Console.WriteLine("Request {0}", i);
                HandleRequest(context);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("-----------------------------------------------\n");
                i++;
            }
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            string requestBody;
            using (System.IO.Stream body = request.InputStream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }
            }

            // Print request details to the console with colors
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Request received at {DateTime.Now}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Method: {request.HttpMethod}");
            Console.WriteLine($"URL: {request.Url}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Headers:");
            Console.ResetColor();

            foreach (string key in request.Headers.AllKeys)
            {
                Console.WriteLine($"{key}: {request.Headers[key]}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Body:");

            // Attempt to parse the body as JSON for better readability
            try
            {
                dynamic jsonBody = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(jsonBody, Newtonsoft.Json.Formatting.Indented));
            } catch
            {
                // If parsing fails, just display the body as it is
                Console.WriteLine(requestBody);
            }

            Console.ResetColor();

            // Send a simple response
            string responseString = "{\"status\": \"OK\", \"date\":\""+DateTime.Now+"\"}";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "application/json";

            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        } catch (Exception ex)
        {
            // Handle exceptions
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error handling request: {ex.Message}");
            Console.ResetColor();
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        } finally
        {
            Console.ResetColor();
            response.Close();
        }
    }


    static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1"; // Default to localhost if no suitable IP address is found
    }
}
