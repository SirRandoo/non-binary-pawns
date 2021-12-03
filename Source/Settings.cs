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

using System;
using SirRandoo.NonBinary.Helpers;
using UnityEngine;
using Verse;

namespace SirRandoo.NonBinary
{
    public class Settings : ModSettings
    {
        public static float NonBinaryChance = 0.1f;
        private static string _nonBinaryChanceBuffer;

        public static void Draw(Rect region)
        {
            _nonBinaryChanceBuffer ??= (NonBinaryChance * 100f).ToString("N");

            GUI.BeginGroup(region);

            var listing = new Listing_Standard();

            listing.Begin(region.AtZero());

            (Rect label, Rect field) = listing.GetRectAsForm();
            SettingsHelper.DrawLabel(label, "NonBinary.Chance".TranslateSimple());

            if (SettingsHelper.DrawTextField(field, _nonBinaryChanceBuffer, out string result) && float.TryParse(result.TrimEnd('%'), out float value))
            {
                NonBinaryChance = value / 100f;
            }

            SettingsHelper.DrawFieldButton(field, "%");

            listing.End();

            GUI.EndGroup();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref NonBinaryChance, "nonBinaryChance", 0.1f);
        }
    }
}
