using System;
using System.Collections.Generic;

namespace Awane.Core.Registry
{
    public class AmbiguousComponentException : Exception
    {
        public IEnumerable<string> MatchedInterfaces { get; }

        public AmbiguousComponentException(string requestedName, IEnumerable<string> matchedInterfaces)
            : base(FormatMessage(requestedName, matchedInterfaces))
        {
            MatchedInterfaces = matchedInterfaces;
        }

        private static string FormatMessage(string requestedName, IEnumerable<string> matchedInterfaces)
        {
            var matches = string.Join(", ", matchedInterfaces);
            return $"Ambiguous interface name '{requestedName}'. Multiple matches found: {matches}. Use a more specific name.";
        }
    }
}