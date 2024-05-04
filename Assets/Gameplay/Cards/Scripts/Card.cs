using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private CardData _debugData;
    private CardData _data;

    [Header("Prefab Objects")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _vitalityText;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _factionText;
    [SerializeField] private TextMeshProUGUI _alignmentText;
    [SerializeField] private Image _image;

    //[Header("Prefabs")]

    [Header("Materials")]
    [SerializeField] private Material _defaultCardMaterial;
    [SerializeField] private Material _goldenCardMaterial;

    //Data
    private string _cardName;
    private int _damage;
    private int _vitality;
    private List<CardAbility> _abilities;
    private string _description;
    private Sprite _sprite;
    private CardData.Faction _faction;
    private CardData.Alignment _alignment;
    private CardData.Rarity _rarity;
    private bool _isGolden;

    public string CardName => _cardName;
    public int Damage => _damage;
    public int Vitality => _vitality;
    public CardData.Faction Faction => _faction;
    public CardData.Alignment Alignment => _alignment;

    //Interaction Logic
    private LineRenderer _lineRenderer;
    private bool _isDragging = false;
    private Vector3 _startPosition;
    [SerializeField] private bool _isDraggable = false;

    private void Awake()
    {
        if(_debugData) Initialize(_debugData);

        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;

    }

    public void Initialize(CardData data, bool isDraggable = true)
    {
        _data = data;
        _cardName = data.cardName;
        _damage = data.damage;
        _vitality = data.vitality;
        _abilities = data.abilities.ToList();
        _description = data.description;
        _sprite = data.image;
        _faction = data.faction;
        _alignment = data.alignment;
        _rarity = data.rarity;
        _isGolden = data.isGolden;

        RemoveNullAbilities();
        UpdateText();

        _isDraggable = isDraggable;
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
        if(_meshRenderer) _meshRenderer.material = _isGolden ? Instantiate(_goldenCardMaterial) : Instantiate(_defaultCardMaterial);
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
        }
    }

    private void OnMouseDown()
    {
        _startPosition = transform.position;
        _isDragging = true;
        if(!_isDraggable) _lineRenderer.enabled = true;
    }

    private void OnMouseUp()
    {
        if (_isDragging)
        {
            _isDragging = false;
            _lineRenderer.enabled = false;
            EndDrag();
        }
    }

    private void OnDragComplete(Card other)
    {
        if (!other) return;

        Debug.Log("Drag Complete with " + other.name);
        if(other)
        {
            Interact(other);
        }
    }

    private void Interact(Card card)
    {
        foreach(CardAbility ability in _abilities)
        {
            ability.Activate(this, card);
        }
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
                    if (board.AddCard(this))
                    {
                        _isDraggable = false;
                        return;
                    }
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

    private void ResetPosition()
    {
        // Logic to reset the card's position if it's not added to the board
        transform.position = _startPosition; // or any other default position
    }

    #endregion

    #region Interactions

    public void Assault(int damage)
    {
        _vitality -= damage;
        UpdateText();
    }

    public void ModifyStats(int assault, int vitality)
    {
        _vitality += vitality;
        _damage += assault;

        UpdateText();
    }

    #endregion
}
