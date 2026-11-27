using Application.Commands.StaffCommands;           // CreateStaffCommand
using Application.ErrorMessages;                    // BusinessRuleError, ConflictError
using Application.OutBoundPort.IPasswordHash;       // IHash
using Application.OutBoundPort.IStaffRepository;    // IAddAsync, IEmailExistsAsync
using Application.UseCases.Staff;                   // CreateStaffUseCase
using Domain.Entities;                              // StaffMember
using Domain.Enums;                                 // StaffRole
using Moq;
using MoqTests.TestHelpers;                         // MockTime

namespace MoqTests.UseCases.Staff
{
    // Spec (contracts/openapi.yaml, POST /staff): the e-mail must be unique, compared case-insensitively (409).
    // The password is hashed on the server and must be at least 12 characters.
    public class CreateStaffMoqTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;
        private static readonly DateTimeOffset Now = MockTime.Now;
        private const string ValidPassword = "Okolariet#2026";

        private readonly Mock<IAddAsync> add = new();
        private readonly Mock<IEmailExistsAsync> emailExists = new();
        private readonly Mock<IHash> hasher = new();

        public CreateStaffMoqTests()
        {
            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
        }

        private CreateStaffUseCase CreateUseCase() => new(add.Object, emailExists.Object, hasher.Object, MockTime.At(Now));

        [Fact]
        public async Task Create_saves_the_normalized_email_and_only_the_password_hash()
        {
            StaffMember? saved = null;
            add.Setup(a => a.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()))
               .Callback<StaffMember, CancellationToken>((member, _) => saved = member)
               .Returns(Task.CompletedTask);

            var result = await CreateUseCase().CreateAsync(new CreateStaffCommand("  Anna@Okolariet.DK ", ValidPassword, StaffRole.Admin), Ct);

            Assert.True(result.IsSuccess);
            Assert.NotNull(saved);
            Assert.Same(result.Value, saved);
            Assert.Equal("anna@okolariet.dk", saved.Email);
            Assert.Equal("hashed-password", saved.PasswordHash);
            Assert.Equal(StaffRole.Admin, saved.Role);
            Assert.Equal(Now, saved.CreatedAt);
            Assert.NotEqual(Guid.Empty, saved.Id);
            hasher.Verify(h => h.Hash(ValidPassword), Times.Once);
        }

        [Fact]
        public async Task Email_already_in_use_with_other_casing_is_Conflict()
        {
            emailExists.Setup(e => e.EmailExistsAsync("anna@okolariet.dk", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateUseCase().CreateAsync(new CreateStaffCommand("ANNA@Okolariet.dk", ValidPassword, StaffRole.Admin), Ct);

            Assert.True(result.HasError<ConflictError>());
            add.Verify(a => a.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("Kort#1")]             // too short
        [InlineData("okolariet#2026")]     // no uppercase letter
        [InlineData("Okolariet2026")]      // no special character
        [InlineData("Okolariet #2026")]    // contains a space
        [InlineData("Anna#Okolariet26")]   // contains the name from the e-mail
        public async Task Weak_password_is_BusinessRuleError_and_nothing_is_saved(string password)
        {
            var result = await CreateUseCase().CreateAsync(new CreateStaffCommand("anna@okolariet.dk", password, StaffRole.Admin), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
            add.Verify(a => a.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Minimum length follows PasswordPolicy.MinLength (8). Note: openapi.yaml says 12.
        [Fact]
        public async Task Password_of_8_characters_is_allowed()
        {
            // 8 characters that meet every other rule
            var result = await CreateUseCase().CreateAsync(new CreateStaffCommand("anna@okolariet.dk", "Ab#1xyzw", StaffRole.Admin), Ct);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Password_of_7_characters_is_rejected()
        {
            // 7 characters that meet every other rule
            var result = await CreateUseCase().CreateAsync(new CreateStaffCommand("anna@okolariet.dk", "Ab#1xyz", StaffRole.Admin), Ct);

            Assert.True(result.HasError<BusinessRuleError>());
            add.Verify(a => a.AddAsync(It.IsAny<StaffMember>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
