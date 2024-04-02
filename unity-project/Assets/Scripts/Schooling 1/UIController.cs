 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

[Serializable]
public struct TemplateContentTypePair
{
    public VisualTreeAsset visualElement;
    public ContentType elementType;
}

public class UIController : MonoBehaviour
{

    [SerializeField] private List<Chapter> _chapters = new List<Chapter>();
    [SerializeField] private Chapter _currentChapter;

    // *** UI ***

    [SerializeField] private UIDocument _uIDocument;

    public List<TemplateContentTypePair> templatesNormal;
    public List<TemplateContentTypePair> templatesGamified;

    // Start is called before the first frame update
    void Start()
    {
        _uIDocument = GetComponent<UIDocument>();

        // gets the root element of the VisualElementTree
        VisualElement root = _uIDocument.rootVisualElement;

        // get chapter elements

        List<Button> temp_btns = root.Query<Button>("chapter-btn").ToList();
        
        foreach(Button btn in temp_btns)
        {
            Chapter temp = new Chapter(btn.text, btn, this);

            btn.clicked += temp.ChapterButtonClickerd;

            _chapters.Add(temp);

            if (btn.ClassListContains("btn-active"))
            {
                _currentChapter = temp;
            }
        }

        Debug.Log("DEBUG: Chapters initialized");
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchChapter(Chapter chapter)
    {
        if (chapter != _currentChapter)
        {
            _currentChapter.referenceElement.RemoveFromClassList("btn-active");
            chapter.referenceElement.AddToClassList("btn-active");
            _currentChapter = chapter;
        }
    }

}

