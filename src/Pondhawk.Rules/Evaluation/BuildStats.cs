using System.Runtime.InteropServices;

namespace Pondhawk.Rules.Evaluation;

[StructLayout(LayoutKind.Auto)]
internal struct BuildStats
{
    internal int VariationsConsidered;
    internal int VariationsFound;
    internal int StepsAdded;
}
