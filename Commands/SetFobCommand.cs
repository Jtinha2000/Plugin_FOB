using Rocket.API;
using Rocket.Unturned.Extensions;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZDG_FOB.Models;

namespace ZDG_FOB.Commands
{
    public class SetFobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "SetFob";

        public string Help => "";

        public string Syntax => "/SetFob";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "SetFobCommand" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Player = PlayerTool.getPlayer(caller.DisplayName);
            RaycastHit[] Hits = Physics.RaycastAll(new Ray(Player.look.aim.position, Player.look.aim.forward), 1.75f, RayMasks.BARRICADE);
            if(Hits.Length == 0)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NeedsBarricade"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            BarricadeDrop Drop = BarricadeManager.FindBarricadeByRootTransform(Hits.FirstOrDefault().transform);
            if (Drop is null || (!Main.Instance.Configuration.Instance.AttackFobPossibleBarricades.Contains(Drop.asset.id) && !Main.Instance.Configuration.Instance.DefenseFobPossibleBarricades.Contains(Drop.asset.id)))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NeedsBarricade"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (IsPlanted(Drop))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("IsPlanted"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Main.Instance.Fobs.Any(X => X.TargetBarricade.instanceID == Drop.GetServersideData().instanceID))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("FobAlreadyRegistered"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Drop.GetServersideData().owner != Player.channel.owner.playerID.steamID.m_SteamID)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NeedsBeOwner"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Provider.clients.Any(X => !X.player.life.isDead && X.playerID.steamID.m_SteamID != Player.channel.owner.playerID.steamID.m_SteamID && !X.ToUnturnedPlayer().IsAdmin && X.playerID.group.m_SteamID != Drop.GetServersideData().group && Vector3.Distance(X.player.transform.position, Drop.GetServersideData().point) <= Main.Instance.Configuration.Instance.MinEnemyDistance))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NextEnemy"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Main.Instance.Fobs.Any(X => !X.HasAcess(Player) && Vector3.Distance(Drop.GetServersideData().point, X.TargetBarricade.point) < Main.Instance.Configuration.Instance.MinEnemyFobDistance))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("EnemyFobNext"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Main.Instance.Fobs.Any(X => X.HasAcess(Player) && Vector3.Distance(Drop.GetServersideData().point, X.TargetBarricade.point) < Main.Instance.Configuration.Instance.MinAllieFobDistance))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("AllieFobNext"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            FobModel NewFob = new FobModel(Drop.GetServersideData());
            Main.Instance.Fobs.Add(NewFob);
            ChatManager.serverSendMessage(Main.Instance.Translate("FobCreated", NewFob.Code, NewFob.NextNodeName), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
        }

        public static bool IsPlanted(BarricadeDrop Drop) =>
            BarricadeManager.tryGetRegion(Drop.model, out byte _, out byte _, out ushort PLANT, out BarricadeRegion _) && PLANT < BarricadeManager.vehicleRegions.Count;
    }
}
