using System;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace SampleGame
{
    public sealed class GameRunner : MonoBehaviour
    {
        private void Start()
        {
            try
            {
                var state = new GameState(100);
                int score = state.Fire(3);
                if (score != 298 || state.Ammo != 97) throw new Exception("business result changed");
                var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "fixture.bundle"));
                if (bundle == null) throw new Exception("bundle missing");
                var asset = bundle.LoadAsset<TextAsset>("message");
                if (asset == null || asset.text != "kingsSafe fixture: asset bytes unchanged\n") throw new Exception("asset changed");
                Debug.Log("PROBE_SEMANTICS_PASS score=" + score + " ammo=" + state.Ammo + " asset=" + asset.text.Trim());
                bundle.Unload(true);
                // Controlled test instrumentation; identical in baseline and protected builds.
                var field = typeof(GameState).GetField("Ammo");
                int raw = (int)field.GetValue(state);
                Debug.Log("PROBE_STORAGE rawEqualsValue=" + (raw == state.Ammo));
                field.SetValue(state, 999);
                bool rejected = false;
                try { state.Fire(1); } catch (InvalidOperationException) { rejected = true; }
                Debug.Log("PROBE_TAMPER rejected=" + rejected);
                bool named = typeof(GameState).GetMethod("Score", BindingFlags.Instance | BindingFlags.NonPublic) != null;
                Debug.Log("PROBE_LOGIC originalPrivateNamePresent=" + named);
                Debug.Log("PROBE_COMPLETED");
                Application.Quit(0);
            }
            catch (Exception e) { Debug.LogException(e); Application.Quit(2); }
        }
    }
}
