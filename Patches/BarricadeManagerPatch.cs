using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDG_FOB.Models;

namespace ZDG_FOB.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), nameof(BarricadeManager.destroyBarricade), new Type[4] { typeof(BarricadeDrop), typeof(byte), typeof(byte), typeof(ushort) })]
    public static class BarricadeManagerPatch
    {
        public static void Prefix(BarricadeDrop barricade, byte x, byte y, ushort plant)
        {
            FobModel Fob = Main.Instance.Fobs.FirstOrDefault(X => X.InstanceID == barricade.GetServersideData().instanceID);
            if (Fob == null)
                return;
            Fob.ClearRequests("FobDestroyed", true);
            Main.Instance.Fobs.Remove(Fob);
        }
    }
}
