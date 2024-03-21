using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContentType
{
    NONE,
    DESCRIPTION,
    MULTIPLE_CHOICE,
}

[Serializable]
public class ContentData
{
    public ContentType contentType = ContentType.NONE;
    [TextArea] public string content_text;
}


