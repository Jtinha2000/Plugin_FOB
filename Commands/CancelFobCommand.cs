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
    public class CancelFobCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "CancelFob";

        public string Help => "";

        public string Syntax => "/CancelFob";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "CancelFobCommand"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Player = PlayerTool.getPlayer(caller.DisplayName);
            FobModel TargetFob = Main.Instance.Fobs.FirstOrDefault(X => X.TeleportRequests.Any(Y => Y.Caller == Player));
            if (TargetFob is null)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NotTeleporting"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            TargetFob.RemoveRequest(Player, "Canceled");
        }
    }
}
