using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CreateAssetMenu(menuName ="Custom/ChapterData", fileName ="new ChapterData")]
public class ChapterData : ScriptableObject
{
    public string title;
    public List<ContentData> contentData;
}


#if UNITY_EDITOR

/*TODO
 - Add Documentation, else nobody knows what stuff does 
 - put all the reorderable list functionality into the creation line same as the sof contributer did -> can simplify the code
 - Add different Content Types, Multiple Choice and Single Choice, Moveable Stuff thing
 */

[CustomEditor(typeof(ChapterData))]
class ChapterDataEditor : Editor
{
    ChapterData data;

    SerializedProperty contentDataProperty; // prop of List<ContenData> contentData

    ReorderableList reorderableList;

    private void OnEnable()
    {
        data = (ChapterData)target;

        contentDataProperty = serializedObject.FindProperty(nameof(ChapterData.contentData));

        reorderableList = new ReorderableList(serializedObject, contentDataProperty, true, true, true, true);

        reorderableList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Chapter Content"); };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

            

            if(element != null ) 
            {
                var contentTextProp = element.FindPropertyRelative(nameof(ContentData.content_text));

                data.contentData[index].contentType = (ContentType) EditorGUI.EnumPopup(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Content Type: ", data.contentData[index].contentType);

                rect.y += EditorGUI.GetPropertyHeight(element.FindPropertyRelative(nameof(ContentData.contentType)));

                switch (data.contentData[index].contentType){
                    case ContentType.NONE: break;
                    case ContentType.DESCRIPTION:
                        var text_heigth = EditorGUI.GetPropertyHeight(contentTextProp);
                        EditorGUI.PropertyField(
                            new Rect(rect.x, rect.y, rect.width, text_heigth), contentTextProp, new GUIContent("Content Text"));
                        break;
                }
            }
        };

        reorderableList.elementHeightCallback = index =>
        {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

            var contenType = element.FindPropertyRelative(nameof(ContentData.contentType));

            float otherContentHeigth = 0f;

            switch (data.contentData[index].contentType)
            {
                case ContentType.NONE: break;
                case ContentType.DESCRIPTION:
                    otherContentHeigth += EditorGUI.GetPropertyHeight(
                        element.FindPropertyRelative(nameof(ContentData.content_text))); 
                    break;
            }

            return EditorGUI.GetPropertyHeight(contenType) + otherContentHeigth + EditorGUIUtility.singleLineHeight;

        };
    }

    public override void OnInspectorGUI()
    {
        base.serializedObject.FindProperty("title").stringValue = EditorGUILayout.TextField("Chapter Title", data.title);

        serializedObject.Update();
        reorderableList.DoLayoutList();

        base.serializedObject.ApplyModifiedProperties();
    }
}

#endif