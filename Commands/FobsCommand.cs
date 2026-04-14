using Rocket.API;
using Rocket.Unturned.Chat;
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
    public class FobsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Fobs";

        public string Help => "";

        public string Syntax => "/Fobs";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "FobsCommand" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Player = PlayerTool.getPlayer(caller.DisplayName);
            List<FobModel> AvaliableFobs = Main.Instance.Fobs.FindAll(X => X.HasAcess(Player));
            if(AvaliableFobs.Count == 0)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("0Fobs"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            int MaxNumberDigits = Main.Instance.Configuration.Instance.MaxFobCodeValue.ToString().Length;
            ChatManager.serverSendMessage(Main.Instance.Translate("FobsListTemplate"), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
            AvaliableFobs.ForEach(X => ChatManager.serverSendMessage(Main.Instance.Translate(("FobsListElement" + ( X.IsAvaliable() ? "Avaliable" : "Blocked")), X.Code.ToString("D" + MaxNumberDigits), X.NextNodeName) + (X.TeleportRequests.Any(XZ => XZ.Caller == Player) ? $" {Main.Instance.Translate("Teleporting")}" : ""), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true));
        }
    }
}
