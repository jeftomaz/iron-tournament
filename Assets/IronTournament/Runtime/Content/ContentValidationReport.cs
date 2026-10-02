using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Content
{
    public sealed class ContentValidationReport
    {
        private readonly ReadOnlyCollection<string> errors;

        internal ContentValidationReport(IList<string> errors)
        {
            this.errors = new ReadOnlyCollection<string>(new List<string>(errors));
        }

        public bool IsValid => errors.Count == 0;

        public IReadOnlyList<string> Errors => errors;
    }

    public sealed class ContentValidationException : Exception
    {
        public ContentValidationException(ContentValidationReport report)
            : base("Invalid game content: " + string.Join(" | ", report.Errors))
        {
            Report = report;
        }

        public ContentValidationReport Report { get; }
    }
}
