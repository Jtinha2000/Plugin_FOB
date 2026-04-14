using HarmonyLib;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace ZDG_FOB
{
    public class Configuration : IRocketPluginConfiguration
    {
        public int FobTimeToWarp { get; set; }
        public int FobCooldown { get; set; }
        public ushort MaxFobCodeValue { get; set; }
        [XmlArrayItem("BarricadeID")]
        public List<ushort> DefenseFobPossibleBarricades { get; set; }
        public List<ushort> AttackFobPossibleBarricades { get; set; }
        public uint MinEnemyFobDistance { get; set; }
        public uint MinEnemyDistance { get; set; }
        public uint MinAllieFobDistance { get; set; }
        public bool EnemyOnlyBlockAttackFOB { get; set; }

        public string FobCommandPermission { get; set; }
        public string FobsCommandPermission { get; set; }
        public string CancelCommandPermission { get; set; }
        public string SetCommandPermission { get; set; }
        public void LoadDefaults()
        {
            EnemyOnlyBlockAttackFOB = true;
            DefenseFobPossibleBarricades = new List<ushort> { 458 };
            AttackFobPossibleBarricades = new List<ushort> { 46 };
            FobCooldown = 60;
            FobTimeToWarp = 15;
            MaxFobCodeValue = 200;
            MinEnemyFobDistance = 101;
            MinEnemyDistance = 50;
            MinAllieFobDistance = 101;

            SetCommandPermission = "SetFobCommand";
            CancelCommandPermission = "CancelFobCommand";
            FobCommandPermission = "FobCommand";
            FobsCommandPermission = "FobsCommand";
        }
    }
}
