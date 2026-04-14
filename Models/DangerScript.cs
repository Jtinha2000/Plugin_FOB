using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ZDG_FOB.Models
{
    public class DangerScript : MonoBehaviour
    {
        public ushort FobCode { get; set; }
        public List<Player> PlayersInside { get; set; } = new List<Player>();

        public void OnTriggerEnter(Collider Other)
        {
            List<Player> Players = new List<Player>();
            if (Other.gameObject.CompareTag("Player"))
            {
                Players.Add(Other.GetComponent<Player>());
            }
            else if (Other.gameObject.CompareTag("Vehicle"))
            {
                InteractableVehicle Vehicle = Other.gameObject.GetComponent<InteractableVehicle>();
                foreach (Passenger Pass in Vehicle.passengers)
                {
                    if (Pass.player != null)
                        Players.Add(Pass.player.player);
                }
            }
            else
                return;
            Players.RemoveAll(X => PlayersInside.Contains(X) || X.life.isDead);
            if(Players.Count > 0)
            {
                PlayersInside.AddRange(Players);
                FobModel TargetFOB = Main.Instance.Fobs.First(X => X.Code == FobCode);
                if (PlayersInside.Any(X => !TargetFOB.HasAcess(X)))
                    TargetFOB.ClearRequests("Next", false);
            }
        }
        public void OnTriggerExit(Collider Other)
        {
            List<Player> Players = new List<Player>();
            if (Other.gameObject.CompareTag("Player"))
            {
                Players.Add(Other.GetComponent<Player>());
            }
            else if (Other.gameObject.CompareTag("Vehicle"))
            {
                InteractableVehicle Vehicle = Other.gameObject.GetComponent<InteractableVehicle>();
                foreach (Passenger Pass in Vehicle.passengers)
                {
                    if (Pass.player != null)
                        Players.Add(Pass.player.player);
                }
            }
            else
                return;

            PlayersInside.RemoveAll(X => Players.Contains(X));
        }
    }
}
