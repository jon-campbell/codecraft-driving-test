﻿using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace ProNet.Test
{
    [TestFixture]
    public class TeamServiceTests
    {
        private INetworkRepository _networkRepository;
        private ISeparationService _separationService;
        private ISkillsService _skillsService;
        private IRankService _rankService;
        private TeamService _teamService;
        private string _skill;
        private IEnumerable<string> _team;
        private double _expectedStrength;

        [SetUp]
        public void SetUp()
        {
            _networkRepository = Substitute.For<INetworkRepository>();
            _separationService = Substitute.For<ISeparationService>();
            _skillsService = Substitute.For<ISkillsService>();
            _rankService = Substitute.For<IRankService>();

            _teamService = new TeamService(_networkRepository, _separationService, _skillsService, _rankService);

            _skill = "";
            _team = new List<string> { "leader", "a", "b" };
        }

        public void AssertTeamStrength()
        {
            var strength = _teamService.GetStrength(_skill, _team);
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

        [Test]
        public void Strength_of_two_member_team_is_expected()
        {
            _team = new List<string>{ "leader", "member" };
            _skill = "skill";
            _separationService.GetDegreesBetween("leader", "member").Returns(4);
            _skillsService.GetSkillIndex("leader", "skill").Returns(3);
            _skillsService.GetSkillIndex("member", "skill").Returns(2);
            _rankService.GetRank("leader").Returns(0.5d);
            _rankService.GetRank("member").Returns(0.6d);
            _expectedStrength = 0.12d;
            AssertTeamStrength();
        }

        [TestCase(1)]
        [TestCase(3)]
        public void Strongest_team_is_of_specified_size(int expected)
        {
            _skillsService.GetSkillIndex(Arg.Any<string>(), Arg.Any<string>()).Returns(1);
            _separationService.GetDegreesBetween(Arg.Any<string>(), Arg.Any<string>()).Returns(1);
            _rankService.GetRank(Arg.Any<string>()).Returns(0);
            _networkRepository.GetAll().Returns(new[]
            {
                new Programmer(null, null, null),
                new Programmer(null, null, null),
                new Programmer(null, null, null)
            });
            var team = _teamService.FindStrongestTeam("", expected);
            Assert.That(team.Count(), Is.EqualTo(expected));
        }

        [Test]
        public void Strongest_team_leader_is_lowest_skill_index_then_highest_page_rank_with_the_skill()
        {
            const string highestPageRank = "highest page rank, lowest skill index";
            _rankService.GetRank(highestPageRank).Returns(10);
            _skillsService.GetSkillIndex(highestPageRank, Arg.Any<string>()).Returns(3);

            const string lowestSkillIndex = "lowest skill index, lowest page rank";
            _rankService.GetRank(lowestSkillIndex).Returns(0);
            _skillsService.GetSkillIndex(lowestSkillIndex, Arg.Any<string>()).Returns(1);

            const string withoutSkill = "without skill";
            _rankService.GetRank(withoutSkill).Returns(20);
            _skillsService.GetSkillIndex(withoutSkill, Arg.Any<string>()).Returns(0);

            _separationService.GetDegreesBetween(Arg.Any<string>(), Arg.Any<string>()).Returns(1);

            _networkRepository.GetAll().Returns(new[]
                {
                    new Programmer(highestPageRank, null, null),
                    new Programmer(lowestSkillIndex, null, null),
                    new Programmer(withoutSkill, null, null)
                });

            var team = _teamService.FindStrongestTeam("skill", 3).ToList();

            Assert.That(team[0], Is.EqualTo(lowestSkillIndex));
            Assert.That(team[1], Is.EqualTo(highestPageRank));
            Assert.That(team.Count, Is.EqualTo(2));
        }

        [Test]
        public void Strongest_team_contains_connected_programmers_over_programmers_with_no_connections()
        {
            const string leader = "leader";
            _rankService.GetRank(leader).Returns(0);
            _skillsService.GetSkillIndex(leader, "skills").Returns(1);

            const string connected = "connected";
            _rankService.GetRank(connected).Returns(0);
            _skillsService.GetSkillIndex(connected, "skill").Returns(2);

            const string notConnected = "not connected";
            _rankService.GetRank(notConnected).Returns(0);
            _skillsService.GetSkillIndex(notConnected, "skill").Returns(2);

            _separationService.GetDegreesBetween(leader, connected).Returns(1);
            _separationService.GetDegreesBetween(leader, notConnected).Returns(0);

            _networkRepository.GetAll().Returns(new[]
            {
                new Programmer(leader, null, null),
                new Programmer(connected, null, null),
                new Programmer(notConnected, null, null)
            });

            var team = _teamService.FindStrongestTeam("skill", 2);

            Assert.That(team.All(programmer => programmer != notConnected), Is.True);
        }

        [Test]
        public void Strongest_team_must_have_at_least_one_member()
        {
            Assert.Throws<ArgumentException>(() => _teamService.FindStrongestTeam("", 0));
        }
    }
}
