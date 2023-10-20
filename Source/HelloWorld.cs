using Verse;

namespace Rimimorpho
{
    [StaticConstructorOnStartup]
    public static class HelloWorld
    {
        static HelloWorld()
        {
            Log.Message($"<color=orange>[Rimimorpho]</color> Hello world!");
        }
    }
}
