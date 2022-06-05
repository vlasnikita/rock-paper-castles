// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Proxy cards are used in the demo game to hide the visual latency when a player plays a card
/// from his hand. Rather than waiting for the server to spawn a new networked card (something that
/// takes some time and causes visible lag), we immediately show a proxy card that is visually
/// equivalent to the card that is going to be spawned but only exists locally. The proxy card is
/// destroyed once the final networked card is spawned and received by the client. This is a nice
/// trick to hide a little the fact we are playing a multiplayer game (which will always have some
/// degree of latency); we ultimately want the experience to feel smooth to the player.
///
/// This is a generally useful technique for multiplayer games: you request something from the server
/// but at the same time execute the game logic locally so that the player notices no lag and, when
/// the client receives the server response, it compares it against its locally-run state and only update
/// it if they differ (meaning something went wrong on the client).
/// </summary>
public class DemoProxyCard : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image GlowImage;
    public Image Image;
    public Text CostText;
    public Text NameText;
    public Text BodyText;
    public Image AttackImage;
    public Text AttackText;
    public Image DefenseImage;
    public Text DefenseText;
    public Text SubtypeText;

    [HideInInspector]
    public Card Card;

    [HideInInspector]
    public NetworkInstanceId OwnerNetId;

    protected DemoHumanPlayer ownerPlayer;

    protected bool startedDrag;
    protected Vector3 initialPos;

    protected virtual void Awake()
    {
        Assert.IsTrue(GlowImage != null);
        Assert.IsTrue(Image != null);
        Assert.IsTrue(CostText != null);
        Assert.IsTrue(NameText != null);
        Assert.IsTrue(BodyText != null);
        Assert.IsTrue(AttackImage != null);
        Assert.IsTrue(AttackText != null);
        Assert.IsTrue(DefenseImage != null);
        Assert.IsTrue(DefenseText != null);
        Assert.IsTrue(SubtypeText != null);
        GlowImage.gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        ownerPlayer = ClientScene.FindLocalObject(OwnerNetId).GetComponent<DemoHumanPlayer>();
        Assert.IsTrue(ownerPlayer != null);
    }

    /// <summary>
    /// Updates the card with the specified card information.
    /// </summary>
    /// <param name="card">Card to display a preview of.</param>
    public void SetCardData(int cardId)
    {
        Card = GameManager.Instance.GetCard(cardId);
        Image.sprite = Resources.Load<Sprite>(Card.GetStringAttribute("Image"));
        CostText.text = Card.GetIntegerAttribute("Cost").ToString();
        NameText.text = Card.Name;
        BodyText.text = Card.GetStringAttribute("Text");
        if (Card.Definition == "Creature")
        {
            AttackImage.enabled = true;
            AttackText.enabled = true;
            DefenseImage.enabled = true;
            DefenseText.enabled = true;

            AttackText.text = Card.GetIntegerAttribute("Attack").ToString();
            DefenseText.text = Card.GetIntegerAttribute("Defense").ToString();
        }
        else
        {
            AttackImage.enabled = false;
            AttackText.enabled = false;
            DefenseImage.enabled = false;
            DefenseText.enabled = false;
        }
        var subtypeText = Card.Definition;
        if (Card.Subtypes.Count > 0)
        {
            subtypeText += " -";
            foreach (var subtype in Card.Subtypes)
            {
                subtypeText += " ";
                subtypeText += subtype;
            }
        }
        SubtypeText.text = subtypeText;
    }

    /// <summary>
    /// Callback called when the card is clicked.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (ownerPlayer.isLocalPlayer && ownerPlayer.IsWaitingForDiscardedCardsSelection())
            {
                ownerPlayer.AddCardToDiscard(Card.Id);
                (ownerPlayer as DemoHumanPlayer).RemoveCardFromHand(this);
                Destroy(gameObject);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameWindowUtils.OpenCardDetailWindow(Card);
        }
    }
    /// <summary>
    /// Callback called when a drag is initiated on this card.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!ownerPlayer.IsActivePlayer)
            return;

        if (Card.GetIntegerAttribute("Cost") > ownerPlayer.GetAttribute("Mana").Value)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            startedDrag = true;
            initialPos = transform.position;
        }
    }

    /// <summary>
    /// Callback called when a drag is performed on this card.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!startedDrag)
            return;

        if (!ownerPlayer.IsActivePlayer)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector2 mousePos;
            var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out mousePos);
            transform.position = canvas.transform.TransformPoint(mousePos);
        }
    }

    /// <summary>
    /// Callback called when a drag on this card is finished.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!startedDrag)
            return;

        startedDrag = false;

        if (!ownerPlayer.IsActivePlayer)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            var boardBottom = GameObject.Find("Canvas/BoardBottom");
            if (RectTransformUtility.RectangleContainsScreenPoint(boardBottom.transform as RectTransform, transform.position))
            {
                if (ownerPlayer.isLocalPlayer)
                    ownerPlayer.PlayCard(this);
            }
            else
            {
                transform.position = initialPos;
            }
        }
    }
}
