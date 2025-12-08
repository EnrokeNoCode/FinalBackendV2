using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

class LocalHostServer
{
    static void Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:10105/");
        listener.Start();
        Console.WriteLine("Servidor local corriendo en http://localhost:10105/");

        while (true)
        {
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            // CORS (permitir todo tipo de solicitudes)
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");

            // Manejo de preflight (OPTIONS)
            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 204; // No Content
                response.ContentType = "text/plain";
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                response.OutputStream.Close();
                continue;
            }

            string path = request.RawUrl.ToLower();

            if (request.HttpMethod == "GET" && path.StartsWith("/load"))
            {
                // GET /load → devuelve el JSON guardado
                string file = "gestiones.json";
                string content = File.Exists(file) ? File.ReadAllText(file) : "{}";
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (request.HttpMethod == "POST" && path.StartsWith("/save"))
            {
                // POST /save → guarda el JSON en archivo
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();
                    File.WriteAllText("gestiones.json", body);
                }
                byte[] buffer = Encoding.UTF8.GetBytes("{\"message\":\"Datos guardados\"}");
                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (request.HttpMethod == "GET" && path == "/")
            {
                // GET / → devuelve el nombre del host
                string hostname = Environment.MachineName;
                byte[] buffer = Encoding.UTF8.GetBytes(hostname);
                response.StatusCode = 200;
                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (request.HttpMethod == "PUT" && path.StartsWith("/update/"))
            {
                string cod = path.Replace("/update/", "");

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = reader.ReadToEnd();

                    string file = "gestiones.json";
                    string content = File.Exists(file) ? File.ReadAllText(file) : "{}";

                    // Si el archivo contiene un solo objeto
                    var gestiones = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                    var nuevosDatos = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                    foreach (var kvp in nuevosDatos)
                    {
                        gestiones[kvp.Key] = kvp.Value;
                    }

                    string actualizado = JsonSerializer.Serialize(gestiones, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(file, actualizado);

                    byte[] buffer = Encoding.UTF8.GetBytes("{\"message\":\"Gestión actualizada\"}");
                    response.StatusCode = 200;
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                // Ruta no encontrada
                response.StatusCode = 404;
            }

            response.OutputStream.Close();
        }
    }
}
