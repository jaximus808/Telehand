using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

public static class WebCommunicator
{

    public static string host = "http://0.0.0.0:3000";
    private static readonly HttpClient client = new HttpClient();

    public static void setHost(string newPort)
    {
        WebCommunicator.host = $"http://{newPort}:3000";
    }

    public static async Task<ReturnData> PostSend(string _route, Dictionary<string, string> data)
    {

        Debug.Log($"Sending data to: {host}{_route}");
        var content = new FormUrlEncodedContent(data);
        var response = await client.PostAsync($"{host}{_route}", content);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Content-Type", "application/json");

        string responseString = await response.Content.ReadAsStringAsync(); 

        ReturnData jsonContent = JsonConvert.DeserializeObject<ReturnData>(responseString);

        return jsonContent; 
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    
}
