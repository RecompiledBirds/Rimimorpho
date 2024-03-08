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
using Verse.AI;
using Verse.Sound;

namespace Rimimorpho
{
    internal class TransformationSelectionWindow : Window
    {
        private const float bttnHeight = 45f;
        private const float bttnDescHeight = 18f;
        private const float margin = 5f;
        private const float subButtonXOffset = 45f;
        private const float predictionBarHeight = 28f;
        private const float barMoveAnimationSpeedMin = 0.01f;
        private const float barMoveAnimationSpeedMax = 8f;
        private static readonly Vector2 initSize = new Vector2(800, 440);

        private readonly Rect window = new Rect(new Vector2(), initSize);
        private readonly Rect windowMargin;
        private readonly Rect titleBar;
        private readonly Rect title;
        private readonly Rect closBttn;
        private readonly Rect scrollAreaRect;
        private readonly Rect predictionBarArea;

        private readonly Vector2 predictionBarBase;

        private readonly Pawn pawn;
        private readonly AmphiShifter pawnAmphiShifter;
        private readonly List<ThingDef> storedThingDefs;
        private readonly Dictionary<ThingDef, RaceList<StoredRace>> storedPawnRaces;

        private readonly Color originalColor;
        private readonly GameFont originalFont;
        private readonly TextAnchor originalAnchor;

        private Dictionary<string, float> targetVar = new Dictionary<string, float>();
        private Dictionary<(ThingDef, XenotypeDef), TransformData> tfDatas = new Dictionary<(ThingDef, XenotypeDef), TransformData>();

        private Rect scrollAreaInnerRect = new Rect();
        private Vector2 scrollHeight = new Vector2();
        private Color curFoodColor = Color.gray;
        private Color curRestColor = Color.gray;

        private TransformData transformData;
        private int selectedRace = -1;

        private Vector2 BaseSpeciesBttnSize => new Vector2(scrollAreaInnerRect.size.x, bttnHeight);


        public TransformationSelectionWindow(Pawn pawn) 
        {
            this.pawn = pawn;
            onlyOneOfTypeAllowed = true;

            originalFont = Text.Font;
            originalColor = GUI.color;
            originalAnchor = Text.Anchor;

            windowMargin = window.ContractedBy(5f);
            Vector2 predictionBarSize = new Vector2(windowMargin.width, predictionBarHeight * 2f + margin);

            titleBar = new Rect(windowMargin.position, new Vector2(windowMargin.width, 26f));
            title = new Rect(titleBar.x, titleBar.y, titleBar.width - titleBar.height - 4f, titleBar.height);
            closBttn = new Rect(titleBar.xMax - titleBar.height, titleBar.y, titleBar.height, titleBar.height + 1f);
            scrollAreaRect = new Rect(windowMargin.x, windowMargin.y + titleBar.height + margin, windowMargin.width, windowMargin.height - margin * 2f - titleBar.height - predictionBarSize.y);
            predictionBarArea = new Rect(new Vector2(windowMargin.x, scrollAreaRect.yMax + margin), predictionBarSize);

            predictionBarBase = new Vector2(windowMargin.width, predictionBarHeight);

            pawnAmphiShifter = pawn.TryGetComp<AmphiShifter>();
            storedPawnRaces = pawnAmphiShifter?.knownSpecies;
            storedThingDefs = storedPawnRaces?.Keys.ToList();
            scrollAreaInnerRect = scrollAreaRect.GetInnerScrollRect((bttnHeight + margin) * storedPawnRaces.Count);
            Log.Message(storedPawnRaces.Join((x) => x.ToString()));
        }

        public override Vector2 InitialSize => initSize;

        protected override float Margin => 0f;

        private void ResetTextAndColor()
        {
            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
            GUI.color = originalColor;
        }

        public override void DoWindowContents(Rect _)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(title, $"Select Transformation:"); //TODO: Translation string
            Widgets.BeginScrollView(scrollAreaRect, ref scrollHeight, scrollAreaInnerRect);
            Widgets.BeginGroup(scrollAreaRect);

