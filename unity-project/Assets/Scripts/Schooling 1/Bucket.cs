using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Bucket
{
    public string bucketName;
    public List<BucketContent> bucketContents;
    public bool foldout = false;

    public List<string> provideBucketStrings()
    {
        System.Random rand = new System.Random();

        List<string> strings = new List<string>();
        bucketContents.ForEach(_ => strings.Add(_.ToString()));

        List<string> shuffled = strings.OrderBy(_ => rand.Next()).ToList();

        return shuffled;
    }

    public bool BucketContainsContent(string content)
    {
        foreach (var item in bucketContents)
        {
            if(item.ToString() == content) return true;
        }

        return false;
    }
}
