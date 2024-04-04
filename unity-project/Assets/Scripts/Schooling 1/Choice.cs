using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Choice
{
    public string choiceText;
    public bool choiceIsCorrect = false;

    public override string ToString()
    {
        return choiceText;
    }
}
