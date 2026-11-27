using Domain.Entities;        // Template
using Domain.Enums;           // TemplateCategory, SlotKind
using Domain.ValueObjects;    // SlotDefinition

namespace MoqTests.UseCases.ModdedTemplates
{
    internal static class TemplateTestData
    {
        public const string MenuType = "menu-ocean";

        // A small menu blueprint with one slot of each kind the tests need.
        public static Template Menu() => Template.Create(MenuType, "Menu - Hav", TemplateCategory.Menu, new[]
        {
            new SlotDefinition("title", "Titel", SlotKind.Text, MaxLength: 50),
            new SlotDefinition("image", "Billede", SlotKind.Image),
            new SlotDefinition("button1", "Knap 1", SlotKind.Button, MaxLength: 30),
        });
    }
}
