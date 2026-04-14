using Newtonsoft.Json;
using Rocket.Unturned.Extensions;
using SDG.Framework.Devkit;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ZDG_FOB.Models
{
    public class FobModel
    {
        public uint InstanceID { get; set; }
        [JsonIgnore]
        public bool IsDefenseFob => Main.Instance.Configuration.Instance.DefenseFobPossibleBarricades.Contains(TargetBarricade.barricade.asset.id);
        [JsonIgnore]
        public GameObject Collider { get; set; }
        [JsonIgnore]
        public BarricadeData TargetBarricade { get; set; } = null;
        [JsonIgnore]
        public List<Request> TeleportRequests { get; set; } = new List<Request>();
        [JsonIgnore]
        public string NextNodeName { get; set; } = "NULL";
        [JsonIgnore]
        public ushort Code { get; set; } = 0;
        public FobModel()
        {

        }
        public FobModel(BarricadeData targetBarricade)
        {
            TargetBarricade = targetBarricade;
            InstanceID = targetBarricade.instanceID;
            TeleportRequests = new List<Request>();
            if (Level.isLoaded)
            {
                GetNextNodeName();
                GenerateCode();
                GenerateCollider();
            }
        }

        //Ending:
        public void Request_OnCounterEnded(Request Request)
        {
            if (TeleportPlayer(Request.Caller))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("RequestedSucess"), Main.MessagesColor, null, Request.Caller.channel.owner, EChatMode.SAY, null, true);
                Main.Instance.LastTeleport[Request.Caller] = DateTime.Now;
            }
            else
                ChatManager.serverSendMessage(Main.Instance.Translate("RequestedFail"), Main.MessagesColor, null, Request.Caller.channel.owner, EChatMode.SAY, null, true);
            RemoveRequest(Request);
        }

        //Starting:
        public void AddRequest(Player Player)
        {
            Request TpRequest = TeleportRequests.FirstOrDefault(X => X.Caller == Player);
            if (TpRequest != null)
                return;

            TeleportRequests.Add(new Request(this, Player, Main.Instance.Configuration.Instance.FobTimeToWarp));
            ChatManager.serverSendMessage(Main.Instance.Translate("RequestedAdded", NextNodeName, Main.Instance.Configuration.Instance.FobTimeToWarp, (Main.Instance.Configuration.Instance.FobTimeToWarp > 1 ? "s" : "")), Main.MessagesColor, null, Player.channel.owner, EChatMode.SAY, null, true);
        }

        //Removing:
        public void RemoveRequest(Request Request, string Canceled = null)
        {
            if (Request is null)
                return;

            if (Request.Countdown != null)
                Main.Instance.StopCoroutine(Request.Countdown);
            Request.OnCounterEnded -= Request_OnCounterEnded;
            TeleportRequests.Remove(Request);

            if (Canceled != null)
                ChatManager.serverSendMessage(Main.Instance.Translate("RequestedCanceled", Main.Instance.Translate(Canceled)), Main.MessagesColor, null, Request.Caller.channel.owner, EChatMode.SAY, null, true);
        }
        public void RemoveRequest(Player Player, string Canceled = null)
        {
            Request TpRequest = TeleportRequests.FirstOrDefault(X => X.Caller == Player);
            if (TpRequest is null)
                return;

            RemoveRequest(TpRequest, Canceled);
        }

        //Utils:
        public bool IsAvaliable() =>
            Collider == null || !Collider.GetComponent<DangerScript>().PlayersInside.Any(X => !HasAcess(X));
        public bool TeleportPlayer(Player Player)
        {
            Player.teleportToLocationUnsafe(TargetBarricade.point, Player.look.yaw);
            return true;
        }
        public void GenerateCollider()
        {
            if (IsDefenseFob && Main.Instance.Configuration.Instance.EnemyOnlyBlockAttackFOB)
                return;

            Collider = new GameObject($"{NextNodeName} #{Code}");
            Collider.SetActive(true);
            Collider.layer = 30;
            Collider.transform.position = TargetBarricade.point;

            Collider.AddComponent<Rigidbody>();
            Rigidbody Body = Collider.GetComponent<Rigidbody>();
            Body.isKinematic = true;
            Body.useGravity = false;

            Collider.AddComponent(typeof(SphereCollider));
            SphereCollider Col = Collider.GetComponent<SphereCollider>();
            Col.isTrigger = true;
            Col.radius = Main.Instance.Configuration.Instance.MinEnemyDistance;

            DangerScript Script = Collider.AddComponent<DangerScript>();
            Script.FobCode = Code;
            Script.PlayersInside.AddRange(Provider.clients.FindAll(X => !X.player.life.isDead && Vector3.Distance(TargetBarricade.point, X.player.transform.position) <= Main.Instance.Configuration.Instance.MinEnemyDistance).ConvertAll<Player>(Z => Z.player));
        }
        public void GenerateCode()
        {
            System.Random SystemRandom = new System.Random();
            do
            {
                Code = (ushort)SystemRandom.Next(1, Main.Instance.Configuration.Instance.MaxFobCodeValue);
            } while (Main.Instance.Fobs.Any(X => X != this && X.Code == Code));
        }
        public void GetNextNodeName()
        {
            IOrderedEnumerable<LocationDevkitNode> NodesList = LocationDevkitNodeSystem.Get().GetAllNodes().OrderBy(X => Vector3.Distance(X.inspectablePosition, TargetBarricade.point));
            NextNodeName = NodesList.Count() == 0 ? "NULL" : NodesList.FirstOrDefault().locationName + " #" + (Main.Instance.Fobs.Count(X => X.NextNodeName.StartsWith(NodesList.FirstOrDefault().locationName)) + 1).ToString("D2");
        }
        public bool HasAcess(Player Player) =>
            TargetBarricade.owner == Player.channel.owner.playerID.steamID.m_SteamID || Player.channel.owner.ToUnturnedPlayer().IsAdmin || (Player.quests.groupID.m_SteamID == TargetBarricade.group && Player.quests.groupID.m_SteamID != 0);
        public void ClearRequests(string Reason, bool EndFOB = false)
        {
            if (EndFOB && Collider != null)
                GameObject.Destroy(Collider);
            while (TeleportRequests.Count > 0)
                RemoveRequest(TeleportRequests[0], Reason);
        }
    }
}
