using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContentType
{
    NONE,
    DESCRIPTION,
    DESCRIPTION_IMAGE,
    MULTIPLE_CHOICE,
    ALLOCATION,
}

[Serializable]
public class ContentData
{
    public ContentType contentType = ContentType.NONE;
    public string contentHeading;
    [TextArea] public string contentText;

    public Texture contentImage;

    public List<Choice> contentChoices;

    public List<Bucket> contentBuckets;

}


