// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Human-controlled player.
    /// </summary>
    public class HumanPlayer : Player
    {
        protected override void Awake()
        {
            base.Awake();
            IsHuman = true;
        }
    }
}
