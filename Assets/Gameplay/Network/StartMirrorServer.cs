using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StartMirrorServer : MonoBehaviour
{
    public InputField ipAddressInput; // Input field for entering server IP address

    public void StartServer()
    {
        // Start the Mirror server if it's not already running
        if (!NetworkServer.active)
        {
            NetworkManager.singleton.StartHost();
        }
    }

    public void JoinServerAsClient()
    {
        // Join the server as a client
        string networkAddress = "127.0.0.1";
        if (ipAddressInput != null && ipAddressInput.text != "") networkAddress = ipAddressInput.text; 
        NetworkManager.singleton.networkAddress = networkAddress;
        NetworkManager.singleton.StartClient();
    }
}
