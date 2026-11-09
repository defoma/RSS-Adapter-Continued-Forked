using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace FreeIvaSeatScaleFix
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Loader : MonoBehaviour
    {
        void Awake()
        {
            try
            {
                Debug.Log("[FreeIvaSeatScaleFix] " + SeatPatch.Apply());
            }
            catch (Exception e)
            {
                Debug.LogError("[FreeIvaSeatScaleFix] Patch failed, kerbals will still grow on re-seat: " + e);
            }
        }
    }

    public static class SeatPatch
    {
        const string TargetType = "FreeIva.KerbalIvaAddon";
        const string TargetMethod = "MoveKerbalToSeat";

        public static string Apply()
        {
            Type type = AccessTools.TypeByName(TargetType);
            if (type == null) return "FreeIVA not installed, nothing to patch";

            MethodInfo target = AccessTools.Method(type, TargetMethod);
            if (target == null) throw new MissingMethodException(TargetType, TargetMethod);

            new Harmony("FreeIvaSeatScaleFix").Patch(target,
                new HarmonyMethod(typeof(SeatPatch), nameof(Prefix)),
                new HarmonyMethod(typeof(SeatPatch), nameof(Postfix)));
            return "Patched " + TargetType + "." + TargetMethod;
        }

        public static void Prefix(ProtoCrewMember crewMember, out Vector3 __state)
        {
            __state = crewMember?.seat != null ? crewMember.seat.kerbalScale : Vector3.one;
        }

        public static void Postfix(ProtoCrewMember crewMember, Vector3 __state)
        {
            Kerbal kerbal = crewMember?.KerbalRef;
            if (kerbal == null || __state.x == 0f || __state.y == 0f || __state.z == 0f) return;

            Transform t = kerbal.transform;
            Vector3 s = t.localScale;
            t.localScale = new Vector3(s.x / __state.x, s.y / __state.y, s.z / __state.z);

            Debug.Log("[FreeIvaSeatScaleFix] " + crewMember.name + " re-seated: world scale " + t.lossyScale.ToString("F3"));
        }
    }
}
