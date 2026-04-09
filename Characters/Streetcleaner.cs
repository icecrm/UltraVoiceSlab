using HarmonyLib;
using UnityEngine;
using UltraVoice.Utilities;

namespace UltraVoice.Characters
{
    public class StreetcleanerCharacter
    {
        // Voice line storage
        public static AudioClip[] ChatterClips;
        public static AudioClip[] AttackClips;
        public static AudioClip[] ParryClips;

        // Subtitle storage
        public static readonly string[] ChatterSubs =
        {
            "IMPURITY",
            "SANITIZE",
            "EXTERMINATE",
            "PURGE",
            "UNCLEAN"
        };

        public static readonly string[] AttackSubs =
        {
            "CLEANSING",
            "PURIFYING",
            "TO ASH",
            "QUIT MOVING",
            "STOP RESISTING"
        };

        public static readonly string[] ParrySubs =
        {
            "DENIED",
            "DEFLECTED",
            "HA HA"
        };

        public static void LoadVoiceLines(AssetBundle bundle, BepInEx.Logging.ManualLogSource logger)
        {
            ChatterClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "sc_Chatter1"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Chatter2"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Chatter3"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Chatter4"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Chatter5")
            };

            AttackClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "sc_Attack1"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Attack2"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Attack3"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Attack4"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Attack5")
            };

            ParryClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "sc_Parry1"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Parry2"),
                UltraVoicePlugin.LoadClip(bundle, "sc_Parry3"),
            };

            logger.LogInfo("Streetcleaner voice lines loaded successfully!");
        }

        public static void PlayRandomVoice(UnityEngine.Component streetcleaner, AudioClip[] clips, string[] subs, bool interrupt = false, UnityEngine.Color? colorOverride = null)
        {
            if (clips == null || clips.Length == 0)
                return;

            int i = Random.Range(0, clips.Length);
            string sub = null;

            if (subs != null && i < subs.Length)
                sub = subs[i];

            VoiceManager.CreateVoiceSource(streetcleaner, "Streetcleaner", clips[i], sub, interrupt, colorOverride);
        }
    }

    // STREETCLEANER PATCHES

    [HarmonyPatch(typeof(Streetcleaner), "Start")]
    class StreetcleanerSpawnTrackPatch
    {
        static void Postfix(Streetcleaner __instance)
        {
            VoiceManager.enemySpawnTimes[__instance] = Time.time;
        }
    }

    [HarmonyPatch(typeof(Streetcleaner), "Update")]
    class StreetcleanerChatterPatch
    {
        static void Postfix(Streetcleaner __instance)
        {
            if (!UltraVoicePlugin.StreetcleanerVoiceEnabled.value) return;

            if (__instance == null)
                return;

            if (__instance.dead)
                return;

            if (!VoiceManager.CheckCooldown(__instance, 4f))
                return;

            if (VoiceManager.TooSoonAfterSpawn(__instance, 1f))
                return;

            if (Random.Range(0f, 1f) > 0.75)
                StreetcleanerCharacter.PlayRandomVoice(
                    __instance,
                    StreetcleanerCharacter.ChatterClips,
                    StreetcleanerCharacter.ChatterSubs
                );
        }
    }

    [HarmonyPatch(typeof(Streetcleaner), "StartFire")]
    class StreetcleanerFlameAttackPatch
    {
        static void Postfix(Streetcleaner __instance)
        {
            if (!UltraVoicePlugin.StreetcleanerVoiceEnabled.value) return;

            if (Random.Range(0f, 1f) > 0.75)
                StreetcleanerCharacter.PlayRandomVoice(
                    __instance,
                    StreetcleanerCharacter.AttackClips,
                    StreetcleanerCharacter.AttackSubs
                );
        }
    }

    [HarmonyPatch(typeof(Streetcleaner), "DeflectShot")]
    class StreetcleanerParryPatch
    {
        static void Postfix(Streetcleaner __instance)
        {
            if (!UltraVoicePlugin.StreetcleanerVoiceEnabled.value) return;

            if (!VoiceManager.CheckCooldown(__instance, 4f))
                return;

            StreetcleanerCharacter.PlayRandomVoice(
                __instance,
                StreetcleanerCharacter.ParryClips,
                StreetcleanerCharacter.ParrySubs,
                interrupt: true
            );
        }
    }

    [HarmonyPatch(typeof(Streetcleaner), "OnGoLimp")]
    class StreetcleanerDeathInterruptPatch
    {
        static void Postfix(Streetcleaner __instance)
        {
            VoiceManager.InterruptVoices(__instance);
        }
    }
}