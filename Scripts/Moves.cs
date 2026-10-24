
using UnityEngine;
using TMPro;

public class Moves : MonoBehaviour
{
    public int limit = 5;
    public TMP_Text text;

    private int left;

    void Start()
    {
        left = limit;
        Show();
    }

    public bool Available()
    {
        return left > 0;
    }

    public void Use()
    {
        if (left <= 0)
            return;

        left--;
        Show();
    }

    void Show()
    {
        if (text != null)
            text.text = "MOVES LEFT: " + left;
    }
}
