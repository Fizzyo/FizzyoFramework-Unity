using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for catagory button data
/// </summary>
public class AchievementButton : MonoBehaviour
{
    // Game Object that links to the area where the achievements are shown
    public GameObject achList;

    // Button states
    public Sprite neutral, selected;

    // Button sprite
    private Image sprite;

    void Awake()
    {
        sprite = GetComponent<Image>();
    }

    /// <summary>
    /// Changes the sprite of a button and sets it active
    /// </summary>
    public void Click()
    {
        if (sprite.sprite == neutral)
        {
            sprite.sprite = selected;
            achList.SetActive(true);
        }
        else
        {
            sprite.sprite = neutral;
            achList.SetActive(false);
        }
    }
}
