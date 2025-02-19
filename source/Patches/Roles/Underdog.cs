﻿using TownOfUs.ImpostorRoles.UnderdogMod;

namespace TownOfUs.Roles
{
    public class Underdog : Role
    {
        public Underdog(PlayerControl player) : base(player, RoleEnum.Underdog)
        {
            ImpostorText = () => "Use your comeback power to win";
            TaskText = () => "long kill cooldown when 2 imps, short when 1 imp";
        }

        protected override void DoOnMeetingEnd()
        {
            SetKillTimer();
        }

        public float MaxTimer() => PlayerControl.GameOptions.KillCooldown * (
            PerformKill.LastImp() ? 0.5f : 1.5f
        );

        public void SetKillTimer()
        {
            Player.SetKillTimer(MaxTimer());
        }
    }
}
