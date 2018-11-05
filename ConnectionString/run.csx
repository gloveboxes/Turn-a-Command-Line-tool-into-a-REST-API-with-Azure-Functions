#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    string result;
    string functionName = "ConnectionString";

    string scope = req.Query["scope"];
    string deviceid = req.Query["deviceid"];
    string key = req.Query["key"];

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    scope = scope ?? data?.scope;
    deviceid = deviceid ?? data?.deviceid;
    key = key ?? data?.key;

    if (String.IsNullOrEmpty(scope) ||
        String.IsNullOrEmpty(deviceid) ||
        String.IsNullOrEmpty(key) )
    {
        return new BadRequestObjectResult("Please pass a IoT Central device Scope, DeviceId and key (url encoded) on the query string or in the request body");
    }

    var workingDirectory = Path.Combine(@"d:\home\site\wwwroot", functionName);
    Directory.SetCurrentDirectory(workingDirectory);//fun fact - the default working directory is d:\windows\system32
    
    string arguments = $"\"{scope}\" \"{deviceid}\" \"{key}\"";
    
    ProcessStartInfo start = new ProcessStartInfo();
    start.FileName = @"dps_cstr.exe"; 
    start.UseShellExecute = false;
    start.RedirectStandardOutput = true;
    start.Arguments = arguments;

    using (Process process = Process.Start(start))
    {
        using (StreamReader reader = process.StandardOutput)
        {
            result = reader.ReadToEnd();
        }
    }

    return result != null
        ? (ActionResult)new OkObjectResult($"{result}")
        : new BadRequestObjectResult("Please pass a IoT Central device Scope, DeviceId and key (url encoded) on the query string or in the request body");
}
