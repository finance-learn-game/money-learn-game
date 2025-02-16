using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace SmbcApp.LearnGame.Data
{
    [MessagePackObject]
    public record GameSaveData
    {
        [Key(0)] public string CurrentProfile { get; init; }
        [Key(1)] public List<string> Profiles { get; init; }

        public static GameSaveData Default => new()
        {
            CurrentProfile = null,
            Profiles = new List<string>()
        };

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append($"{nameof(CurrentProfile)}: {CurrentProfile}");
            builder.Append($", {nameof(Profiles)}: [{string.Join(", ", Profiles)}]");
            builder.Append("}");

            return builder.ToString();
        }
    }
}