            int highlightHeight = 0;
            float yOffset = 0f;
            for (int i = 0; i < storedThingDefs.Count; i++)
            {
                bool indexIsSelectedRace = i == selectedRace;
                Rect raceButton = GetTmpBttnRect(i);
                Rect descLabelRect = new Rect(margin * 2f + raceButton.height + bttnDescHeight, raceButton.yMax - bttnDescHeight, raceButton.width, bttnDescHeight);
                Rect expandImageRect = new Rect(descLabelRect) { x = margin + raceButton.height, width = descLabelRect.height };
                ThingDef currentThingDef = storedThingDefs[i];

                HighlightUtil.DrawHighlights(highlightHeight++, raceButton);

                Widgets.DrawHighlightIfMouseover(raceButton);
                Widgets.DrawLineHorizontal(0f, raceButton.yMax, raceButton.width);

                Widgets.DefIcon(raceButton.LeftPartPixels(raceButton.height), currentThingDef);
                Text.Font = GameFont.Tiny;
                Widgets.Label(descLabelRect, currentThingDef.LabelCap);
                Widgets.DrawTextureFitted(expandImageRect, indexIsSelectedRace ? TexButton.Collapse : TexButton.Reveal, 1f);
                DrawRaceButton(i, indexIsSelectedRace, raceButton, currentThingDef);

                if (!indexIsSelectedRace) continue;

                for (int j = 0; j < storedPawnRaces[currentThingDef].Length; j++)
                {
                    yOffset += bttnHeight;
                    Rect tmpSubBttnRect = GetTmpBttnRect(i, subButtonXOffset);

                    StoredRace race = storedPawnRaces[currentThingDef][j];

                    HighlightUtil.DrawHighlights(highlightHeight++, tmpSubBttnRect);

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    XenotypeDef xenoDef = race.XenotypeDef;
                    Widgets.Label(tmpSubBttnRect, $"Rimimorpho_TransformToLabel".Translate(GetTFLabel(currentThingDef, xenoDef).Named("LABEL")));
                    HandleMouseOver(currentThingDef, tmpSubBttnRect, xenoDef);
                    HandleSubButton(currentThingDef, tmpSubBttnRect, race, xenoDef);
                }
                ResetTextAndColor();
            }

            Widgets.EndGroup();
            Widgets.EndScrollView();
            ResetTextAndColor();
            DrawNeedsBars();

            if (Widgets.CloseButtonFor(closBttn)) Close();

            Rect GetTmpBttnRect(int i, float xOffset = 0f)
            {
                return new Rect(xOffset, (bttnHeight + margin) * i + yOffset, BaseSpeciesBttnSize.x - xOffset, BaseSpeciesBttnSize.y);
            }
        }

        private void DrawNeedsBars()
        {
            Rect foodPredictionBar = new Rect(predictionBarArea.position, predictionBarBase);
            Rect sleepPredictionBar = new Rect(foodPredictionBar) { y = foodPredictionBar.yMax + margin };

            Color targetFoodBorderColor = Color.gray;
            Color targetRestBorderColor = Color.gray;

            float foodRequirement = 0f;
            float foodOverTimeRequirement = 0f;

            float restRequirement = 0f;
            float restOverTimeRequirement = 0f;

            if (transformData != null)
            {
                foodRequirement = (float)transformData.CalculatedEnergyUsed / 2;
                foodOverTimeRequirement = transformData.PredictedTotalFoodUse;

                restRequirement = (float)transformData.CalculatedEnergyUsed / 2;
                restOverTimeRequirement = transformData.PredictedTotalRestUse;

                if (!transformData.HasEnoughFoodLeft(transformData.CalculatedWorkTicks, true)) targetFoodBorderColor = Color.red;
                if (!transformData.HasEnoughRestLeft(transformData.CalculatedWorkTicks, true)) targetRestBorderColor = Color.red;
            }

            DrawNeedsBar(foodPredictionBar, targetFoodBorderColor, ref curFoodColor, foodRequirement, foodOverTimeRequirement, pawn.needs.food);
            DrawNeedsBar(sleepPredictionBar, targetRestBorderColor, ref curRestColor, restRequirement, restOverTimeRequirement, pawn.needs.rest);

            ResetTextAndColor();
        }

        private void HandleSubButton(ThingDef currentThingDef, Rect tmpSubBttnRect, StoredRace race, XenotypeDef xenoDef)
        {
            if (Widgets.ButtonInvisible(tmpSubBttnRect))
            {
                if (tfDatas[(currentThingDef, xenoDef)].AbleToTransformFullWork)
                {
                    MakePawnTransformInto(race, xenoDef);
                }
                else
                {
                    Messages.Message($"Rimimorpho_CantTransformMessage".Translate(pawn.LabelCap.Named("PAWN"), GetTFLabel(currentThingDef, xenoDef).Named("LABEL")), MessageTypeDefOf.RejectInput, false);
                }
            }
        }

