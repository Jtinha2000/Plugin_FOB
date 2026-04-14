using Rocket.API;
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
    public class FobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Fob";

        public string Help => "";

        public string Syntax => "/Fob <ID>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "FobCommand"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Player = PlayerTool.getPlayer(caller.DisplayName);
            if(command.Length < 1 || !ushort.TryParse(command[0], out ushort TargetCode))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("FobWrongUsage", Syntax), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            FobModel TargetFob = Main.Instance.Fobs.FirstOrDefault(X => X.Code == TargetCode);
            if(TargetFob == null)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("DoesntExist", TargetCode), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (!TargetFob.HasAcess(Player))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("HasntAcess", TargetCode), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (!TargetFob.IsAvaliable())
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("IsntAvaliable", TargetCode), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (Main.Instance.Fobs.Any(X => X.TeleportRequests.Any(Y => Y.Caller == Player)))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("AlreadyTeleporting"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            TimeSpan RemainingTime = (DateTime.Now - Main.Instance.LastTeleport[Player]);
            if (RemainingTime.TotalSeconds < Main.Instance.Configuration.Instance.FobCooldown)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("FobCooldown", RemainingTime.ToString(@"mm\:ss")), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            TargetFob.AddRequest(Player);
        }
    }
}
