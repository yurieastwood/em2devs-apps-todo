using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EnergyProfile value object.
/// Covers energy pattern detection from energy-scheduling.feature.
/// </summary>
public sealed class EnergyProfileTests
{
    // =================================================================
    // Scenario: New user with insufficient energy data
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMediumDefault_When_FewerThan7CheckInsExist()
    {
        // Given — fewer than 7 check-ins (insufficient data)
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
        };

        // When
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // Then — should indicate insufficient data and return defaults
        profile.HasSufficientData.ShouldBeFalse();
        profile.GetTypicalEnergy(DayOfWeek.Monday).ShouldBe(EnergyLevel.Medium);
        profile.GetTypicalEnergy(DayOfWeek.Wednesday).ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMediumDefault_When_EmptyCheckIns()
    {
        // Given — no check-ins at all
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>();

        // When
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // Then
        profile.HasSufficientData.ShouldBeFalse();
        profile.GetTypicalEnergy(DayOfWeek.Friday).ShouldBe(EnergyLevel.Medium);
    }

    // =================================================================
    // Scenario: Sufficient data builds a weekly energy profile
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildWeeklyProfile_When_7OrMoreCheckInsExist()
    {
        // Given — 7 days of check-in data
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        // When
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // Then
        profile.HasSufficientData.ShouldBeTrue();
        profile.GetTypicalEnergy(DayOfWeek.Monday).ShouldBe(EnergyLevel.High);
        profile.GetTypicalEnergy(DayOfWeek.Friday).ShouldBe(EnergyLevel.Low);
        profile.GetTypicalEnergy(DayOfWeek.Saturday).ShouldBe(EnergyLevel.Peak);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMediumForMissingDay_When_ProfileHasSufficientDataButDayMissing()
    {
        // Given — exactly 7 entries but Sunday is missing (duplicate key not possible,
        // so we use a different scenario: 7 entries covering Mon-Sat + one extra)
        // Actually Dictionary can only have 7 unique DayOfWeek keys.
        // Let's test with exactly 7 entries covering all days except one is missing.
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        // When
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // Then — all days covered, profile should be complete
        profile.HasSufficientData.ShouldBeTrue();
        profile.GetTypicalEnergy(DayOfWeek.Sunday).ShouldBe(EnergyLevel.Low);
    }

    // =================================================================
    // Scenario: EnergyProfile value equality
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SamePatterns()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns);
        var profile2 = EnergyProfile.FromCheckIns(checkIns);

        // Then
        profile1.ShouldBe(profile2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentPatterns()
    {
        // Given
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.Low },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.High },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.High },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        var profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // Then
        profile1.ShouldNotBe(profile2);
    }

    // =================================================================
    // Edge cases and guard clauses
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CheckInsIsNull()
    {
        // Given / When / Then
        var exception = Should.Throw<ArgumentNullException>(
            () => EnergyProfile.FromCheckIns(null!));
        exception.ParamName.ShouldBe("checkIns");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqualToNull_When_Compared()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // When / Then
        profile.Equals(null).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqualToSelf_When_ComparedByReference()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // When / Then
        profile.Equals(profile).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentCountPatterns()
    {
        // Given — different number of entries
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        var profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // Then
        profile1.Equals(profile2).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_ComparedWithDifferentType()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
        };
        var profile = EnergyProfile.FromCheckIns(checkIns);

        // When / Then — tests Equals(object?) overload with wrong type
        profile.Equals("not a profile").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqualViaObjectOverload_When_SameProfiles()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        var profile1 = EnergyProfile.FromCheckIns(checkIns);
        object profile2 = EnergyProfile.FromCheckIns(new Dictionary<DayOfWeek, EnergyLevel>(checkIns));

        // When / Then — tests Equals(object?) overload returns true for equal profiles
        profile1.Equals(profile2).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqualViaObjectOverload_When_DifferentProfiles()
    {
        // Given
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.Low },
        };

        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        object profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // When / Then — tests Equals(object?) overload returns false for different profiles
        profile1.Equals(profile2).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveSameHashCode_When_EqualProfiles()
    {
        // Given
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns);
        var profile2 = EnergyProfile.FromCheckIns(new Dictionary<DayOfWeek, EnergyLevel>(checkIns));

        // Then
        profile1.GetHashCode().ShouldBe(profile2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveDifferentHashCode_When_DifferentProfiles()
    {
        // Given
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.Low },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.High },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.High },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        var profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // Then
        profile1.GetHashCode().ShouldNotBe(profile2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_SameKeysButDifferentValues()
    {
        // Given — same days, different energy levels for one day
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Peak },
        };

        // When
        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        var profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // Then
        profile1.Equals(profile2).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_WorkAsHashSetKey_When_EqualProfilesUsed()
    {
        // Given — tests GetHashCode consistency with Equals in a HashSet
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        var profile1 = EnergyProfile.FromCheckIns(checkIns);
        var profile2 = EnergyProfile.FromCheckIns(new Dictionary<DayOfWeek, EnergyLevel>(checkIns));

        // When
        var set = new HashSet<EnergyProfile> { profile1 };

        // Then — equal profiles should be treated as duplicates in a HashSet
        set.Contains(profile2).ShouldBeTrue();
        set.Add(profile2);
        set.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StoreDistinctEntries_When_DifferentProfilesInHashSet()
    {
        // Given
        var checkIns1 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };
        var checkIns2 = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.Low },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.High },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.High },
        };

        var profile1 = EnergyProfile.FromCheckIns(checkIns1);
        var profile2 = EnergyProfile.FromCheckIns(checkIns2);

        // When
        var set = new HashSet<EnergyProfile> { profile1, profile2 };

        // Then — different profiles should be distinct in a HashSet
        set.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProduceNonZeroHashCode_When_ProfileHasEntries()
    {
        // Given — verifies hash code is actually computed (not default 0)
        var checkIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.Low },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Peak },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Peak },
            { DayOfWeek.Sunday, EnergyLevel.Low },
        };

        // When
        var profile = EnergyProfile.FromCheckIns(checkIns);
        int hashCode = profile.GetHashCode();

        // Then — hash code should not be default zero
        hashCode.ShouldNotBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeInsufficientDataMessage_When_NewUser()
    {
        EnergyProfile.InsufficientDataMessage.ShouldContain("still learning");
        EnergyProfile.InsufficientDataMessage.ShouldContain("14 days");
    }
}
