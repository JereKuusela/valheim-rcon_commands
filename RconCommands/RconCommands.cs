using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using HarmonyLib;
namespace RconCommands;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("nl.avii.plugins.rcon", BepInDependency.DependencyFlags.HardDependency)]
public class RconCommands : BaseUnityPlugin {
  public const string GUID = "rcon_commands";
  public const string NAME = "Rcon Commands";
  public const string VERSION = "1.0";
#nullable disable
  rcon.rcon RCON;
#nullable enable
  public void Awake() => new Harmony("GUID").PatchAll();

  void OnEnable() {
    RCON = GetComponent<rcon.rcon>();
    RCON.OnUnknownCommand += RCON_OnUnknownCommand;
  }

  private string RCON_OnUnknownCommand(string command, string[] args) {
    Output.Capturing = true;
    Console.instance.TryRunCommand(command + " " + string.Join(" ", args));
    Output.Capturing = false;
    return string.Join("\n", Output.Captured);
  }
}

[HarmonyPatch]
public class Output {
  public static List<string> Captured = new();
  private static bool capturing;
  public static bool Capturing {
    get => capturing;
    set {
      if (value)
        Captured.Clear();
      capturing = value;
    }
  }
  [HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), new[] { typeof(string) }), HarmonyPostfix]
  static void AddString(Terminal __instance, string text) {
    if (!Capturing) return;
    text = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}: {text}";
    Captured.Add(text);
  }
  [HarmonyPatch(typeof(ZLog), nameof(ZLog.Log)), HarmonyPostfix]
  static void Log(object o) {
    if (!Capturing) return;
    var text = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}: {o}";
    Captured.Add(text);
  }
}
