using FlowRight.Core.Results;
using ShadowrunGM.Domain.Character.Events;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.Character;

/// <summary>
/// Comprehensive tests for the Character aggregate covering all business rules and operations.
/// Tests are organized by functionality and follow TDD principles - these tests FAIL first.
/// </summary>
public sealed class CharacterTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Create_WithValidInputs_ShouldRaiseDomainEvent()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, []);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            // Should raise CharacterCreated domain event
            character.DomainEvents.Count.ShouldBe(1);
            character.DomainEvents.First().ShouldBeOfType<CharacterCreated>();
            
            CharacterCreated createdEvent = character.DomainEvents.OfType<CharacterCreated>().Single();
            createdEvent.CharacterId.ShouldBe(character.Id);
            createdEvent.Name.ShouldBe(characterName);
        }

        [Fact]
        public void Create_WithValidInputs_ShouldSetTimestampsCorrectly()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;
            DateTime beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, []);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            DateTime afterCreation = DateTime.UtcNow.AddSeconds(1);
            character.CreatedAt.ShouldBeGreaterThan(beforeCreation);
            character.CreatedAt.ShouldBeLessThan(afterCreation);
            character.ModifiedAt.ShouldBe(character.CreatedAt);
        }

        [Fact]
        public void Create_WithValidInputs_ShouldInitializeHealthFromAttributes()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder()
                .WithBody(6)
                .WithWillpower(4)
                .Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, []);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            // Health should be initialized based on attributes
            character.Health.ShouldNotBeNull();
            // Physical track = (Body / 2) + 8 = (6 / 2) + 8 = 11
            character.Health.PhysicalBoxes.ShouldBe(11);
            // Stun track = (Willpower / 2) + 8 = (4 / 2) + 8 = 10
            character.Health.StunBoxes.ShouldBe(10);
        }

        [Fact]
        public void Create_WithSkills_ShouldAddAllValidSkills()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;
            
            List<Skill> skills = [];
            
            Result<Skill> firearmsResult = Skill.Create("Firearms", 4);
            firearmsResult.TryGetValue(out Skill? firearms).ShouldBeTrue();
            firearms.ShouldNotBeNull();
            skills.Add(firearms);
            
            Result<Skill> hackingResult = Skill.Create("Hacking", 5, "Cybercombat");
            hackingResult.TryGetValue(out Skill? hacking).ShouldBeTrue();
            hacking.ShouldNotBeNull();
            skills.Add(hacking);
            
            Result<Skill> athleticsResult = Skill.Create("Athletics", 3);
            athleticsResult.TryGetValue(out Skill? athletics).ShouldBeTrue();
            athletics.ShouldNotBeNull();
            skills.Add(athletics);

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, skills);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            character.Skills.Count.ShouldBe(3);
            character.Skills.ShouldContain(s => s.Name == "Firearms" && s.Rating == 4);
            character.Skills.ShouldContain(s => s.Name == "Hacking" && s.Rating == 5 && s.Specialization == "Cybercombat");
            character.Skills.ShouldContain(s => s.Name == "Athletics" && s.Rating == 3);
        }

        [Fact]
        public void Create_WithNullSkills_ShouldSucceedWithEmptySkillList()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;
            IEnumerable<Skill>? nullSkills = null;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, nullSkills);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            character.Skills.Count.ShouldBe(0);
        }

        [Fact]
        public void Create_WithInvalidSkillInList_ShouldReturnFailure()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;
            
            // Create a skill that will fail validation
            Result<Skill> invalidSkillResult = Skill.Create("", 4); // Empty name should fail
            List<Skill> skills = [];
            
            // This test documents expected behavior - if Skill.Create fails, the character creation should fail
            // We need to create a scenario where a skill fails validation

            // Act & Assert - This test will fail until proper skill validation is implemented
            // The test documents that character creation with invalid skills should fail
            // Implementation note: This requires skills to have proper validation that can fail
        }
    }

    public sealed class SkillManagement
    {
        [Fact]
        public void AddSkill_WithValidSkill_ShouldAddSuccessfully()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            
            // Act
            Result<Skill> result = character.AddSkill("Firearms", 4, "Pistols");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? addedSkill).ShouldBeTrue();
            addedSkill.ShouldNotBeNull();
            addedSkill.Name.ShouldBe("Firearms");
            addedSkill.Rating.ShouldBe(4);
            addedSkill.Specialization.ShouldBe("Pistols");
            
            character.Skills.Count.ShouldBe(1);
            character.Skills.ShouldContain(addedSkill);
        }

        [Fact]
        public void AddSkill_WithValidSkill_ShouldUpdateModifiedTimestamp()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            DateTime originalModifiedAt = character.ModifiedAt;
            
            // Small delay to ensure timestamp difference
            Thread.Sleep(1);

            // Act
            Result<Skill> result = character.AddSkill("Firearms", 4);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            character.ModifiedAt.ShouldBeGreaterThan(originalModifiedAt);
        }

        [Fact]
        public void AddSkill_WithDuplicateName_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            Result<Skill> result = character.AddSkill("Firearms", 4);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("already exists");
            character.Skills.Count.ShouldBe(1); // Should still only have the original skill
        }

        [Fact]
        public void AddSkill_WithDuplicateNameDifferentCasing_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            Result<Skill> result = character.AddSkill("FIREARMS", 4);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("already exists");
        }

        [Fact]
        public void UpdateSkill_WithValidRating_ShouldUpdateSuccessfully()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            Result<Skill> result = character.UpdateSkill("Firearms", 5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? updatedSkill).ShouldBeTrue();
            updatedSkill.ShouldNotBeNull();
            updatedSkill.Rating.ShouldBe(5);
            
            character.Skills.Count.ShouldBe(1);
            character.Skills.First().Rating.ShouldBe(5);
        }

        [Fact]
        public void UpdateSkill_WithNonExistentSkill_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Act
            Result<Skill> result = character.UpdateSkill("Firearms", 5);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("not found");
        }

        [Fact]
        public void UpdateSkill_CaseInsensitiveName_ShouldWork()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            Result<Skill> result = character.UpdateSkill("FIREARMS", 5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            character.Skills.First().Rating.ShouldBe(5);
        }

        [Fact]
        public void RemoveSkill_WithExistingSkill_ShouldReturnTrue()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            bool result = character.RemoveSkill("Firearms");

            // Assert
            result.ShouldBeTrue();
            character.Skills.Count.ShouldBe(0);
        }

        [Fact]
        public void RemoveSkill_WithNonExistentSkill_ShouldReturnFalse()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Act
            bool result = character.RemoveSkill("Firearms");

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public void RemoveSkill_CaseInsensitiveName_ShouldWork()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.AddSkill("Firearms", 3);

            // Act
            bool result = character.RemoveSkill("FIREARMS");

            // Assert
            result.ShouldBeTrue();
            character.Skills.Count.ShouldBe(0);
        }
    }

    public sealed class EdgeOperations
    {
        [Fact]
        public void SpendEdge_WithValidAmountAndPurpose_ShouldSucceed()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            Result<EdgeSpent> result = character.SpendEdge(2, "Push the Limit");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out EdgeSpent? edgeSpentEvent).ShouldBeTrue();
            edgeSpentEvent.ShouldNotBeNull();
            edgeSpentEvent.CharacterId.ShouldBe(character.Id);
            edgeSpentEvent.Amount.ShouldBe(2);
            edgeSpentEvent.Purpose.ShouldBe("Push the Limit");
            
            character.Edge.Current.ShouldBe(3);
            character.DomainEvents.OfType<EdgeSpent>().Count().ShouldBe(1);
        }

        [Fact]
        public void SpendEdge_WithEmptyPurpose_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            Result<EdgeSpent> result = character.SpendEdge(2, "");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Purpose");
            character.Edge.Current.ShouldBe(5); // Should remain unchanged
        }

        [Fact]
        public void SpendEdge_WithWhitespacePurpose_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            Result<EdgeSpent> result = character.SpendEdge(2, "   ");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Purpose");
        }

        [Fact]
        public void SpendEdge_WithInsufficientEdge_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(2).Build();

            // Act
            Result<EdgeSpent> result = character.SpendEdge(3, "Push the Limit");

            // Assert
            result.IsFailure.ShouldBeTrue();
            character.Edge.Current.ShouldBe(2); // Should remain unchanged
        }

        [Fact]
        public void RegainEdge_WithValidAmount_ShouldSucceed()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();
            character.SpendEdge(3, "Test spending");

            // Act
            Result<Edge> result = character.RegainEdge(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(4);
            character.Edge.Current.ShouldBe(4);
        }

        [Fact]
        public void RegainEdge_AboveMaximum_ShouldCapAtMaximum()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(3).Build();
            character.SpendEdge(1, "Test spending");

            // Act
            Result<Edge> result = character.RegainEdge(5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            character.Edge.Current.ShouldBe(3); // Should cap at maximum
        }

        [Fact]
        public void RefreshEdge_ShouldRestoreToMaximum()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();
            character.SpendEdge(4, "Test spending");

            // Act
            character.RefreshEdge();

            // Assert
            character.Edge.Current.ShouldBe(5);
        }

        [Fact]
        public void BurnEdge_WithDefaultAmount_ShouldReduceMaximumByOne()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            Result<Edge> result = character.BurnEdge();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Max.ShouldBe(4);
            newEdge.Current.ShouldBe(4); // Current should adjust to new maximum
        }

        [Fact]
        public void BurnEdge_WithSpecificAmount_ShouldReduceMaximumByAmount()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            Result<Edge> result = character.BurnEdge(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            character.Edge.Max.ShouldBe(3);
            character.Edge.Current.ShouldBe(3);
        }

        [Fact]
        public void BurnEdge_WhenWouldReduceBelowZero_ShouldReturnFailure()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(1).Build();

            // Act
            Result<Edge> result = character.BurnEdge(2);

            // Assert
            result.IsFailure.ShouldBeTrue();
            character.Edge.Max.ShouldBe(1); // Should remain unchanged
        }
    }

    public sealed class HealthManagement
    {
        [Fact]
        public void TakePhysicalDamage_WithValidAmount_ShouldReduceHealth()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            int originalPhysicalDamage = character.Health.PhysicalDamage;

            // Act
            Result<ConditionMonitor> result = character.TakePhysicalDamage(3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newHealth).ShouldBeTrue();
            newHealth.ShouldNotBeNull();
            newHealth.PhysicalDamage.ShouldBe(originalPhysicalDamage + 3);
            character.Health.PhysicalDamage.ShouldBe(originalPhysicalDamage + 3);
        }

        [Fact]
        public void TakeStunDamage_WithValidAmount_ShouldReduceHealth()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            int originalStunDamage = character.Health.StunDamage;

            // Act
            Result<ConditionMonitor> result = character.TakeStunDamage(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newHealth).ShouldBeTrue();
            newHealth.ShouldNotBeNull();
            newHealth.StunDamage.ShouldBe(originalStunDamage + 2);
            character.Health.StunDamage.ShouldBe(originalStunDamage + 2);
        }

        [Fact]
        public void HealPhysicalDamage_WithValidAmount_ShouldRestoreHealth()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.TakePhysicalDamage(5);

            // Act
            Result<ConditionMonitor> result = character.HealPhysicalDamage(3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newHealth).ShouldBeTrue();
            newHealth.ShouldNotBeNull();
            newHealth.PhysicalDamage.ShouldBe(2);
            character.Health.PhysicalDamage.ShouldBe(2);
        }

        [Fact]
        public void HealStunDamage_WithValidAmount_ShouldRestoreHealth()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.TakeStunDamage(4);

            // Act
            Result<ConditionMonitor> result = character.HealStunDamage(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newHealth).ShouldBeTrue();
            newHealth.ShouldNotBeNull();
            newHealth.StunDamage.ShouldBe(2);
            character.Health.StunDamage.ShouldBe(2);
        }

        [Fact]
        public void HealAll_ShouldRestoreAllHealth()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.TakePhysicalDamage(3);
            character.TakeStunDamage(4);

            // Act
            character.HealAll();

            // Assert
            character.Health.PhysicalDamage.ShouldBe(0);
            character.Health.StunDamage.ShouldBe(0);
        }

        [Fact]
        public void HealAll_ShouldUpdateModifiedTimestamp()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            character.TakePhysicalDamage(1);
            DateTime originalModifiedAt = character.ModifiedAt;
            
            // Small delay to ensure timestamp difference
            Thread.Sleep(1);

            // Act
            character.HealAll();

            // Assert
            character.ModifiedAt.ShouldBeGreaterThan(originalModifiedAt);
        }
    }

    public sealed class BusinessRuleValidation
    {
        [Fact]
        public void Character_ShouldMaintainAggregateInvariants_HealthAlwaysValid()
        {
            // Arrange & Act
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Assert - Health should always be in valid state
            character.Health.ShouldNotBeNull();
            character.Health.PhysicalDamage.ShouldBeGreaterThanOrEqualTo(0);
            character.Health.StunDamage.ShouldBeGreaterThanOrEqualTo(0);
            character.Health.PhysicalBoxes.ShouldBeGreaterThan(0);
            character.Health.StunBoxes.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Character_ShouldMaintainAggregateInvariants_EdgeAlwaysValid()
        {
            // Arrange & Act
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Assert - Edge should always be in valid state
            character.Edge.ShouldNotBeNull();
            character.Edge.Current.ShouldBeGreaterThanOrEqualTo(0);
            character.Edge.Max.ShouldBeGreaterThan(0);
            character.Edge.Current.ShouldBeLessThanOrEqualTo(character.Edge.Max);
        }

        [Fact]
        public void Character_ShouldMaintainAggregateInvariants_UniqueSkillNames()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Act
            character.AddSkill("Firearms", 4);
            character.AddSkill("Hacking", 3);
            character.AddSkill("Athletics", 2);

            // Assert - All skill names should be unique (case insensitive)
            List<string> skillNames = [.. character.Skills.Select(s => s.Name.ToLowerInvariant())];
            skillNames.Distinct().Count().ShouldBe(skillNames.Count);
        }

        [Fact]
        public void Character_ShouldMaintainAggregateInvariants_TimestampsConsistent()
        {
            // Arrange & Act
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();

            // Assert - Timestamps should be consistent
            character.CreatedAt.ShouldBeLessThanOrEqualTo(character.ModifiedAt);
        }

        [Fact]
        public void Character_AllOperations_ShouldUpdateModifiedAtTimestamp()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.Build();
            DateTime originalModifiedAt = character.ModifiedAt;

            // Act & Assert - All state-changing operations should update ModifiedAt
            Thread.Sleep(1);
            
            character.AddSkill("Firearms", 4);
            character.ModifiedAt.ShouldBeGreaterThan(originalModifiedAt);
            originalModifiedAt = character.ModifiedAt;
            
            Thread.Sleep(1);
            character.SpendEdge(1, "Test");
            character.ModifiedAt.ShouldBeGreaterThan(originalModifiedAt);
            originalModifiedAt = character.ModifiedAt;
            
            Thread.Sleep(1);
            character.TakePhysicalDamage(1);
            character.ModifiedAt.ShouldBeGreaterThan(originalModifiedAt);
        }
    }

    public sealed class DomainEventBehavior
    {
        [Fact]
        public void Character_WhenCreated_ShouldRaiseCharacterCreatedEvent()
        {
            // Arrange
            string characterName = "Test Runner";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributes, startingEdge, []);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            
            character.DomainEvents.Count.ShouldBe(1);
            CharacterCreated createdEvent = character.DomainEvents.OfType<CharacterCreated>().Single();
            createdEvent.CharacterId.ShouldBe(character.Id);
            createdEvent.Name.ShouldBe(characterName);
        }

        [Fact]
        public void Character_WhenEdgeSpent_ShouldRaiseEdgeSpentEvent()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(3).Build();
            character.ClearDomainEvents(); // Clear creation event

            // Act
            Result<EdgeSpent> result = character.SpendEdge(2, "Push the Limit");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            character.DomainEvents.Count.ShouldBe(1);
            EdgeSpent edgeSpentEvent = character.DomainEvents.OfType<EdgeSpent>().Single();
            edgeSpentEvent.CharacterId.ShouldBe(character.Id);
            edgeSpentEvent.Amount.ShouldBe(2);
            edgeSpentEvent.Purpose.ShouldBe("Push the Limit");
        }

        [Fact]
        public void Character_DomainEvents_ShouldPersistUntilCleared()
        {
            // Arrange
            CharacterBuilder builder = new();
            ShadowrunGM.Domain.Character.Character character = builder.WithStartingEdge(5).Build();

            // Act
            character.SpendEdge(1, "First spend");
            character.SpendEdge(1, "Second spend");

            // Assert
            character.DomainEvents.Count.ShouldBe(3); // 1 creation + 2 edge spent events
            character.DomainEvents.OfType<CharacterCreated>().Count().ShouldBe(1);
            character.DomainEvents.OfType<EdgeSpent>().Count().ShouldBe(2);
        }
    }
}