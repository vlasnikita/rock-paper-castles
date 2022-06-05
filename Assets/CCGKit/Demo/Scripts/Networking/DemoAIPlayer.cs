// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Networking;

using CCGKit;

/// <summary>
/// Computer-controlled player that is used in the single-player mode from the demo game.
/// </summary>
public class DemoAIPlayer : AIPlayer
{
    /// <summary>
    /// Stores the player target of the last spell that was played.
    /// </summary>
    protected Player nextSpellPlayerTarget;

    /// <summary>
    /// Stores the creature target of the last spell that was played.
    /// </summary>
    protected NetworkCard nextSpellCreatureTarget;

    /// <summary>
    /// Called when the player needs to select a target player.
    /// </summary>
    /// <param name="msg">Select target player message.</param>
    public override void OnSelectTargetPlayer(SelectTargetPlayerMessage msg)
    {
        if (nextSpellPlayerTarget != null)
        {
            base.OnSelectTargetPlayer(msg);
            var targetSelectedMsg = new TargetPlayerSelectedMessage();
            targetSelectedMsg.NetId = nextSpellPlayerTarget.netId;
            client.Send(NetworkProtocol.TargetPlayerSelected, targetSelectedMsg);
            SetWaitingForEffectPlayerTargetSelection(false);
        }
    }

    /// <summary>
    /// Called when the player needs to select a target card.
    /// </summary>
    /// <param name="msg">Select target card message.</param>
    public override void OnSelectTargetCard(SelectTargetCardMessage msg)
    {
        if (nextSpellCreatureTarget != null)
        {
            base.OnSelectTargetCard(msg);
            var targetSelectedMsg = new TargetCardSelectedMessage();
            targetSelectedMsg.NetId = nextSpellCreatureTarget.netId;
            client.Send(NetworkProtocol.TargetCardSelected, targetSelectedMsg);
            SetWaitingForEffectCardTargetSelection(false);
        }
    }

