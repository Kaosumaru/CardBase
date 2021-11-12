using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class CardDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public Targetable Target;
    public Color DisabledColor = new Color(1, 1, 1, 0.0f);
    public Color EnabledColor = new Color(1, 1, 1, 0.5f);
    public Color AffectedColor = new Color(1, 1, 0.5f, 1.0f);
    public Color DropColor = new Color(1, 1, 1, 1.0f);
    public bool RaycastEnabledOutsideDrag = false;

    Image _image;
    bool _enabled = false;
    bool _affected = false;
    bool _dragInside = false;

    public static CardDragEvent OnNewDropTarget = new CardDragEvent();

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _image.raycastTarget = RaycastEnabledOutsideDrag;
        UpdateColor();

        CardDrag.OnStartedDrag.AddListener(OnStartedDrag);
        CardDrag.OnStoppedDrag.AddListener(OnStoppedDrag);

        OnNewDropTarget.AddListener(OnNewTarget);
    }

    // Update is called once per frame
    void Update()
    {

    }

    CardLogicBase BaseLogic(Card card)
    {
        return CardBaseList.GetBaseLogic(card.id);
    }

    void OnStartedDrag(CardDragArgs args)
    {
        var targets = BaseLogic(args.Card).AvailableTargets(args.Card);

        _enabled = targets.Any((t) => t == Target);
        _image.raycastTarget = _enabled;
        UpdateColor();
    }

    void OnStoppedDrag(CardDragArgs args)
    {
        _enabled = false;
        _affected = false;
        _image.raycastTarget = RaycastEnabledOutsideDrag;
        UpdateColor();
    }

    void OnNewTarget(CardDragArgs args)
    {
        _affected = args.AffectedTargets?.Contains(Target) ?? false;
        UpdateColor();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (!data.pointerDrag) return;
        var cardObj = data.pointerDrag.GetComponentInParent<CardData>();
        if (!cardObj) return;

        if (_enabled)
        {
            var affected = AffectedFields(cardObj.Card);
            OnNewDropTarget.Invoke(new CardDragArgs { Card = cardObj.Card, AffectedTargets = affected });
        }

        _dragInside = true;
        UpdateColor();
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (_enabled)
        {
            OnNewDropTarget.Invoke(new CardDragArgs { Card = null, AffectedTargets = null });
        }

        _dragInside = false;
        UpdateColor();
    }

    public void OnDrop(PointerEventData data)
    {
        UpdateColor();
        var cardObj = data.pointerDrag.GetComponentInParent<CardData>();
        if (cardObj == null) return;

        OnCardDropAction(cardObj.Card);
    }

    virtual protected IEnumerable<Targetable> AffectedFields(Card card)
    {
        // override this to provide a list of affected fields
        return null;
    }

    virtual protected void OnCardDropAction(Card card)
    {
        // override this to achieve your action on drop
    }

    void UpdateColor()
    {
        if (_enabled && _dragInside)
            _image.color = DropColor;
        else if (_affected)
            _image.color = AffectedColor;
        else if (_enabled)
            _image.color = EnabledColor;
        else
            _image.color = DisabledColor;
    }
}
