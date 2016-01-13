using System;

namespace CodeToCaml.SpTypes
{
    public abstract class SpAbstract
    {
        public string Title { get; set; }

        public string Author { get; set; }

        [SpData(Name = "Author")]
        public int AuthorId { get; set; }

        public string Editor { get; set; }

        [SpData(Name = "Editor")]
        public int EditorId { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        [SpData(Name = "_ModerationStatus", ValueType = "ModStat")]
        public SpModerationStatus ModerationStatus { get; set; }

        [SpData(Name = "RecurrenceID")]
        public object RecurrenceId { get; set; }
    }
}
