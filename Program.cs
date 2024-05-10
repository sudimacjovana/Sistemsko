using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace WebServer
{
    class Program
    {
        static readonly string RootDirectory = Directory.GetCurrentDirectory();  
        static HttpListener listener; 

        static void Main(string[] args)
        {
            listener = new HttpListener(); 
            listener.Prefixes.Add("http://localhost:5050/"); 
            listener.Start(); 

            Console.WriteLine("Web server pokrenut");

            while (true) 
            {
                try
                {
                    HttpListenerContext context = listener.GetContext(); 
                    ThreadPool.QueueUserWorkItem(ProcessRequest, context); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void ProcessRequest(object state)  
        {
            HttpListenerContext context = (HttpListenerContext)state; 

            string requestUrl = context.Request.Url.LocalPath; 

            if (requestUrl == "/") 
            {
                SendFileListResponse(context); 
            }
            else 
            {
                SendFileResponse(context, requestUrl); 
            }
        }

        static void SendFileListResponse(HttpListenerContext context) 
        {
            string[] files = Directory.GetFiles(RootDirectory);
            string responseHtml = "<html><body><h1>Dostupni fajlovi u root direktorijumu:</h1><ul>"; 

            foreach (string file in files) 
            {
                string fileName = Path.GetFileName(file);
                responseHtml += $"<li><a href=\"/{fileName}\">{fileName}</a></li>"; 
            }

            responseHtml += "</ul></body></html>"; 

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml); 
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length; 
            context.Response.OutputStream.Write(buffer, 0, buffer.Length); 
            context.Response.OutputStream.Close(); 
        }

        static void SendFileResponse(HttpListenerContext context, string requestUrl) 
        {
            string filePath = Path.Combine(RootDirectory, requestUrl.TrimStart('/')); 

            if (File.Exists(filePath)) 
            {
                byte[] fileBytes = File.ReadAllBytes(filePath); 
                context.Response.ContentType = "application/octet-stream"; 
                context.Response.ContentLength64 = fileBytes.Length; 
                context.Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath)); 
                context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length); 
                context.Response.OutputStream.Close(); 
            }
            else 
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; 
                context.Response.OutputStream.Close(); 
            }
        }
    }
}