using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


[CreateAssetMenu(menuName ="Custom/ChapterData", fileName ="new ChapterData")]
public class ChapterData : ScriptableObject
{
    public string title;
    public List<ContentData> chapterContent;
}


#if UNITY_EDITOR

/*TODO
 - Add Documentation, else nobody knows what stuff does 
 - Add different Content Types, Multiple Choice and Single Choice, Moveable Stuff thing
 */

[CustomEditor(typeof(ChapterData))]
class ChapterDataEditor : Editor
{
    private const int MAX_MULTICHOICE = 5;

    ChapterData data;

    SerializedProperty chapterContentProperty; // prop of List<ContenData> chapterContent)

    ReorderableList chapterContentList;

    Dictionary<string, ReorderableList> innerListLookup = new Dictionary<string, ReorderableList>();

    private void OnEnable()
    {
        data = (ChapterData)target;

        chapterContentProperty = serializedObject.FindProperty(nameof(ChapterData.chapterContent));

        // List that holds the individual chapterContent Items
        chapterContentList = new ReorderableList(serializedObject, chapterContentProperty)
        {
            displayAdd = true,
            displayRemove = true,
            draggable = true,

            drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Chapter Content List"); },

            drawElementCallback = (Rect rect, int contentIndex, bool isActive, bool isFocused) =>
            {
                var element = chapterContentList.serializedProperty.GetArrayElementAtIndex(contentIndex);

                var contentTextProp = element.FindPropertyRelative(nameof(ContentData.contentText));
                var contenTypeProp = element.FindPropertyRelative(nameof(ContentData.contentType));
                var contentChoicesProp = element.FindPropertyRelative(nameof(ContentData.contentChoices));
                var contentBucketsProp = element.FindPropertyRelative(nameof(ContentData.contentBuckets));

                string listKey = element.propertyPath;

                if (element != null)
                {
                    data.chapterContent[contentIndex].contentType = (ContentType)EditorGUI.EnumPopup(
                         new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Content Type: ", data.chapterContent[contentIndex].contentType);

                    rect.y += EditorGUI.GetPropertyHeight(contenTypeProp);

                    var text_heigth = EditorGUI.GetPropertyHeight(contentTextProp);

                    switch (data.chapterContent[contentIndex].contentType)
                    {
                        
                        case ContentType.NONE: break;
                        case ContentType.DESCRIPTION:
                            
                            EditorGUI.PropertyField(
                                 new Rect(rect.x, rect.y, rect.width, text_heigth), contentTextProp, new GUIContent("Content Text"));
                            break;

                        case ContentType.MULTIPLE_CHOICE: 
                            
                            EditorGUI.PropertyField(
                                 new Rect(rect.x, rect.y, rect.width, text_heigth), contentTextProp, new GUIContent("Question Text"));

                            //second list that holds multiple choice
                            ReorderableList multipleChoiceList;

                            listKey = element.propertyPath + data.chapterContent[contentIndex].contentType.ToString();

                            if (innerListLookup.ContainsKey(listKey))
                            {
                                multipleChoiceList = innerListLookup[listKey];
                            }
                            else
                            {
                                multipleChoiceList = new ReorderableList(contentChoicesProp.serializedObject, contentChoicesProp)
                                {
                                    displayAdd = true,
                                    displayRemove = true,
                                    draggable = true,

                                    drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Answer Options"); },

                                    drawElementCallback = (Rect rect, int multChoiceElementIndex, bool isActive, bool isFocused) =>
                                    {
                                        var multiChoiceElement = contentChoicesProp.GetArrayElementAtIndex(multChoiceElementIndex);

                                        var choiceText = multiChoiceElement.FindPropertyRelative(nameof(Choice.choiceText));


                                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), choiceText);
                                        rect.y += EditorGUIUtility.singleLineHeight;

                                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight), "Correct Answer?");

                                        data.chapterContent[contentIndex].contentChoices[multChoiceElementIndex].choiceIsCorrect =
                                        EditorGUI.Toggle(new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight), data.chapterContent[contentIndex].contentChoices[multChoiceElementIndex].choiceIsCorrect);
                                    },

                                    elementHeight = 2 * EditorGUIUtility.singleLineHeight,

                                    onAddCallback = (ReorderableList list) =>
                                    {
                                        // secures the only 5 multiple choice anwers are possible, also adds a completly fresh one instead of copying the previous
                                        if (list.count < MAX_MULTICHOICE)
                                        {
                                            list.serializedProperty.arraySize++;
                                            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);

                                            var newChoiceText = newElement.FindPropertyRelative(nameof(Choice.choiceText));

                                            newChoiceText.stringValue = "";
                                        }
                                        else
                                        {
                                            Debug.Log("Reached Max Multiple Choice Count!");
                                        }
                                    }
                                };

                                innerListLookup[listKey] = multipleChoiceList;
                            }

