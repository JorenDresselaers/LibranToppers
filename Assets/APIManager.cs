using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class APIManager : MonoBehaviour
{
    private const string SignInURL = "https://libra-toppers-server.azurewebsites.net/signin";

    string _email = "";
    string _password = "";

    public void SetPassword(string password)
    {
        _password = password;
    }
    
    public void SetMail(string mail)
    {
        _email = mail;
    }

    public async void LogIn()
    {
        print("Logging in");
        try
        {

            string response = await SignIn(_email, _password);

            // Parse the response string to a JObject
            JObject jsonResponse = JObject.Parse(response);

            // Convert the JObject to a formatted JSON string
            string jsonFormatted = jsonResponse.ToString();

            print("Succesfully logged in!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to log in: " + ex);
        }
    }

    public async Task<string> SignIn(string email, string password)
    {
        using (HttpClient client = new HttpClient())
        {
            // Prepare the request body
            var requestBody = new
            {
                email = email,
                password = password
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Send POST request to the sign-in endpoint
            HttpResponseMessage response = await client.PostAsync(SignInURL, content);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and return the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                // Log or handle the error
                Console.WriteLine($"Failed to sign in. Status code: {response.StatusCode}");
                return null;
            }
        }
    }
}
