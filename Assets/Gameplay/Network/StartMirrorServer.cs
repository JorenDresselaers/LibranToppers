using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Collections;

public class StartMirrorServer : MonoBehaviour
{
    public TMP_InputField _joinCodeInput;
    public TMP_InputField _ipAddressInput;
    [SerializeField] private string _sceneName;
    public bool _useRelay = true;

    private void Awake()
    {
        if(!_useRelay)
        {
            _joinCodeInput.gameObject.SetActive(false);
        }
    }

    public void StartServer()
    {
        // Start the Mirror server if it's not already running
        if (!NetworkServer.active)
        {
            LibranToppersNetworkManager manager = NetworkManager.singleton as LibranToppersNetworkManager;

            if (_useRelay)
            {
                manager.StartRelayHost(2);
                StartCoroutine(SetCodeWhenAvailable());
            }
            else
            {
                NetworkManager.singleton.StartHost();
            }

            APIManager.Instance?.ToggleUI(false);
        }
    }

    private IEnumerator SetCodeWhenAvailable()
    {
        LibranToppersNetworkManager manager = NetworkManager.singleton as LibranToppersNetworkManager;
        yield return new WaitUntil(() => manager.relayJoinCode != "");
        _joinCodeInput.text = manager.relayJoinCode;
    }

    public void JoinServerAsClient()
    {
        // Join the server as a client
        if (_useRelay)
        {
            LibranToppersNetworkManager manager = NetworkManager.singleton as LibranToppersNetworkManager;
            if(_joinCodeInput.text != "")
            {
                manager.relayJoinCode = _joinCodeInput.text;
                manager.JoinRelayServer();
            }
        }
        else
        {
            string networkAddress = "127.0.0.1";
            if (_ipAddressInput != null && _ipAddressInput.text != "") networkAddress = _ipAddressInput.text;
            NetworkManager.singleton.networkAddress = networkAddress;
            NetworkManager.singleton.StartClient();
        }
        APIManager.Instance?.ToggleUI(false);
    }
}
