using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Sprite hand, drop;
    public Image interaction;
    
    private void EnableInteraction(Sprite sprite)
    {
        interaction.sprite = sprite;
        interaction.enabled = true;
    }
    
    public void HideInteract()
    {
        interaction.enabled = false;
    }
    
    public void ShowHand()
    {
        EnableInteraction(hand);
    }
    
    public void ShowDrop()
    {
        EnableInteraction(drop);
    }
}
