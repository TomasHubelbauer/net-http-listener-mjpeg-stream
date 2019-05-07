// https://www.nuget.org/packages/System.Drawing.Common
#r "nuget: System.Drawing.Common, 4.5.1"

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

var httpListener = new HttpListener();
httpListener.Prefixes.Add("http://localhost:8000/");
httpListener.Start();

Console.WriteLine("http://localhost:8000/");

while (true)
{
  var context = await httpListener.GetContextAsync();
  switch (context.Request.Url.LocalPath)
  {
    case "/":
      {
        var bytes = await File.ReadAllBytesAsync("index.html");
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
        break;
      }
    case "/index.js":
      {
        var bytes = await File.ReadAllBytesAsync("index.js");
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
        break;
      }
    case "/index.css":
      {
        var bytes = await File.ReadAllBytesAsync("index.css");
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
        break;
      }
    case "/favicon.ico":
      {
        using (var bitmap = new Bitmap(16, 16))
        {
          using (var graphics = Graphics.FromImage(bitmap))
          {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawEllipse(Pens.White, 0, 0, 15, 15);
            graphics.DrawEllipse(Pens.Black, 1, 1, 15, 15);
            graphics.DrawString(".NET", SystemFonts.DefaultFont, Brushes.White, 0, 0);
            graphics.DrawString(".NET", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            graphics.Save();
            //bitmap.Save(context.Response.OutputStream, ImageFormat.Png);
            Icon.FromHandle(bitmap.GetHicon()).Save(context.Response.OutputStream);
            context.Response.Close();
          }
        }

        break;
      }
    case "/stream":
      {
        // Fire and forget to not block the main loop
        var _ = Task.Run(async () =>
        {
          context.Response.SendChunked = true;
          using (var bitmap = new Bitmap(1280, 720))
          {
            using (var fontFamily = new FontFamily("Arial"))
            {
              using (var font = new Font(fontFamily, 120))
              {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                  for (var index = 0; index < 100; index++)
                  {
                    graphics.Clear(Color.White);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.DrawEllipse(Pens.Black, 10, 10, 500, 500);
                    graphics.DrawString("TEST " + index.ToString(), font, Brushes.Black, 50, 50);
                    graphics.Save();
                    bitmap.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                    await context.Response.OutputStream.FlushAsync();
                  }

                  context.Response.Close();
                }
              }
            }
          }
        });

        break;
      }
    default:
      {
        context.Response.StatusCode = 404;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes($"Invalid path {context.Request.Url.LocalPath}"));
        context.Response.Close();
        break;
      }
  }
}
