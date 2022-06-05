// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Computer-controlled player that is used in single-player mode.
    /// </summary>
    public class AIPlayer : Player
    {
        /// <summary>
        /// This value between 0 and 1 indicates the tendency of the AI to target the opponent
        /// creatures (0) versus targetting the opponent player (1) in combat.
        /// </summary>
        protected float aggression = 0.5f;

        /// <summary>
        /// Random number generator used in internal logic.
        /// </summary>
        protected System.Random rng = new System.Random();

        /// <summary>
        /// Cached reference to the human opponent player.
        /// </summary>
        protected Player humanPlayer;

        /// <summary>
        /// True if the current game has ended; false otherwise.
        /// </summary>
        protected bool gameEnded;

        /// <summary>
        /// Unity's Awake.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            IsHuman = false;
        }

        /// <summary>
        /// Called when the game starts.
        /// </summary>
        /// <param name="msg">Start game message.</param>
        public override void OnStartGame(StartGameMessage msg)
        {
            base.OnStartGame(msg);
            humanPlayer = NetworkingUtils.GetHumanLocalPlayer();
        }

        /// <summary>
        /// Called when the game ends.
        /// </summary>
        /// <param name="msg">End game message.</param>
        public override void OnEndGame(EndGameMessage msg)
        {
            base.OnEndGame(msg);
            gameEnded = true;
            StopAllCoroutines();
        }

        /// <summary>
        /// Called when a new turn for this player starts.
        /// </summary>
        /// <param name="msg">Start turn message.</param>
        public override void OnStartTurn(StartTurnMessage msg)
        {
            base.OnStartTurn(msg);
            if (msg.IsRecipientTheActivePlayer)
                StartCoroutine(RunLogic());
        }

        /// <summary>
        /// Called when the current turn ends.
        /// </summary>
        /// <param name="msg">End turn message.</param>
        public override void OnEndTurn(EndTurnMessage msg)
        {
            base.OnEndTurn(msg);
            if (msg.IsRecipientTheActivePlayer)
                StopAllCoroutines();
        }

        /// <summary>
        /// Called when this player needs to discard cards from his hand.
        /// </summary>
        /// <param name="msg">Discard cards message.</param>
        public override void OnDiscardCards(DiscardCardsMessage msg)
        {
            base.OnDiscardCards(msg);

            for (var i = 0; i < numCardsToDiscard; i++)
            {
                var randomIndex = rng.Next(0, hand.Count - 1);
                AddCardToDiscard(hand[randomIndex]);
            }

            // Also request to end the turn again if this discard was at end-of-turn time.
            if (msg.IsEOTDiscard)
                StopTurn();
        }

        /// <summary>
        /// This method runs the AI logic asynchronously.
        /// </summary>
        /// <returns>The AI logic coroutine.</returns>
        private IEnumerator RunLogic()
        {
            if (gameEnded)
                yield return null;

            // Simulate 'thinking' time. This could be random or dependent on the
            // complexity of the board state for increased realism.
            yield return new WaitForSeconds(2.0f);
            // Actually perform the AI logic in a separate coroutine.
            StartCoroutine(PerformMove());
        }

        /// <summary>
        /// This method is intended to be overriden to perform the game-specific AI logic.
        /// </summary>
        /// <returns>The AI logic coroutine.</returns>
        protected virtual IEnumerator PerformMove()
        {
            yield return null;
        }
    }
}
