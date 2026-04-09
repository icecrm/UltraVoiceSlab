using HarmonyLib;
using UnityEngine;
using UltraVoice.Utilities;

namespace UltraVoice.Characters
{
    public class MannequinCharacter
    {
        // Voice line storage
        public static AudioClip[] ChatterClips;
        public static AudioClip[] DeathClips;

        public static void LoadVoiceLines(AssetBundle bundle, BepInEx.Logging.ManualLogSource logger)
        {
            ChatterClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mq_Laugh1"),
                UltraVoicePlugin.LoadClip(bundle, "mq_Laugh2"),
                UltraVoicePlugin.LoadClip(bundle, "mq_Laugh3"),
                UltraVoicePlugin.LoadClip(bundle, "mq_Laugh4"),
                UltraVoicePlugin.LoadClip(bundle, "mq_Laugh5")
            };

            DeathClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mq_Death1"),
                UltraVoicePlugin.LoadClip(bundle, "mq_Death2"),
            };

            logger.LogInfo("Mannequin voice lines loaded successfully!");
        }

        public static void PlayRandomVoice(UnityEngine.Component mannequin, AudioClip[] clips, string[] subs, bool interrupt = false, UnityEngine.Color? colorOverride = null)
        {
            if (clips == null || clips.Length == 0)
                return;

            int i = Random.Range(0, clips.Length);
            string sub = null;

            if (subs != null && i < subs.Length)
                sub = subs[i];

            VoiceManager.CreateVoiceSource(mannequin, "Mannequin", clips[i], sub, interrupt, colorOverride);
        }
    }

    // MANNEQUIN PATCHES

    [HarmonyPatch(typeof(Mannequin), "Update")]
    class MannequinChatterPatch
    {
        static void Postfix(Mannequin __instance)
        {
            if (!UltraVoicePlugin.MannequinVoiceEnabled.value) return;

            if (ULTRAKILL.Cheats.BlindEnemies.Blind)
                return;

            if (!VoiceManager.CheckCooldown(__instance, 4f))
                return;

            if (Random.Range(0f, 1f) > 0.75f)
                return;

            MannequinCharacter.PlayRandomVoice(
                __instance,
                MannequinCharacter.ChatterClips,
                null
            );
        }
    }

    [HarmonyPatch(typeof(Mannequin), "MeleeAttack")]
    class MannequinSwingPatch
    {
        static void Postfix(Mannequin __instance)
        {
            if (!UltraVoicePlugin.MannequinVoiceEnabled.value) return;

            MannequinCharacter.PlayRandomVoice(
                __instance,
                MannequinCharacter.ChatterClips,
                null
            );
        }
    }

    [HarmonyPatch(typeof(Mannequin), "OnDeath")]
    class MannequinDeathPatch
    {
        static void Postfix(Mannequin __instance)
        {
            if (!UltraVoicePlugin.MannequinVoiceEnabled.value) return;

            MannequinCharacter.PlayRandomVoice(
                __instance,
                MannequinCharacter.DeathClips,
                null,
                true
            );
        }
    }
}