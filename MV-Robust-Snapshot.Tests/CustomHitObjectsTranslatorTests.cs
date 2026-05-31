using Xunit;
using MapsetVerifier.Parser.Objects;
using MapsetVerifier.Parser.Objects.HitObjects;
using MapsetVerifier.Plugin.CustomSnapshots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MV_Robust_Snapshot.Tests
{
    public class CustomHitObjectsTranslatorTests
    {
        private Beatmap CreateBeatmap(string mode = "0")
        {
            string code = $"[General]\nMode: {mode}\n\n[Metadata]\nTitle:Test\nArtist:Test\nCreator:Test\nVersion:Test\n\n[TimingPoints]\n0,500,4,1,0,100,1,0\n\n[HitObjects]\n";
            return new Beatmap(code, "dummy", $"dummy_{Guid.NewGuid():N}.osu");
        }

        [Fact]
        public void TestHorizontalFlip()
        {
            var beatmap = CreateBeatmap("0");
            string oldCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
100,100,1000,1,0,0:0:0:0:
";
            string newCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
412,100,1000,1,0,0:0:0:0:
";
            var (sections, globalShift, timingShift) = CustomHitObjectsTranslator.GetShiftSections(beatmap, oldCode, newCode);
            Assert.NotEmpty(sections);
            var matchedStep = sections.SelectMany(s => s.Steps).FirstOrDefault(s => s.OldObj != null && s.NewObj != null);
            Assert.NotNull(matchedStep);
            Assert.Equal(1000, matchedStep.NewObj.time);
            Assert.Equal(100, matchedStep.OldObj.Position.X);
            Assert.Equal(412, matchedStep.NewObj.Position.X);
        }

        [Fact]
        public void TestVerticalFlip()
        {
            var beatmap = CreateBeatmap("0");
            string oldCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
100,100,1000,1,0,0:0:0:0:
";
            string newCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
100,284,1000,1,0,0:0:0:0:
";
            var (sections, globalShift, timingShift) = CustomHitObjectsTranslator.GetShiftSections(beatmap, oldCode, newCode);
            Assert.NotEmpty(sections);
            var matchedStep = sections.SelectMany(s => s.Steps).FirstOrDefault(s => s.OldObj != null && s.NewObj != null);
            Assert.NotNull(matchedStep);
            Assert.Equal(1000, matchedStep.NewObj.time);
            Assert.Equal(100, matchedStep.OldObj.Position.Y);
            Assert.Equal(284, matchedStep.NewObj.Position.Y);
        }

        [Fact]
        public void TestCatchHorizontalFlip()
        {
            var beatmap = CreateBeatmap("2");
            string oldCode = @"[General]
Mode: 2
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
100,100,1000,1,0,0:0:0:0:
";
            string newCode = @"[General]
Mode: 2
[TimingPoints]
0,500,4,1,0,100,1,0
[HitObjects]
412,100,1000,1,0,0:0:0:0:
";
            var (sections, globalShift, timingShift) = CustomHitObjectsTranslator.GetShiftSections(beatmap, oldCode, newCode);
            Assert.NotEmpty(sections);
            var matchedStep = sections.SelectMany(s => s.Steps).FirstOrDefault(s => s.OldObj != null && s.NewObj != null);
            Assert.NotNull(matchedStep);
            Assert.Equal(1000, matchedStep.NewObj.time);
            Assert.Equal(100, matchedStep.OldObj.Position.X);
            Assert.Equal(412, matchedStep.NewObj.Position.X);
        }

        [Fact]
        public void TestLocalTimingShiftAlignment()
        {
            var beatmap = CreateBeatmap("0");
            string oldCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
9215,500,4,1,0,100,1,0
[HitObjects]
100,100,9215,1,0,0:0:0:0:
120,120,9446,1,0,0:0:0:0:
140,140,9677,1,0,0:0:0:0:
160,160,9908,1,0,0:0:0:0:
180,180,10139,1,0,0:0:0:0:
";
            string newCode = @"[General]
Mode: 0
[TimingPoints]
0,500,4,1,0,100,1,0
8984,500,4,1,0,100,1,0
[HitObjects]
200,200,8984,1,0,0:0:0:0:
220,220,9215,1,0,0:0:0:0:
240,240,9446,1,0,0:0:0:0:
260,260,9677,1,0,0:0:0:0:
280,280,9908,1,0,0:0:0:0:
";
            var (sections, globalShift, timingShift) = CustomHitObjectsTranslator.GetShiftSections(beatmap, oldCode, newCode);
            
            var shiftSection = sections.FirstOrDefault(s => s.IsSectionShift && Math.Abs(s.Shift - (-231)) <= 2.0);
            Assert.NotNull(shiftSection);
            Assert.Equal(5, shiftSection.Steps.Count(s => s.OldObj != null && s.NewObj != null));
        }

        [Fact]
        public void TestCenterObjectsAndSpinnersNoFlipOrMove()
        {
            var beatmap = CreateBeatmap("0");

            // Create objects exactly at the center (256, 192)
            var oldSpinner = new Spinner(new[] { "256", "192", "1000", "12", "0", "1500", "0:0:0:0:" }, beatmap);
            var newSpinner = new Spinner(new[] { "256", "192", "1000", "12", "0", "1500", "0:0:0:0:" }, beatmap);

            var oldCircle = new Circle(new[] { "256", "192", "2000", "1", "0", "0:0:0:0:" }, beatmap);
            var newCircle = new Circle(new[] { "256", "192", "2000", "1", "0", "0:0:0:0:" }, beatmap);

            var spinnerChanges = CustomHitObjectsTranslator.GetChanges(newSpinner, oldSpinner, beatmap).ToList();
            var circleChanges = CustomHitObjectsTranslator.GetChanges(newCircle, oldCircle, beatmap).ToList();

            Assert.Empty(spinnerChanges);
            Assert.Empty(circleChanges);
        }
    }
}
