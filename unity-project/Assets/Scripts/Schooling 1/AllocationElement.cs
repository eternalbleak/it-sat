using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AllocationElement : VisualElement
{
    public VisualElement bucket;

    public event Action<Vector2, AllocationElement> OnStartDrag = delegate { };

    public AllocationElement(VisualTreeAsset template, VisualElement parent, string text)
    {
        bucket = parent;
        
        template.CloneTree(parent);
        
        var newBucketContent = (Label)parent.Query<Label>("bucketContent");
        newBucketContent.name = text;
        newBucketContent.text = text;

        RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0) return;

        OnStartDrag.Invoke(evt.position, this);
        evt.StopPropagation();
    }
}
