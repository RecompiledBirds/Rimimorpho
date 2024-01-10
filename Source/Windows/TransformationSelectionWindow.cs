using HarmonyLib;
using RimWorld;
using RVCRestructured.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimimorpho
{
    internal class TransformationSelectionWindow : Window
    {
        private const float bttnHeight = 45f;
        private const float bttnDescHeight = 18f;
        private const float margin = 5f;

        private static readonly Vector2 initSize = new Vector2(800, 440);

        private static readonly Rect window = new Rect(new Vector2(), initSize);
        private static readonly Rect windowMargin = window.ContractedBy(5f);
        private static readonly Rect topBarRect = new Rect(windowMargin.position, new Vector2(windowMargin.width, 26f));
        private static readonly Rect titleRect = new Rect(topBarRect.x, topBarRect.y, topBarRect.width - topBarRect.height - 4f, topBarRect.height);
        private static readonly Rect closBttnRect = new Rect(topBarRect.xMax - topBarRect.height, topBarRect.y, topBarRect.height, topBarRect.height + 1f);

        private readonly Pawn pawn;
        private readonly AmphiShifter pawnAmphiShifter;
        private readonly List<ThingDef> storedThingDefs;
        private readonly Dictionary<ThingDef, RaceList<StoredRace>> storedPawnRaces;

        private readonly Color originalColor;
        private readonly GameFont originalFont;
        
        private Rect scrollAreaRect = new Rect(windowMargin.x, windowMargin.y + topBarRect.height + margin, windowMargin.width, windowMargin.height - margin - topBarRect.height);
        private Rect scrollAreaInnerRect = new Rect();
        private Vector2 scrollHeight = new Vector2();

        private int selectedRace = -1;

        private Vector2 BaseSpeciesBttnSize => new Vector2(scrollAreaInnerRect.size.x, bttnHeight);


        public TransformationSelectionWindow(Pawn pawn) 
        {
            this.pawn = pawn;
            Log.Message("TexButton.Collapse height: " + TexButton.Collapse.height.ToString());
            onlyOneOfTypeAllowed = true;

            originalFont = Text.Font;
            originalColor = GUI.color;

            pawnAmphiShifter = pawn.TryGetComp<AmphiShifter>();
            storedPawnRaces = pawnAmphiShifter?.knownSpecies;
            storedThingDefs = storedPawnRaces?.Keys.ToList();
            scrollAreaInnerRect = scrollAreaRect.GetInnerScrollRect((bttnHeight + margin) * storedPawnRaces.Count);
            Log.Message(storedPawnRaces.Join((x) => x.ToString()));
        }

        public override Vector2 InitialSize => initSize;

        protected override float Margin => 0f;

        private void ResetFontAndColor()
        {
            Text.Font = originalFont;
            GUI.color = originalColor;
        }

        //TODO: Translations strings
        public override void DoWindowContents(Rect _)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, $"Select Transformation:");
            Widgets.BeginScrollView(scrollAreaRect, ref scrollHeight, scrollAreaInnerRect);
            Widgets.BeginGroup(scrollAreaRect);
            
            float yOffset = 0f;
            for (int i = 0; i < storedThingDefs.Count; i++)
            {
                bool thisIsSelectedRace = i == selectedRace;
                Rect tmpBttnRect = GetTmpBttnRect(i);
                Rect descLabelRect = new Rect(margin * 2f + tmpBttnRect.height + bttnDescHeight, tmpBttnRect.yMax - bttnDescHeight, tmpBttnRect.width, bttnDescHeight);
                Rect expandImageRect = new Rect(descLabelRect) { x = margin + tmpBttnRect.height, width = descLabelRect.height };
                ThingDef currentThingDef = storedThingDefs[i];

                Widgets.DrawHighlightIfMouseover(tmpBttnRect);
                Widgets.DrawTextHighlight(descLabelRect);

                Widgets.DefIcon(tmpBttnRect.LeftPartPixels(tmpBttnRect.height), currentThingDef);
                Text.Font = GameFont.Tiny;
                Widgets.Label(descLabelRect, currentThingDef.LabelCap);
                Widgets.DrawTextureFitted(expandImageRect, thisIsSelectedRace ? TexButton.Collapse : TexButton.Reveal, 1f);
                if (Widgets.ButtonInvisible(tmpBttnRect))
                {
                    if (thisIsSelectedRace)
                    {
                        selectedRace = -1;
                        scrollAreaInnerRect = scrollAreaRect.GetInnerScrollRect((bttnHeight + margin) * storedPawnRaces.Count);
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }
                    else
                    {
                        selectedRace = i;
                        scrollAreaInnerRect = scrollAreaRect.GetInnerScrollRect((bttnHeight + margin) * (storedPawnRaces.Count + storedPawnRaces[currentThingDef].Length));
                        SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    }
                }

                if (thisIsSelectedRace)
                {
                    for (int j = 0; j < storedPawnRaces[currentThingDef].Length; j++)
                    {
                        yOffset += bttnHeight;
                        Rect tmpSubBttnRect = GetTmpBttnRect(i, 45f);
                        Widgets.DrawBox(tmpSubBttnRect);
                    }
                }
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
            ResetFontAndColor();
            if (Widgets.CloseButtonFor(closBttnRect)) Close();

            Rect GetTmpBttnRect(int i, float xOffset = 0f)
            {
                return new Rect(xOffset, (bttnHeight + margin) * i + yOffset, BaseSpeciesBttnSize.x - xOffset, BaseSpeciesBttnSize.y);
            }
        }
    }
}
