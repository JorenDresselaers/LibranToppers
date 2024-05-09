using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StartMirrorServer : MonoBehaviour
{
    public InputField _ipAddressInput; // Input field for entering server IP address
    [SerializeField] private string _sceneName;

    public void StartServer()
    {
        // Start the Mirror server if it's not already running
        if (!NetworkServer.active)
        {
            NetworkManager.singleton.StartHost();
            APIManager.Instance?.ToggleUI(false);
        }
    }

    public void JoinServerAsClient()
    {
        // Join the server as a client
        string networkAddress = "127.0.0.1";
        if (_ipAddressInput != null && _ipAddressInput.text != "") networkAddress = _ipAddressInput.text; 
        NetworkManager.singleton.networkAddress = networkAddress;
        NetworkManager.singleton.StartClient();
        APIManager.Instance?.ToggleUI(false);
    }
}