    /// <summary>
    /// This methods performs the actual AI logic.
    /// </summary>
    /// <returns>The AI logic coroutine.</returns>
    protected override IEnumerator PerformMove()
    {
        // Try to play any life gain spells.
        var lifeGainSpells = GetSpellCardsInHand(PlayerEffectActionType.IncreaseAttribute, "Life");
        foreach (var spell in lifeGainSpells)
        {
            if (TryToPlayCard(spell))
            {
                nextSpellPlayerTarget = this;
                yield return new WaitForSeconds(1.0f);
            }
        }

        // Try to play any player damage spells.
        var playerDamageSpells = GetSpellCardsInHand(PlayerEffectActionType.DecreaseAttribute, "Life");
        foreach (var spell in playerDamageSpells)
        {
            if (TryToPlayCard(spell))
            {
                nextSpellPlayerTarget = humanPlayer;
                yield return new WaitForSeconds(1.0f);
            }
        }

        // Try to play any creature bonus spells.
        var creatureBonusSpells = GetSpellCardsInHand(CardEffectActionType.AddCounter, "Attack");
        creatureBonusSpells.AddRange(GetSpellCardsInHand(CardEffectActionType.AddCounter, "Defense"));
        foreach (var spell in creatureBonusSpells)
        {
            var effect = spell.Effects[0] as CardEffect;
            if (effect != null)
            {
                if (effect.Target == CardEffectTargetType.TargetCard || effect.Target == CardEffectTargetType.CurrentPlayerCard)
                {
                    nextSpellCreatureTarget = GetPotentialTargetCreatureFromMe(effect);
                    if (nextSpellCreatureTarget != null && TryToPlayCard(spell))
                        yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    if (TryToPlayCard(spell))
                        yield return new WaitForSeconds(1.0f);
                }
            }
        }

        // Try to play any creature handicap spells.
        var creatureHandicapSpells = GetSpellCardsInHand(CardEffectActionType.RemoveCounter, "Attack");
        creatureHandicapSpells.AddRange(GetSpellCardsInHand(CardEffectActionType.RemoveCounter, "Defense"));
        creatureHandicapSpells.AddRange(GetSpellCardsInHand("Destroy target creature"));
        foreach (var spell in creatureHandicapSpells)
        {
            var effect = spell.Effects[0] as CardEffect;
            if (effect != null)
            {
                if (effect.Target == CardEffectTargetType.TargetCard || effect.Target == CardEffectTargetType.CurrentOpponentCard)
                {
                    nextSpellCreatureTarget = GetPotentialTargetCreatureFromOpponent(effect);
                    if (nextSpellCreatureTarget != null && TryToPlayCard(spell))
                        yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    if (TryToPlayCard(spell))
                        yield return new WaitForSeconds(1.0f);
                }
            }
        }

        // Try to play any creature.
        var creaturesInHand = GetCreatureCardsInHand();
        foreach (var creature in creaturesInHand)
        {
            TryToPlayCard(creature);
            yield return new WaitForSeconds(1.0f);
        }

        // Start combat.
        var creatures = GetMyCreatureCardsOnBoard();
        // If the AI can win this turn, go for it and attack the opponent player with everything.
        if (CanIWinThisTurn())
        {
            foreach (var creature in creatures)
            {
                if (creature.CanAttack)
                {
                    creature.OnCardSelected();
                    yield return new WaitForSeconds(1.0f);

                    ChoosePlayerTarget();
                    yield return new WaitForSeconds(1.0f);
                }
            }
        }
        // If the AI can lose next turn, try to prevent it by attacking the opponent creatures.
        else if (CanILoseNextTurn())
        {
            foreach (var creature in creatures)
            {
                if (creature.CanAttack)
                {
                    creature.OnCardSelected();
                    yield return new WaitForSeconds(1.0f);

                    ChooseOpponentCreatureTarget();
                    yield return new WaitForSeconds(1.0f);
                }
            }
        }
        // Attack the opponent player or the opponent creatures depending on the AI's level of aggression.
        else
        {
            foreach (var creature in creatures)
            {
                if (creature.CanAttack)
                {
                    creature.OnCardSelected();
                    yield return new WaitForSeconds(1.0f);

                    ChooseAttackTarget();
                    yield return new WaitForSeconds(1.0f);
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
        StopTurn();
    }

    /// <summary>
    /// Plays the specified card if able.
    /// </summary>
    /// <param name="card">The card to play.</param>
    /// <returns>True if the card could be played; false otherwise.</returns>
    protected bool TryToPlayCard(Card card)
    {
        var availableMana = GetAttribute("Mana").Value;
        if (card.GetIntegerAttribute("Cost") <= availableMana)
        {
            PlayCard(card.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the best target creature for the specified card effect from the set of creatures
    /// controlled by the AI (if any).
    /// </summary>
    /// <param name="effect">Card effect to check for.</param>
    /// <returns>The best target creature for the specified card effect; null otherwise.</returns>
    protected NetworkCard GetPotentialTargetCreatureFromMe(CardEffect effect)
    {
        var cardEffectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as CardEffectDefinition;
        if (cardEffectDefinition != null)
        {
            var cardEffectAttribute = cardEffectDefinition.Attribute;
            var creatures = GetMyCreatureCardsOnBoard();
            creatures.RemoveAll(x => !x.CanBeTargetOfEffect(effect));
            creatures.RemoveAll(x => !EffectResolver.IsValidTarget(x, effect));
            creatures.RemoveAll(x => x.GetAttribute(cardEffectAttribute) != null && x.GetAttribute(cardEffectAttribute).Value.Value > effect.Value);
            creatures.OrderBy(x => x.GetAttribute(cardEffectAttribute).Value.Value);
            if (creatures.Count > 0)
                return creatures[creatures.Count - 1];
        }
        return null;
    }

    /// <summary>
    /// Returns the best target creature for the specified card effect from the set of creatures
    /// controlled by the opponent player (if any).
    /// </summary>
    /// <param name="effect">Card effect to check for.</param>
    /// <returns>The best target creature for the specified card effect; null otherwise.</returns>
    protected NetworkCard GetPotentialTargetCreatureFromOpponent(CardEffect effect)
    {
        var cardEffectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as CardEffectDefinition;
        if (cardEffectDefinition != null)
        {
            var cardEffectAttribute = cardEffectDefinition.Attribute;
            var creatures = GetOpponentCreatureCardsOnBoard();
            creatures.RemoveAll(x => !x.CanBeTargetOfEffect(effect));
            creatures.RemoveAll(x => !EffectResolver.IsValidTarget(x, effect));
            if (cardEffectDefinition.Action == CardEffectActionType.RemoveCounter)
                creatures.RemoveAll(x => x.GetAttribute(cardEffectAttribute).Value.Value > effect.Value);
            creatures.OrderBy(x => x.GetAttribute(cardEffectAttribute).Value.Value);
            if (creatures.Count > 0)
                return creatures[creatures.Count - 1];
        }
        return null;
    }

    /// <summary>
    /// Randomly chooses between attacking the opponent player or one of the creatures he controls, taking
    /// the AI's level of aggression into account.
    /// </summary>
    protected void ChooseAttackTarget()
    {
        var attackChance = rng.NextDouble();
        if (attackChance <= aggression)
            ChoosePlayerTarget();
        else
            ChooseOpponentCreatureTarget();
    }

    /// <summary>
    /// Selects the human opponent as the target of the current AI attack.
    /// </summary>
    protected void ChoosePlayerTarget()
    {
        var msg = new AttackedPlayerSelectedMessage();
        msg.NetId = humanPlayer.netId;
        client.Send(NetworkProtocol.AttackedPlayerSelected, msg);
        SetWaitingForAttackTargetSelection(false);
    }

    /// <summary>
    /// Selects an opponent creature as the target of the current AI attack.
    /// </summary>
    protected void ChooseOpponentCreatureTarget()
    {
        var creatures = GetOpponentCreatureCardsOnBoard();
        if (creatures.Count > 0)
        {
            var toughestCreature = creatures.Aggregate((x, y) => x.GetAttribute("Attack").Value.Value > y.GetAttribute("Attack").Value.Value ? x : y);
            if (toughestCreature != null)
            {
                var msg = new AttackedCardSelectedMessage();
                msg.NetId = toughestCreature.netId;
                client.Send(NetworkProtocol.AttackedCardSelected, msg);
                SetWaitingForAttackTargetSelection(false);
            }
            else
            {
                // If there are no creatures left to attack, just go for the player.
                ChoosePlayerTarget();
            }
        }
    }



    protected override void RegisterWithServer()
    { 
        var defaultDeckJson = Resources.Load<TextAsset>("CastleDeck").text;
        var defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckJson);
        var msgDefaultDeck = new List<int>(defaultDeck.Size);
        for (var i = 0; i < defaultDeck.Cards.Count; i++)
        {
            for (var j = 0; j < defaultDeck.Cards[i].Count; j++)
                msgDefaultDeck.Add(defaultDeck.Cards[i].Id);
        }

        // Register the player to the game and send the server his information.
        var msg = new RegisterPlayerMessage();
        msg.NetId = netId;
        if (IsHuman)
        {
            var playerName = GameManager.Instance.PlayerName;
            msg.Name = string.IsNullOrEmpty(playerName) ? "Unnamed wizard" : playerName;
        }
        else
        {
            msg.Name = "Loh228";
        }
        msg.IsHuman = IsHuman;
        msg.Deck = msgDefaultDeck.ToArray();
        client.Send(NetworkProtocol.RegisterPlayer, msg);
    }

    /// <summary>
    /// Returns true if the AI can win the match with his creatures this turn and false otherwise.
    /// </summary>
    /// <returns>True if the AI can win the match with his creatures this turn; false otherwise.</returns>
    protected bool CanIWinThisTurn()
    {
        var life = humanPlayer.GetAttribute("Life").Value;
        var myCreatures = GetMyCreatureCardsOnBoard();
        var totalDamage = 0;
        foreach (var creature in myCreatures)
            if (creature.CanAttack)
                totalDamage += creature.GetAttribute("Attack").Value.Value;
        return totalDamage >= life;
    }

    /// <summary>
    /// Returns true if the human opponent can win the match with his creatures the next turn and false
    /// otherwise.
    /// </summary>
    /// <returns>True if the human opponent can win the match with his creatures the next turn; false
    /// otherwise.</returns>
    protected bool CanILoseNextTurn()
    {
        var life = GetAttribute("Life").Value;
        var opponentCreatures = GetOpponentCreatureCardsOnBoard();
        var totalDamage = 0;
        foreach (var creature in opponentCreatures)
            totalDamage += creature.GetAttribute("Attack").Value.Value;
        return totalDamage >= life;
    }

    /// <summary>
    /// Returns the spell cards the AI has in his hand with an effect of the specified type that targets the specified
    /// player attribute.
    /// </summary>
    /// <param name="type">The type of the spell's effect.</param>
    /// <param name="attribute">The attribute of the spell's effect.</param>
    /// <returns>A list of the spell cards from the AI's hand with the specified effect conditions.</returns>
    protected List<Card> GetSpellCardsInHand(PlayerEffectActionType type, string attribute)
    {
        var spells = new List<Card>();
        foreach (var id in hand)
        {
            var card = GameManager.Instance.GetCard(id);
            if (card.Definition == "Spell")
            {
                foreach (var effect in card.Effects)
                {
                    var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition);
                    if (effectDefinition.Type == EffectType.TargetPlayer)
                    {
                        var playerEffectDefinition = effectDefinition as PlayerEffectDefinition;
                        if (playerEffectDefinition.Action == type &&
                            playerEffectDefinition.Attribute == attribute)
                        {
                            spells.Add(card);
                        }
                    }
                }
            }
        }
        return spells;
    }

    /// <summary>
    /// Returns the spell cards the AI has in his hand with an effect of the specified type that targets the specified
    /// card attribute.
    /// </summary>
    /// <param name="type">The type of the spell's effect.</param>
    /// <param name="attribute">The attribute of the spell's effect.</param>
    /// <returns>A list of the spell cards from the AI's hand with the specified effect conditions.</returns>
    protected List<Card> GetSpellCardsInHand(CardEffectActionType type, string attribute)
    {
        var spells = new List<Card>();
        foreach (var id in hand)
        {
            var card = GameManager.Instance.GetCard(id);
            if (card.Definition == "Spell")
            {
                foreach (var effect in card.Effects)
                {
                    var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as CardEffectDefinition;
                    if (effectDefinition != null && effectDefinition.Type == EffectType.TargetCard)
                    {
                        var cardEffectDefinition = effectDefinition as CardEffectDefinition;
                        if (cardEffectDefinition.Action == type &&
                            cardEffectDefinition.Attribute == attribute)
                        {
                            spells.Add(card);
                        }
                    }
                }
            }
        }
        return spells;
    }

    /// <summary>
    /// Returns the spell cards the AI has in his hand with an effect with the specified name.
    /// </summary>
    /// <param name="effectName">The name of the spell's effect.</param>
    /// <returns>A list of the spell cards from the AI's hand with the specified effect name.</returns>
    protected List<Card> GetSpellCardsInHand(string effectName)
    {
        var spells = new List<Card>();
        foreach (var id in hand)
        {
            var card = GameManager.Instance.GetCard(id);
            if (card.Definition == "Spell")
            {
                foreach (var effect in card.Effects)
                {
                    if (effect.Definition == effectName)
                        spells.Add(card);
                }
            }
        }
        return spells;
    }

    /// <summary>
    /// Returns the creature cards the AI has in his hand.
    /// </summary>
    /// <returns>A list of the creature cards from the AI's hand.</returns>
    protected List<Card> GetCreatureCardsInHand()
    {
        var creatures = new List<Card>();
        foreach (var id in hand)
        {
            var card = GameManager.Instance.GetCard(id);
            if (card.Definition == "Creature")
                creatures.Add(card);
        }
        return creatures;
    }

    /// <summary>
    /// Returns the creature cards the AI has on the board.
    /// </summary>
    /// <returns>A list of the creature cards the AI has on the board.</returns>
    protected List<DemoNetworkCard> GetMyCreatureCardsOnBoard()
    {
        var creatures = new List<DemoNetworkCard>();
        var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
        foreach (var key in keys)
        {
            var netCard = NetworkServer.objects[key].gameObject.GetComponent<DemoNetworkCard>();
            if (netCard != null && netCard.IsAlive && netCard.OwnerNetId == netId)
            {
                var card = GameManager.Instance.GetCard(netCard.CardId);
                if (card.Definition == "Creature")
                    creatures.Add(netCard);
            }
        }
        return creatures;
    }

    /// <summary>
    /// Returns the creature cards the opponent player has on the board.
    /// </summary>
    /// <returns>A list of the creature cards the opponent player has on the board.</returns>
    protected List<DemoNetworkCard> GetOpponentCreatureCardsOnBoard()
    {
        var creatures = new List<DemoNetworkCard>();
        var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
        foreach (var key in keys)
        {
            var netCard = NetworkServer.objects[key].gameObject.GetComponent<DemoNetworkCard>();
            if (netCard != null && netCard.IsAlive && netCard.OwnerNetId == humanPlayer.netId)
            {
                var card = GameManager.Instance.GetCard(netCard.CardId);
                if (card.Definition == "Creature")
                    creatures.Add(netCard);
            }
        }
        return creatures;
    }
}
