using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AllocationElementHolder 
{
    public int id;
    public List<AllocationElement> elements = new List<AllocationElement>();
    public List<VisualElement> buckets = new List<VisualElement>();

    private VisualTreeAsset templateBucket, templateBucketContent;

    private List<Bucket> bucketData;
    private VisualElement containerElement;

    protected static VisualElement ghostElement;

    private bool isDragging;
    private AllocationElement currentElement;
    private VisualElement currentBucket;

    private UIController controller;

    private VisualElement bucketsHolder, possibilitiesHolder;

    private void Start()
    {
        templateBucket = Resources.Load("other/bucketContainer") as VisualTreeAsset;
        templateBucketContent = Resources.Load("other/bucketContent") as VisualTreeAsset;
    }

    // Start is called before the first frame update
    public void Start(UIController controller, ContentData contentData, VisualElement bucketContainer)
    {
        Start();

        this.controller = controller;

        templateBucketContent.CloneTree(bucketContainer);

        ghostElement = (VisualElement)bucketContainer.Query<VisualElement>("bucketContent");
        ghostElement.name = "ghostElement";
        ghostElement.style.position = Position.Absolute;
        ghostElement.style.visibility = Visibility.Hidden;

        bucketData = contentData.contentBuckets;
        containerElement = bucketContainer;

        GenerateAllocation(contentData);

        ghostElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        ghostElement.RegisterCallback<PointerUpEvent>(OnPointerUp);

        foreach (var element in elements)
        {
            element.OnStartDrag += OnPointerDown;
        }
    }

    void OnPointerDown(Vector2 position, AllocationElement element)
    {
        isDragging = true;
        currentElement = element;
        currentBucket = element.bucket;


        controller.SetGhostElement(ghostElement);

        // hidde original element
        // let ghostElement look like the original one

        ghostElement.style.visibility = Visibility.Visible;
    }

    void OnPointerMove(PointerMoveEvent evt)
    {
        //Debug.LogWarning(evt.position);

        //if (!isDragging) return;

        //Debug.Log(evt.position);

        //controller.SetGhostElement(ghostElement);
    }

    void OnPointerUp(PointerUpEvent evt) 
    {
        if (!isDragging) return;
        // figure out in which bucket we are 

        // by @adammyhre
        VisualElement closesBucket = buckets
            .Where(elem => elem.worldBound.Overlaps(ghostElement.worldBound))
            .OrderBy(elem => Vector2.Distance(elem.worldBound.position, ghostElement.worldBound.position))
            .FirstOrDefault();

        if(closesBucket != null && closesBucket != currentBucket)
        {
            var closesBucketContentHolder = (VisualElement) closesBucket.Query<VisualElement>("bucketContentContainer");
            closesBucketContentHolder.Add(currentElement);

            //var currentBucketContentHolder = (VisualElement)currentBucket.Query<VisualElement>("bucketContentContainer");
            //currentBucketContentHolder.Remove(currentElement);
          
            currentElement.bucket = closesBucket;
        }
        else
        {
            // reset ghost and hidden allocation element
        }

        isDragging = false;
        currentElement = null;
        currentBucket = null;
        ghostElement.style.visibility = Visibility.Hidden;
    }

    private void OnDestroy()
    {
        foreach (var element in elements)
        {
            element.OnStartDrag -= OnPointerDown;
        }
    }

    public void GenerateAllocation(ContentData contentData)
    {
        // generate buckets

        var bucketsContainer = (VisualElement)containerElement.Query<VisualElement>("contentBuckets");
        var possibilitiesContainer = (VisualElement)containerElement.Query<VisualElement>("possibilitiesBucket");

        bucketsContainer.Clear();

        foreach (var bucket in contentData.contentBuckets)
        {
            templateBucket.CloneTree(bucketsContainer);

            var bucketContainer = (VisualElement)bucketsContainer.Query<VisualElement>("bucketContainer");
            bucketContainer.name = bucketContainer.name + buckets.Count;

            var bucketName = (Label) bucketContainer.Query<Label>("bucketName");
            bucketName.text = bucket.bucketName;

            buckets.Add(bucketContainer);
        }

        
        possibilitiesContainer.Clear();

        List<string> possibilities = contentData.provideAllBucketStrings();

        foreach (string possibilty in possibilities)
        {
            elements.Add(new AllocationElement(possibilitiesContainer, possibilty));
        }

        buckets.Add(possibilitiesContainer);
        // generate possiblieties bucket
    }

    public void SaveContentBuckets()
    {
        bucketsHolder = (VisualElement)containerElement.Query<VisualElement>("contentBuckets");
        possibilitiesHolder = (VisualElement)containerElement.Query<VisualElement>("possibilitiesBucket");

        foreach (var element in elements)
        {
            element.OnStartDrag -= OnPointerDown;
        }
    }

    public void LoadContentBuckets(VisualElement containerElement)
    {
        this.containerElement = containerElement;
        var contentBuckets = (VisualElement)containerElement.Query<VisualElement>("contentBuckets");
        var possibilitiesBuckets = (VisualElement)containerElement.Query<VisualElement>("possibilitiesBucket");

        containerElement.Remove(contentBuckets);
        containerElement.Remove(possibilitiesBuckets);

        containerElement.Add(bucketsHolder);
        containerElement.Add(possibilitiesHolder);

        foreach (var element in elements)
        {
            element.LoadElement();
            element.OnStartDrag += OnPointerDown;
        }
    }


    public List<string> GetBucketSelection(VisualElement bucket)
    {
        List<string> bucketSelection = new List<string>();

        var bucketContentContainer = (VisualElement) bucket.Query<VisualElement>("bucketContentContainer");
        var bucketContentChildren = bucketContentContainer.Children().Cast<Label>();

        foreach (var bucketContent in bucketContentChildren)
        {
            bucketSelection.Add(bucketContent.text);
        }
        
        return bucketSelection;
    }

    public bool CheckBucket(List<string> correctContents, List<string> selectedContents)
    {
        if (correctContents.Count != selectedContents.Count) return false;

        foreach (var correct in correctContents)
        {
            if(!selectedContents.Contains(correct)) return false;
        }

        return true;
    }

    public bool CheckAllocation(List<VisualElement> bucketsElements, List<Bucket> bucketDatas)
    {
        for (int i = 0; i < bucketData.Count; i++)
        {
            if (!CheckBucket(bucketDatas[i].provideBucketStrings(), GetBucketSelection(bucketsElements[i]))) return false;
        }

        return true;
    }


}
