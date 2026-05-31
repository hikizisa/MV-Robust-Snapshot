using Xunit;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Plugin.CustomSnapshots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MV_Robust_Snapshot.Tests
{
    public class CustomTimingTranslatorTests
    {
        private Beatmap CreateBeatmap(string mode = "0")
        {
            string code = $"[General]\nMode: {mode}\n\n[Metadata]\nTitle:Test\nArtist:Test\nCreator:Test\nVersion:Test\n\n[TimingPoints]\n0,500,4,1,0,100,1,0\n\n[HitObjects]\n";
            return new Beatmap(code, "dummy", $"dummy_{Guid.NewGuid():N}.osu");
        }

        [Fact]
        public void TestTimingPointsShift()
        {
            var beatmap = CreateBeatmap("0");
            string oldCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
9215,500,4,1,0,100,1,0
";
            string newCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
8984,500,4,1,0,100,1,0
";
            var sections = CustomTimingTranslator.GetTimingShiftSections(beatmap, oldCode, newCode);
            Assert.NotEmpty(sections);
            var shiftSection = sections.FirstOrDefault(s => Math.Abs(s.Shift - (-231)) <= 2.0);
            Assert.Equal(8984, shiftSection.StartTime);
        }
    }
}
