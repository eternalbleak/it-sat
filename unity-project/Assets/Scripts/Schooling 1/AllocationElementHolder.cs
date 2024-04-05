using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AllocationElementHolder 
{
    public List<AllocationElement> elements = new List<AllocationElement>();
    public List<VisualElement> buckets = new List<VisualElement>();

    private VisualTreeAsset templateBucket, templateBucketContent;

    private List<Bucket> bucketData;
    private VisualElement containerElement;

    protected static VisualElement ghostElement;

    private bool isDragging;
    private AllocationElement currentElement;
    private VisualElement currentBucket;

    private void Start()
    {
        templateBucket = Resources.Load("other/bucketContainer") as VisualTreeAsset;
        templateBucketContent = Resources.Load("other/bucketContent") as VisualTreeAsset;
    }

    // Start is called before the first frame update
    public void Start(ContentData contentData, VisualElement bucketContainer)
    {
        Start();

        var currentView = bucketContainer.parent;

        templateBucketContent.CloneTree(currentView); ;

        ghostElement = (VisualElement) currentView.Query<VisualElement>("bucketContent");
        ghostElement.name = "ghostElement";
        ghostElement.style.position = Position.Absolute;
        ghostElement.style.visibility = Visibility.Hidden;

        bucketData = contentData.contentBuckets;
        containerElement = bucketContainer;

        GenerateAllocation(contentData);

        ghostElement?.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        ghostElement?.RegisterCallback<PointerUpEvent>(OnPointerUp);

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


        UpdateGhostElementPosition(position);

        // hidde original element
        // let ghostElement look like the original one

        ghostElement.style.visibility = Visibility.Visible;
    }

    void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging) return;

        UpdateGhostElementPosition(evt.position);
    }

    void OnPointerUp(PointerUpEvent evt) 
    {
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

            var currentBucketContentHolder = (VisualElement)currentBucket.Query<VisualElement>("bucketContentContainer");
            currentBucketContentHolder.Remove(currentElement);
          
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

    void UpdateGhostElementPosition(Vector2 pos)
    {
        ghostElement.style.top = pos.y - ghostElement.layout.height / 2;
        ghostElement.style.left = pos.x - ghostElement.layout.width / 2;
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
        containerElement.Clear();

        foreach (var bucket in contentData.contentBuckets)
        {
            templateBucket.CloneTree(containerElement);

            var bucketContainer = (VisualElement) containerElement.Query<VisualElement>("bucketContainer");
            bucketContainer.name = bucketContainer.name + buckets.Count;

            var bucketName = (Label) bucketContainer.Query<Label>("bucketName");
            bucketName.text = bucket.bucketName;

            buckets.Add(bucketContainer);
        }

        var possibilitiesContainer = (VisualElement) containerElement.parent.Query<VisualElement>("possibilitiesBucket");
        possibilitiesContainer.Clear();

        List<string> possibilities = contentData.provideAllBucketStrings();

        foreach (string possibilty in possibilities)
        {
            elements.Add(new AllocationElement(templateBucketContent, possibilitiesContainer, possibilty));
        }

        buckets.Add(possibilitiesContainer);
        // generate possiblieties bucket
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