        private void HandleMouseOver(ThingDef currentThingDef, Rect tmpSubBttnRect, XenotypeDef xenoDef)
        {
            if (Mouse.IsOver(tmpSubBttnRect))
            {
                if (!tfDatas.TryGetValue((currentThingDef, xenoDef), out transformData))
                {
                    transformData = ShiftUtils.GetTransformData(pawn, pawnAmphiShifter, currentThingDef, xenoDef);
                    tfDatas.Add((currentThingDef, xenoDef), transformData);
                }

                Widgets.DrawHighlight(tmpSubBttnRect);
            }
        }

        private void DrawRaceButton(int i, bool thisIsSelectedRace, Rect tmpBttnRect, ThingDef currentThingDef)
        {
            if (Widgets.ButtonInvisible(tmpBttnRect))
            {
                if (thisIsSelectedRace)
                {
                    selectedRace = -1;
                    transformData = null;
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
        }

        private void DrawNeedsBar(Rect inRect, Color targetBorderColor, ref Color currentBorderColor, float requirement, float requirementOverTime, Need need)
        {
            inRect.SplitVertically(60f, out Rect left, out Rect right);

            currentBorderColor = currentBorderColor.MoveTowards(targetBorderColor);

            if (!targetVar.ContainsKey($"{need.LabelCap}.cur"))
            {
                targetVar.Add($"{need.LabelCap}.cur", need.CurLevel);
                targetVar.Add($"{need.LabelCap}.curReq", Mathf.Max(need.CurLevel - requirement, 0));
                targetVar.Add($"{need.LabelCap}.curTimeReq", Mathf.Max(need.CurLevel - requirementOverTime, 0));
            }
            else
            {
                targetVar[$"{need.LabelCap}.cur"] = MoveTowardsFor(targetVar[$"{need.LabelCap}.cur"], need.CurLevel);
                targetVar[$"{need.LabelCap}.curReq"] = MoveTowardsFor(targetVar[$"{need.LabelCap}.curReq"], need.CurLevel - requirement);
                targetVar[$"{need.LabelCap}.curTimeReq"] = MoveTowardsFor(targetVar[$"{need.LabelCap}.curTimeReq"], need.CurLevel - requirementOverTime);
            }

            right.DrawProjectedCurrentBar(currentBorderColor, 2, 2,
                (1f, Color.black, $"{pawn.LabelShortCap}s maximum {need.LabelCap} level", false),
                (targetVar[$"{need.LabelCap}.cur"], Color.red, $"How much {need.LabelCap} transforming would cost ({requirement})", true),
                (targetVar[$"{need.LabelCap}.curReq"], Color.yellow, $"How much {need.LabelCap} {pawn.LabelShortCap} naturally needs during the transformation ({requirementOverTime})", true),
                (targetVar[$"{need.LabelCap}.curTimeReq"], Color.green, $"How much {need.LabelCap} will be left once transforming is complete ({need.CurLevel - requirement})", false)); //TODO: Translationstring

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(left, $"{need.LabelCap}:");
        }

        private float MoveTowardsFor(float target, float current)
        {
            current = Mathf.Max(current, 0);
            return Mathf.MoveTowards(target, current, MaxDeltaFor(target, current));
        }

        private float MaxDeltaFor(float target, float current) => Mathf.Lerp(barMoveAnimationSpeedMin, barMoveAnimationSpeedMax, Mathf.Abs(target - current)) * Time.deltaTime;

        private string GetTFLabel(ThingDef thingDef, XenotypeDef xenoDef = null)
        {
            if (xenoDef == null) return thingDef.LabelCap;
            return $"{xenoDef.LabelCap} {thingDef.LabelCap}";
        }

        private void MakePawnTransformInto(StoredRace race, XenotypeDef xenotype = null)
        {
            TransformTargetJob.NextRaceTarget = race;
            TransformTargetJob.NextXenoTarget = xenotype;
            TransformTargetJob.NextBodyTypeTarget = race.BodyTypeDef ?? pawn.story.bodyType;
            pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(AmphiDefs.RimMorpho_TransformTarget));
            Close();
        }
    }
}
