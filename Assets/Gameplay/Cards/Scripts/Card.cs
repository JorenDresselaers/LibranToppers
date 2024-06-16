using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Card : NetworkBehaviour
{
    [SerializeField] private CardData _debugData;
    [SyncVar]
    private CardData _data;

    [Header("Prefab Objects")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _vitalityText;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _factionText;
    [SerializeField] private TextMeshProUGUI _alignmentText;
    [SerializeField] private Image _image;

    [Header("Scene Objects")]
    [SyncVar] public Player _player;
    [SyncVar] private Board _board;
    public Board Board => _board;
    [SyncVar] private Hand _hand;
    public Hand Hand => _hand;

    //[Header("Prefabs")]

    [Header("Materials")]
    [SerializeField] private Material _defaultCardMaterial;
    [SerializeField] private Material _goldenCardMaterial;
    [SerializeField] private Material _flippedMaterial;

    //Data
    [SyncVar] private string _cardName;
    [SyncVar] private int _damage;
    [SyncVar] private int _startingDamage;
    [SyncVar] private int _vitality;
    [SyncVar] private int _startingVitality;
    private List<CardAbility> _abilities; //should probably be split up in lists per ability type
    [SyncVar] private string _description;
    private Sprite _sprite;
    [SyncVar] private CardData.Faction _faction;
    [SyncVar] private CardData.Alignment _alignment;
    [SyncVar] private CardData.Rarity _rarity;
    [SyncVar] private bool _isGolden;
    public List<string> _tags;


    [SyncVar(hook = nameof(OnFlipStatusChanged))]
    private bool _isFlipped;

    public string CardName => _cardName;
    public int Damage => _damage;
    public int MissingDamage => _startingDamage - _damage;
    public int Vitality => _vitality;
    public int MissingVitality => _startingVitality - _vitality;
    public CardData.Faction Faction => _faction;
    public CardData.Alignment Alignment => _alignment;

    //Interaction Logic
    private LineRenderer _lineRenderer;
    [SyncVar] private bool _isDragging = false;
    [SyncVar] private Vector3 _startPosition;
    [SerializeField, SyncVar] private bool _isDraggable = false;

    [SyncVar] private int _interactionsPerTurn = 1;
    [SyncVar] private int _interactionsLeft = 0;
    private bool _isTargetingAbility = false;
    [SyncVar] private CardAbility.Trigger _currentAbilityTrigger;

    private bool CanInteract => _interactionsLeft > 0;
    private bool IsOwnedByClient => _player.gameObject == NetworkClient.localPlayer.gameObject;
    public bool _isClickable = false;

    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    [SerializeField] private float _inspectScale = 1.5f;


    public override void OnStartClient()
    {
        Initialize(_data);
        if(_board != null) Flip();
    }

    private void OnFlipStatusChanged(bool oldValue, bool newValue)
    {
        Flip();
    }

    private void Awake()
    {
        if(_debugData) Initialize(_debugData);

        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;

        _defaultCardMaterial = Instantiate(_defaultCardMaterial);
        _flippedMaterial = Instantiate(_flippedMaterial);
        _goldenCardMaterial = Instantiate(_goldenCardMaterial);

        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;
    }

    public void SetHand(Hand hand)
    {
        _hand = hand;
    }

    public void Initialize(CardData data, bool isDraggable = true)
    {
        if(data == null)
        {
            if (_data) data = _data;
            else return;
        }

        _data = data;
        _cardName = data.cardName;
        _damage = data.damage;
        _startingDamage = data.damage;
        _vitality = data.vitality;
        _startingVitality = _vitality;
        _abilities = data.abilities.ToList();
        _description = data.description;
        _sprite = data.image;
        _faction = data.faction;
        _alignment = data.alignment;
        _rarity = data.rarity;
        _isGolden = data.isGolden;

        RemoveNullAbilities();
        UpdateText();

        _interactionsLeft = _interactionsPerTurn;

        _isDraggable = isDraggable;
    }

    public void SetFlipped(bool flipped)
    {
        if (isServer)
        {
            _isFlipped = flipped;
            RpcFlip();
        }
        else
        {
            CmdFlip(flipped);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdFlip(bool flipped)
    {
        _isFlipped = flipped;
        RpcFlip();
    }

    [ClientRpc]
    private void RpcFlip()
    {
        Flip();
    }

    // Method to flip the card
    private void Flip()
    {
        if (_player?.gameObject == NetworkClient.localPlayer?.gameObject)
        {
            _canvas.enabled = true;
            _meshRenderer.material = _isGolden ? _goldenCardMaterial : _defaultCardMaterial;
        }
        else
        {
            _canvas.enabled = !_isFlipped;
            _meshRenderer.material = _isFlipped ? _flippedMaterial : _isGolden ? _goldenCardMaterial : _defaultCardMaterial;
        }
    }

    private void RemoveNullAbilities()
    {
        if (_abilities.Count <= 0) return;

        List<int> toRemove = new();
        for(int currentAbility = 0; currentAbility < _abilities.Count; currentAbility++)
        {
            if (_abilities[currentAbility] == null) toRemove.Add(currentAbility);
        }

        foreach(int abilityToRemove in toRemove)
        {
            _abilities.RemoveAt(abilityToRemove);
        }
    }

    private void UpdateText()
    {
        if (_nameText) _nameText.text = _cardName;
        if (_damageText) _damageText.text = _damage.ToString();
        if (_vitalityText) _vitalityText.text = _vitality.ToString();
        if (_descriptionText) _descriptionText.text = _description;
        if (_image) _image.sprite = _sprite;
        if (_factionText) _factionText.text = _faction == CardData.Faction.None ? "" : _faction.ToString();
        if (_alignmentText) _alignmentText.text = _alignment == CardData.Alignment.None ? "" : _alignment.ToString();
        if (_meshRenderer)
        {
            if (_player?.gameObject == NetworkClient.localPlayer?.gameObject)
            {
                _meshRenderer.material = _isGolden ? _goldenCardMaterial : _defaultCardMaterial;
            }
            else
            {
                _meshRenderer.material = _isFlipped ? _flippedMaterial : _isGolden ? _goldenCardMaterial : _defaultCardMaterial;
            }
        }
    }

    private IEnumerator UpdateTextCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        UpdateText();
    }

    void Update()
    {
        if (_isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(_startPosition).z));
            mousePos.z = _startPosition.z; // Ensure the z position is constant

            if (_isDraggable)
            {
                transform.position = mousePos;
            }
            else
            {
                float lineRendererZOffset = -1f;

                _lineRenderer.SetPosition(0, new Vector3(_startPosition.x, _startPosition.y, _startPosition.z + lineRendererZOffset));
                _lineRenderer.SetPosition(1, new Vector3(mousePos.x, mousePos.y, mousePos.z + lineRendererZOffset));
            }

            if(_isTargetingAbility && Input.GetMouseButtonDown(0))
            {
                _isDragging = false;
                _lineRenderer.enabled = false;
                _isTargetingAbility = false;
                EndDrag();
            }
        }
    }

    private void OnMouseEnter()
    {
        // Enlarge the card and keep rotation as is
        transform.localScale = _originalScale * _inspectScale;
        transform.localRotation = _originalRotation;
    }

    private void OnMouseExit()
    {
        // Reset the card scale and rotation
        transform.localScale = _originalScale;
        transform.localRotation = _originalRotation;

        if(_hand) _hand.UpdateCardPositions();
    }

    private void OnMouseDown()
    {
        if (_board == null)
        {
            BeginInteractDrag();
        }
        else
        {
            foreach (CardAbility ability in _abilities)
            {
                if (ability._abilityTrigger == CardAbility.Trigger.ASSAULT)
                {
                    CmdSetCurrentTrigger(CardAbility.Trigger.ASSAULT);
                    print("Assaulting");
                    BeginInteractDrag();
                    return;
                }
            }
        }
    }

    public void BeginInteractDrag()
    {
        if (!IsOwnedByClient || !_isClickable) return;

        if (CanInteract || _isDraggable)
        {
            _startPosition = transform.position;
            _isDragging = true;
            if (!_isDraggable)
            {
                _lineRenderer.enabled = true;
            }
        }
    }

    private void OnMouseUp()
    {
        if (!IsOwnedByClient || !_isClickable) return;

        if (_isDragging)
        {
            _isDragging = false;
            _lineRenderer.enabled = false;
            EndDrag();
        }
    }

    private void OnDragComplete(Card other)
    {
        if (!other || other.Board == null) return;

        Debug.Log("Drag Complete with " + other.name);
        if(other)
        {
            CmdInteract(other);
        }
    }

    [Command]
    private void CmdSetCurrentTrigger(CardAbility.Trigger trigger)
    {
        _currentAbilityTrigger = trigger;
    }

    [Command]
    private void CmdInteract(Card card)
    {
        foreach (CardAbility ability in _abilities)
        {
            if (ability._abilityTrigger == _currentAbilityTrigger)
            {
                ability.Activate(this, card);
            }
        }

        if (_currentAbilityTrigger == CardAbility.Trigger.ASSAULT)
        {
            foreach (CardAbility ability in card._abilities)
            {
                if (ability._abilityTrigger == CardAbility.Trigger.DEFEND)
                {
                    ability.Activate(card, this);
                }
            }
        }
        RpcUpdateCard();
    }

    private void Interact(Card card)
    {
        OnAssault();
        card.CmdUpdateCard();
        _interactionsLeft--;
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateCard()
    {
        if (_vitality <= 0)
        {
            OnDeath();
        }
        else
        {
            RpcUpdateCard();
        }
    }

    /// <summary>
    /// Updates after .1 seconds to ensure data has arrived
    /// </summary>
    [ClientRpc]
    private void RpcUpdateCard()
    {
        StartCoroutine(UpdateTextCoroutine(0.1f));
    }

    #region Dragging

    public void BeginDrag()
    {
        StartCoroutine(DragCard());
    }

    private IEnumerator DragCard()
    {
        while (!Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
            mousePos.z = transform.position.z; // Ensure the z position is constant
            transform.position = mousePos;

            // Update line renderer or other visual feedback if necessary
            yield return null;
        }
        EndDrag();
    }

    private void EndDrag()
    {
        if (_isDraggable) //Interact with what it's dropped on
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Board")))
            {
                Board board = hit.collider.GetComponent<Board>();
                if (board != null)
                {
                    CmdAddToBoard(board);
                }
            }

            // If not dropped on a board, or board is full, reset position or handle otherwise
            ResetPosition();
        }
        else //Interact with other cards
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Card")))
            {
                if (hit.collider != null)
                {
                    OnDragComplete(hit.transform.GetComponent<Card>());
                }
            }
        }

    }

    [Command(requiresAuthority = false)]
    public void CmdAddToBoard(Board board)
    {
        RpcAddToBoard(board);
    }

    [ClientRpc]
    private void RpcAddToBoard(Board board)
    {
        if (board.AddCard(this))
        {
            _isFlipped = false;
            Flip();
            _isDraggable = false;
            _board = board;
            OnPlayed();
            OnEnterBoard();
            return;
        }
    }


    private void ResetPosition()
    {
        // Logic to reset the card's position if it's not added to the board
        transform.position = _startPosition; // or any other default position
    }

    #endregion
    #region Interactions

    private void TakeAction()
    {

    }

    public void Assault(int damage)
    {
        _vitality -= damage;
        CmdUpdateCard();
        OnStatChange();
    }

    public void ModifyStats(int assault, int vitality, bool triggerAbilities = true)
    {
        _vitality += vitality;
        _damage += assault;

        CmdUpdateCard();
        if (triggerAbilities) OnStatChange();
    }

    public void ResetStats(bool assault, bool vitality, bool triggerAbilities = true)
    {
        if(assault) _damage = _startingDamage;
        if(vitality) _vitality = _startingVitality;

        CmdUpdateCard();
        if(triggerAbilities) OnStatChange();
    }

    #endregion
    #region Ability Triggers

    private void TriggerAbility(CardAbility.Trigger trigger, bool updateCard = true)
    {
        foreach (CardAbility ability in _abilities)
        {
            if (ability != null && ability._abilityTrigger == trigger)
            {
                if (ability.IsTargeted && ability.BoardsContainsValidTarget(this, _player))
                {
                    _isTargetingAbility = true;
                    CmdSetCurrentTrigger(trigger);
                    BeginInteractDrag();
                }
                else
                {
                    ability.Activate(this);
                }
            }
        }
        if(updateCard) CmdUpdateCard();
    }

    private void OnPlayed()
    {
        TriggerAbility(CardAbility.Trigger.PLAYED);
    }

    public void OnEnterBoard()
    {
        TriggerAbility(CardAbility.Trigger.ENTEREDBOARD);
    }

    private void OnDeath()
    {
        TriggerAbility(CardAbility.Trigger.DEATH, false);
        _player.Graveyard.CmdAddCard(_data);
        _board.CmdRemoveCard(this);
    }

    private void OnDrawn()
    {
        TriggerAbility(CardAbility.Trigger.DRAWN);
    }

    private void OnAssault()
    {
        TriggerAbility(CardAbility.Trigger.ASSAULT);
    }

    private void OnDefend()
    {
        TriggerAbility(CardAbility.Trigger.DEFEND);
    }

    private void OnOtherCardDefends()
    {
        TriggerAbility(CardAbility.Trigger.OTHERCARDDEFENDS);
    }

    public void OnStartOfTurn()
    {
        _interactionsLeft = _interactionsPerTurn;
        TriggerAbility(CardAbility.Trigger.STARTOFTURN);
    }

    public void OnEndOfTurn()
    {
        TriggerAbility(CardAbility.Trigger.ENDOFTURN);
    }

    public void OnAuraCheck()
    {
        TriggerAbility(CardAbility.Trigger.AURA);
    }

    public void OnStatChange()
    {
        TriggerAbility(CardAbility.Trigger.STATCHANGE);
    }

    #endregion

    #region Tags

    public void AddTag(string tag)
    {
        _tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        _tags.Remove(tag);
    }

    public bool HasTag(string tag)
    {
        return _tags.Contains(tag);
    }

    #endregion
}
