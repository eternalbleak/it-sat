using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    ReorderableList chapterContentList, multipleChoiceList;

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
                                    if(list.count < MAX_MULTICHOICE)
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

                            //displaying new sublist
                            multipleChoiceList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + GetMultipleChoiceHeight(contentChoicesProp.GetArrayElementAtIndex(contentIndex)), rect.width, rect.height));

                            break;

                        case ContentType.ALLOCATION: break;
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
                    case ContentType.ALLOCATION: break;
                }

                return EditorGUI.GetPropertyHeight(contenTypeProp) + otherContentHeigth + EditorGUIUtility.singleLineHeight;


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

        height +=  EditorGUIUtility.singleLineHeight * 2 * Mathf.Max(1, multichoice.arraySize) + EditorGUI.GetPropertyHeight(multichoice);

        return height;
    }

    #endregion

}

#endif