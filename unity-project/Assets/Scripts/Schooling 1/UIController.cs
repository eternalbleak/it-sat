 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

/// <summary>
/// Pairs a VisualTreeAsset to a specifc ContentType.
/// </summary>
[Serializable]
public struct TemplateContentTypePair
{
    public VisualTreeAsset visualElement;
    public ContentType elementType;
}

[Serializable]
public struct ChoiceElementContainer
{
    public int contentIndex;
    public List<VisualElement> choiceElements;
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

    [SerializeField] private List<ChoiceElementContainer> choiceElements;

    public List<TemplateContentTypePair> templatesNormal;
    public List<TemplateContentTypePair> templatesGamified;

    public VisualTreeAsset choiceTemplate;

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
        else if(chapterData[_currentChapterIndex].chapterContent[_currentContentIndex].contentType == ContentType.MULTIPLE_CHOICE)
        {
            Debug.Log(checkChoices(chapterData[_currentChapterIndex].chapterContent[_currentContentIndex].contentChoices, GetChoiceElements(_currentContentIndex)));
            return;
        }


        ++_currentContentIndex;

        SwitchContent(chapterData[_currentChapterIndex].chapterContent[_currentContentIndex]);

        

    }

    private void SwitchContent(ContentData targetContent)
    {
        _contentContainer.Clear();

        VisualTreeAsset temp = null;

        Label headingLabel, descriptionLabel;
        VisualElement imageElement, choicesContainer;

        switch (targetContent.contentType)
        {
            case ContentType.DESCRIPTION:

                temp = FindAsset(templatesNormal, targetContent.contentType);

                temp.CloneTree(_contentContainer);

                headingLabel = (Label) _contentContainer.Query<Label>("contentHeading");
                descriptionLabel = (Label) _contentContainer.Query<Label>("contentText");

                headingLabel.text = targetContent.contentHeading;
                descriptionLabel.text = targetContent.contentText;

                break;

            case ContentType.DESCRIPTION_IMAGE:

                temp = FindAsset(templatesNormal, targetContent.contentType);

                temp.CloneTree(_contentContainer);

                headingLabel = (Label)_contentContainer.Query<Label>("contentHeading");
                descriptionLabel = (Label)_contentContainer.Query<Label>("contentText");
                imageElement = (VisualElement)_contentContainer.Query<VisualElement>("contentImage");

                headingLabel.text = targetContent.contentHeading;
                descriptionLabel.text = targetContent.contentText;
                imageElement.style.backgroundImage = new StyleBackground(targetContent.contentImage);

                break;

            case ContentType.MULTIPLE_CHOICE:

                temp = FindAsset(templatesNormal, targetContent.contentType);

                temp.CloneTree(_contentContainer);

                headingLabel = (Label)_contentContainer.Query<Label>("contentHeading");
                choicesContainer = (VisualElement)_contentContainer.Query<VisualElement>("choicesContainer");

                headingLabel.text = targetContent.contentText;
                choicesContainer.Clear();

                // only create the multichoice layout in the first round. Otherwise the answers are always in random arrangements
                if (!ChoicesAlreadyGenerated(_currentContentIndex))
                {
                    ChoiceElementContainer tempContainer = new ChoiceElementContainer();
                    tempContainer.contentIndex = _currentContentIndex;
                    tempContainer.choiceElements = new List<VisualElement>();

                    // shuffles the contentChoices List to be different every time by Erick William (https://stackoverflow.com/questions/273313/randomize-a-listt)
                    System.Random rand = new System.Random();
                    List<Choice> shuffledChoices = targetContent.contentChoices.OrderBy(_ => rand.Next()).ToList();


                    foreach (Choice choice in shuffledChoices)
                    {
                        choiceTemplate.CloneTree(choicesContainer);

                        var currentChoiceElement = (VisualElement)choicesContainer.Query<VisualElement>("choiceContainer");
                        currentChoiceElement.name = currentChoiceElement.name + choiceElements.Count;

                        var choiceText = (Label)currentChoiceElement.Query<Label>("choiceText");

                        choiceText.text = choice.choiceText;

                        tempContainer.choiceElements.Add(currentChoiceElement);
                    }

                    choiceElements.Add(tempContainer);
                    break;
                }



                foreach (VisualElement choiceElement in GetChoiceElements(_currentContentIndex))
                {
                    choicesContainer.Add(choiceElement);
                }


                break;

            case ContentType.ALLOCATION:

                temp = FindAsset(templatesNormal, targetContent.contentType);
                temp.CloneTree(_contentContainer);

                var contentBuckets = (VisualElement)_contentContainer.Query<VisualElement>("contentBuckets");

                AllocationElementHolder holder = new AllocationElementHolder();
                holder.Start(targetContent, contentBuckets);


                break;

            default: break;
        }
    }

    /// <summary>
    /// Finds the right VSA for a given contentType within a given list.
    /// </summary>
    /// <param name="srcList"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
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

        if(returnAsset == null)
        {
            Debug.LogWarning("No Template Content Pair found.");
        }

        return returnAsset;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool ChoicesAlreadyGenerated(int index)
    {
        foreach (var item in choiceElements)
        {
            if(item.contentIndex == index)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private List<VisualElement> GetChoiceElements(int index)
    {
        foreach(var item in choiceElements)
        {
            if (item.contentIndex == index)
            {
                return item.choiceElements;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if the choices checked are the correct ones
    /// </summary>
    /// <param name="choicesSolution">correct choices</param>
    /// <param name="choicesToCheck">choices that need to be checked</param>
    /// <returns></returns>
    private bool checkChoices(List<Choice> choicesSolution, List<VisualElement> choicesToCheck)
    {
        // only use the checked/correct ones other can be discarded
        var correctChoices = choicesSolution.Where(_ => _.choiceIsCorrect == true).ToList();
        var checkedChoices = choicesToCheck.Where(_ => {
            var toggle = (Toggle)_.Query<Toggle>("choiceToggle");
            return toggle.value;
        }).ToList();

        // below only checks if all correct are selected, if there anymore selected the number varies from the correct one -> conversion must not be execute everytime
        if(checkedChoices.Count != correctChoices.Count) { return false; }

        // converting visualelements and choices to string for easyier comparison
        List<string> answers = new List<string>();

   
        foreach (var answer in checkedChoices)
        {
            var choiceText = (Label)answer.Query<Label>("choiceText");
            answers.Add(choiceText.text);
        }

        foreach(var solution in correctChoices)
        {
            if (!answers.Contains(solution.ToString())) { return false; }
        }

        return true;
    }


}

