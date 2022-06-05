// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Card preview showing a card's in-game appearance when selecting a card in the deck editor scene.
/// </summary>
public class CardPreview : MonoBehaviour
{
    public Image Image;
    public Text CostText;
    public Text NameText;
    public Text BodyText;
    public Image AttackImage;
    public Text AttackText;
    public Image DefenseImage;
    public Text DefenseText;
    public Text SubtypeText;

    private void Awake()
    {
        Assert.IsTrue(Image != null);
        Assert.IsTrue(CostText != null);
        Assert.IsTrue(NameText != null);
        Assert.IsTrue(BodyText != null);
        Assert.IsTrue(AttackImage != null);
        Assert.IsTrue(AttackText != null);
        Assert.IsTrue(DefenseImage != null);
        Assert.IsTrue(DefenseText != null);
        Assert.IsTrue(SubtypeText != null);
    }

    /// <summary>
    /// Update the card preview with the specified card information.
    /// </summary>
    /// <param name="card">Card to display a preview of.</param>
    public void SetCardData(Card card)
    {
        Image.sprite = Resources.Load<Sprite>(card.GetStringAttribute("Image"));
        CostText.text = card.GetIntegerAttribute("Cost").ToString();
        NameText.text = card.Name;
        BodyText.text = card.GetStringAttribute("Text");
        if (card.Definition == "Creature")
        {
            AttackImage.enabled = true;
            AttackText.enabled = true;
            DefenseImage.enabled = true;
            DefenseText.enabled = true;

            AttackText.text = card.GetIntegerAttribute("Attack").ToString();
            DefenseText.text = card.GetIntegerAttribute("Defense").ToString();
        }
        else
        {
            AttackImage.enabled = false;
            AttackText.enabled = false;
            DefenseImage.enabled = false;
            DefenseText.enabled = false;
        }
        var subtypeText = card.Definition;
        if (card.Subtypes.Count > 0)
        {
            subtypeText += " -";
            foreach (var subtype in card.Subtypes)
            {
                subtypeText += " ";
                subtypeText += subtype;
            }
        }
        SubtypeText.text = subtypeText;
    }
}
