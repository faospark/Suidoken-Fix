﻿extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class InstantMessagePatch
{
    private static bool _skipMessage = true;
    private static int _messagePage = 0;

    [HarmonyPatch(typeof(ShareSaveData), nameof(ShareSaveData.LoadFile), [typeof(string), typeof(Il2CppSystem.Action<bool>)])]
    [HarmonyPostfix]
    static void GSD1_ForceFastestSpeed()
    {
        var config = ShareSaveData.system_config;
        if (config != null)
        {
            config.message_speed = 0;
        }
    }

    // For GSD2, the Update code is skipped in GSD1

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Opened))]
    [HarmonyPostfix]
    static void Opened(UIMessageWindow __instance)
    {
        _messagePage = __instance.nowPage;

        if (!__instance.isMessageInputWait)
        {
            _skipMessage = true;
        }
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPrefix]
    static void SkipMessage(UIMessageWindow __instance, ref bool __state)
    {
        __state = __instance.isTextAllDisp;

        if (!__instance.isMessageInputWait)
        {
            if (_messagePage != __instance.nowPage)
            {
                _skipMessage = true;
                _messagePage = __instance.nowPage;
            }
        
            if (_skipMessage)
            {
                __instance.isTextAllDisp = true; // same as doing an input in this context
                _skipMessage = false;
            }
        }
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPostfix]
    static void SkipMessagePost(UIMessageWindow __instance, bool __state)
    {
        __instance.isTextAllDisp = __state;
    }

    // For GSD1

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPrefix]
    static void GSD1_CheckButton(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;

        if (GSD1.Event_c.eventMsgWaitF != 1)
        {
            sysWork.PadTrig |= 0x20; // simulate a Confirm input
        }
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPostfix]
    static void GSD1_CheckButtonPost(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadTrig = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.W_serifu_c), nameof(GSD1.W_serifu_c.war_WriteSerifuWindow))]
    [HarmonyPrefix]
    static void GSD1_CheckButtonWar(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;
        sysWork.PadData |= 0x20; // simulate a Confirm input
    }

    [HarmonyPatch(typeof(GSD1.W_serifu_c), nameof(GSD1.W_serifu_c.war_WriteSerifuWindow))]
    [HarmonyPostfix]
    static void GSD1_CheckButtonWarPost(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadData = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.Ws_serif_c), nameof(GSD1.Ws_serif_c.WriteSerifuWindow))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.WriteSerifuWindow))]
    [HarmonyPrefix]
    static void GSD1_CheckButton2(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;
        sysWork.PadTrig |= 0x20; // simulate a Confirm input
    }

    [HarmonyPatch(typeof(GSD1.Ws_serif_c), nameof(GSD1.Ws_serif_c.WriteSerifuWindow))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.WriteSerifuWindow))]
    [HarmonyPostfix]
    static void GSD1_CheckButton2Post(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadTrig = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.D_azukar_c), nameof(GSD1.D_azukar_c.azukari_DispKaiwaBuff))]
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.DispKaiwaBuff))]
    [HarmonyPatch(typeof(GSD1.G1_i_main_c), nameof(GSD1.G1_i_main_c.i_DispKaiwaBuff))]
    [HarmonyPrefix]
    static void GSD1_KaiwaBuffer(GSD1.KAIWA_BUFFER kaiwa_buff)
    {
        if (kaiwa_buff == null)
        {
            return;
        }

        kaiwa_buff.timer = 1; // Decremented first
    }

    [HarmonyPatch(typeof(GSD1.G1_i_main_c), nameof(GSD1.G1_i_main_c.i_disp_window))]
    [HarmonyPrefix]
    static void GSD1_Battle1v1(GSD1.G1_i_main_c __instance)
    {
        var kaiwaBuffer = __instance.ikki_work?.kaiwa;
        if (kaiwaBuffer == null)
        {
            return;
        }

        kaiwaBuffer.timer = 1; // Decremented first
    }
}
