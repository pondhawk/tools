using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Rql.Builder;

/// <summary>
/// Specifies how a criteria property maps to an RQL predicate operand during introspection.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single refers to a single operand, not the System.Single type")]
public enum OperandKind { Single, From, To, List, ListOfInt, ListOfLong }
