using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Bam
{
    /// <summary>
    /// Resolves a type name a generation config gives against a loaded assembly, and says why when it does not
    /// resolve. <see cref="Assembly.GetType(string)"/> alone reports three different failures as null: the
    /// runtime rejected the name itself, the assembly holds no type of that name, or the type is there but
    /// cannot be loaded (a dependency is missing, or is a build the type does not fit). The
    /// <see cref="TypeLoadException"/> thrown when asked names the requested type in the last two cases alike,
    /// so absence is read from the assembly file's own metadata (<see cref="DefinesType"/>).
    /// </summary>
    /// <remarks>
    /// Shared by <see cref="GenerateDecoratorCommand"/> and <see cref="GenerateServiceClientCommand"/> so both
    /// commands report a configured name the same way (bamtk#133). Stateless and thread-safe.
    /// </remarks>
    public static class TypeNameResolver
    {
        /// <summary>
        /// Resolves <paramref name="typeName"/> in <paramref name="assembly"/>. Never throws for a name that does
        /// not resolve: the outcome says which of the three failures it was.
        /// </summary>
        /// <param name="assembly">The assembly to look in; it must have been loaded from a file, since absence is read from that file.</param>
        /// <param name="typeName">The name as configured, as <see cref="Assembly.GetType(string)"/> takes it (nested types spelled <c>Outer+Inner</c>, a closed generic with its arguments in brackets).</param>
        /// <returns>The type, or the reason it did not resolve.</returns>
        public static TypeResolution Resolve(Assembly assembly, string typeName)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            ArgumentNullException.ThrowIfNull(typeName);

            // The runtime's parser skips leading whitespace but keeps it at the end of the name and before a
            // generic argument list, then finds nothing. Say why instead of "not found".
            if (typeName.Length > 0 && char.IsWhiteSpace(typeName[typeName.Length - 1]))
            {
                return TypeResolution.Rejected("the name ends in whitespace.");
            }

            int argumentsStart = typeName.IndexOf('[');
            if (argumentsStart > 0 && char.IsWhiteSpace(typeName[argumentsStart - 1]))
            {
                return TypeResolution.Rejected("there is whitespace before '['.");
            }

            try
            {
                return TypeResolution.Found(assembly.GetType(typeName, true)!);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // Generic arguments on a type that is not generic, the wrong number of them, one that breaks a
                // constraint, or a malformed name.
                return TypeResolution.Rejected(ex.Message);
            }
            catch (Exception ex) when (IsLoadFailure(ex))
            {
                if (ex is TypeLoadException && !DefinesType(assembly.Location, typeName))
                {
                    return TypeResolution.NotFound;
                }

                string? unqualified = UnresolvedUnqualifiedArgument(assembly, typeName);
                if (unqualified is not null)
                {
                    // IGen`1[System.String]: an argument with no assembly is looked for in the target assembly
                    // alone, so it fails as though a dependency were missing. The name is what is wrong.
                    return TypeResolution.Rejected(
                        $"generic argument '{unqualified}' names no assembly, so it is looked for in '{assembly.GetName().Name}' only, which does not define it; qualify it, as [[System.String, System.Private.CoreLib]].");
                }

                return TypeResolution.Unloadable(ex.Message);
            }
        }

        /// <summary>
        /// Whether the type-definition table of the assembly file at <paramref name="assemblyPath"/> holds the type
        /// <paramref name="typeName"/> refers to. A closed generic name is matched on its definition, the part
        /// before the first <c>[</c>: the table holds <c>IRepo`1</c>, never <c>IRepo`1[[System.String, ...]]</c>.
        /// Leading whitespace is dropped, as the runtime's parser drops it. Reads metadata only and loads
        /// nothing; a row's name is compared in place in the string heap, so a row that cannot match costs no
        /// allocation (bamtk#133).
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly file.</param>
        /// <param name="typeName">The type name, as <see cref="Assembly.GetType(string)"/> takes it.</param>
        /// <returns>True when the file defines the type; false when it does not or cannot be read.</returns>
        public static bool DefinesType(string assemblyPath, string typeName)
        {
            ArgumentNullException.ThrowIfNull(typeName);

            int argumentsStart = typeName.IndexOf('[');
            string definitionName = (argumentsStart < 0 ? typeName : typeName.Substring(0, argumentsStart)).TrimStart();
            string[] rowNames = CandidateRowNames(definitionName);
            try
            {
                using FileStream stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using PEReader peReader = new PEReader(stream);
                MetadataReader reader = peReader.GetMetadataReader();
                MetadataStringComparer comparer = reader.StringComparer;
                foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
                {
                    TypeDefinition definition = reader.GetTypeDefinition(handle);
                    // Only a row whose own name is the tail of the requested name pays for the walk up its
                    // declaring types.
                    if (MatchesAny(comparer, definition.Name, rowNames)
                        && FullNameOf(reader, definition, definitionName.Length) == definitionName)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is InvalidOperationException || ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                // The file cannot be read back; "not found" is then the runtime's word for it.
                return false;
            }
        }

        /// <summary>
        /// Whether <paramref name="ex"/> is one of the exceptions the runtime throws when an assembly or type
        /// cannot be loaded, as opposed to a defect in the caller.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>True for a load failure.</returns>
        public static bool IsLoadFailure(Exception ex)
        {
            return ex is FileNotFoundException
                || ex is FileLoadException
                || ex is BadImageFormatException
                || ex is TypeLoadException
                || ex is ReflectionTypeLoadException;
        }

        // The names a type-definition row would carry if it were the one requested: after the last '+' for a
        // nested type; for a top-level type the name after any '.', since the namespace/name split is not
        // knowable from the full name alone (a name may itself hold a '.'). A handful of strings per request,
        // allocated once, instead of one per row.
        private static string[] CandidateRowNames(string definitionName)
        {
            int nested = definitionName.LastIndexOf('+');
            if (nested >= 0)
            {
                return new string[] { definitionName.Substring(nested + 1) };
            }

            List<string> names = new List<string> { definitionName };
            for (int dot = definitionName.IndexOf('.'); dot >= 0; dot = definitionName.IndexOf('.', dot + 1))
            {
                names.Add(definitionName.Substring(dot + 1));
            }

            return names.ToArray();
        }

        private static bool MatchesAny(MetadataStringComparer comparer, StringHandle name, string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (comparer.Equals(name, candidate))
                {
                    return true;
                }
            }

            return false;
        }

        // Namespace.Outer+Inner for a type-definition row, or null once the name being built is longer than
        // maxLength: a longer full name can never match, so a deep chain of nested types (or one of long names)
        // costs no more than the requested name's length. The walk up the declaring types is also bounded by the
        // number of rows, so it ends whatever the nesting table says (the runtime refuses to load an assembly
        // whose nesting is cyclic, but this reads the file, not the loaded assembly).
        private static string? FullNameOf(MetadataReader reader, TypeDefinition definition, int maxLength)
        {
            string name = reader.GetString(definition.Name);
            TypeDefinition current = definition;
            for (int steps = reader.TypeDefinitions.Count; steps > 0; steps--)
            {
                TypeDefinitionHandle declaring = current.GetDeclaringType();
                if (declaring.IsNil)
                {
                    break;
                }

                current = reader.GetTypeDefinition(declaring);
                string declaringName = reader.GetString(current.Name);
                if (declaringName.Length + 1 + name.Length > maxLength)
                {
                    return null;
                }

                name = declaringName + "+" + name;
            }

            string @namespace = reader.GetString(current.Namespace);
            string fullName = @namespace.Length == 0 ? name : @namespace + "." + name;
            return fullName.Length > maxLength ? null : fullName;
        }

        // The first top-level generic argument of typeName that names no assembly and that the assembly does not
        // define, or null when there is none. Such an argument is the reason the name failed to load, provided
        // the generic definition itself loads; when it does not, the definition's failure is the one to report.
        private static string? UnresolvedUnqualifiedArgument(Assembly assembly, string typeName)
        {
            int open = typeName.IndexOf('[');
            int close = typeName.LastIndexOf(']');
            if (open < 0 || close <= open)
            {
                return null;
            }

            try
            {
                if (assembly.GetType(typeName.Substring(0, open), false) is null)
                {
                    return null;
                }
            }
            catch (Exception ex) when (IsLoadFailure(ex))
            {
                return null;
            }

            foreach (string argument in TopLevelArguments(typeName.Substring(open + 1, close - open - 1)))
            {
                string unwrapped = argument.Trim();
                if (unwrapped.StartsWith('[') && unwrapped.EndsWith(']'))
                {
                    unwrapped = unwrapped.Substring(1, unwrapped.Length - 2).Trim();
                }

                if (TopLevelArguments(unwrapped).Count == 1 && !DefinesType(assembly.Location, unwrapped))
                {
                    return unwrapped;
                }
            }

            return null;
        }

        // Splits a bracketed argument list on the commas outside any nested brackets.
        private static List<string> TopLevelArguments(string list)
        {
            List<string> arguments = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < list.Length; i++)
            {
                char c = list[i];
                if (c == '[')
                {
                    depth++;
                }
                else if (c == ']')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    arguments.Add(list.Substring(start, i - start));
                    start = i + 1;
                }
            }

            arguments.Add(list.Substring(start));
            return arguments;
        }
    }

    /// <summary>
    /// How a configured type name resolved (<see cref="TypeNameResolver.Resolve"/>): to a type, or to one of three
    /// reasons it did not. At most one of <see cref="Type"/>, <see cref="LoadFailure"/> and
    /// <see cref="Rejection"/> is set; none set means the assembly holds no type of that name.
    /// </summary>
    /// <param name="Type">The resolved type; null when the name did not resolve.</param>
    /// <param name="LoadFailure">The loader's message when the type is there but could not be loaded.</param>
    /// <param name="Rejection">Why the runtime refused the name itself.</param>
    public readonly record struct TypeResolution(Type? Type, string? LoadFailure, string? Rejection)
    {
        /// <summary>Gets the outcome for a name the assembly does not define.</summary>
        public static TypeResolution NotFound => new TypeResolution(null, null, null);

        /// <summary>Gets a value indicating whether the name resolved to a type.</summary>
        public bool IsFound => Type is not null;

        /// <summary>Creates the outcome for a name that resolved.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The outcome.</returns>
        public static TypeResolution Found(Type type) => new TypeResolution(type, null, null);

        /// <summary>Creates the outcome for a type that is defined but could not be loaded.</summary>
        /// <param name="loadFailure">The loader's message.</param>
        /// <returns>The outcome.</returns>
        public static TypeResolution Unloadable(string loadFailure) => new TypeResolution(null, loadFailure, null);

        /// <summary>Creates the outcome for a name the runtime refused.</summary>
        /// <param name="rejection">Why.</param>
        /// <returns>The outcome.</returns>
        public static TypeResolution Rejected(string rejection) => new TypeResolution(null, null, rejection);

        /// <summary>
        /// Gets the sentence that explains a name that did not resolve, for a skip reason or an exception message.
        /// </summary>
        /// <param name="role">What the type is to the command, such as "interface" or "service".</param>
        /// <param name="typeName">The name as configured.</param>
        /// <param name="assembly">The assembly it was looked for in.</param>
        /// <returns>The reason, starting with <paramref name="role"/>.</returns>
        public string Reason(string role, string typeName, Assembly assembly)
        {
            if (Rejection is not null)
            {
                return $"{role} type '{typeName}' is not a type name the runtime accepts for assembly '{assembly.Location}': {Rejection}";
            }

            return LoadFailure is null
                ? $"{role} type '{typeName}' was not found in assembly '{assembly.Location}'."
                : $"{role} type '{typeName}' is in assembly '{assembly.Location}', but a dependency could not be loaded: {LoadFailure}";
        }
    }
}
