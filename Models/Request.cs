using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ZDG_FOB.Models
{
    public class Request
    {
        public delegate void CounterEnded(Request Request);
        public event CounterEnded OnCounterEnded;

        public Player Caller { get; set; }
        public int Timer { get; set; }
        public Coroutine Countdown { get; set; }
        public Request()
        {
            
        }
        public Request(FobModel Fob, Player caller, int timer)
        {
            Caller = caller;
            Timer = timer;
            OnCounterEnded += Fob.Request_OnCounterEnded;
            Countdown = Main.Instance.StartCoroutine(Counter());
        }

        public IEnumerator Counter()
        {
            while(Timer > 0)
            {
                if(Timer <= 3)
                    ChatManager.serverSendMessage(Main.Instance.Translate("TeleportingIn", Timer, (Timer > 1 ? "s" : "")), Main.MessagesColor, null, Caller.channel.owner, EChatMode.SAY, null, true);
                yield return new WaitForSeconds(1f);
                Timer--;
            }

            if (OnCounterEnded != null)
                OnCounterEnded.Invoke(this);
        }

        public override string ToString() =>
            new TimeSpan(0, 0, Timer).ToString("ss");
    }
}
