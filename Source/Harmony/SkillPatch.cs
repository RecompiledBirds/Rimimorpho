using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    public static class SkillPatch
    {
        public static bool Prefix(Pawn p, Vector2 offset, SkillUI.SkillDrawMode mode, Rect container)
        {
            if (p.def == AmphiDefs.RimMorpho_Amphimorpho)
            {
                container.height += 30f;
                container.y -= 30f;
            }
            float levelLabelWidth = (float)typeof(SkillUI).GetField("levelLabelWidth", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            List<SkillDef> skillDefsInOrder= (List<SkillDef>)typeof(SkillUI).GetField("skillDefsInListOrderCached", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Text.Font = GameFont.Small;
            if (p.DevelopmentalStage.Baby())
            {
                return true;
            }
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                float x = Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst()).x;
                if (x > levelLabelWidth)
                {
                    typeof(SkillUI).GetField("levelLabelWidth", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, x);
                    levelLabelWidth = x;
                }
            }

            int subOffsetBy = 0;
            for (int j = 0; j < skillDefsInOrder.Count; j++)
            {
                SkillDef skillDef = skillDefsInOrder[j];
                if (skillDef == AmphiDefs.RimMorpho_Shifting && p.def != AmphiDefs.RimMorpho_Amphimorpho)
                {
                    subOffsetBy +=1;
                    continue;
                }
                float y = (j-subOffsetBy) * 27f + offset.y;
                SkillUI.DrawSkill(p.skills.GetSkill(skillDef), new Vector2(offset.x, y), mode, "");
            }
            return false;
        }
    }
}
