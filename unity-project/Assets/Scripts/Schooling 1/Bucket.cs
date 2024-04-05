using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Bucket
{
    public string bucketName;
    public List<BucketContent> bucketContents;
    public bool foldout = false;

    public List<string> provideBucketStrings()
    {
        List<string> strings = new List<string>();
        bucketContents.ForEach(_ => strings.Add(_.ToString())); 
        return strings;
    }
}
