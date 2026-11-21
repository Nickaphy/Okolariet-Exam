using Domain.Entities;       // ModdedTemplate
using Domain.Enums;          // AssetKind
using Domain.ValueObjects;   // Appearance

namespace xUnitTests.DomainTests
{
    public class ModdedTemplateTests
    {
        // ---------- Create / Update ----------

        [Fact]
        public void Create_copies_template_type_and_defaults_appearance_to_Empty()
        {
            var projectId = Guid.NewGuid();

            var mt = ModdedTemplate.Create(projectId, TestData.MenuTemplate(), "Forside", null, 2);

            Assert.Equal(projectId, mt.ProjectId);
            Assert.Equal(TestData.MenuType, mt.TemplateType);
            Assert.Equal("Forside", mt.Title);
            Assert.Equal(Appearance.Empty, mt.Appearance);
            Assert.Equal(2, mt.SortOrder);
            Assert.Empty(mt.Items);
        }

        [Fact]
        public void Create_keeps_given_appearance()
        {
            var appearance = new Appearance("#0A6E8A", null, null, "Nunito");

            var mt = ModdedTemplate.Create(Guid.NewGuid(), TestData.MenuTemplate(), "Forside", appearance, 0);

            Assert.Equal(appearance, mt.Appearance);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_with_blank_title_throws(string title)
        {
            Assert.Throws<ArgumentException>(() => ModdedTemplate.Create(Guid.NewGuid(), TestData.MenuTemplate(), title, null, 0));
        }

        [Fact]
        public void Title_of_120_is_ok_but_121_throws()
        {
            var ok = ModdedTemplate.Create(Guid.NewGuid(), TestData.MenuTemplate(), new string('a', 120), null, 0);

            Assert.Equal(120, ok.Title.Length);
            Assert.Throws<ArgumentException>(() => ModdedTemplate.Create(Guid.NewGuid(), TestData.MenuTemplate(), new string('a', 121), null, 0));
        }

        [Fact]
        public void Update_changes_title_appearance_and_sort_order_but_not_type()
        {
            var mt = TestData.MenuModdedTemplate();
            var appearance = new Appearance(null, "#FF0000", null, null);

            mt.Update("Ny titel", appearance, 5);

            Assert.Equal("Ny titel", mt.Title);
            Assert.Equal(appearance, mt.Appearance);
            Assert.Equal(5, mt.SortOrder);
            Assert.Equal(TestData.MenuType, mt.TemplateType);
        }

        [Fact]
        public void Update_with_null_appearance_resets_to_Empty()
        {
            var mt = ModdedTemplate.Create(Guid.NewGuid(), TestData.MenuTemplate(), "Forside", new Appearance("#000000", null, null, null), 0);

            mt.Update("Forside", null, 0);

            Assert.Equal(Appearance.Empty, mt.Appearance);
        }

        [Fact]
        public void Update_with_invalid_title_throws_and_keeps_old_title()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.Update("", null, 0));
            Assert.Equal("Forside", mt.Title);
        }

        // ---------- SetItem ----------

        [Fact]
        public void SetItem_on_empty_slot_creates_an_item()
        {
            var mt = TestData.MenuModdedTemplate();

            var item = mt.SetItem(TestData.MenuTemplate(), "heading", "Velkommen", null, null);

            Assert.Same(item, Assert.Single(mt.Items));
            Assert.Equal(mt.Id, item.ModdedTemplateId);
            Assert.Equal("heading", item.SlotKey);
            Assert.Equal("Velkommen", item.Text);
        }

        [Fact]
        public void SetItem_twice_on_same_slot_replaces_instead_of_adding()
        {
            var mt = TestData.MenuModdedTemplate();
            var template = TestData.MenuTemplate();

            var first = mt.SetItem(template, "heading", "Første", null, null);
            var second = mt.SetItem(template, "heading", "Anden", null, null);

            Assert.Single(mt.Items);
            Assert.Same(first, second);   // same object, updated in place
            Assert.Equal("Anden", second.Text);
        }

        [Fact]
        public void SetItem_on_different_slots_gives_one_item_each()
        {
            var mt = TestData.MenuModdedTemplate();
            var template = TestData.MenuTemplate();

            mt.SetItem(template, "heading", "Velkommen", null, null);
            mt.SetItem(template, "background", null, TestData.Asset(AssetKind.Image), null);

            Assert.Equal(2, mt.Items.Count);
        }

