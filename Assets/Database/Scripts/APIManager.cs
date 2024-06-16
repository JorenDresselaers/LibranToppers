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
using static CardData;

public class APIManager : MonoBehaviour
{
    private static APIManager _instance;
    public static APIManager Instance => _instance;

    private const string SignInURL = "https://libra-toppers-server.azurewebsites.net/signin";

    [SerializeField] private GameObject _loginObject;
    [SerializeField] private TextMeshProUGUI _statusText;

    private string _email = "";
    private string _password = "";
    private User _user;
    public User UserData => _user;
    private Collection _collection;

    public bool IsLoggedIn => _user != null;

    [Serializable]
    public class User
    {
        public string _id;
        public string id;
        public string username;
        public string email;
        public string password;
        public List<BoosterPack> boosterPacks;
        public List<object> starterPacks; // You might want to replace `object` with a specific type if you know the type of starter packs.
        public bool isAdmin;
        public int __v;

        [Serializable]
        public class BoosterPack
        {
            public string id;
            public string name;
            public int amount;
            public string _id;
        }

        public string token;
    }
    [Serializable]
    public class Card
    {
        public string _id;
        public string id;
        public string name;
        public string type;
        public string[] factions;
        public string alignment;
        public int red;
        public int blue;
        public string quote;
        public List<object> abilities; // You might want to replace `object` with a specific type if you know the type of abilities.
        public int version;
        public bool isTemp;
        public int __v;
        public string img;
        public int amount;

        public Faction[] GetFactions()
        {
            Faction[] parsedFactions = new Faction[factions.Length];
            for (int i = 0; i < factions.Length; i++)
            {
                parsedFactions[i] = (Faction)Enum.Parse(typeof(Faction), factions[i].Replace(" ", ""), true);
            }
            return parsedFactions;
        }

        public Alignment GetAlignment()
        {
            return (Alignment)Enum.Parse(typeof(Alignment), alignment.Replace(" ", ""), true);
        }
    }

    [Serializable]
    public class Collection
    {
        public float version;
        public List<Card> cards;
    }

    private void Awake()
    {
        _instance = this;
    }

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
        if (_email == "" || _password == "") return;

        print("Logging in");
        try
        {
            string response = await SignIn(_email, _password);

            // Parse the response string to a JObject
            JObject jsonResponse = JObject.Parse(response);
            _user = JsonUtility.FromJson<User>(jsonResponse["user"].ToString());
            _collection = JsonUtility.FromJson<Collection>(jsonResponse["user"]["cardCollection"].ToString());
            _user.token = jsonResponse["token"].ToString();

            print("Succesfully logged in!");
            _statusText.text = "Logged in!";
            _loginObject.SetActive(false);
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to log in: " + ex);
        }

        _statusText.text = "Failed to log in.";
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

    public void ToggleUI(bool toggle)
    {
        _loginObject.SetActive(toggle);
    }
}
