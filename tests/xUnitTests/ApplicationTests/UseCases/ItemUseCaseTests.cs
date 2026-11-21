using Application.Commands.ModdedTemplateCommands;   // SetItemCommand, ClearItemCommand
using Application.ErrorMessages;                     // NotFoundError, BusinessRuleError, UnexpectedError
using Application.UseCases.ModdedTemplates;          // SetItemUseCase, ClearItemUseCase
using Domain.Entities;                               // ModdedTemplate
using Domain.Enums;                                  // AssetKind
using xUnitTests.ApplicationTests.Fakes;             // fakes

namespace xUnitTests.ApplicationTests.UseCases
{
    public class ItemUseCaseTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private readonly FakeModdedTemplateRepository moddedTemplates = new();
        private readonly FakeTemplateCatalog catalog = new();
        private readonly FakeAssetRepository assets = new();
        private readonly Guid projectId = Guid.NewGuid();
        private readonly ModdedTemplate menu;

        public ItemUseCaseTests()
        {
            menu = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(menu);
        }

        private SetItemUseCase SetItemUseCase() => new(moddedTemplates, catalog, assets, moddedTemplates);

        // ---------- SetItem ----------

        [Fact]
        public async Task SetItem_on_empty_slot_is_Created_and_saved()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "heading", "Velkommen", null, null), Ct);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Created);   // -> 201
            Assert.Equal("Velkommen", result.Value.Item.Text);
            Assert.Single(menu.Items);
            Assert.Equal(1, moddedTemplates.UpdateCount);
        }

        [Fact]
        public async Task SetItem_on_filled_slot_is_replaced_not_created()
        {
            var useCase = SetItemUseCase();
            await useCase.SetItemAsync(new SetItemCommand(menu.Id, "heading", "Første", null, null), Ct);

            var result = await useCase.SetItemAsync(new SetItemCommand(menu.Id, "heading", "Anden", null, null), Ct);

            Assert.False(result.Value.Created);   // -> 200
            Assert.Equal("Anden", Assert.Single(menu.Items).Text);
        }

        [Fact]
        public async Task SetItem_on_unknown_modded_template_is_NotFound()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(Guid.NewGuid(), "heading", "Hej", null, null), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }

        [Fact]
        public async Task SetItem_when_template_is_gone_from_catalog_is_UnexpectedError()
        {
            catalog.Templates.Clear();

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "heading", "Hej", null, null), Ct);

            Assert.True(result.HasError<UnexpectedError>());
        }

        [Fact]
        public async Task SetItem_with_unknown_slot_is_BusinessRuleError()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "button99", "Hej", null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(menu.Items);
            Assert.Equal(0, moddedTemplates.UpdateCount);
        }

        [Fact]
        public async Task SetItem_with_too_long_text_is_BusinessRuleError()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "heading", new string('a', 41), null, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        [Fact]
        public async Task SetItem_with_matching_asset_works()
        {
            var image = TestData.Asset(AssetKind.Image);
            assets.Assets.Add(image);

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "background", null, image.Id, null), Ct);

            Assert.Equal(image.Id, result.Value.Item.AssetId);
        }

        [Fact]
        public async Task SetItem_with_unknown_asset_is_NotFound()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "background", null, Guid.NewGuid(), null), Ct);

            Assert.True(result.HasError<NotFoundError>());
            Assert.Empty(menu.Items);
        }

        [Fact]
        public async Task SetItem_with_wrong_asset_kind_is_BusinessRuleError()
        {
            var audio = TestData.Asset(AssetKind.Audio);
            assets.Assets.Add(audio);

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "background", null, audio.Id, null), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        [Fact]
        public async Task SetItem_button_to_template_in_same_project_works()
        {
            var target = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(target);

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "button1", "Quiz", null, target.Id), Ct);

            Assert.Equal(target.Id, result.Value.Item.TargetModdedTemplateId);
        }

        [Fact]
        public async Task SetItem_button_to_template_in_other_project_is_BusinessRuleError()
        {
            var otherProject = TestData.MenuModdedTemplate();   // random project id
            moddedTemplates.ModdedTemplates.Add(otherProject);

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "button1", "Quiz", null, otherProject.Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            Assert.Empty(menu.Items);
        }

        [Fact]
        public async Task SetItem_button_to_unknown_template_is_BusinessRuleError()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "button1", "Quiz", null, Guid.NewGuid()), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        [Fact]
        public async Task SetItem_button_to_itself_is_BusinessRuleError()
        {
            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "button1", "Mig", null, menu.Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        [Fact]
        public async Task SetItem_target_on_text_slot_is_BusinessRuleError()
        {
            var target = TestData.MenuModdedTemplate(projectId);
            moddedTemplates.ModdedTemplates.Add(target);

            var result = await SetItemUseCase().SetItemAsync(new SetItemCommand(menu.Id, "heading", "Hej", null, target.Id), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
        }

        // ---------- ClearItem ----------

        [Fact]
        public async Task ClearItem_removes_item_and_saves()
        {
            menu.SetItem(TestData.MenuTemplate(), "heading", "Velkommen", null, null);
            var useCase = new ClearItemUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.ClearItemAsync(new ClearItemCommand(menu.Id, "heading"), Ct);

            Assert.True(result.IsSuccess);
            Assert.Empty(menu.Items);
            Assert.Equal(1, moddedTemplates.UpdateCount);
        }

        [Fact]
        public async Task ClearItem_on_empty_slot_still_succeeds()
        {
            var useCase = new ClearItemUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.ClearItemAsync(new ClearItemCommand(menu.Id, "heading"), Ct);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ClearItem_on_unknown_modded_template_is_NotFound()
        {
            var useCase = new ClearItemUseCase(moddedTemplates, moddedTemplates);

            var result = await useCase.ClearItemAsync(new ClearItemCommand(Guid.NewGuid(), "heading"), Ct);

            Assert.True(result.HasError<NotFoundError>());
        }
    }
}