        [Fact]
        public void SetItem_with_another_template_throws()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.SetItem(TestData.QuizTemplate(), "heading", "Hej", null, null));
        }

        [Fact]
        public void SetItem_with_unknown_slot_throws()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.SetItem(TestData.MenuTemplate(), "button99", "Hej", null, null));
            Assert.Empty(mt.Items);
        }

        [Fact]
        public void SetItem_text_at_max_length_is_ok_but_one_more_throws()
        {
            var mt = TestData.MenuModdedTemplate();
            var template = TestData.MenuTemplate();   // heading has MaxLength 40

            mt.SetItem(template, "heading", new string('a', 40), null, null);

            Assert.Throws<ArgumentException>(() => mt.SetItem(template, "heading", new string('a', 41), null, null));
        }

        [Fact]
        public void SetItem_with_matching_asset_stores_the_asset_id()
        {
            var mt = TestData.MenuModdedTemplate();
            var image = TestData.Asset(AssetKind.Image);

            var item = mt.SetItem(TestData.MenuTemplate(), "background", null, image, null);

            Assert.Equal(image.Id, item.AssetId);
        }

        [Theory]
        [InlineData("background", AssetKind.Audio)]   // audio in an image slot
        [InlineData("video", AssetKind.Image)]        // image in a video slot
        [InlineData("sound", AssetKind.Video)]        // video in an audio slot
        [InlineData("heading", AssetKind.Image)]      // any file in a text slot
        public void SetItem_with_wrong_asset_kind_throws(string slotKey, AssetKind assetKind)
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.SetItem(TestData.MenuTemplate(), slotKey, null, TestData.Asset(assetKind), null));
        }

        [Fact]
        public void SetItem_target_on_button_slot_is_ok()
        {
            var mt = TestData.MenuModdedTemplate();
            var targetId = Guid.NewGuid();

            var item = mt.SetItem(TestData.MenuTemplate(), "button1", "Quiz", null, targetId);

            Assert.Equal(targetId, item.TargetModdedTemplateId);
        }

        [Fact]
        public void SetItem_target_on_non_button_slot_throws()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.SetItem(TestData.MenuTemplate(), "heading", "Hej", null, Guid.NewGuid()));
        }

        [Fact]
        public void SetItem_button_pointing_to_itself_throws()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Throws<ArgumentException>(() => mt.SetItem(TestData.MenuTemplate(), "button1", "Mig selv", null, mt.Id));
        }

        [Fact]
        public void SetItem_replacing_can_clear_old_values()
        {
            var mt = TestData.MenuModdedTemplate();
            var template = TestData.MenuTemplate();
            mt.SetItem(template, "button1", "Quiz", null, Guid.NewGuid());

            var item = mt.SetItem(template, "button1", "Ingen link", null, null);

            Assert.Null(item.TargetModdedTemplateId);
        }

        // ---------- FindItem / ClearItem ----------

        [Fact]
        public void FindItem_returns_null_for_empty_slot()
        {
            var mt = TestData.MenuModdedTemplate();

            Assert.Null(mt.FindItem("heading"));
        }

        [Fact]
        public void ClearItem_removes_the_item()
        {
            var mt = TestData.MenuModdedTemplate();
            mt.SetItem(TestData.MenuTemplate(), "heading", "Velkommen", null, null);

            mt.ClearItem("heading");

            Assert.Empty(mt.Items);
        }

        [Fact]
        public void ClearItem_only_removes_that_slot()
        {
            var mt = TestData.MenuModdedTemplate();
            var template = TestData.MenuTemplate();
            mt.SetItem(template, "heading", "Velkommen", null, null);
            mt.SetItem(template, "button1", "Start", null, null);

            mt.ClearItem("heading");

            Assert.Equal("button1", Assert.Single(mt.Items).SlotKey);
        }

        [Fact]
        public void ClearItem_on_empty_slot_does_not_throw()
        {
            var mt = TestData.MenuModdedTemplate();

            var ex = Record.Exception(() => mt.ClearItem("heading"));

            Assert.Null(ex);
        }
    }
}
