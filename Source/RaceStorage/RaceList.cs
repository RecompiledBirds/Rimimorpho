using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimimorpho
{
    public class RaceList<T> : IExposable, IEnumerable where T : StoredRace
    {
        private List<StoredRace> storedRaces = new List<StoredRace>();

        public StoredRace this[int index]
        {
            get
            {
                return storedRaces[index];
            }
        }
        public int Length
        {
            get
            {
                return storedRaces.Count;
            }
        }
        public void Add(StoredRace storedRace)
        {
            storedRaces.Add(storedRace);
        }

        public bool Empty { get { return storedRaces.Count == 0; } }

        public void Remove(StoredRace storedRace)
        {
            storedRaces.Remove(storedRace);
        }


        public void Remove(int index)
        {
            storedRaces.RemoveAt(index);
        }

        public void Clear()
        {
            storedRaces.Clear();
        }

        public void ExposeData()
        {
           Scribe_Collections.Look(ref storedRaces, nameof(storedRaces),LookMode.Deep);
        }

        public RaceEnumerator GetEnumerator()
        {
            return new RaceEnumerator(storedRaces);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class RaceEnumerator : IEnumerator
    {
        public List<StoredRace> races;
        int index = 0;
        public RaceEnumerator(List<StoredRace> races)
        {
            this.races = races;
        }

        public bool MoveNext()
        {
            index++;
            return index < races.Count;
        }

        public void Reset()
        {
            index = 0;
        }

        public StoredRace Current
        {
            get
            {
                return races[index];
            }
        }

        object IEnumerator.Current => Current;
    }
}
