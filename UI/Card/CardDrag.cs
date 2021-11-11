using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragArgs : System.EventArgs
{
    public Card Card { get; set; }
    public IEnumerable<Targetable> AffectedTargets { get; set; }
}

public class CardDragEvent : UnityEvent<CardDragArgs>
{
}

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [System.NonSerialized]
    Card _card;
    Image _image;
    Image _glow;
    CanvasGroup _canvasGroup;
    Canvas _canvas;
    Vector3 _offset;
    Vector3 _originalPosition;
    bool _pointerInside = false;
    bool _dragging = false;

    public bool Draggable = true;

    public Color HilightColor = new Color(1,1,1,0.5f);
    public Color SelectedHilightColor = new Color(1, 1, 1, 0.5f);

    public static bool DraggingAnyCard
    {
        get;
        protected set;
    }

    public static CardDrag MouseOverCard
    {
        get;
        protected set;
    }

    public static CardDrag SelectedCard
    {
        get;
        protected set;
    }

    public static CardDragEvent OnStartedDrag = new CardDragEvent();
    public static CardDragEvent OnStoppedDrag = new CardDragEvent();

    public bool Dragging
    {
        get { return _dragging; }
        set
        {
            _dragging = value;
            DraggingAnyCard = value;
            SetSorting();
        }
    }

    public bool PointerInside
    {
        get
        {
            return _pointerInside;
        }

        set
        {
            _pointerInside = value;
            SetSorting();
            var hand = transform.parent.parent; //TODO ugly hack
            LayoutRebuilder.MarkLayoutForRebuild(hand.GetComponent<RectTransform>());
        }
    }

    protected void SetSorting()
    {
        bool v = PointerInside || Dragging;
        if (_canvas.overrideSorting == v) return;
        _canvas.overrideSorting = v;
        _canvas.sortingLayerName = _canvas.overrideSorting ? "Cards" : "Default";
    }

    void Start()
    {
        _card = GetComponentInParent<CardData>().Card;
        _image = GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();
        _glow = transform.Find("Glow").GetComponent<Image>();
    }

    public Card GetCard()
    {
        return _card;
    }

    public void OnDrag(PointerEventData data)
    {
        Vector3 rayPoint = PointerToWorldPosition(data.position);
        rayPoint.z = 10.0f;
        transform.position = rayPoint + _offset;
    }

    Vector3 PointerToWorldPosition(Vector2 press)
    {
        //Create a ray going from the camera through the press position
        Ray ray = Camera.main.ScreenPointToRay(press);
        //Calculate the distance between the Camera and the GameObject, and go this distance along the ray
        return ray.GetPoint(Vector3.Distance(transform.position, Camera.main.transform.position));
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (!Draggable || DraggingAnyCard)
        {
            data.pointerDrag = null;
            return;
        }

        Dragging = true;
        _originalPosition = transform.position;
        _image.raycastTarget = false;
  
        Vector3 rayPoint = PointerToWorldPosition(data.position);
        _offset = transform.position - rayPoint;
        _offset.z = 0.0f;

        _canvasGroup.alpha = 0.7f;
        OnStartedDrag.Invoke(new CardDragArgs { Card = _card });
    }

    public void OnEndDrag(PointerEventData data)
    {
        Dragging = false;
        _canvasGroup.alpha = 1f;

        PointerInside = false;

        transform.localPosition = Vector3.zero;
        _image.raycastTarget = true;
        OnStoppedDrag.Invoke(new CardDragArgs { Card = _card });
    }

    void Update()
    {
        if (Dragging)
        {
            _image.raycastTarget = false;
            return;
        }

        bool thisSelected = SelectedCard == this;

        _glow.color = thisSelected ? SelectedHilightColor : HilightColor;

        if (MouseOverCard != null)
        {
            _image.raycastTarget = MouseOverCard == this;
            return;
        }

        _image.raycastTarget = true;
    }


    void OnDestroy()
    {
        if (MouseOverCard == this) MouseOverCard = null;
        if (SelectedCard == this) SelectedCard = null;

        if (_dragging)
        {
            DraggingAnyCard = false;
            OnStoppedDrag.Invoke(new CardDragArgs { Card = _card });
        }
    }

    public void OnDrop(PointerEventData data)
    {
        // TODO
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (data.pointerCurrentRaycast.gameObject != gameObject) return;
        if (!data.pointerDrag)
        {
            PointerInside = true;
            MouseOverCard = this;
            return;
        }

        if (!data.pointerDrag || data.pointerDrag == gameObject) return;

    }

    public void OnPointerExit(PointerEventData data)
    {
        MouseOverCard = null;
        PointerInside = false;
    }


    public void OnPointerDown(PointerEventData data)
    {
    }

    public void OnPointerUp(PointerEventData data)
    {
        SelectedCard = this;
    }
}
