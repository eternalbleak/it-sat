using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class AllocationElement : Label
{
    public VisualElement bucket;

    public event Action<Vector2, AllocationElement> OnStartDrag = delegate { };

    public AllocationElement(VisualElement parent, string text)
    {
        bucket = parent;
        parent.Add(this);

        this.name = text;
        this.text = text;

        AddToClassList("bucketContent");

        RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    public void LoadElement()
    {
        RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        //Debug.Log(evt.position);

        if (evt.button != 0) return;

        OnStartDrag.Invoke(evt.localPosition, this);
        evt.StopPropagation();
    }
}
