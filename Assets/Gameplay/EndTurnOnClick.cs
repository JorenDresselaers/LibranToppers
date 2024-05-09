using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndTurnOnClick : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private void OnMouseDown()
    {
        Player localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
        TurnManager.Instance?.CmdEndTurn(localPlayer);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }
}
