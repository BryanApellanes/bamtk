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

            if (HasWhitespaceBeforeBracket(typeName))
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

        // Whitespace between a name and the '[' that opens its argument list or array suffix, at any depth
        // (IGen`1[Svc.Other`1 [System.String]]). The runtime keeps it as part of the name and then finds nothing.
        // Whitespace after ',' or '[' (IPair`2[[A, X], [B, Y]]) separates arguments, and whitespace after ']'
        // (IGen`1[Svc.Local[] []], IGen`1[Svc.Local] []) comes before another array rank; the runtime accepts
        // both, so neither is reported.
        private static bool HasWhitespaceBeforeBracket(string typeName)
        {
            for (int i = 1; i < typeName.Length; i++)
            {
                if (typeName[i] != '[' || !char.IsWhiteSpace(typeName[i - 1]))
                {
                    continue;
                }

                int previous = i - 1;
                while (previous >= 0 && char.IsWhiteSpace(typeName[previous]))
                {
                    previous--;
                }

                if (previous >= 0 && typeName[previous] != ',' && typeName[previous] != '[' && typeName[previous] != ']')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether the type-definition table of the assembly file at <paramref name="assemblyPath"/> holds the type
        /// <paramref name="typeName"/> refers to. A closed generic name is matched on its definition, the part
        /// before the first <c>[</c>: the table holds <c>IRepo`1</c>, never <c>IRepo`1[[System.String, ...]]</c>.
        /// Leading whitespace is dropped, as the runtime's parser drops it. Reads metadata only and loads
        /// nothing; a row's name is compared in place in the string heap, so a row that cannot match costs no
        /// allocation (bamtk#133). A type in a namespace is found when its own name holds at most seven dots
        /// (no C# compiler emits one with any); a type with no namespace is found whatever its name holds.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly file.</param>
        /// <param name="typeName">The type name, as <see cref="Assembly.GetType(string)"/> takes it.</param>
        /// <returns>True when the file defines the type; false when it does not or cannot be read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="assemblyPath"/> or <paramref name="typeName"/> is null.</exception>
        public static bool DefinesType(string assemblyPath, string typeName)
        {
            ArgumentNullException.ThrowIfNull(assemblyPath);
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

        // How many of the last dots in a top-level name are tried as the namespace/name split. A type's own name
        // holding a dot is rare, and one holding this many is not worth a candidate each.
        private const int MaxNameDots = 8;

        // The names a type-definition row would carry if it were the one requested: after the last '+' for a
        // nested type; for a top-level type the name after each of its last MaxNameDots dots, and the whole name
        // (a type with no namespace), since the namespace/name split is not knowable from the full name alone (a
        // name may itself hold a '.'). At most MaxNameDots + 1 strings per request, allocated once, instead of one
        // per row; a name with thousands of dots costs no more than one with nine.
        private static string[] CandidateRowNames(string definitionName)
        {
            int nested = definitionName.LastIndexOf('+');
            if (nested >= 0)
            {
                return new string[] { definitionName.Substring(nested + 1) };
            }

            List<string> names = new List<string>();
            int dot = definitionName.Length;
            for (int found = 0; found < MaxNameDots; found++)
            {
                dot = dot == 0 ? -1 : definitionName.LastIndexOf('.', dot - 1);
                if (dot < 0)
                {
                    break;
                }

                names.Add(definitionName.Substring(dot + 1));
            }

            names.Add(definitionName);
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

        // How deep UnresolvedUnqualifiedArgument follows generic arguments that are themselves closed generics.
        private const int MaxArgumentDepth = 16;

        // The first generic argument of typeName that names no assembly and that the assembly does not define, or
        // null when there is none. Such an argument is the reason the name failed to load, provided each generic
        // definition on the way to it loads; when one does not, that definition's failure is the one to report.
        // An unqualified argument the assembly does define is followed into its own arguments
        // (IGen`1[Svc.Other`1[System.String]]); a qualified one ([[X, Asm]]) is never blamed or followed, since
        // its assembly, not the target, is where it is looked for. An array of a type the target defines
        // (Svc.Local[]) has no argument list to follow: its brackets hold a rank.
        private static string? UnresolvedUnqualifiedArgument(Assembly assembly, string typeName, int depth = 0)
        {
            int open = typeName.IndexOf('[');
            int close = MatchingBracket(typeName, open);
            if (open < 0 || close < 0 || depth >= MaxArgumentDepth || IsArrayRank(typeName.Substring(open + 1, close - open - 1)))
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

                if (TopLevelArguments(unwrapped).Count != 1)
                {
                    continue;
                }

                if (!DefinesType(assembly.Location, unwrapped))
                {
                    return unwrapped;
                }

                string? inner = UnresolvedUnqualifiedArgument(assembly, unwrapped, depth + 1);
                if (inner is not null)
                {
                    return inner;
                }
            }

            return null;
        }

        // Whether bracketed text is an array rank ("", ",", "*") rather than a generic argument list.
        private static bool IsArrayRank(string bracketed)
        {
            foreach (char c in bracketed)
            {
                if (c != ',' && c != '*' && !char.IsWhiteSpace(c))
                {
                    return false;
                }
            }

            return true;
        }

        // The index of the ']' that closes the '[' at open, or -1. A trailing array suffix ("[]") after the
        // argument list is not part of it.
        private static int MatchingBracket(string name, int open)
        {
            if (open < 0)
            {
                return -1;
            }

            int depth = 0;
            for (int i = open; i < name.Length; i++)
            {
                if (name[i] == '[')
                {
                    depth++;
                }
                else if (name[i] == ']' && --depth == 0)
                {
                    return i;
                }
            }

            return -1;
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
        /// <exception cref="InvalidOperationException">The name resolved; there is no reason to give.</exception>
        public string Reason(string role, string typeName, Assembly assembly)
        {
            if (Type is not null)
            {
                throw new InvalidOperationException($"{role} type '{typeName}' resolved; there is no reason it did not.");
            }

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
