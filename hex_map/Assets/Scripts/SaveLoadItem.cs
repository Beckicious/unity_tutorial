using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem : MonoBehaviour
{
    public SaveLoadMenu menu;

    string mapName;
    public string MapName
    {
        get => MapName;
        set
        {
            mapName = value;
            transform.GetChild(0).GetComponent<Text>().text = value;
        }
    }

    public void Select()
    {
        menu.SelectItem(mapName);
    }
}
