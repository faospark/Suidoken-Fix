﻿using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class FrameratePatch
{
    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.SetTargetFrameRate))]
    [HarmonyPrefix]
    static bool SetTargetFrameRate()
    {
        if (Plugin.Config.FPS.Value >= 0)
        {
            Plugin.Log.LogInfo($"FPS set to {Plugin.Config.FPS.Value}.");
            Application.targetFrameRate = Plugin.Config.FPS.Value;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.SetVsyncCount))]
    [HarmonyPrefix]
    static bool SetVsyncCount()
    {
        if (Plugin.Config.Vsync.Value > 0)
        {
            Plugin.Log.LogInfo("VSync enabled.");
            QualitySettings.vSyncCount = 1;
            return false;
        }
        else if (Plugin.Config.Vsync.Value == 0)
        {
            Plugin.Log.LogInfo("VSync disabled.");
            QualitySettings.vSyncCount = 0;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject._FrameChange))]
    [HarmonyPostfix]
    static void FrameChange()
    {
        SetVsyncCount();
        SetTargetFrameRate();

        if (Plugin.Config.NoFrameSkip.Value)
        {
            SystemObject.force60FPS = true;
        }
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.Force60FPS))]
    [HarmonyPrefix]
    static bool Force60FPS(bool isOn)
    {
        return false;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.IsUpdateFrame), MethodType.Getter)]
    [HarmonyPrefix]
    static bool IsUpdateFrame(out bool __result)
    {
        __result = Plugin.Config.NoFrameSkip.Value || SystemObject._isUpdateFrame;

        return false;
    }
}
