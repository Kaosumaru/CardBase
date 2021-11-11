using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.UI.Game.Common;

[ExecuteAlways]
public class HandLayout : LayoutGroup
{
    public float m_maxXDistance = 170;
    public float m_OffsetXWhenLeftToSelected = 45;
    public float m_OffsetXWhenRightToSelected = 45;
    public float m_OffsetYWhenSelected = 45;

    public float m_OffsetY = -0.0003f;
    public float FocusY = -3000;
    public float m_RotationOffset = 6f;
    public float Speed = 1000.0f;
    public float m_maxAnglePerSecond = 30.0f;

    RectTransform m_rectTransform;

    class ObjectData
    {
        GameObject obj;
        CardDrag drag;
        public Vector3 targetPosition = Vector3.zero;
        public Quaternion targetRotation = Quaternion.identity;

        public static ObjectData Create(RectTransform element)
        {
            var obj = new ObjectData();
            obj.obj = element.gameObject;
            obj.drag = element.gameObject.GetComponentInChildren<CardDrag>();
            return obj;
        }

        public bool IsSelected()
        {
            return drag.PointerInside || drag.Dragging;
        }

        public void Update(HandLayout layout)
        {
            if (!obj) return;
            var pos = MovementUtils.ChangeVectorToTargetSlerp(obj.transform.localPosition, targetPosition, layout.Speed, out _);

            if (float.IsNaN(pos.x)) return; // TODO check why
            obj.transform.localPosition = pos;

            // ignoring rotation animation for now
            float maxAngle = layout.m_maxAnglePerSecond * Time.deltaTime;

            obj.transform.localRotation = Quaternion.Slerp(obj.transform.localRotation, targetRotation, maxAngle);
            //obj.transform.localRotation = targetRotation;
        }

        public void ApplyFinalPlacement()
        {
            obj.transform.localPosition = targetPosition;
            obj.transform.localRotation = targetRotation;
        }
    }

    List<ObjectData> m_data = new List<ObjectData>();


    public override void CalculateLayoutInputVertical()
    {
    }
    public override void CalculateLayoutInputHorizontal()
    {
    }

    public override void SetLayoutHorizontal()
    {
        CalculateChildrenPositions();
    }

    public override void SetLayoutVertical()
    {
        CalculateChildrenPositions();
    }

    protected override void OnEnable() { base.OnEnable(); CalculateChildrenPositions(); }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateChildrenPositions();
    }
#endif

    protected override void Start()
    {
        
    }

    void Update()
    {
        m_data.ForEach(x => x.Update(this));
    }

    void ApplyFinalPlacement()
    {
        m_data.ForEach(x => x.ApplyFinalPlacement());
    }

    private void CalculateChildrenPositions()
    {
        if (!m_rectTransform)
        {
            m_rectTransform = transform.GetComponent<RectTransform>();
        }

        var children = GetChildren();

        FillData(children);

        m_Tracker.Clear();
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            DrivenTransformProperties drivenProperties = DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Rotation;
            m_Tracker.Add(this, child, drivenProperties);

            ProcessElement(child, i, children.Count, m_data[i]);
        }

        if (!Application.isPlaying)
        {
            ApplyFinalPlacement();
        }
    }

    private void FillData(List<RectTransform> elements)
    {
        m_data = new List<ObjectData>();
        foreach (var element in elements)
        {
            m_data.Add(ObjectData.Create(element));
        }
    }

    private List<RectTransform> GetChildren()
    {
        var children = transform.Cast<RectTransform>();

        var enumerator = children.Where(rect =>
        {
            if (!rect.gameObject.activeSelf) return false;
            if (rect.GetComponents<ILayoutIgnorer>().Any(r => r.ignoreLayout)) return false;
            return true;
        });

        return enumerator.ToList();
    }


    /*
     * The layout system will first invoke SetLayoutHorizontal and then SetLayoutVertical.
     * In the SetLayoutHorizontal call it is valid to call LayoutUtility.GetMinWidth, LayoutUtility.GetPreferredWidth, and LayoutUtility.GetFlexibleWidth on the RectTransform of itself or any of its children.
     * In the SetLayoutVertical call it is valid to call LayoutUtility.GetMinHeight, LayoutUtility.GetPreferredHeight, and LayoutUtility.GetFlexibleHeight on the RectTransform of itself or any of its children.
     */

    bool IsSelectedOnRight(int i)
    {
        if (i + 1 >= m_data.Count) return false;
        return m_data[i + 1].IsSelected();
    }

    bool IsSelectedOnLeft(int i)
    {
        if (i == 0) return false;
        return m_data[i - 1].IsSelected();
    }

    private void ProcessElement(RectTransform element, int index, int count, ObjectData data)
    {
        float layouterWidth = m_rectTransform.rect.width;
        layouterWidth -= m_Padding.horizontal;

        float offset = layouterWidth / count;

        offset = Mathf.Clamp(offset, 0, m_maxXDistance);

        float width = offset * count;
        float startX = -width / 2.0f + offset / 2.0f;

        int distance = index - (count / 2);

        float posX = startX + offset * index;
        if (IsSelectedOnRight(index)) posX -= m_OffsetXWhenLeftToSelected;
        if (IsSelectedOnLeft(index)) posX += m_OffsetXWhenRightToSelected;

        float posY = m_OffsetY * posX * posX;

        data.targetPosition = new Vector3(
                posX,
                data.IsSelected() ? m_OffsetYWhenSelected : posY,
                0.0f);

        var focus = new Vector3(0, FocusY, 0);
        var v = data.targetPosition - focus;
        var angle = Vector3.SignedAngle(new Vector3(0, 100, 0), v, new Vector3(0, 0, 1));

        if (data.IsSelected())
        {
            angle = 0;
        }

        data.targetRotation = Quaternion.AngleAxis(angle, new Vector3(0,0,1));
    }
}
