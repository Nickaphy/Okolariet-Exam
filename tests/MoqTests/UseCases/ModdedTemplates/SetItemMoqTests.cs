using Application.Commands.ModdedTemplateCommands;          // SetItemCommand
using Application.ErrorMessages;                            // NotFoundError, BusinessRuleError, UnexpectedError
using Application.Helpers;                                  // SetItemResult
using Application.OutBoundPort.IAssetRepository;            // IGetAssetByIdAsync
using Application.OutBoundPort.IModdedTemplateRepository;   // IGetModdedTemplateByIdAsync, IUpdateModdedTemplateAsync
using Application.OutBoundPort.ITemplateRepository;         // IGetTemplateByTypeAsync
using Application.UseCases.ModdedTemplates;                 // SetItemUseCase
using Domain.Entities;                                      // ModdedTemplate, Template, Asset
using Domain.Enums;                                         // AssetKind
using FluentResults;                                        // Result
using Moq;
using MoqTests.TestHelpers;                                 // MockTime

namespace MoqTests.UseCases.ModdedTemplates
{
    // Slots in TemplateTestData.Menu(): "title" (text, max 50), "image" (image), "button1" (button).
    public class SetItemMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly Mock<IGetModdedTemplateByIdAsync> getModdedTemplate = new();
        private readonly Mock<IGetTemplateByTypeAsync> getTemplate = new();
        private readonly Mock<IGetAssetByIdAsync> getAsset = new();
        private readonly Mock<IUpdateModdedTemplateAsync> update = new();

        private readonly Guid projectId = Guid.NewGuid();
        private readonly Template template = TemplateTestData.Menu();
        private readonly ModdedTemplate moddedTemplate;

        public SetItemMoqTests()
        {
            moddedTemplate = ModdedTemplate.Create(projectId, template, "Forside", null, 1);
            getModdedTemplate.Setup(g => g.GetModdedTemplateByIdAsync(moddedTemplate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(moddedTemplate);
            getTemplate.Setup(g => g.GetTemplateByTypeAsync(TemplateTestData.MenuType, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        }

        private SetItemUseCase CreateUseCase() => new(getModdedTemplate.Object, getTemplate.Object, getAsset.Object, update.Object);

        private Task<Result<SetItemResult>> SetItem(string slot, string? text = null, Guid? assetId = null, Guid? target = null)
            => CreateUseCase().SetItemAsync(new SetItemCommand(moddedTemplate.Id, slot, text, assetId, target), Ct);

        // Another modded template "in the database".
        private ModdedTemplate ExistingModdedTemplate(Guid inProject)
        {
            var other = ModdedTemplate.Create(inProject, template, "Underside", null, 2);
            getModdedTemplate.Setup(g => g.GetModdedTemplateByIdAsync(other.Id, It.IsAny<CancellationToken>())).ReturnsAsync(other);
            return other;
        }

        private Asset ExistingAsset(AssetKind kind, string contentType)
        {
            var asset = Asset.Create(kind, "fil", "stored-fil", contentType, 1_000, MockTime.Now);
            getAsset.Setup(g => g.GetAssetByIdAsync(asset.Id, It.IsAny<CancellationToken>())).ReturnsAsync(asset);
            return asset;
        }

        private void AssertNothingSaved()
        {
            Assert.Empty(moddedTemplate.Items);
            update.Verify(u => u.UpdateModdedTemplateAsync(It.IsAny<ModdedTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ---------- Create vs replace (201 vs 200) ----------

        [Fact]
        public async Task Filling_an_empty_slot_is_Created()
        {
            var result = await SetItem("title", "Velkommen");

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Created);
            Assert.Equal("Velkommen", Assert.Single(moddedTemplate.Items).Text);
            update.Verify(u => u.UpdateModdedTemplateAsync(moddedTemplate, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Filling_a_filled_slot_replaces_it_and_is_not_Created()
        {
            await SetItem("title", "Velkommen");

            var result = await SetItem("title", "Goddag");

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Created);
            Assert.Equal("Goddag", Assert.Single(moddedTemplate.Items).Text);
        }

        // ---------- Slot rules ----------

        [Fact]
        public async Task Unknown_slot_is_BusinessRuleError()
        {
            var result = await SetItem("does-not-exist", "Tekst");

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Text_at_the_slots_max_length_is_allowed()
        {
            var result = await SetItem("title", new string('a', 50));

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Text_over_the_slots_max_length_is_BusinessRuleError()
        {
            var result = await SetItem("title", new string('a', 51));

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        // ---------- Files ----------

        [Fact]
        public async Task Image_in_an_image_slot_is_allowed()
        {
            var image = ExistingAsset(AssetKind.Image, "image/png");

            var result = await SetItem("image", assetId: image.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(image.Id, result.Value.Item.AssetId);
        }

        [Fact]
        public async Task Audio_in_an_image_slot_is_BusinessRuleError()
        {
            var audio = ExistingAsset(AssetKind.Audio, "audio/mpeg");

            var result = await SetItem("image", assetId: audio.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Unknown_asset_is_NotFound()
        {
            var result = await SetItem("image", assetId: Guid.NewGuid());

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        // ---------- Buttons ----------

        [Fact]
        public async Task Button_to_a_modded_template_in_the_same_project_is_allowed()
        {
            var target = ExistingModdedTemplate(inProject: projectId);

            var result = await SetItem("button1", "Til quizzen", target: target.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(target.Id, result.Value.Item.TargetModdedTemplateId);
        }

        [Fact]
        public async Task Button_to_a_modded_template_in_another_project_is_BusinessRuleError()
        {
            var target = ExistingModdedTemplate(inProject: Guid.NewGuid());

            var result = await SetItem("button1", "Til quizzen", target: target.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Button_to_a_modded_template_that_does_not_exist_is_BusinessRuleError()
        {
            var result = await SetItem("button1", "Til quizzen", target: Guid.NewGuid());

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Button_to_its_own_modded_template_is_BusinessRuleError()
        {
            var result = await SetItem("button1", "Til mig selv", target: moddedTemplate.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Only_buttons_can_lead_somewhere()
        {
            var target = ExistingModdedTemplate(inProject: projectId);

            var result = await SetItem("title", "Tekst", target: target.Id);

            Assert.True(result.HasError<BusinessRuleError>());
            AssertNothingSaved();
        }

        // ---------- Missing data ----------

        [Fact]
        public async Task Unknown_modded_template_is_NotFound()
        {
            var result = await CreateUseCase().SetItemAsync(new SetItemCommand(Guid.NewGuid(), "title", "Tekst", null, null), Ct);

            Assert.True(result.HasError<NotFoundError>());
            AssertNothingSaved();
        }

        [Fact]
        public async Task Template_removed_from_the_catalog_is_UnexpectedError()
        {
            getTemplate.Setup(g => g.GetTemplateByTypeAsync(TemplateTestData.MenuType, It.IsAny<CancellationToken>())).ReturnsAsync((Template?)null);

            var result = await SetItem("title", "Tekst");

            Assert.True(result.HasError<UnexpectedError>());
            AssertNothingSaved();
        }
    }
}
