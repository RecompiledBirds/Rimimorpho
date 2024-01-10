using HarmonyLib;
using RVCRestructured.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    internal class TransformationSelectionWindow : Window
    {
        private const float bttnHeight = 45f;
        private const float bttnDescHeight = 16f;
        private const float margin = 5f;

        private static readonly Vector2 initSize = new Vector2(860, 540);

        private static readonly Rect window = new Rect(new Vector2(), initSize);
        private static readonly Rect windowMargin = window.ContractedBy(5f);
        private static readonly Rect topBarRect = new Rect(windowMargin.position, new Vector2(windowMargin.width, 26f));
        private static readonly Rect titleRect = new Rect(topBarRect.x, topBarRect.y, topBarRect.width - topBarRect.height - 4f, topBarRect.height);
        private static readonly Rect closBttnRect = new Rect(topBarRect.xMax - topBarRect.height, topBarRect.y, topBarRect.height, topBarRect.height + 1f);


        private readonly Pawn pawn;
        private readonly AmphiShifter pawnAmphiShifter;
        private readonly List<StoredRace> storedPawnRaces;

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
            
            onlyOneOfTypeAllowed = true;

            originalFont = Text.Font;
            originalColor = GUI.color;

            pawnAmphiShifter = pawn.TryGetComp<AmphiShifter>();
            storedPawnRaces = pawnAmphiShifter?.knownRaces;
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
            for (int i = 0; i < storedPawnRaces.Count; i++)
            {
                Rect tmpBttnRect = new Rect(0f, (bttnHeight + margin) * i, BaseSpeciesBttnSize.x, BaseSpeciesBttnSize.y);
                Rect descLabelRect = new Rect(0f, tmpBttnRect.yMax - bttnDescHeight, tmpBttnRect.width, bttnDescHeight);
                StoredRace currentRace = storedPawnRaces[i];

                Widgets.DrawHighlightIfMouseover(tmpBttnRect);
                Widgets.DrawTextHighlight(descLabelRect);

                Widgets.DefIcon(tmpBttnRect.LeftPartPixels(tmpBttnRect.height), currentRace.ThingDef);
                Text.Font = GameFont.Tiny;
                Widgets.Label(descLabelRect.MoveRect(new Vector2(margin + tmpBttnRect.height, 0f)), currentRace?.ThingDef?.LabelCap ?? "<color=red>ThingDef Missing</color>");
                if (Widgets.ButtonInvisible(tmpBttnRect))
                {
                    selectedRace = (i != selectedRace) ? i : -1;
                }
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
            ResetFontAndColor();
            if (Widgets.CloseButtonFor(closBttnRect)) Close();
        }
    }
}
