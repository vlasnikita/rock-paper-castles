// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// The demo network card is a subclass of the core NetworkCard type which extends it with demo-specific
/// functionality. Most of which is straightforward updating of the user interface when receiving
/// new state from the server.
/// </summary>
public class DemoNetworkCard : NetworkCard, IPointerDownHandler
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
    public Image AttackingImage;
    public Text SubtypeText;
    public ParticleSystem SleepingParticleSystem;

    public GameObject DeathSkullPrefab;
    public GameObject HealthIncreaseTextPrefab;
    public GameObject HealthDecreaseTextPrefab;

    protected CanvasGroup canvasGroup;

    protected GameObject deathSkull;
    protected GameObject healthIncreaseText;
    protected GameObject healthDecreaseText;

    [SyncVar]
    protected bool attacking;

    [SyncVar]
    protected bool beingKilled;

    [SyncVar]
    protected bool firstTurn = true;

    [SyncVar]
    protected bool canAttack = true;

    public bool CanAttack
    {
        get { return canAttack; }
    }

    protected override void Awake()
    {
        base.Awake();

        canvasGroup = GetComponent<CanvasGroup>();

        Assert.IsTrue(canvasGroup != null);
        Assert.IsTrue(GlowImage != null);
        Assert.IsTrue(Image != null);
        Assert.IsTrue(CostText != null);
        Assert.IsTrue(NameText != null);
        Assert.IsTrue(BodyText != null);
        Assert.IsTrue(AttackImage != null);
        Assert.IsTrue(AttackText != null);
        Assert.IsTrue(DefenseImage != null);
        Assert.IsTrue(DefenseText != null);
        Assert.IsTrue(AttackingImage != null);
        Assert.IsTrue(SubtypeText != null);
        Assert.IsTrue(SleepingParticleSystem != null);

        GlowImage.gameObject.SetActive(false);
        AttackingImage.gameObject.SetActive(false);

        SetSleepingParticleSystemEnabled(false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameObject boardZone;
        var numBoardCards = 0;
        if (ownerPlayer.isLocalPlayer && ownerPlayer.IsHuman)
        {
            boardZone = GameObject.Find("Canvas/BoardBottom/CardZone");
            (ownerPlayer as DemoHumanPlayer).AddCardToBottomBoard(this);
            numBoardCards = (ownerPlayer as DemoHumanPlayer).GetNumberOfCardsInBottomBoard();
        }
        else
        {
            boardZone = GameObject.Find("Canvas/BoardTop/CardZone");
            var localPlayer = NetworkingUtils.GetLocalPlayer() as DemoHumanPlayer;
            if (localPlayer != null)
            {
                localPlayer.AddCardToTopBoard(this);
                numBoardCards = localPlayer.GetNumberOfCardsInTopBoard();
            }
        }
        transform.SetParent(boardZone.transform, false);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(210.0f * (numBoardCards - 1), 0);

        Attributes.Callback += OnAttributesChanged;
    }

    protected override void SetCardData()
    {
        base.SetCardData();

        var card = GameManager.Instance.GetCard(CardId);
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

            var fury = card.Effects.Find(x => x.Definition == "Fury");
            if (fury == null)
                SetSleepingParticleSystemEnabled(true);
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

    protected override void OnAttributesChanged(SyncListCardAttributes.Operation op, int index)
    {
        base.OnAttributesChanged(op, index);

        var card = GameManager.Instance.GetCard(CardId);
        if (card.Definition == "Creature")
        {
            if (GetAttribute("Attack").HasValue)
                AttackText.text = GetAttribute("Attack").Value.Value.ToString();
            if (GetAttribute("Defense").HasValue)
            {
                var defenseValue = GetAttribute("Defense").Value.Value;
                int oldDefense = int.Parse(DefenseText.text);
                if (defenseValue > oldDefense)
                {
                    if (isClient)
                        ShowHealthIncreaseText(defenseValue - oldDefense);
                }
                else if (defenseValue < oldDefense)
                {
                    if (isClient)
                        ShowHealthDecreaseText(oldDefense - defenseValue);
                }
                DefenseText.text = defenseValue.ToString();
            }
        }
    }

    public override void Kill()
    {
        base.Kill();

        if (beingKilled)
            return;

        beingKilled = true;

        WrapperShowKillIcon();
        WrapperSetSleepingParticleSystemEnabled(false);
    }

    protected void WrapperShowKillIcon()
    {
        ShowKillIcon();
        RpcShowKillIcon();
    }

    [ClientRpc]
    protected void RpcShowKillIcon()
    {
        if (!isServer)
            ShowKillIcon();
    }

    protected void ShowKillIcon()
    {
        deathSkull = Instantiate(DeathSkullPrefab);
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        deathSkull.transform.SetParent(canvas.transform, false);
        deathSkull.transform.position = gameObject.transform.position;
        StartCoroutine(FadeOutCard(1.0f));
    }

    private IEnumerator FadeOutCard(float speed)
    {
        while (canvasGroup.alpha > 0.0f)
        {
            canvasGroup.alpha -= speed * Time.deltaTime;
            yield return null;
        }
        Destroy(deathSkull);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnCardSelected();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            var card = GameManager.Instance.GetCard(CardId);
            GameWindowUtils.OpenCardDetailWindow(card);
        }
    }

    public override void OnCardSelected()
    {
        var card = GameManager.Instance.GetCard(CardId);
        if (card.Definition != "Creature")
            return;

        var client = NetworkManager.singleton.client;
        var localPlayer = NetworkingUtils.GetActiveLocalPlayer();
        if (localPlayer.IsWaitingForEffectCardTargetSelection())
        {
            var msg = new TargetCardSelectedMessage();
            msg.NetId = netId;
            client.Send(NetworkProtocol.TargetCardSelected, msg);
            localPlayer.SetWaitingForEffectCardTargetSelection(false);
            return;
        }

        if (ownerPlayer == localPlayer)
        {
            if (!ownerPlayer.IsActivePlayer)
                return;

            var fury = card.Effects.Find(x => x.Definition == "Fury");
            if (firstTurn && fury == null)
                return;

            if (!canAttack)
                return;

            if (!attacking)
            {
                Debug.Log("DeNeCa: Attacking! My NET ID: " + netId);
                attacking = true;
                AttackingImage.gameObject.SetActive(true);
                var msg = new AttackingCardSelectedMessage();
                msg.NetId = netId;
                client.Send(NetworkProtocol.AttackingCardSelected, msg);
                localPlayer.SetWaitingForAttackTargetSelection(true);
            }
            else
            {
                attacking = false;
                AttackingImage.gameObject.SetActive(false);
                var msg = new AttackingCardUnselectedMessage();
                msg.NetId = netId;
                client.Send(NetworkProtocol.AttackingCardUnselected, msg);
                localPlayer.SetWaitingForAttackTargetSelection(false);
            }
        }
        else
        {
            Debug.Log("DeNeCa: I'm under attack. My NET ID: " + netId);
            if (localPlayer.IsWaitingForAttackTargetSelection())
            {
                var msg = new AttackedCardSelectedMessage();
                msg.NetId = netId;
                client.Send(NetworkProtocol.AttackedCardSelected, msg);
                localPlayer.SetWaitingForAttackTargetSelection(false);
            }
        }
    }

    public void WrapperSetAttackingIconEnabled(bool enabled)
    {
        if (hasAuthority)
        {
            if (isServer)
            {
                SetAttackingIconEnabled(enabled);
                RpcSetAttackingIconEnabled(enabled);
            }
            else
            {
                SetAttackingIconEnabled(enabled);
            }
        }
        else
        {
            SetAttackingIconEnabled(enabled);
        }
    }

    [Command]
    public void CmdSetAttackingIconEnabled(bool enabled)
    {
        SetAttackingIconEnabled(enabled);
        RpcSetAttackingIconEnabled(enabled);
    }

    [ClientRpc]
    private void RpcSetAttackingIconEnabled(bool enabled)
    {
        SetAttackingIconEnabled(enabled);
    }

    public void SetAttackingIconEnabled(bool enabled)
    {
        attacking = enabled;
        AttackingImage.gameObject.SetActive(enabled);
    }

    [Command]
    private void CmdShowHealthIncreaseText(int damage)
    {
        ShowHealthIncreaseText(damage);
        RpcShowHealthIncreaseText(damage);
    }

    [ClientRpc]
    private void RpcShowHealthIncreaseText(int damage)
    {
        if (!isServer)
            ShowHealthIncreaseText(damage);
    }

    private void ShowHealthIncreaseText(int damage)
    {
        healthIncreaseText = Instantiate(HealthIncreaseTextPrefab);
        healthIncreaseText.GetComponent<Text>().text = "+" + damage.ToString();
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        healthIncreaseText.transform.SetParent(canvas.transform, false);
        healthIncreaseText.transform.position = gameObject.transform.position;
        Invoke("DestroyHealthIncreaseText", 1.0f);
    }

    private void DestroyHealthIncreaseText()
    {
        Destroy(healthIncreaseText);
    }

    [Command]
    private void CmdShowHealthDecreaseText(int damage)
    {
        ShowHealthDecreaseText(damage);
        RpcShowHealthDecreaseText(damage);
    }

    [ClientRpc]
    private void RpcShowHealthDecreaseText(int damage)
    {
        if (!isServer)
            ShowHealthDecreaseText(damage);
    }

    private void ShowHealthDecreaseText(int damage)
    {
        healthDecreaseText = Instantiate(HealthDecreaseTextPrefab);
        healthDecreaseText.GetComponent<Text>().text = "-" + damage.ToString();
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        healthDecreaseText.transform.SetParent(canvas.transform, false);
        healthDecreaseText.transform.position = gameObject.transform.position;
        Invoke("DestroyHealthDecreaseText", 1.0f);
    }

    private void DestroyHealthDecreaseText()
    {
        Destroy(healthDecreaseText);
    }

    public override void OnStartTurn(NetworkInstanceId activePlayerNetId)
    {
        base.OnStartTurn(activePlayerNetId);

        if (OwnerNetId == activePlayerNetId)
            WrapperSetSleepingParticleSystemEnabled(false);
    }

    public override void OnEndTurn(NetworkInstanceId activePlayerNetid)
    {
        base.OnEndTurn(activePlayerNetid);

        attacking = false;
        firstTurn = false;
        canAttack = true;
        WrapperSetAttackingIconEnabled(false);
    }

    private void WrapperSetSleepingParticleSystemEnabled(bool enabled)
    {
        SetSleepingParticleSystemEnabled(enabled);
        RpcSetSleepingParticleSystemEnabled(enabled);
    }

    [ClientRpc]
    private void RpcSetSleepingParticleSystemEnabled(bool enabled)
    {
        if (!isServer)
            SetSleepingParticleSystemEnabled(enabled);
    }

    private void SetSleepingParticleSystemEnabled(bool enabled)
    {
        var newEmission = SleepingParticleSystem.emission;
        newEmission.enabled = enabled;
    }

    public void EnableAttack()
    {
        canAttack = true;
    }

    public void DisableAttack()
    {
        canAttack = false;
    }

    public override bool CanBeTargetOfEffect(Effect effect)
    {
        if (effect.Unavoidable)
            return true;

        var card = GameManager.Instance.GetCard(CardId);
        var spellProtection = card.Effects.Find(x => x.Definition == "Spell protection");
        if (spellProtection == null)
            return true;

        var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition);
        if (effectDefinition.Type == EffectType.TargetCard)
        {
            var cardEffectDefinition = effectDefinition as CardEffectDefinition;
            if (cardEffectDefinition.Action == CardEffectActionType.Kill ||
                cardEffectDefinition.Action == CardEffectActionType.RemoveCounter ||
                cardEffectDefinition.Action == CardEffectActionType.SetAttribute ||
                cardEffectDefinition.Action == CardEffectActionType.Transform)
                return false;
        }
        return true;
    }
}
