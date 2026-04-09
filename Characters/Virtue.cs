using HarmonyLib;
using System.Collections;
using UnityEngine;
using UltraVoice.Utilities;

namespace UltraVoice.Characters
{
    public class VirtueCharacter
    {
        // Voice line storage
        public static AudioClip[] SpawnClips;
        public static AudioClip[] AttackClips;
        public static AudioClip[] EnrageClips;
        public static AudioClip[] DeathClips;

        // Subtitle storage
        public static readonly string[] SpawnSubs =
        {
            "It comes to this.",
            "I will do what must be done.",
            "I pray this ends swiftly.",
            "My orders have been given.",
            "Forgive me, machine."
        };

        public static readonly string[] AttackSubs =
        {
            "Why must it be this way?",
            "I take no joy in this.",
            "Do not make this harder for yourself.",
            "Must purpose demand such cruelty?",
            "I would spare you, if I could."
        };

        public static readonly string[] EnrageSubs =
        {
            "You force my hand!",
            "You leave me no choice!",
            "So be it…"
        };

        public static bool IsVirtue(Drone d)
        {
            if (d == null || d.eid == null)
                return false;

            if (d.GetComponent<DroneFlesh>() != null)
                return false;

            return d.eid.enemyType == EnemyType.Virtue;
        }

        public static void LoadVoiceLines(AssetBundle bundle, BepInEx.Logging.ManualLogSource logger)
        {
            SpawnClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "virtue_Spawn1"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Spawn2"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Spawn3"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Spawn4"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Spawn5")
            };

            AttackClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "virtue_Attack1"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Attack2"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Attack3"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Attack4"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Attack5")
            };

            EnrageClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "virtue_Enrage1"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Enrage2"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Enrage3"),
            };

            DeathClips = new AudioClip[]
            {
                UltraVoicePlugin.LoadClip(bundle, "virtue_Death1"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Death2"),
                UltraVoicePlugin.LoadClip(bundle, "virtue_Death3"),
            };

            logger.LogInfo("Virtue voice lines loaded successfully!");
        }

        public static void PlayRandomVoice(UnityEngine.Component virtue, AudioClip[] clips, string[] subs, bool interrupt = false, UnityEngine.Color? colorOverride = null)
        {
            if (clips == null || clips.Length == 0)
                return;

            int i = Random.Range(0, clips.Length);
            string sub = null;

            if (subs != null && i < subs.Length)
                sub = subs[i];

            VoiceManager.CreateVoiceSource(virtue, "Virtue", clips[i], sub, interrupt, colorOverride);
        }
    }

    // VIRTUE PATCHES

    [HarmonyPatch(typeof(Drone), "Start")]
    class VirtueSpawnPatch
    {
        static void Postfix(Drone __instance)
        {
            if (!UltraVoicePlugin.VirtueVoiceEnabled.value) return;

            if (!VirtueCharacter.IsVirtue(__instance))
                return;

            VoiceManager.enemySpawnTimes[__instance] = Time.time;

            VirtueCharacter.PlayRandomVoice(
                __instance,
                VirtueCharacter.SpawnClips,
                VirtueCharacter.SpawnSubs
            );
        }
    }

    [HarmonyPatch(typeof(VirtueInsignia), "Activating")]
    class VirtueAttackPatch
    {
        static void Postfix(VirtueInsignia __instance)
        {
            if (!UltraVoicePlugin.VirtueVoiceEnabled.value) return;

            Drone drone = null;

            if (__instance.parentEnemy)
                __instance.parentEnemy.TryGetComponent(out drone);
            else if (__instance.parentDrone)
                drone = __instance.parentDrone;

            if (drone == null || !VirtueCharacter.IsVirtue(drone))
                return;

            if (drone.isEnraged == true)
                return;

            if (!VoiceManager.CheckCooldown(drone, 4f))
                return;

            if (VoiceManager.TooSoonAfterSpawn(drone, 0.5f))
                return;
            
            if (Random.Range(0f, 1f) > 0.5f)
            VirtueCharacter.PlayRandomVoice(
                    drone,
                    VirtueCharacter.AttackClips,
                    VirtueCharacter.AttackSubs
            );
        }
    }

    [HarmonyPatch(typeof(Drone), "Enrage")]
    class VirtueEnragePatch
    {
        static void Postfix(Drone __instance)
        {
            if (!UltraVoicePlugin.VirtueVoiceEnabled.value) return;

            if (!VirtueCharacter.IsVirtue(__instance))
                return;

            UltraVoicePlugin.Instance.StartCoroutine(PlayEnrage(__instance));
        }

        static IEnumerator PlayEnrage(Drone drone)
        {
            yield return new WaitForSeconds(0.1f);

            VirtueCharacter.PlayRandomVoice(
                drone,
                VirtueCharacter.EnrageClips,
                VirtueCharacter.EnrageSubs,
                interrupt: true
            );
        }
    }

    [HarmonyPatch(typeof(Drone), "Death")]
    class VirtueDeathPatch
    {
        static void Postfix(Drone __instance)
        {
            if (!UltraVoicePlugin.VirtueVoiceEnabled.value) return;

            if (!VirtueCharacter.IsVirtue(__instance))
                return;

            UltraVoicePlugin.Instance.StartCoroutine(PlayDeath(__instance));
        }

        static IEnumerator PlayDeath(Drone drone)
        {
            yield return new WaitForSeconds(0f);

            VirtueCharacter.PlayRandomVoice(
                drone,
                VirtueCharacter.DeathClips,
                null,
                interrupt: true
            );
        }
    }
}