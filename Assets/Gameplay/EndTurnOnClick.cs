using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndTurnOnClick : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _interactionIndicator;

    private void OnMouseDown()
    {
        Player localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
        TurnManager.Instance?.CmdEndTurn(localPlayer);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void ToggleInteractionIndicator(bool toggle)
    {
        _interactionIndicator.SetActive(toggle);
    }
}
