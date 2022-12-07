using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APIG2.Twitch;

public static class AuthFlow
{
    private static readonly HttpListener _listener = new();

    public static async Task<string> DoOAuthFlowAsync(string url)
    {
        _listener.Prefixes.Add("http://localhost:8888/");
        var result = string.Empty;
        try
        {
            _listener.Start();

            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            });
            while (_listener.IsListening)
            {
                var context = await Task.Run(_listener.GetContextAsync);
                var query = context?.Request.Url!.Query;
                if (string.IsNullOrWhiteSpace(context?.Request.Url!.Query))
                {
                    string responseString = GetTokenHtml;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                    context!.Response.ContentLength64 = buffer.Length;
                    var output = context.Response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                }
                else if (context?.Request.Url!.Query.Contains("access_token") == true)
                {
                    var raw = context?.Request.Url!.Query.Replace("?access_token=", "");
                    result = raw!.Split('&', StringSplitOptions.RemoveEmptyEntries)[0];
                    string responseString = CloseWindowHtml;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                    context!.Response.ContentLength64 = buffer.Length;
                    var output = context.Response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                    break;
                }
                else
                {
                    break;
                }
            }

            _listener.Stop();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            _listener.Stop();
        }

        return result;
    }

    private const string GetTokenHtml = """ 
                                  <HTML>
                                    <HEAD>
                                      <script>
                                        window.onload = function() {
                                          window.location.href = 'http://localhost:8888?' + document.location.hash.replace('#', '');
                                  		    document.getElementById('code').innerHTML = document.location.hash.replace('#', '');
                                        }
                                      </script>
                                    </HEAD>
                                    <BODY>
                                      <p id='code'></p>
                                    </BODY>
                                  </HTML>
                                  """;

    private const string CloseWindowHtml = """ 
                                     <HTML>
                                       <HEAD>
                                         <script>
                                           window.onload = function() {
                                             window.close();
                                           }
                                         </script>
                                       </HEAD>      
                                       <BODY>
                                       </BODY>
                                     </HTML>
                                     """;
}