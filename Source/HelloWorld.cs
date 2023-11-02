using Verse;

namespace Rimimorpho
{
    [StaticConstructorOnStartup]
    public static class HelloWorld
    {
        static HelloWorld()
        {
            Log.Message($"<color=green>[Rimimorpho]</color> Hello world!");
        }
    }
}
