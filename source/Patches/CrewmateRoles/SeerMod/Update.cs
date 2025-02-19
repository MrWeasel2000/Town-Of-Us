using System.Linq;
using HarmonyLib;
using TownOfUs.ImpostorRoles.CamouflageMod;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.SeerMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Update
    {
        public static string NameText(PlayerControl player, string str = "", bool meeting = false)
        {
            if (CamouflageUnCamouflage.IsCamoed)
            {
                if (meeting && !CustomGameOptions.MeetingColourblind) return player.name + str;

                return "";
            }

            return player.name + str;
        }

        private static void RevealSeerInMeeting(MeetingHud __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Seer))
            {
                var seerRole = (Seer) role;
                if (!seerRole.Investigated.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) continue;
                if (!seerRole.CheckSeeReveal(PlayerControl.LocalPlayer)) continue;
                var state = __instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == seerRole.Player.PlayerId);
                state.NameText.color = seerRole.Color;
                state.NameText.text = NameText(seerRole.Player, " (Seer)", true);
            }
        }

        // Assumes the local player is the Seer
        private static void RevealSightsInMeeting(MeetingHud __instance, Seer seer)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!seer.Investigated.TryGetValue(player.PlayerId, out var successfulInvestigation) || !successfulInvestigation) continue;
                foreach (var state in __instance.playerStates)
                {
                    if (player.PlayerId != state.TargetPlayerId) continue;
                    var roleType = Utils.GetRole(player);
                    switch (roleType)
                    {
                        case RoleEnum.Crewmate:
                            state.NameText.color =
                                CustomGameOptions.SeerInfo == SeerInfo.Faction ? Color.green : Color.white;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.SeerInfo == SeerInfo.Role ? " (Crew)" : "", true);
                            break;
                        case RoleEnum.Impostor:
                            state.NameText.color = CustomGameOptions.SeerInfo == SeerInfo.Faction
                                ? Color.red
                                : Palette.ImpostorRed;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.SeerInfo == SeerInfo.Role ? " (Imp)" : "", true);
                            break;
                        default:
                            var role = Role.GetRole(player);
                            state.NameText.color = CustomGameOptions.SeerInfo == SeerInfo.Faction
                                ? role.FactionColor
                                : role.Color;
                            state.NameText.text = NameText(player,
                                CustomGameOptions.SeerInfo == SeerInfo.Role ? $" ({role.Name})" : "", true);
                            break;
                    }
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(HudManager __instance)
        {
            if (
                PlayerControl.AllPlayerControls.Count <= 1
                || PlayerControl.LocalPlayer == null
                || PlayerControl.LocalPlayer.Data == null
                || (PlayerControl.LocalPlayer.Data.IsDead && CustomGameOptions.DeadSeeRoles)
            )
            {
                return;
            }
            foreach (var role in Role.GetRoles(RoleEnum.Seer))
            {
                var seerRole = (Seer) role;
                if (!seerRole.Investigated.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) continue;
                if (!seerRole.CheckSeeReveal(PlayerControl.LocalPlayer)) continue;

                seerRole.Player.nameText.color = seerRole.Color;
                seerRole.Player.nameText.text = NameText(seerRole.Player, " (Seer)");
            }

            if (MeetingHud.Instance != null) RevealSeerInMeeting(MeetingHud.Instance);
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Seer)) return;
            var seer = Role.GetRole<Seer>(PlayerControl.LocalPlayer);
            if (MeetingHud.Instance != null) RevealSightsInMeeting(MeetingHud.Instance, seer);


            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!seer.Investigated.TryGetValue(player.PlayerId, out var successfulInvestigation) || !successfulInvestigation) continue;
                var roleType = Utils.GetRole(player);
                player.nameText.transform.localPosition = new Vector3(0f, 2f, -0.5f);
                switch (roleType)
                {
                    case RoleEnum.Crewmate:
                        player.nameText.color =
                            CustomGameOptions.SeerInfo == SeerInfo.Faction ? Color.green : Color.white;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.SeerInfo == SeerInfo.Role ? " (Crew)" : "");
                        break;
                    case RoleEnum.Impostor:
                        player.nameText.color = CustomGameOptions.SeerInfo == SeerInfo.Faction
                            ? Color.red
                            : Palette.ImpostorRed;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.SeerInfo == SeerInfo.Role ? " (Imp)" : "");
                        break;
                    default:
                        var role = Role.GetRole(player);
                        player.nameText.color = CustomGameOptions.SeerInfo == SeerInfo.Faction
                            ? role.FactionColor
                            : role.Color;
                        player.nameText.text = NameText(player,
                            CustomGameOptions.SeerInfo == SeerInfo.Role ? $" ({role.Name})" : "");
                        break;
                }
            }
        }
    }
}