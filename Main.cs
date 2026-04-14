using HarmonyLib;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZDG_FOB.Models;

namespace ZDG_FOB
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Color MessagesColor = new Color(82, 97, 127, 255);
        public Dictionary<Player, DateTime> LastTeleport { get; set; }
        public static Main Instance { get; set; }
        public static Harmony HarmonyProfile { get; set; }
        public List<FobModel> Fobs { get; set; }
        protected override void Load()
        {
            Instance = this;
            HarmonyProfile = new Harmony("ZDG-FOB");
            LastTeleport = new Dictionary<Player, DateTime>();
            HarmonyProfile.PatchAll();
            if (!File.Exists(this.Directory + @"\Fobs.json"))
                File.Create(this.Directory + @"\Fobs.json");
            else
                Fobs = JsonConvert.DeserializeObject<List<FobModel>>(File.ReadAllText(this.Directory + @"\Fobs.json"));
            if (Fobs is null)
                Fobs = new List<FobModel>();

            if (Configuration.Instance.FobCooldown < 1)
            {
                Configuration.Instance.FobCooldown = 1;
                Rocket.Core.Logging.Logger.Log("ZDGFOB > COOLDOWN ENTRE OS PEDIDOS DE TELEPORTE MENOR QUE UM! VALOR REAJUSTADO.");
            }
            if (Configuration.Instance.MaxFobCodeValue <= Fobs.Count)
            {
                Configuration.Instance.MaxFobCodeValue = (ushort)(Fobs.Count + 10);
                Rocket.Core.Logging.Logger.Log("ZDGFOB > NÚMERO MÁXIMO DE CÓDIGO DE FOB MENOR DO QUE A QUANTIDADE DE FOBS! VALOR REAJUSTADO.");
            }
            if (Configuration.Instance.AttackFobPossibleBarricades.Any(X => X != 0 && Configuration.Instance.DefenseFobPossibleBarricades.Contains(X)))
            {
                Configuration.Instance.AttackFobPossibleBarricades = new List<ushort> { 0 };
                Configuration.Instance.DefenseFobPossibleBarricades = new List<ushort> { 0 };
                Rocket.Core.Logging.Logger.Log("ZDGFOB > INTERSEÇÃO ENTRE OS ID'S DAS POSSIVEIS FOB'S DE ATAQUE E DEFESA ENCONTRADA, REDEFININDO PARA AS CONFIGURAÇÕES DE SEGURANÇA.");
            }
            if (Configuration.Instance.FobCommandPermission != "FobCommand")
                Configuration.Instance.FobCommandPermission = "FobCommand";
            if (Configuration.Instance.FobsCommandPermission != "FobsCommand")
                Configuration.Instance.FobCommandPermission = "FobsCommand";
            if (Configuration.Instance.CancelCommandPermission != "CancelFobCommand")
                Configuration.Instance.CancelCommandPermission = "CancelFobCommand";
            if (Configuration.Instance.SetCommandPermission != "SetFobCommand")
                Configuration.Instance.SetCommandPermission = "SetFobCommand";
            Configuration.Save();

            if (Level.isLoaded)
            {
                Provider.clients.ForEach(X => LastTeleport.Add(X.player, DateTime.MinValue));
                Level_OnLevelLoaded(0);
            }
            else
                Level.onLevelLoaded += Level_OnLevelLoaded;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected_ColliderManagment;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected_RequestManagment;
            UnturnedPlayerEvents.OnPlayerDead += UnturnedPlayerEvents_OnPlayerDead_ColliderManagment;
            UnturnedPlayerEvents.OnPlayerDead += UnturnedPlayerEvents_OnPlayerDead_RequestManagment;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
        }
        protected override void Unload()
        {
            HarmonyProfile.UnpatchAll();

            using (StreamWriter Writer = new StreamWriter(this.Directory + @"\Fobs.json", false))
            {
                Writer.Write(JsonConvert.SerializeObject(Fobs));
            }
            Fobs.ForEach(X => X.ClearRequests("Shutdown", true));

            Level.onLevelLoaded -= Level_OnLevelLoaded;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected_ColliderManagment;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected_RequestManagment;
            UnturnedPlayerEvents.OnPlayerDead -= UnturnedPlayerEvents_OnPlayerDead_ColliderManagment;
            UnturnedPlayerEvents.OnPlayerDead -= UnturnedPlayerEvents_OnPlayerDead_RequestManagment;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
        }
        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"TeleportingIn",  "<color=#7F4740>[FOB]</color> Você será teleportado em <color=#7F4740>{0}</color> segundo{1}..."},
            {"RequestedAdded", "<color=#7F4740>[FOB]</color> Você será teleportado à <color=#7F4740>{0}</color> em <color=#7F4740>{1}</color> segundo{2}..."},
            {"RequestedCanceled", "<color=#7F4740>[FOB]</color> Seu teletransporte para o fob foi cancelado (<color=#7F4740>{0}</color>)..." },
            {"RequestedSucess", "<color=#7F4740>[FOB]</color> Você acaba de ser teletransportado!"},
            {"RequestedFail", "<color=#7F4740>[FOB]</color> O teletransporte falhou...)"},

            {"Shutdown", "SERVER SHUTDOWN"},
            {"FobDestroyed", "DESTROYED FOB"},
            {"Canceled", "CANCELED"},
            {"Next", "ENEMIES NEARBY"},

            {"NextEnemy", "<color=#7F4740>[FOB]</color> Seu FOB está próximo demais a outro jogador inimigo!" },
            {"AllieFobNext", "<color=#7F4740>[FOB]</color> Seu FOB está próximo demais a outro fob aliado!"},
            {"EnemyFobNext", "<color=#7F4740>[FOB]</color> Seu FOB está próximo demais a outro fob inimigo!"},
            {"IsPlanted", "<color=#7F4740>[FOB]</color> Não é possível definir uma FOB em veiculos!" },

            {"NeedsBeOwner", "<color=#7F4740>[FOB]</color> É necessário que você seja o dono da barricada!"},
            {"FobAlreadyRegistered", "<color=#7F4740>[FOB]</color> Esta barricada ja está registrada como um FOB!" },
            {"NotTeleporting", "<color=#7F4740>[FOB]</color> Você deve estar teleportando!" },

            {"FobCooldown", "<color=#7F4740>[FOB]</color> Aguarde <color=#7F4740>{0}</color> para teletransporta-se novamente!" },
            {"FobWrongUsage", "<color=#7F4740>[FOB]</color> Uso errado do comando! Tente: <color=#7F4740>{0}</color>..."},
            {"DoesntExist", "<color=#7F4740>[FOB]</color> O Fob de código <color=#7F4740>{0}</color> não existe, tente utilizar /Fobs!"},
            {"IsntAvaliable", "<color=#7F4740>[FOB]</color> Há inimigos próximos a Fob <color=#7F4740>{0}</color>!" },
            {"HasntAcess", "<color=#7F4740>[FOB]</color> Você não tem acesso a Fob <color=#7F4740>{0}</color>!"},
            {"AlreadyTeleporting", "<color=#7F4740>[FOB]</color> Você ja está teleportando para outro Fob! Tente utilizar /CancelFob..." },

            {"0Fobs", "<color=#7F4740>[FOB]</color> Não há nenhum fob registrado!" },
            {"FobsListTemplate", "<color=#7F4740>[FOB]</color> ID | LOCALIZAÇÃO:"},
            {"FobsListElementAvaliable", "[FOB] <color=#5AA7E6>{0}</color> | <color=#5AA7E6>{1}</color>"},
            {"FobsListElementBlocked", "[FOB] <color=#E6AA5A>{0}</color> | <color=#E6AA5A>{1}</color>"},
            {"Teleporting", "(Teleportando)"},

            {"FobCreated", "<color=#7F4740>[FOB]</color> Fob criada: ID: <color=#7F4740>{0}</color> | Localização: <color=#7F4740>{1}</color>"},
            {"NeedsBarricade", "<color=#7F4740>[FOB]</color> É necessário estar olhando para uma barricada válida..."},
        };

        public void Level_OnLevelLoaded(int _)
        {
            List<BarricadeDrop> AllBarricades = new List<BarricadeDrop>();
            foreach (var Regions in BarricadeManager.BarricadeRegions)
                AllBarricades.AddRange(Regions.drops);

            List<FobModel> FobsToRemove = new List<FobModel>();
            Fobs.ForEach(X =>
            {
                BarricadeDrop Target = AllBarricades.FirstOrDefault(Z => Z.instanceID == X.InstanceID);
                if (Target != null)
                    X.TargetBarricade = Target.GetServersideData();

                if (Main.Instance.Configuration.Instance.AttackFobPossibleBarricades.Contains(Target.asset.id) || Main.Instance.Configuration.Instance.DefenseFobPossibleBarricades.Contains(Target.asset.id))
                {
                    X.GenerateCode();
                    X.GetNextNodeName();
                    X.GenerateCollider();
                }
                else
                    FobsToRemove.Add(X);
            });
            FobsToRemove.ForEach(X => Fobs.Remove(X));
        }
        public void UnturnedPlayerEvents_OnPlayerDead_RequestManagment(Rocket.Unturned.Player.UnturnedPlayer player, Vector3 position) =>
            Events_OnPlayerDisconnected_RequestManagment(player);
        public void Events_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            if (!LastTeleport.ContainsKey(player.Player))
                LastTeleport.Add(player.Player, DateTime.MinValue);
        }
        public void Events_OnPlayerDisconnected_RequestManagment(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            FobModel PossibleFob = Main.Instance.Fobs.FirstOrDefault(X => X.TeleportRequests.Any(Y => Y.Caller == player.Player));
            if (PossibleFob == null)
                return;

            PossibleFob.RemoveRequest(player.Player);
        }
        public void UnturnedPlayerEvents_OnPlayerDead_ColliderManagment(Rocket.Unturned.Player.UnturnedPlayer player, Vector3 position) =>
            Events_OnPlayerDisconnected_ColliderManagment(player);
        public void Events_OnPlayerDisconnected_ColliderManagment(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            List<FobModel> PossiblesFob = Main.Instance.Fobs.FindAll(X => X.Collider != null && X.Collider.GetComponent<DangerScript>().PlayersInside.Contains(player.Player));
            if (PossiblesFob.Count == 0)
                return;

            PossiblesFob.ForEach(X => X.Collider.GetComponent<DangerScript>().PlayersInside.Remove(player.Player));
        }
    }
}
