// MIT License
//
// Copyright (c) 2021 SirRandoo
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using SirRandoo.NonBinary.Defs;
using Verse;

namespace SirRandoo.NonBinary.Patches
{
    [HarmonyPatch]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class ResolveAllGraphicsPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics");
        }

        [HarmonyPriority(Priority.First)]
        public static void Prefix([CanBeNull] PawnGraphicSet __instance, out Gender __state)
        {
            var comp = __instance?.pawn?.TryGetComp<GenderControllerComp>();

            if (comp == null)
            {
                __state = __instance?.pawn?.gender ?? Gender.Male;
                return;
            }

            __state = __instance.pawn.gender;
            __instance.pawn.gender = comp.InitialGender;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix([CanBeNull] PawnGraphicSet __instance, Gender __state)
        {
            if (__instance?.pawn == null)
            {
                return;
            }

            __instance.pawn.gender = __state;
        }
    }
}