                            //displaying new sublist
                            multipleChoiceList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + GetMultipleChoiceHeight(contentChoicesProp.GetArrayElementAtIndex(contentIndex)), rect.width, rect.height));

                            break;

                        case ContentType.ALLOCATION:

                            //buckets list
                            ReorderableList bucketList;

                            listKey = element.propertyPath + data.chapterContent[contentIndex].contentType.ToString();

                            if (innerListLookup.ContainsKey(listKey))
                            {
                                bucketList = innerListLookup[listKey];
                            }
                            else
                            {
                                bucketList = new ReorderableList(contentBucketsProp.serializedObject, contentBucketsProp)
                                {
                                    displayAdd = true,
                                    displayRemove = true,
                                    draggable = true,

                                    drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Buckets"); },

                                    drawElementCallback = (Rect rect, int bucketIndex, bool isActive, bool isFocused) =>
                                    {
                                        var bucketElement = contentBucketsProp.GetArrayElementAtIndex(bucketIndex);

                                        var bucketContents = bucketElement.FindPropertyRelative(nameof(Bucket.bucketContents));

                                        var bucketFoldout = bucketElement.FindPropertyRelative(nameof(Bucket.foldout));
                                        var bucketName = bucketElement.FindPropertyRelative(nameof(Bucket.bucketName));

                                        bucketFoldout.boolValue = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, 10, EditorGUIUtility.singleLineHeight), bucketFoldout.boolValue,
                                            bucketFoldout.boolValue ? "" : bucketName.displayName + " : " + bucketName.stringValue);

                                        if (bucketFoldout.boolValue == true)
                                        {
                                            EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), bucketName);

                                            // Bucket Content List
                                            ReorderableList bucketContentList;

                                            listKey = contentBucketsProp.propertyPath + bucketName.propertyPath;

                                            if (innerListLookup.ContainsKey(listKey))
                                            {
                                                bucketContentList = innerListLookup[listKey];
                                            }
                                            else
                                            {
                                                bucketContentList = new ReorderableList(bucketContents.serializedObject, bucketContents)
                                                {
                                                    displayAdd = true,
                                                    displayRemove = true,
                                                    draggable = true,

                                                    drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, bucketName.stringValue); },

                                                    drawElementCallback = (Rect rect, int bucketContentIndex, bool isActive, bool isFocused) =>
                                                    {
                                                        var bucketContentElement = bucketContents.GetArrayElementAtIndex(bucketContentIndex);
                                                        var contentText = bucketContentElement.FindPropertyRelative(nameof(BucketContent.contentText));

                                                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), contentText);
                                                    },

                                                    onAddCallback = (ReorderableList list) =>
                                                    {
                                                        list.serializedProperty.arraySize++;

                                                        var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                                                        var contentText = newElement.FindPropertyRelative(nameof(BucketContent.contentText));

                                                        contentText.stringValue = "";
                                                        
                                                    },
                                                };

                                                innerListLookup[listKey] = bucketContentList;
                                            }

                                            bucketContentList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, rect.width));

                                        }
                                    },

                                    elementHeightCallback = (int index) =>
                                    {
                                        return GetBucketHeight(contentBucketsProp.GetArrayElementAtIndex(index));
                                    },

                                    onAddCallback = (ReorderableList list) =>
                                    {
                                        list.serializedProperty.arraySize++;
                                        var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);

                                        var bucketName = newElement.FindPropertyRelative(nameof(Bucket.bucketName));
                                        var bucketContents = newElement.FindPropertyRelative(nameof(Bucket.bucketContents));
                                        var bucketFoldout = newElement.FindPropertyRelative(nameof(Bucket.foldout));

                                        bucketName.stringValue = "New Bucket";
                                        bucketContents.arraySize = 0;
                                        bucketFoldout.boolValue = false;

                                    }

                                    
                                };

                                innerListLookup[listKey] = bucketList;
                            }

                            bucketList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, GetBucketsHeight(contentBucketsProp)));

                            break;
                    }
                }
            },

            // bad make better
            elementHeightCallback = (int index) =>
            {
                var element = chapterContentList.serializedProperty.GetArrayElementAtIndex(index);

                var contentTextProp = element.FindPropertyRelative(nameof(ContentData.contentText));
                var contenTypeProp = element.FindPropertyRelative(nameof(ContentData.contentType));
                var contentChoicesProp = element.FindPropertyRelative(nameof(ContentData.contentChoices));
                var contentBucketsProp = element.FindPropertyRelative(nameof(ContentData.contentBuckets));

                float otherContentHeigth = 0f;

                switch (data.chapterContent[index].contentType)
                {
                    case ContentType.NONE: break;
                    case ContentType.DESCRIPTION:
                        otherContentHeigth += EditorGUI.GetPropertyHeight(contentTextProp);
                        break;
                    case ContentType.MULTIPLE_CHOICE:
                        otherContentHeigth += EditorGUI.GetPropertyHeight(contentTextProp);     
                        otherContentHeigth += GetMultipleChoiceHeight(contentChoicesProp);
                        otherContentHeigth += EditorGUIUtility.singleLineHeight * 2;
                        break;
                    case ContentType.ALLOCATION:
                        otherContentHeigth += GetBucketsHeight(contentBucketsProp);
                        break;
                }

                return EditorGUI.GetPropertyHeight(contenTypeProp) + otherContentHeigth + EditorGUIUtility.singleLineHeight;


            },

            onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize++;

                var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                var contentType = newElement.FindPropertyRelative(nameof(ContentData.contentType));

                contentType.enumValueIndex = 0;

            }
        };
    }

    public override void OnInspectorGUI()
    {
        base.serializedObject.FindProperty("title").stringValue = EditorGUILayout.TextField("Chapter Title", data.title);

        serializedObject.Update();
        chapterContentList.DoLayoutList();


        base.serializedObject.ApplyModifiedProperties();
    }


    #region Helper

    private float GetMultipleChoiceHeight(SerializedProperty multichoice)
    {
        var height = EditorGUIUtility.singleLineHeight;

        if (multichoice.isArray) 
        {
            height += EditorGUIUtility.singleLineHeight * 2 * Mathf.Max(1, multichoice.arraySize) + EditorGUI.GetPropertyHeight(multichoice);
        }
        else
        {
            height += EditorGUIUtility.singleLineHeight * 2 + EditorGUI.GetPropertyHeight(multichoice);
        }
        

        return height;
    }

    private float GetBucketsHeight(SerializedProperty buckets)
    {
        var height = EditorGUIUtility.singleLineHeight * 4;

        for(int i = 0; i < buckets.arraySize; i++)
        {
            var element = buckets.GetArrayElementAtIndex(i);

           

            if (element.FindPropertyRelative(nameof(Bucket.foldout)).boolValue != false)
            {
                //height += 2 * EditorGUIUtility.singleLineHeight; //padding

                height += GetBucketHeight(buckets.GetArrayElementAtIndex(i));
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
            }
        }

        return height;
    }

    private float GetBucketHeight(SerializedProperty bucket)
    {
        var height = EditorGUIUtility.singleLineHeight;

        var bucketFoldout = bucket.FindPropertyRelative(nameof(Bucket.foldout));
        var bucketContents = bucket.FindPropertyRelative(nameof(Bucket.bucketContents));

        if (bucketFoldout.boolValue == true)
        {
            height += EditorGUIUtility.singleLineHeight * 3;

            if (bucketContents.isArray)
            {
                height += EditorGUIUtility.singleLineHeight * 2 * Mathf.Max(1, bucketContents.arraySize);
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight * 2;
            }
        }

            

        return height;
    }

    #endregion

}

#endif