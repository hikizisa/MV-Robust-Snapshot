using System;
using System.Collections.Generic;
using System.Reflection;
using MapsetVerifier.Framework.Objects;
using MapsetVerifier.Framework.Objects.Attributes;
using MapsetVerifier.Framework.Objects.Metadata;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Snapshots;
using MapsetVerifier.Snapshots.Objects;
using Serilog;

namespace MapsetVerifier.Plugin.CustomSnapshots
{
    [Check]
    public class CustomSnapshotPlugin : GeneralCheck
    {
        private static bool Hooked = false;

        public CustomSnapshotPlugin()
        {
            InitializeHook();
        }

        private static void InitializeHook()
        {
            if (Hooked) return;

            try
            {
                Log.Information("[CustomSnapshotsPlugin] Initializing snapshot translator replacement hook...");

                // Force initialization of default translators
                var translatorsList = TranslatorRegistry.GetTranslators();

                // Get private static field 'translators' via reflection
                var field = typeof(TranslatorRegistry).GetField("translators", BindingFlags.Static | BindingFlags.NonPublic);
                if (field == null)
                {
                    Log.Error("[CustomSnapshotsPlugin] Could not find private field 'translators' in TranslatorRegistry.");
                    return;
                }

                if (field.GetValue(null) is List<DiffTranslator> translators)
                {
                    // Find and remove existing TimingPoints translator
                    int removedCount = translators.RemoveAll(t => t.Section == "TimingPoints");
                    Log.Information("[CustomSnapshotsPlugin] Removed {Count} default TimingPoints translator(s).", removedCount);

                    // Add our custom translator
                    var customTranslator = new CustomTimingTranslator();
                    translators.Add(customTranslator);
                    Log.Information("[CustomSnapshotsPlugin] Registered custom TimingPoints translator successfully.");

                    // Find and remove existing HitObjects translator
                    int removedHitObjectsCount = translators.RemoveAll(t => t.Section == "HitObjects");
                    Log.Information("[CustomSnapshotsPlugin] Removed {Count} default HitObjects translator(s).", removedHitObjectsCount);

                    // Add our custom HitObjects translator
                    var customHitObjectsTranslator = new CustomHitObjectsTranslator();
                    translators.Add(customHitObjectsTranslator);
                    Log.Information("[CustomSnapshotsPlugin] Registered custom HitObjects translator successfully.");
                }
                else
                {
                    Log.Error("[CustomSnapshotsPlugin] Field 'translators' is not of type List<DiffTranslator>.");
                }

                Hooked = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomSnapshotsPlugin] Failed to replace snapshot translator.");
            }
        }

        public override CheckMetadata GetMetadata() =>
            new CheckMetadata
            {
                Category = "Snapshots",
                Message = "Custom Snapshots Plugin",
                Author = "Self"
            };

        public override Dictionary<string, IssueTemplate> GetTemplates() => new Dictionary<string, IssueTemplate>();

        public override IEnumerable<Issue> GetIssues(BeatmapSet beatmapSet) => [];
    }
}
