using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextDescription : MonoBehaviour {

    const float C_LeftBoundary = 0.325f;

    const float C_TextBoxTopOffset = 0.0225f;
    const float C_LineDistanceFromText = 0.096f;

    const float C_LineThickness = 0.0025f;
    const float C_LineTextDistance = 0.021f;

    [SerializeField] GameObject m_DescriptionTextPrefab;
    List<GameObject> descriptionTexts;

    [SerializeField] ObjectHash<RectTransform> m_ControllerButtonLocations;
	// Use this for initialization
	void Awake () {
        m_ControllerButtonLocations = ObjectHash<RectTransform>.GetStringDictionaryFromArray
                                                    (transform.Find("ButtonLocations").GetComponentsInChildren<RectTransform>());
            //transform.Find("ButtonLocations").GetComponentsInChildren<RectTransform>();


	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public void MakeDescriptionText (string buttonName, bool alignMent, string content) {

        RectTransform spawnPoint = m_ControllerButtonLocations[buttonName];
        GameObject newDescriptionText = Instantiate(m_DescriptionTextPrefab, transform);

        Vector3 oldAnchorPosition = newDescriptionText.GetComponent<RectTransform>().anchoredPosition;
        newDescriptionText.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            oldAnchorPosition.x,
            spawnPoint.anchoredPosition.y
        );

        RectTransform line = newDescriptionText.transform.Find("Line").GetComponent<RectTransform>();
        RectTransform text = newDescriptionText.transform.Find("DescriptionText").GetComponent<RectTransform>();
        if (!alignMent)
        {

            text.anchorMax = new Vector2(C_LeftBoundary, text.anchorMax.y);
            text.anchorMin = new Vector2(0.15f, text.anchorMin.y);

            line.anchorMax = new Vector2(0.5f,line.anchorMax.y);
            line.anchorMin = new Vector2(C_LeftBoundary + C_LineTextDistance, line.anchorMin.y);

            line.offsetMin = new Vector2(
                0,
                line.offsetMin.y
            );
            line.offsetMax = new Vector2(
                spawnPoint.offsetMax.x,
                line.offsetMax.y
            );
            line.offsetMin = new Vector2(0, line.offsetMin.y);

            text.GetComponent<Text>().alignment = TextAnchor.UpperRight;

        }
        else {
         //   text.anchorMin = new Vector2(1 - C_LeftBoundary, text.anchorMin.y);
            text.anchorMax = new Vector2(1-0.208f, text.anchorMax.y);

            line.anchorMin = new Vector2(0.5f, line.anchorMin.y);
            line.anchorMax = new Vector2(1 - C_LeftBoundary - C_LineTextDistance, line.anchorMax.y);

         
            line.offsetMin = new Vector2(
                spawnPoint.offsetMin.x,
                line.offsetMin.y
            );
            line.offsetMax = new Vector2(
                0,
                line.offsetMax.y
            );

            text.GetComponent<Text>().alignment = TextAnchor.UpperRight;
            //text.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
        }
        text.GetComponent<Text>().text = content;
    


    }
}
