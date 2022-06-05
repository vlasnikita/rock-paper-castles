// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This utility class defines all the unique identifiers for every network message in a game.
    /// </summary>
    public class NetworkProtocol
    {
        public static short RegisterPlayer = 1000;
        public static short StartGame = 1001;
        public static short EndGame = 1002;
        public static short StartTurn = 1003;
        public static short EndTurn = 1004;
        public static short StopTurn = 1005;
        public static short UpdatePlayerAttributes = 1006;
        public static short UpdateOpponentAttributes = 1007;
        public static short PlayCard = 1008;
        public static short SelectTargetPlayer = 1009;
        public static short SelectTargetCard = 1010;
        public static short TargetPlayerSelected = 1011;
        public static short TargetCardSelected = 1012;
        public static short AttackingCardSelected = 1013;
        public static short AttackingCardUnselected = 1014;
        public static short AttackedPlayerSelected = 1015;
        public static short AttackedCardSelected = 1016;
        public static short DrawCards = 1017;
        public static short DiscardCards = 1018;
        public static short CardsToDiscardSelected = 1019;
        public static short CardsAutoDiscarded = 1020;
        public static short SendChatTextMessage = 1021;
        public static short BroadcastChatTextMessage = 1022;
        public static short KilledCard = 1023;
        public static short MoveCards = 1024;
    }

    // Every network message has a corresponding message class that carries the information needed
    // per message.

    public class RegisterPlayerMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the player to be registered.
        /// </summary>
        public NetworkInstanceId NetId;

        /// <summary>
        /// Name of the player to be registered (this is also the nickname to display during a game).
        /// </summary>
        public string Name;

        /// <summary>
        /// True if the player is controlled by a human; false otherwise (AI).
        /// </summary>
        public bool IsHuman;

        public string ActiveDeck;

        /// <summary>
        /// Array of card identifiers that constitute this player's deck.
        /// </summary>
        public int[] Deck;
    }

    public class StartGameMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Index of the player.
        /// </summary>
        public int PlayerIndex;

        /// <summary>
        /// Array containing all the player names of this game.
        /// </summary>
        public string[] PlayerNames;

        /// <summary>
        /// The duration of a turn in this game (in seconds).
        /// </summary>
        public int TurnDuration;

        /// <summary>
        /// The static game zones owned by the player.
        /// </summary>
        public NetStaticGameZone[] StaticGameZones;

        /// <summary>
        /// The dynamic game zones owned by the player.
        /// </summary>
        public NetDynamicGameZone[] DynamicGameZones;
    }

    public class EndGameMessage : MessageBase
    {
        /// <summary>
        /// Index of the player that has won the game.
        /// </summary>
        public int WinnerPlayerIndex;
    }

    public class StartTurnMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// True if the recipient player is the active player of this turn;
        /// false otherwise.
        /// </summary>
        public bool IsRecipientTheActivePlayer;

        /// <summary>
        /// Turn number.
        /// </summary>
        public int Turn;

        /// <summary>
        /// The static game zones owned by the player.
        /// </summary>
        public NetStaticGameZone[] StaticGameZones;

        /// <summary>
        /// The dynamic game zones owned by the player.
        /// </summary>
        public NetDynamicGameZone[] DynamicGameZones;

        /// <summary>
        /// Array of attribute names that need to be updated on the client.
        /// </summary>
        public string[] AttributeNames;

        /// <summary>
        /// Array of attribute values with the updated values for the client.
        /// </summary>
        public int[] AttributeValues;
    }

    public class EndTurnMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// True if the recipient player is the active player of this turn;
        /// false otherwise.
        /// </summary>
        public bool IsRecipientTheActivePlayer;
    }

    public class StopTurnMessage : MessageBase
    {
    }

    // Note how we avoid sending the entire player's deck card information to prevent manipulation by a
    // malevolent client.
    public class UpdatePlayerAttributesMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Network identifier of this player.
        /// </summary>
        public NetworkInstanceId NetId;

        /// <summary>
        /// Array of attribute names that need to be updated on the client.
        /// </summary>
        public string[] Names;

        /// <summary>
        /// Array of attribute values with the updated values for the client.
        /// </summary>
        public int[] Values;

        /// <summary>
        /// The static game zones owned by this player.
        /// </summary>
        public NetStaticGameZone[] StaticGameZones;

        /// <summary>
        /// The dynamic game zones owned by this player.
        /// </summary>
        public NetDynamicGameZone[] DynamicGameZones;
    }

    // Note how we avoid sending any detailed card information from an opponent to prevent manipulation by a
    // malevolent client.
    public class UpdateOpponentAttributesMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Network identifier of this player.
        /// </summary>
        public NetworkInstanceId NetId;

        /// <summary>
        /// Array of attribute names that need to be updated on the client.
        /// </summary>
        public string[] Names;

        /// <summary>
        /// Array of attribute values with the updated values for the client.
        /// </summary>
        public int[] Values;

        /// <summary>
        /// The static game zones owned by this player.
        /// </summary>
        public NetStaticGameZone[] StaticGameZones;

        /// <summary>
        /// The dynamic game zones owned by this player.
        /// </summary>
        public NetDynamicGameZone[] DynamicGameZones;
    }

    public class PlayCardMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the player requesting to play a card.
        /// </summary>
        public NetworkInstanceId NetId;

        /// <summary>
        /// Identifier of the card to play.
        /// </summary>
        public int CardId;
    }

    public class SelectTargetPlayerMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;
    }

    public class SelectTargetCardMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;
    }

    public class TargetPlayerSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the player selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class TargetCardSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the card selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class AttackingCardSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the card selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class AttackingCardUnselectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the card selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class AttackedPlayerSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the player selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class AttackedCardSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the card selected as a target.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class DrawCardsMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Array containing the identifiers of the cards to draw.
        /// </summary>
        public int[] Cards;
    }

    public class DiscardCardsMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Number of cards to discard.
        /// </summary>
        public int NumCards;

        /// <summary>
        /// True if we are discarding cards at the end of the turn; false otherwise.
        /// </summary>
        public bool IsEOTDiscard;
    }

    public class CardsToDiscardSelectedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the sender of this message.
        /// </summary>
        public NetworkInstanceId SenderNetId;

        /// <summary>
        /// Array containing the identifiers of the cards to discard.
        /// </summary>
        public int[] Cards;
    }

    public class CardsAutoDiscardedMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient of this message.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// Array containing the identifiers of the cards that were automatically
        /// discarded by the server.
        /// </summary>
        public int[] Cards;
    }

    public class SendChatTextMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the sender of this message.
        /// </summary>
        public NetworkInstanceId SenderNetId;

        /// <summary>
        /// Chat text that is being sent.
        /// </summary>
        public string Text;
    }

    public class BroadcastChatTextMessage : MessageBase
    {
        /// <summary>
        /// Chat text that is being sent.
        /// </summary>
        public string Text;
    }

    public class KilledCardMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the card that was killed.
        /// </summary>
        public NetworkInstanceId NetId;
    }

    public class MoveCardsMessage : MessageBase
    {
        /// <summary>
        /// Network identifier of the recipient player.
        /// </summary>
        public NetworkInstanceId RecipientNetId;

        /// <summary>
        /// The name of the origin zone.
        /// </summary>
        public string OriginZone;

        /// <summary>
        /// The name of the destination zone.
        /// </summary>
        public string DestinationZone;

        /// <summary>
        /// The number of cards to move.
        /// </summary>
        public int NumCards;
    }
}
