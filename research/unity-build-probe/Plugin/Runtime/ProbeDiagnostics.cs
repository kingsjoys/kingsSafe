using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace KingsSafe.Probe
{
    // Independent test instrumentation for this own-code fixture; never a shipping component.
    public static class ProbeDiagnostics
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CheckBoundaries()
        {
            if (!Environment.GetCommandLineArgs().Contains("--probe-attacks")) return;
            try
            {
                var type = AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("SampleGame.GameState")).First(t=>t!=null);
                var fields = type.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
                var ammo=type.GetField("Ammo");
                var tag=type.GetField("__ks_tag_Ammo",BindingFlags.Instance|BindingFlags.NonPublic);
                var fire=type.GetMethod("Fire");
                var state=Activator.CreateInstance(type,new object[]{100});
                int encoded=ProbeRuntime.Encode(999);
                ammo.SetValue(state,encoded); tag.SetValue(state,~encoded);
                bool coordinated=(int)fire.Invoke(state,new object[]{1})==3001;
                Debug.Log("PROBE_BOUNDARY coordinatedRewriteAccepted="+coordinated);
                state=Activator.CreateInstance(type,new object[]{100});
                var snapshot=fields.Select(f=>f.GetValue(state)).ToArray();
                int first=(int)fire.Invoke(state,new object[]{1});
                for(int i=0;i<fields.Length;i++) fields[i].SetValue(state,snapshot[i]);
                int replay=(int)fire.Invoke(state,new object[]{1});
                Debug.Log("PROBE_BOUNDARY fullStateReplayAccepted="+(first==replay));
                byte[] container=ProbeRuntime.Wrap(new byte[]{1,2,3,4,5});
                container[20]^=1;
                bool rejected=false;
                try { ProbeRuntime.Unwrap(container); } catch(InvalidDataException) { rejected=true; }
                Debug.Log("PROBE_AUTH corruptedCiphertextRejected="+rejected);
                if(!coordinated || first!=replay || !rejected) throw new Exception("Unexpected fixture boundary result");
                Debug.Log("PROBE_BOUNDARIES_COMPLETED");
            }
            catch(Exception e) { Debug.LogException(e); Application.Quit(3); }
        }
    }
}
