using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Chapter
{
    public string title;
    public bool done;
    public VisualElement referenceElement;

    public Chapter(string title, VisualElement referenceElement)
    {
        this.title = title;
        this.done = false;
        this.referenceElement = referenceElement;
    }
}
