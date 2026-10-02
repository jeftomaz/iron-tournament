using System;

namespace IronTournament.Core
{
    internal static class ConfigurationGuard
    {
        public static string DisplayName(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A name is required.", parameterName);
            }

            var normalized = value.Trim();
            if (normalized.Length > 48)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                if (char.IsControl(character) || character == '<' || character == '>')
                {
                    throw new ArgumentException("The name contains unsafe characters.", parameterName);
                }
            }

            return normalized;
        }
    }
}
