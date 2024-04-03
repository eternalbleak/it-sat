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

    [SerializeField] public List<ChapterData> chapterData = new List<ChapterData>();
    [SerializeField] private List<Chapter> _chapters;
    [SerializeField] private int _currentChapterIndex = 0;
    [SerializeField] private int _currentContentIndex = -1;

    // *** UI ***

    [SerializeField] private UIDocument _uIDocument;

    [SerializeField] private Button _backButton, _forwardButton;
    [SerializeField] private VisualElement _sideBarContainer, _contentContainer;

    public List<TemplateContentTypePair> templatesNormal;
    public List<TemplateContentTypePair> templatesGamified;

    // Start is called before the first frame update
    void Start()
    {
        _uIDocument = GetComponent<UIDocument>();

        // gets the root element of the VisualElementTree
        VisualElement root = _uIDocument.rootVisualElement;

        // setup progress buttons

        _backButton = root.Query<Button>("backButton");
        _backButton.clicked += OnClickBack;

        _forwardButton = root.Query<Button>("forwardButton");
        _forwardButton.clicked += OnClickForward;

        // setup container

        _sideBarContainer = root.Query<VisualElement>("sidebarContainer");
        _contentContainer = root.Query<VisualElement>("contentContainer");
          
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    private void OnClickBack()
    {

        if (_currentContentIndex - 1 < 0) 
        {
            Debug.LogWarning("Can't go back more!");
        }
        else
        {
            --_currentContentIndex;

            SwitchContent(chapterData[_currentChapterIndex].chapterContent[_currentContentIndex]);
        }
    }

    private void OnClickForward()
    {
        if(_currentContentIndex == -1)
        {
            _backButton.RemoveFromClassList("hidden");
        }

        ++_currentContentIndex;

        SwitchContent(chapterData[_currentChapterIndex].chapterContent[_currentContentIndex]);

        

    }

    private void SwitchContent(ContentData targetContent)
    {
        _contentContainer.Clear();

        switch (targetContent.contentType)
        {
            case ContentType.DESCRIPTION:
                VisualTreeAsset temp = FindAsset(templatesNormal, targetContent.contentType);

                temp.CloneTree(_contentContainer);

                var headingLabel = (Label) _contentContainer.Query<Label>("contentHeading");
                var descriptionLabel = (Label) _contentContainer.Query<Label>("contentText");

                headingLabel.text = targetContent.contentHeading;
                descriptionLabel.text = targetContent.contentText;


                break;
            default: break;
        }
    }

    private VisualTreeAsset FindAsset(List<TemplateContentTypePair> srcList, ContentType contentType) 
    {
        VisualTreeAsset returnAsset = null; 

        foreach (TemplateContentTypePair pair in srcList)
        {
            if(pair.elementType == contentType)
            {
                returnAsset = pair.visualElement;
            }
        }

        return returnAsset;
    }


}

