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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace SirRandoo.NonBinary.Helpers
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class SettingsHelper
    {
        private const float PriceButtonWidth = 50f;
        private static readonly FieldInfo SelectedModField;
        private static readonly GameFont[] GameFonts;

        static SettingsHelper()
        {
            GameFonts = Enum.GetNames(typeof(GameFont))
               .Select(f => (GameFont)Enum.Parse(typeof(GameFont), f))
               .OrderByDescending(f => (int)f)
               .ToArray();
            SelectedModField = AccessTools.Field(typeof(Dialog_ModSettings), "selMod");
        }

        public static bool DrawFieldButton(Rect region, string label, [CanBeNull] string tooltip = null)
        {
            var buttonRegion = new Rect(region.x + region.width - 16f, region.y, 16f, region.height);
            Widgets.ButtonText(buttonRegion, label, false);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(buttonRegion, tooltip);
            }

            bool wasClicked = Event.current.type == EventType.Used && Input.GetMouseButtonDown(0);
            bool shouldTrigger = Mouse.IsOver(buttonRegion) && wasClicked;

            if (!shouldTrigger)
            {
                return false;
            }

            GUIUtility.keyboardControl = 0;
            return true;
        }

        public static bool DrawFieldButton(Rect region, Texture2D icon, [CanBeNull] string tooltip = null)
        {
            var buttonRegion = new Rect(
                region.x + region.width - region.height + 6f,
                region.y + 6f,
                region.height - 12f,
                region.height - 12f
            );
            Widgets.ButtonImage(buttonRegion, icon);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(buttonRegion, tooltip);
            }

            bool wasClicked = Event.current.type == EventType.Used && Input.GetMouseButtonDown(0);
            bool shouldTrigger = Mouse.IsOver(buttonRegion) && wasClicked;

            if (!shouldTrigger)
            {
                return false;
            }

            GUIUtility.keyboardControl = 0;
            return true;
        }

        public static bool DrawClearButton(Rect region)
        {
            return DrawFieldButton(region, "×");
        }

        public static void DrawPriceField(Rect region, ref int price)
        {
            var reduceRect = new Rect(region.x, region.y, PriceButtonWidth, region.height);
            var raiseRect = new Rect(
                region.x + region.width - PriceButtonWidth,
                region.y,
                PriceButtonWidth,
                region.height
            );
            var fieldRect = new Rect(
                region.x + PriceButtonWidth + 2f,
                region.y,
                region.width - PriceButtonWidth * 2 - 4f,
                region.height
            );
            var buffer = price.ToString();
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);


            switch (control)
            {
                case true when shift:
                    buffer = DrawControlShiftPriceButtons(ref price, reduceRect, buffer, raiseRect);
                    break;
                case true:
                    buffer = DrawControlPriceButtons(ref price, reduceRect, buffer, raiseRect);
                    break;
                default:
                {
                    buffer = shift
                        ? DrawShiftPriceButtons(ref price, reduceRect, buffer, raiseRect)
                        : DrawBasePriceButtons(ref price, reduceRect, buffer, raiseRect);

                    break;
                }
            }


            Widgets.TextFieldNumeric(fieldRect, ref price, ref buffer, 1f);
        }

        private static string DrawControlShiftPriceButtons(
            ref int price,
            Rect reduceRegion,
            string buffer,
            Rect raiseRegion
        )
        {
            if (Widgets.ButtonText(reduceRegion, "-1000"))
            {
                price -= 1000;
                buffer = price.ToString();
            }

            if (!Widgets.ButtonText(raiseRegion, "+1000"))
            {
                return buffer;
            }

            price += 1000;
            buffer = price.ToString();

            return buffer;
        }

        private static string DrawControlPriceButtons(ref int price, Rect reduce, string buffer, Rect raise)
        {
            if (Widgets.ButtonText(reduce, "-100"))
            {
                price -= 100;
                buffer = price.ToString();
            }

            if (!Widgets.ButtonText(raise, "+100"))
            {
                return buffer;
            }

            price += 100;
            buffer = price.ToString();

            return buffer;
        }

        private static string DrawShiftPriceButtons(ref int price, Rect reduce, string buffer, Rect raise)
        {
            if (Widgets.ButtonText(reduce, "-10"))
            {
                price -= 10;
                buffer = price.ToString();
            }

            if (!Widgets.ButtonText(raise, "+10"))
            {
                return buffer;
            }

            price += 10;
            buffer = price.ToString();

            return buffer;
        }

        private static string DrawBasePriceButtons(ref int price, Rect reduce, string buffer, Rect raise)
        {
            if (Widgets.ButtonText(reduce, "-1"))
            {
                price -= 1;
                buffer = price.ToString();
            }

            if (!Widgets.ButtonText(raise, "+1"))
            {
                return buffer;
            }

            price += 1;
            buffer = price.ToString();

            return buffer;
        }

        public static bool DrawDoneButton(Rect region)
        {
            return DrawFieldButton(region, "✔");
        }

        public static void DrawShowButton(Rect region, Texture2D icon, ref bool state)
        {
            if (DrawFieldButton(region, icon))
            {
                state = !state;
            }
        }

        public static bool WasLeftClicked(this Rect region)
        {
            return WasMouseButtonClicked(region, 0);
        }

        public static bool WasRightClicked(this Rect region)
        {
            return WasMouseButtonClicked(region, 1);
        }

        public static bool WasMouseButtonClicked(this Rect region, int mouseButton)
        {
            if (!Mouse.IsOver(region))
            {
                return false;
            }

            Event current = Event.current;
            bool was = current.button == mouseButton;

            switch (current.type)
            {
                case EventType.Used when was:
                case EventType.MouseDown when was:
                    current.Use();
                    return true;
                default:
                    return false;
            }
        }

        public static Rect ShiftLeft(this Rect region, float padding = 5f)
        {
            return new Rect(region.x - region.width - padding, region.y, region.width, region.height);
        }

        public static Rect ShiftRight(this Rect region, float padding = 5f)
        {
            return new Rect(region.x + region.width + padding, region.y, region.width, region.height);
        }

        public static bool IsRegionVisible(this Rect region, Rect scrollRect, Vector2 scrollPos)
        {
            return (region.y >= scrollPos.y || region.y + region.height - 1f >= scrollPos.y)
                   && region.y <= scrollPos.y + scrollRect.height;
        }

        public static void DrawColored(this Texture2D t, Rect region, Color color)
        {
            Color old = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(region, t);
            GUI.color = old;
        }

        public static void DrawLabel(
            Rect region,
            string text,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false
        )
        {
            Text.Anchor = anchor;
            Text.Font = fontScale;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            Widgets.Label(region, text);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public static void DrawTabBackground(
            Rect region,
            Color activeColor,
            Color inactiveColor,
            bool vertical = false,
            bool selected = false
        )
        {
            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            GUI.color = selected ? activeColor : inactiveColor;
            Widgets.DrawHighlight(region);
            GUI.color = Color.white;

            if (!selected && Mouse.IsOver(region))
            {
                Widgets.DrawLightHighlight(region);
            }

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }
        }

        [ContractAnnotation("=> true,newContent:notnull; => false,newContent:null")]
        public static bool DrawTextField(Rect region, string content, [CanBeNull] out string newContent)
        {
            string text = Widgets.TextField(region, content);

            newContent = !text.Equals(content) ? text : null;
            return newContent != null;
        }

        public static bool DrawTableHeader(
            Rect backRect,
            Rect textRect,
            string text,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false
        )
        {
            Text.Anchor = anchor;
            Text.Font = fontScale;

            if (vertical)
            {
                backRect.y += backRect.width;
                GUIUtility.RotateAroundPivot(-90f, backRect.position);
            }

            GUI.color = new Color(0.62f, 0.65f, 0.66f);
            Widgets.DrawHighlight(backRect);
            GUI.color = Color.white;

            if (Mouse.IsOver(backRect))
            {
                GUI.color = Color.grey;
                Widgets.DrawLightHighlight(backRect);
                GUI.color = Color.white;
            }

            Widgets.Label(textRect, text);
            bool pressed = Widgets.ButtonInvisible(backRect);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            return pressed;
        }

        public static bool DrawTableHeader(Rect backRect, Rect iconRect, Texture2D icon, bool vertical = false)
        {
            if (vertical)
            {
                backRect.y += backRect.width;
                GUIUtility.RotateAroundPivot(-90f, backRect.position);
            }

            GUI.color = new Color(0.62f, 0.65f, 0.66f);
            Widgets.DrawHighlight(backRect);
            GUI.color = Color.white;

            if (Mouse.IsOver(backRect))
            {
                GUI.color = Color.grey;
                Widgets.DrawLightHighlight(backRect);
                GUI.color = Color.white;
            }

            GUI.DrawTexture(iconRect, icon);
            bool pressed = Widgets.ButtonInvisible(backRect);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            return pressed;
        }

        public static bool DrawTabButton(
            Rect region,
            string text,
            Color activeColor,
            Color inactiveColor,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false,
            bool selected = false
        )
        {
            Text.Anchor = anchor;
            Text.Font = fontScale;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            GUI.color = selected ? activeColor : inactiveColor;
            Widgets.DrawHighlight(region);
            GUI.color = Color.white;

            if (!selected && Mouse.IsOver(region))
            {
                Widgets.DrawLightHighlight(region);
            }

            Widgets.Label(region, text);
            bool pressed = Widgets.ButtonInvisible(region);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            return pressed;
        }

        public static void DrawColoredLabel(
            Rect region,
            string text,
            Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont fontScale = GameFont.Small,
            bool vertical = false
        )
        {
            GUI.color = color;
            DrawLabel(region, text, anchor, fontScale, vertical);
            GUI.color = Color.white;
        }

        public static void DrawFittedLabel(
            Rect region,
            string text,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            GameFont maxScale = GameFont.Small,
            bool vertical = false
        )
        {
            Text.Anchor = anchor;

            if (vertical)
            {
                region.y += region.width;
                GUIUtility.RotateAroundPivot(-90f, region.position);
            }

            var maxFontScale = (int)maxScale;
            foreach (GameFont f in GameFonts)
            {
                if ((int)f > maxFontScale)
                {
                    continue;
                }

                Text.Font = f;

                if (Text.CalcSize(text).x <= region.width)
                {
                    break;
                }
            }

            Widgets.Label(region, text);

            if (vertical)
            {
                GUI.matrix = Matrix4x4.identity;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        [NotNull]
        public static Tuple<Rect, Rect> ToForm(this Rect region, float factor = 0.8f)
        {
            float leftWidth = Mathf.FloorToInt(region.width * factor);
            var left = new Rect(region.x, region.y, leftWidth - 2f, region.height);

            return new Tuple<Rect, Rect>(
                left,
                new Rect(left.x + left.width + 2f, left.y, region.width - left.width - 2f, left.height)
            );
        }

        [NotNull]
        public static Tuple<Rect, Rect> GetRectAsForm([NotNull] this Listing listing, float factor = 0.8f)
        {
            return listing.GetRect(Text.LineHeight).ToForm(factor);
        }

        [NotNull]
        public static string Tagged(this string s, string tag)
        {
            return $"<{tag}>{s}</{tag}>";
        }

        [NotNull]
        public static string ColorTagged(this string s, string hex)
        {
            if (!hex.StartsWith("#"))
            {
                hex = $"#{hex}";
            }

            return $@"<color=""{hex}"">{s}</color>";
        }

        [NotNull]
        public static string ColorTagged(this string s, Color color)
        {
            return ColorTagged(s, ColorUtility.ToHtmlStringRGB(color));
        }

        public static void TipRegion(this Rect region, string tooltip)
        {
            TooltipHandler.TipRegion(region, tooltip);
            Widgets.DrawHighlightIfMouseover(region);
        }

        public static void OpenSettingsMenuFor(Mod modInstance)
        {
            var settings = new Dialog_ModSettings();
            SelectedModField.SetValue(settings, modInstance);

            Find.WindowStack.Add(settings);
        }

        public static void OpenSettingsMenuFor<T>() where T : Mod
        {
            var settings = new Dialog_ModSettings();
            SelectedModField.SetValue(settings, LoadedModManager.GetMod<T>());

            Find.WindowStack.Add(settings);
        }

        public static Rect TrimLeft(this Rect region, float amount)
        {
            return new Rect(region.x + amount, region.y, region.width - amount, region.height);
        }

        public static Rect WithWidth(this Rect region, float width)
        {
            return new Rect(region.x, region.y, width, region.height);
        }

        public static Rect RectForIcon(Rect region)
        {
            float shortest = Mathf.Min(region.width, region.height);
            float half = Mathf.FloorToInt(shortest / 2f);
            Vector2 center = region.center;

            return new Rect(
                Mathf.Clamp(center.x - half, region.x, region.x + region.width),
                Mathf.Clamp(center.y - half, region.y, region.y + region.height),
                shortest,
                shortest
            );
        }

        public static void DrawExperimentalNotice([NotNull] this Listing listing, Color color)
        {
            listing.DrawDescription("NonBinary.Experimental".TranslateSimple(), color);
        }

        public static void DrawDescription([NotNull] this Listing listing, string description, Color color)
        {
            GameFont fontCache = Text.Font;
            GUI.color = color;
            Text.Font = GameFont.Tiny;
            float height = Text.CalcHeight(description, listing.ColumnWidth * 0.7f);
            Rect line = listing.GetRect(height);
            DrawLabel(
                line.TrimLeft(10f).WithWidth(Mathf.FloorToInt(listing.ColumnWidth * 0.7f)),
                description,
                TextAnchor.UpperLeft,
                GameFont.Tiny
            );
            GUI.color = Color.white;
            Text.Font = fontCache;

            listing.Gap(6f);
        }

        public static void DrawGroupHeader([NotNull] this Listing listing, string heading, bool gapPrefix = true)
        {
            if (gapPrefix)
            {
                listing.Gap(Mathf.CeilToInt(Text.LineHeight * 1.25f));
            }

            DrawLabel(listing.GetRect(Text.LineHeight), heading, TextAnchor.LowerLeft, GameFont.Tiny);
            listing.GapLine(6f);
        }

        public static bool DrawCheckbox(Rect region, ref bool state)
        {
            bool proxy = state;
            float smallest = Mathf.Min(region.width, region.height);
            Widgets.Checkbox(region.position, ref proxy, smallest, paintable: true);

            bool changed = proxy != state;
            state = proxy;
            return changed;
        }

        public static bool LabeledPaintableCheckbox(Rect region, string label, ref bool state)
        {
            var labelRect = new Rect(region.x, region.y, region.width - region.height - 2f, region.height);
            var checkPos = new Vector2(region.x + region.width - region.height + 2f, region.y);
            bool proxy = state;

            DrawLabel(labelRect, label);
            Widgets.Checkbox(checkPos, ref proxy, region.height, paintable: true);

            bool changed = proxy != state;
            state = proxy;
            return changed;
        }
    }
}
