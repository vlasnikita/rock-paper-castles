// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    ///  This server handler is responsible for managing the network aspects of a combat between
    ///  two cards.
    ///
    /// Combat follow this sequence:
    ///     - A player selects a card that is eligible as an attacker during his turn and sends
    ///       this information to the server.
    ///     - A player then selects a target player or card as the attack's target and sends this
    ///       information to the server.
    ///     - The server then proceeds with resolving the attack authoritatively and updating all
    ///       the clients afterwards.
    ///
    /// This process is currently very much suited to the traditional way of resolving combats in
    /// CCGs (the attacker's attack value is substracted from the attacked's defense value, and
    /// vice versa). This is something we would like to expand upon in future updates to allow for
    /// more varied/complex mechanics.
    /// </summary>
    public class CombatHandler : ServerHandler
    {
        protected NetworkCard attackingCard;
        protected NetworkCard attackedCard;

        public CombatHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.AttackingCardSelected, OnAttackingCardSelected);
            NetworkServer.RegisterHandler(NetworkProtocol.AttackingCardUnselected, OnAttackingCardUnselected);
            NetworkServer.RegisterHandler(NetworkProtocol.AttackedPlayerSelected, OnAttackedPlayerSelected);
            NetworkServer.RegisterHandler(NetworkProtocol.AttackedCardSelected, OnAttackedCardSelected);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.AttackedCardSelected);
            NetworkServer.UnregisterHandler(NetworkProtocol.AttackedPlayerSelected);
            NetworkServer.UnregisterHandler(NetworkProtocol.AttackingCardUnselected);
            NetworkServer.UnregisterHandler(NetworkProtocol.AttackingCardSelected);
        }

        public override void OnStartTurn()
        {
            base.OnStartTurn();

            // Clear the internal state when starting a new turn. This effectively cancels any attack
            // currently in progress that did not finish in time.
            attackingCard = null;
            attackedCard = null;
        }

        public virtual void OnAttackingCardSelected(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<AttackingCardSelectedMessage>();
            var obj = NetworkingUtils.GetNetworkObject(msg.NetId);
            var card = obj.GetComponent<DemoNetworkCard>();
            card.WrapperSetAttackingIconEnabled(true);
            attackingCard = card;
        }

        public virtual void OnAttackingCardUnselected(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<AttackingCardUnselectedMessage>();
            var obj = NetworkingUtils.GetNetworkObject(msg.NetId);
            var card = obj.GetComponent<DemoNetworkCard>();
            card.WrapperSetAttackingIconEnabled(false);
            attackingCard = null;
        }

        public virtual void OnAttackedPlayerSelected(NetworkMessage netMsg)
        {
            // TODO: decouple this class from any game-specific attributes like
            // 'life' or 'attack'.

            if (attackingCard != null)
            {
                // Check for triggered effects.
                server.OnCardAttacked(attackingCard.netId);

                var msg = netMsg.ReadMessage<AttackedPlayerSelectedMessage>();
                var playerState = server.Players.Find(x => x.NetId == msg.NetId);
                var attribute = playerState.GetAttribute("Life");
                var attack = attackingCard.GetAttribute("Attack");
                playerState.SetAttribute("Life", attribute.Value.Value - attack.Value.Value);
                server.SendPlayerStateToAllClients(playerState);
                attackingCard.GetComponent<DemoNetworkCard>().DisableAttack();
                attackingCard = null;
            }
        }

        public virtual void OnAttackedCardSelected(NetworkMessage netMsg)
        {
            if (attackingCard != null)
            {
                Debug.Log("0. ATTACKING card netId: " + attackingCard.netId);
                var canAttack = true;

                var msg = netMsg.ReadMessage<AttackedCardSelectedMessage>();
                var obj = NetworkingUtils.GetNetworkObject(msg.NetId);
                var card = obj.GetComponent<NetworkCard>();
                attackedCard = card;

                var attackedCardInstance = GameManager.Instance.GetCard(attackedCard.CardId);
                Debug.Log("1. ATTACKED card name: " + attackedCardInstance.Name);

                // try{
                //     var enemyBoardCards = server.CurrentOpponents[0].GetDynamicGameZone("Board").Cards;
                //     if(enemyBoardCards.Count > 0){
                //         for (var i = 0; i < enemyBoardCards.Count; i++){
                //             var iNetEnemyCard = NetworkingUtils.GetNetworkObject(enemyBoardCards[i]).GetComponent<NetworkCard>();
                //             var iEnemyCard = GameManager.Instance.GetCard(iNetEnemyCard.CardId);
                //             for(var j = 0; j < iEnemyCard.Effects.Count; j++){
                //                 // var cardEffectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == card.Effects[j].Definition);
                //                 var isTaunt = iEnemyCard.Effects[j].Definition == "Taunt";
                //                 var isAttackedCard = enemyBoardCards[i] == attackingCard.netId;
                //                 Debug.Log("NET_ID COMPARISON: " + enemyBoardCards[i] + " | " + attackedCard);
                //                 if(isTaunt && !isAttackedCard) canAttack = false;
                //             }
                //         } 
                //     }   
                //     Debug.Log("1. CAN ATTACK THIS: " + canAttack.ToString()); 
                // } catch (Exception e) {Debug.Log(e);}

                if(canAttack){
                    Debug.Log("3. INITIATE ATTACK: " + canAttack.ToString()); 
                    // Check for triggered effects.
                    server.OnCardAttacked(attackingCard.netId);
                    attackingCard.GetComponent<DemoNetworkCard>().DisableAttack();
                    ResolveCombatDamage();
                } else {
                    Debug.Log("ATTACK PREVENTED!");
                }
            }
        }

        private void ResolveCombatDamage()
        {
            if (attackingCard != null && attackedCard != null)
            {
                // TODO: decouple this class from any game-specific attributes like
                // 'life' or 'attack'.
                var attackingAttack = attackingCard.GetAttribute("Attack").Value.Value;
                var attackingDefense = attackingCard.GetAttribute("Defense").Value.Value;
                var attackedAttack = attackedCard.GetAttribute("Attack").Value.Value;
                var attackedDefense = attackedCard.GetAttribute("Defense").Value.Value;
                attackedCard.SetAttribute("Defense", attackedDefense - attackingAttack);
                attackingCard.SetAttribute("Defense", attackingDefense - attackedAttack);

                attackingCard = null;
                attackedCard = null;
            }
        }
    }
}
