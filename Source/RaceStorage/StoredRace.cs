using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace Rimimorpho
{
    public class StoredRace : IExposable, ILoadReferenceable
    {
        private XenotypeDef storedXenotypeDef;
        private ThingDef storedThingDef;

        public XenotypeDef XenotypeDef => storedXenotypeDef;
        public ThingDef ThingDef => storedThingDef;

        /// <summary>
        ///     DON'T USE
        /// </summary>
        public StoredRace() 
        {
            if (Scribe.mode == LoadSaveMode.Inactive) throw new InvalidOperationException("You can't make a new StoredRace during gameplay using this constructor!");
        }

        public StoredRace(ThingDef thingDef)
        {
            storedThingDef = thingDef;
        }

        public StoredRace(ThingDef thingDef, XenotypeDef xenotypeDef) 
        {
            storedThingDef = thingDef;
            storedXenotypeDef = xenotypeDef;
        }

        public bool ContainsFeature(ThingDef thingDef, XenotypeDef xenotypeDef = null)
        {
            if (thingDef == storedThingDef) return true;
            if (xenotypeDef == null) return false;
            if (xenotypeDef == storedXenotypeDef) return true;
            return false;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref storedThingDef, nameof(storedThingDef));
            Scribe_Defs.Look(ref storedXenotypeDef, nameof(storedXenotypeDef));
        }

        public string GetUniqueLoadID()
        {
           return $"{GetHashCode()}";
        }
    }
}
