using Rimimorpho.Windows;
using RimWorld;
using RVCRestructured.Shifter;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public class AmphiShifterProps : CompProperties
    {
        public AmphiShifterProps() 
        { 
            this.compClass=typeof(AmphiShifter);
        }
    }
    public class AmphiShifter : ShapeshifterComp
    {
        private bool shifted = false;

        private Dictionary<Pawn,int> attackedPawns = new Dictionary<Pawn, int>();

        public Dictionary<Pawn, int> AttackedPawns
        {
            get
            {
                
                return attackedPawns;
            }
        }


        public void AddPawnToAttackedList(Pawn pawn)
        {
            if (!AttackedPawns.ContainsKey(pawn)) AttackedPawns[pawn] = GenTicks.TicksGame;
        }

        public void CleanupAttackedPawns()
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach(Pawn p in AttackedPawns.Keys)
            {
                int ticks = GenTicks.TicksGame;
                int timeElapsed = ticks- AttackedPawns[p];
                if (timeElapsed > 60000)
                {
                    pawns.Add(p);
                }
            }
            foreach(Pawn pawn in pawns) { AttackedPawns.Remove(pawn); }
        }
        public Dictionary<ThingDef, RaceList<StoredRace>> knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>();

        public void LearnSpecies(Pawn pawn)
        {
            if (knownSpecies == null) { knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>(); }
            if (!knownSpecies.ContainsKey(pawn.def))
            {
                knownSpecies.Add(pawn.def, new RaceList<StoredRace>());
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn.genes?.Xenotype,pawn.story?.bodyType));
                return;
            }
            
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[pawn.def], race => race.ContainsFeature(pawn.def, pawn?.genes?.Xenotype)))
            {
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn.genes?.Xenotype, pawn.story.bodyType));
            }
        }

        public void LearnSpecies(ThingDef def, XenotypeDef xenotypeDef)
        {
            if (knownSpecies == null) { knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>(); }
            if (!knownSpecies.ContainsKey(def)|| knownSpecies[def].Empty)
            {
                knownSpecies[def] = new RaceList<StoredRace>
                {
                    new StoredRace(def, xenotypeDef)
                };
                return;
            }
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[def], race => race.ContainsFeature(def, xenotypeDef)))
            {
                knownSpecies[def].Add(new StoredRace(def, xenotypeDef));
            }
        }
        public void LearnSpecies(ThingDef def, XenotypeDef xenotypeDef, BodyTypeDef bodyTypeDef)
        {
            if (knownSpecies == null) { knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>(); }
            if (!knownSpecies.ContainsKey(def) || knownSpecies[def].Empty)
            {
                knownSpecies[def] = new RaceList<StoredRace>
                {
                    new StoredRace(def, xenotypeDef,bodyTypeDef)
                };
                return;
            }
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[def], race => race.ContainsFeature(def, xenotypeDef)))
            {
                knownSpecies[def].Add(new StoredRace(def, xenotypeDef,bodyTypeDef));
            }
        }

        public void LearnSpecies(ThingDef def)
        {
            if (!knownSpecies.ContainsKey(def) || (knownSpecies[def].Empty))
            {
                knownSpecies[def] = new RaceList<StoredRace>
                {
                    new StoredRace(def)
                };
                return;
            }
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[def], race => race.ContainsFeature(def)))
            {
                knownSpecies[def].Add(new StoredRace(def));
            }
        }
        //TODO: Make strings translateable
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = parent as Pawn;
            Command_Target command = new Command_Target
            {
                defaultLabel = "Rimimorpho_LearnRace".Translate(),
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                targetingParams = new TargetingParameters
                {
                    canTargetAnimals = true,
                    canTargetPawns = true,
                    canTargetHumans = true,
                    canTargetSelf = false
                },
                action = delegate (LocalTargetInfo target)
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_TouchPawn, target.Pawn, pawn);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            };
            Command_Action revert = new Command_Action
            {
                defaultLabel = "Rimimorpho_RevertForm".Translate(),
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = delegate ()
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_RevertForm, pawn);
                    pawn.jobs.TryTakeOrderedJob(job);
                }

            };
            Command_Action transMenu = new Command_Action
            {
                defaultLabel = "Rimimorpho_OpenTransformationWindow".Translate(),
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = () => Find.WindowStack.Add(new TransformationSelectionWindow(pawn))
            };
            yield return command;
            yield return revert;
            yield return transMenu;
        }

        private RaceProperties raceProperties = null;
        public override RaceProperties RaceProperties
        {
            get
            {
                if (raceProperties == null)
                {
                    raceProperties =base.RaceProperties;
                    FieldInfo field = typeof(RaceProperties).GetField("bloodDef", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    field.SetValue(raceProperties, field.GetValue(parent.def.race));
                }
                return raceProperties;
            }
        }


        private int ticksDownedFor = 0;
        public override void CompTick()
        {
            if (CurrentForm == parent.def) return;
            Pawn pawn = parent as Pawn;
            if (!pawn.Downed)
            {
                ticksDownedFor = 0;
                return;
            }
            ticksDownedFor++;

            if (ticksDownedFor >= 3600)
            {
                RevertForm();
            }

            if(GenTicks.TicksGame % 10 == 0 && CurrentForm!=parent.def)
            {

                pawn.skills.Learn(AmphiDefs.RimMorpho_Shifting, 2);
            }
            base.CompTick();
        }

        public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
        {
            RevertForm();
            base.Notify_KilledLeavingsLeft(leavings);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksDownedFor, nameof(ticksDownedFor));
            Scribe_Values.Look(ref shifted, nameof(shifted));
            Scribe_Collections.Look(ref knownSpecies, nameof(knownSpecies), LookMode.Def, LookMode.Deep);
        }

        public override float OffsetStat(StatDef stat)
        {
            return base.OffsetStat(stat);
        }
    }
}
