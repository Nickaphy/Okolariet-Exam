using Domain.Entities;       // Project
using Domain.ValueObjects;   // Appearance

namespace xUnitTests.DomainTests
{
    public class ProjectTests
    {
        private static readonly Appearance Ocean = new("#0A6E8A", "#FFB703", "#FFFFFF", "Arial");

        [Fact]
        public void Create_trims_name_and_defaults_appearance_to_Empty()
        {
            var project = Project.Create("  Havets Hemmeligheder  ");

            Assert.Equal("Havets Hemmeligheder", project.Name);
            Assert.Equal(Appearance.Empty, project.Appearance);
            Assert.NotEqual(Guid.Empty, project.Id);
        }

        [Fact]
        public void Create_keeps_given_appearance()
        {
            var project = Project.Create("Havet", Ocean);

            Assert.Equal(Ocean, project.Appearance);
        }

        [Fact]
        public void New_project_has_no_start_screen_and_is_not_published()
        {
            var project = Project.Create("Havet");

            Assert.Null(project.MainModdedTemplateId);
            Assert.Null(project.LatestVersionNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_without_name_throws(string? name)
        {
            Assert.Throws<ArgumentException>(() => Project.Create(name!));
        }

        [Fact]
        public void Name_of_120_characters_is_ok_but_121_throws()
        {
            Project.Create(new string('a', 120));

            Assert.Throws<ArgumentException>(() => Project.Create(new string('a', 121)));
        }

        [Fact]
        public void Surrounding_spaces_do_not_count_towards_the_length()
        {
            var project = Project.Create("  " + new string('a', 120) + "  ");

            Assert.Equal(120, project.Name.Length);
        }

        [Fact]
        public void Update_changes_name_start_screen_and_appearance()
        {
            var project = Project.Create("Havet");
            var startScreen = Guid.NewGuid();

            project.Update(" Skoven ", startScreen, Ocean);

            Assert.Equal("Skoven", project.Name);
            Assert.Equal(startScreen, project.MainModdedTemplateId);
            Assert.Equal(Ocean, project.Appearance);
        }

        [Fact]
        public void Update_with_null_appearance_resets_to_Empty()
        {
            var project = Project.Create("Havet", Ocean);

            project.Update("Havet", null, null);

            Assert.Equal(Appearance.Empty, project.Appearance);
        }

        [Fact]
        public void Update_with_invalid_name_throws_and_keeps_old_values()
        {
            var project = Project.Create("Havet", Ocean);

            Assert.Throws<ArgumentException>(() => project.Update("", Guid.NewGuid(), null));

            Assert.Equal("Havet", project.Name);
            Assert.Null(project.MainModdedTemplateId);
            Assert.Equal(Ocean, project.Appearance);
        }

        [Fact]
        public void RegisterPublishedVersion_sets_latest_version_number()
        {
            var project = Project.Create("Havet");

            project.RegisterPublishedVersion(3);

            Assert.Equal(3, project.LatestVersionNumber);
        }
    }
}
