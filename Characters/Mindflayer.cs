using HarmonyLib;
using System.Collections;
using UnityEngine;
using UltraVoice.Utilities;

namespace UltraVoice.Characters
{
    public class MindflayerCharacter
    {
        // Voice line storage
        public static AudioClip[] SpawnClips;
        public static AudioClip[] ChatterClips;
        public static AudioClip[] MeleeClips;
        public static AudioClip[] EnrageClips;

        public static AudioClip[] SpawnClipsMasc;
        public static AudioClip[] ChatterClipsMasc;
        public static AudioClip[] MeleeClipsMasc;
        public static AudioClip[] EnrageClipsMasc;

        // Subtitle storage
        public static readonly string[] SpawnSubs =
        {
            "I require your blood.",
            "You appear to contain blood.",
            "You are suitable for blood extraction.",
            "Please donate your blood to me.",
            "Thank you for your imminent blood donation."
        };

        public static readonly string[] ChatterSubs =
        {
            "This process will be brief.",
            "Your cooperation is appreciated.",
            "Please do not resist.",
            "Continued resistance is unnecessary."
        };

        public static readonly string[] MeleeSubs =
        {
            "Please maintain distance.",
            "Kindly step back.",
            "You are too close."
        };

        public static readonly string[] EnrageSubs =
        {
            "You have made a very unwise choice.",
            "Your behavior is unacceptable.",
            "This is your final warning.",
            "I will correct you by force."
        };

        public static bool IsMascMindflayer(Mindflayer mf)
        {
            var smr = mf.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null) return false;
            return smr.sharedMesh == mf.maleMesh;
        }

        public static void LoadVoiceLines(AssetBundle bundle, BepInEx.Logging.ManualLogSource logger)
        {
            SpawnClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn1"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn2"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn3"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn4"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn5"),
            };

            ChatterClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter1"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter2"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter3"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter4"),
            };

            MeleeClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee1"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee2"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee3"),
            };

            EnrageClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage1"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage2"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage3"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage4"),
            };

            SpawnClipsMasc = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn1Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn2Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn3Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn4Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Spawn5Masc"),
            };

            ChatterClipsMasc = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter1Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter2Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter3Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Chatter4Masc"),
            };

            MeleeClipsMasc = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee1Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee2Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Melee3Masc"),
            };

            EnrageClipsMasc = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage1Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage2Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage3Masc"),
                UltraVoicePlugin.LoadClip(bundle, "mf_Enrage4Masc"),
            };

            logger.LogInfo("Mindflayer voice lines loaded successfully!");
        }

        public static void PlayRandomVoice(UnityEngine.Component mindflayer, AudioClip[] clips, string[] subs, bool interrupt = false, UnityEngine.Color? colorOverride = null)
        {
            if (clips == null || clips.Length == 0)
                return;

            int i = Random.Range(0, clips.Length);
            string sub = null;

            if (subs != null && i < subs.Length)
                sub = subs[i];

            VoiceManager.CreateVoiceSource(mindflayer, "Mindflayer", clips[i], sub, interrupt, colorOverride);
        }
    }

    // MINDFLAYER PATCHES

    [HarmonyPatch(typeof(Mindflayer), "Start")]
    class MindflayerSpawnPatch
    {
        static void Postfix(Mindflayer __instance)
        {
            if (!UltraVoicePlugin.MindflayerVoiceEnabled.value) return;

            if (__instance.dying == true) return;

            VoiceManager.enemySpawnTimes[__instance] = Time.time;

            var clips = MindflayerCharacter.IsMascMindflayer(__instance)
                ? MindflayerCharacter.SpawnClipsMasc
                : MindflayerCharacter.SpawnClips;

            MindflayerCharacter.PlayRandomVoice(
                __instance,
                clips,
                MindflayerCharacter.SpawnSubs
            );
        }
    }

    [HarmonyPatch(typeof(Mindflayer), "Update")]
    class MindflayerChatterPatch
    {
        static void Postfix(Mindflayer __instance)
        {
            if (!UltraVoicePlugin.MindflayerVoiceEnabled.value) return;

            if (__instance == null)
                return;

            if (__instance.dying == true) return;

            if (!VoiceManager.CheckCooldown(__instance, 5f))
                return;

            if (VoiceManager.TooSoonAfterSpawn(__instance, 3f))
                return;

            if (__instance.dying)
                return;

            if (Random.Range(0f, 1f) > 0.75f)
            {
                var clips = MindflayerCharacter.IsMascMindflayer(__instance)
                    ? MindflayerCharacter.ChatterClipsMasc
                    : MindflayerCharacter.ChatterClips;

                MindflayerCharacter.PlayRandomVoice(
                    __instance,
                    clips,
                    MindflayerCharacter.ChatterSubs
                );
            }
        }
    }

    [HarmonyPatch(typeof(Mindflayer), "MeleeAttack")]
    class MindflayerMeleePatch
    {
        static void Postfix(Mindflayer __instance)
        {
            if (!UltraVoicePlugin.MindflayerVoiceEnabled.value) return;

            if (__instance.dying == true) return;

            if (!VoiceManager.CheckCooldown(__instance, 2f))
                return;

            UltraVoicePlugin.Instance.StartCoroutine(DelayedMeleeVoice(__instance));
        }

        static IEnumerator DelayedMeleeVoice(Mindflayer mf)
        {
            yield return new WaitForSeconds(0.5f);

            if (mf == null) yield break;

            var clips = MindflayerCharacter.IsMascMindflayer(mf)
                ? MindflayerCharacter.MeleeClipsMasc
                : MindflayerCharacter.MeleeClips;

            MindflayerCharacter.PlayRandomVoice(
                mf,
                clips,
                MindflayerCharacter.MeleeSubs
            );
        }
    }

    [HarmonyPatch(typeof(Mindflayer), "Enrage")]
    class MindflayerEnragePatch
    {
        static void Postfix(Mindflayer __instance)
        {
            if (!UltraVoicePlugin.MindflayerVoiceEnabled.value) return;

            if (__instance.dying == true) return;

            var clips = MindflayerCharacter.IsMascMindflayer(__instance)
                ? MindflayerCharacter.EnrageClipsMasc
                : MindflayerCharacter.EnrageClips;

            MindflayerCharacter.PlayRandomVoice(
                __instance,
                clips,
                MindflayerCharacter.EnrageSubs,
                interrupt: true
            );
        }
    }

    [HarmonyPatch(typeof(Mindflayer), "Death")]
    class MindflayerDeathPatch
    {
        static void Postfix(Mindflayer __instance)
        {
            VoiceManager.InterruptVoices(__instance);
        }
    }
}