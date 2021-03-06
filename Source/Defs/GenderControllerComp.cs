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
using JetBrains.Annotations;
using Verse;

namespace SirRandoo.NonBinary.Defs
{
    [UsedImplicitly]
    public class GenderControllerComp : ThingComp
    {
        private bool _casted;
        private Gizmo _gizmo;
        private List<FloatMenuOption> _options;
        private Pawn _pawn;

        private bool _wasGenderSwapped;
        private Gender _initialGender = Gender.Male;

        [CanBeNull]
        private Pawn Pawn
        {
            get
            {
                if (_casted)
                {
                    return _pawn;
                }

                _pawn = parent as Pawn;
                _casted = true;

                return _pawn;
            }
        }

        public Gender InitialGender => _initialGender;

        [ItemNotNull]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!IsValidTarget() || !Prefs.DevMode)
            {
                yield break;
            }

            yield return _gizmo ??= new Command_Action
            {
                action = ShowGenderOptions,
                defaultLabel = "NonBinary.Gizmos.ChangeGender.Label".TranslateSimple(),
                defaultDesc = "NonBinary.Gizmos.ChangeGender.Description".TranslateSimple(),
                groupKey = 0
            };
        }

        private void ShowGenderOptions()
        {
            _options ??= new List<FloatMenuOption>
            {
                new FloatMenuOption("Male".TranslateSimple().CapitalizeFirst(), () => ChangeGender(Gender.Male)),
                new FloatMenuOption("Female".TranslateSimple().CapitalizeFirst(), () => ChangeGender(Gender.Female)),
                new FloatMenuOption("None".TranslateSimple().CapitalizeFirst(), () => ChangeGender(Gender.None))
            };

            Find.WindowStack.Add(new FloatMenu(_options));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Pawn == null || !Pawn.RaceProps.Humanlike || !(Pawn is { IsColonist: true }))
            {
                return;
            }

            Scribe_Values.Look(ref _initialGender, "initialGender");
        }

        private bool IsValidTarget()
        {
            if (Pawn == null)
            {
                return false;
            }

            return Pawn.RaceProps.Humanlike && Pawn is { IsColonist: true };
        }

        internal void ChangeGender(Gender gender)
        {
            if (!_wasGenderSwapped)
            {
                _wasGenderSwapped = true;
                _initialGender = Pawn!.gender;
            }

            Pawn!.gender = gender;
        }

        internal void ValidateSettings()
        {
            if (TryCorrectForAndroids())
            {
                return;
            }
        }
        private bool TryCorrectForAndroids()
        {
            if (!Registry.AndroidTiersActive) // Gynoids depends on Android Tiers
            {
                return false;
            }

            switch (_initialGender)
            {
                case Gender.Female when !Registry.GynoidsActive:
                    _initialGender = Gender.Male;

                    return true;
                default:
                    return false;
            }
        }
    }
}
