using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SchoolingSelector : MonoBehaviour
{
    public Scene schooling1, schooling2;

    private Button _schoolingBtn1, _schoolingBtn2;

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = this.gameObject.GetComponent<UIDocument>().rootVisualElement;

        _schoolingBtn1 = (Button)root.Query<Button>("schooling1");
        _schoolingBtn1.clicked += OnClickedFirst;

        _schoolingBtn2 = (Button)root.Query<Button>("schooling2");
        _schoolingBtn2.clicked += OnClickedSecond;
    }

    void OnClickedFirst()
    {
        SceneManager.LoadScene(1);
    }

    void OnClickedSecond()
    {
        SceneManager.LoadScene(2);
    }
}
