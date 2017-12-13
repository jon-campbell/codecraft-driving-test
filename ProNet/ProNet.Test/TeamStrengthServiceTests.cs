﻿using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace ProNet.Test
{
    [TestFixture]
    public class TeamStrengthServiceTests
    {
        private ISeparationService _separationService;
        private ISkillsService _skillsService;
        private IRankService _rankService;
        private ITeamStrengthService _teamStrengthService;
        private string _skill;
        private IEnumerable<string> _team;
        private double _expectedStrength;

        [SetUp]
        public void SetUp()
        {
            _separationService = Substitute.For<ISeparationService>();
            _skillsService = Substitute.For<ISkillsService>();
            _rankService = Substitute.For<IRankService>();

            _teamStrengthService = new TeamStrengthService(_separationService, _skillsService, _rankService);

            _skill = "";
            _team = new List<string> { "leader", "a", "b" };
        }

        public void AssertTeamStrength()
        {
            var strength = _teamStrengthService.GetTeamStrength(_skill, _team);
            Assert.That(strength, Is.EqualTo(_expectedStrength).Within(0.01d));
        }

        [TestCase(new string[] {}, 0)]
        [TestCase(new[] { "leader" }, 0)]
        public void Strength_of_empty_team_is_zero(IEnumerable<string> team, double expected)
        {
            _team = team;
            _expectedStrength = expected;

            AssertTeamStrength();
        }

        [Test]
        public void Strength_of_team_with_no_connection_is_zero()
        {
            _separationService.GetDegreesBetween(Arg.Any<string>(), Arg.Any<string>()).Returns(0);
            _expectedStrength = 0;

            AssertTeamStrength();
        }

        [Test]
        public void Strength_of_team_with_member_that_does_not_know_chosen_skill_is_zero()
        {
            _skill = "skill";
            _skillsService.GetSkills("leader").Returns(new [] { "skill" });
            _skillsService.GetSkills("a").Returns(new[] { "NOT skill" });
            _skillsService.GetSkills("b").Returns(new[] { "skill" });
            _expectedStrength = 0;

            AssertTeamStrength();
        }

        [TestCase(1)]
        [TestCase(5)]
        public void Strength_of_team_with_one_for_skill_index_and_degrees_of_separation_is_average_of_page_ranks(int averageRank)
        {
            _skillsService.GetSkillIndex(Arg.Any<string>(), Arg.Any<string>()).Returns(1);
            _separationService.GetDegreesBetween(Arg.Any<string>(), Arg.Any<string>()).Returns(1);
            _rankService.GetRank(Arg.Any<string>()).Returns(averageRank);
            _expectedStrength = averageRank;

            AssertTeamStrength();
        }

        [TestCase("memberA", "memberB")]
        [TestCase("memberB", "memberA")]
        public void Strength_is_the_same_regardless_of_leader(string leader, string member)
        {
            _team = new List<string> { leader, member };
            _skill = "skill";
            _separationService.GetDegreesBetween(leader, member).Returns(4);
            _skillsService.GetSkillIndex(leader, _skill).Returns(3);
            _skillsService.GetSkillIndex(member, _skill).Returns(2);
            _rankService.GetRank(leader).Returns(0.5d);
            _rankService.GetRank(member).Returns(0.6d);
            _expectedStrength = 0.12d;

            AssertTeamStrength();
        }

        [Test]
        public void Strength_of_single_member_team_is_rank_over_skill_index()
        {
            const string programmerId = "only member";
            const string skill = "skill";

            _rankService.GetRank(programmerId).Returns(2);
            _skillsService.GetSkillIndex(programmerId, skill).Returns(3);

            var strength = new TeamStrengthService(_separationService, _skillsService, _rankService).GetTeamStrength(skill, new List<string> { programmerId });

            Assert.That(strength, Is.EqualTo(2d / 3d));
        }

        [Test]
        public void Strength_of_individual_is_rank_over_skill_index()
        {
            const string skill = "valid skill";
            const string programmerId = "programmer id";
            var skillService = Substitute.For<ISkillsService>();
            skillService.GetSkillIndex(programmerId, skill).Returns(3);
            var rankService = Substitute.For<IRankService>();
            rankService.GetRank(programmerId).Returns(2d);

            var strength = new TeamStrengthService(null, skillService, rankService).GetIndividualStrength(skill, programmerId);

            const double expected = 2d / 3d;
            Assert.That(strength, Is.EqualTo(expected));
        }
    }
}
