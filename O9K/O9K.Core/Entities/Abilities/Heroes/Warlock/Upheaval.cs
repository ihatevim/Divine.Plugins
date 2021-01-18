﻿namespace O9K.Core.Entities.Abilities.Heroes.Warlock
{
    using Base;
    using Base.Components;
    using Base.Types;

    using Divine;

    using Helpers;

    using Metadata;

    [AbilityId(AbilityId.warlock_upheaval)]
    public class Upheaval : CircleAbility, IChanneled, IDebuff
    {
        public Upheaval(Ability baseAbility)
            : base(baseAbility)
        {
            this.RadiusData = new SpecialData(baseAbility, "aoe");
            this.ChannelTime = baseAbility.AbilityData.GetChannelMaximumTime(1);
        }

        public float ChannelTime { get; }

        public string DebuffModifierName { get; } = "modifier_warlock_upheaval";

        public bool IsActivatesOnChannelStart { get; } = true;
    }
}