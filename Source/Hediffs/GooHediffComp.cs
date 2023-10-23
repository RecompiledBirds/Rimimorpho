using Rimimorpho;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public class GooHediffCompProps : HediffCompProperties
    {
        public GooHediffCompProps()
        {
            compClass=typeof(GooHediffComp);
        }
    }
    public class GooHediffComp : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            float severity = parent.Severity;
            if (severity <= 0.01)
            {
                return;
            }
            Pawn.Strip();
            Pawn.def = AmphiDefs.RimMorpho_Amphimorpho;
            Pawn.AllComps.Clear();
            Pawn.InitializeComps();
            RVRComp comp = Pawn.TryGetComp<RVRComp>();
            if (comp != null)
            {
                comp.GenGraphics();
                comp.InformGraphicsDirty();

                Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            Pawn.health.RemoveHediff(parent);

        }
    }
}
