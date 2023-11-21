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
        public ThingDef storedDef;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref storedDef, nameof(storedDef));

        }

        public string GetUniqueLoadID()
        {
           return $"{this.GetHashCode()}";
        }
    }

    public class StoredRaceWithXenoType : StoredRace
    {
        public XenotypeDef storedXenotypeDef;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref storedXenotypeDef, nameof(storedXenotypeDef));
        }
    }
}
