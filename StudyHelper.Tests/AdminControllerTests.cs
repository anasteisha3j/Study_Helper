using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using StudyApp.Data;
using StudyApp.Models;
using StudyHelper.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


public class AdminControllerTests
{
    private void SeedRoles(ApplicationDbContext dbContext)
{
    dbContext.Roles.AddRange(
        new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
        new IdentityRole { Name = "User", NormalizedName = "USER" }
    );
    dbContext.SaveChanges();
}
private Mock<UserManager<User>> GetUserManagerMock()
{
    var store = new Mock<IUserStore<User>>();
    return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
}


    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private UserManager<User> GetRealUserManager(ApplicationDbContext context)
    {
        var store = new UserStore<User>(context);
        return new UserManager<User>(
            store,
            new Mock<IOptions<IdentityOptions>>().Object,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<User>>>().Object);
    }

    [Fact]
public async Task Index_ReturnsViewResult()
{
    var dbContext = GetInMemoryDbContext();
    SeedRoles(dbContext); 
    

    var userManagerMock = GetUserManagerMock();
    userManagerMock.Setup(um => um.Users).Returns(new List<User>
    {
        new User { Id = "1", Email = "test@example.com", FullName = "Test User" }
    }.AsQueryable());

    userManagerMock.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), "Admin")).ReturnsAsync(false);
    userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });

    var controller = new AdminController(userManagerMock.Object, dbContext);

    var result = await controller.Index();

    Assert.IsType<ViewResult>(result);
}


    [Fact]
    public async Task DeleteUser_ValidId_RedirectsToIndex()
    {
        var dbContext = GetInMemoryDbContext();
        var userManager = GetRealUserManager(dbContext);
        var user = new User { Id = "1", Email = "test@example.com" };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var controller = new AdminController(userManager, dbContext);
        var result = await controller.DeleteUser("1");

        Assert.IsType<RedirectToActionResult>(result);
    }

   [Fact]
public async Task MakeAdmin_ValidId_AddsRole()
{
    var dbContext = GetInMemoryDbContext();
    SeedRoles(dbContext); 

    var user = new User { Id = "1", Email = "admin@example.com" };
    var userManagerMock = GetUserManagerMock();
    userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
    userManagerMock.Setup(um => um.AddToRoleAsync(user, "Admin"))
        .ReturnsAsync(IdentityResult.Success);

    var controller = new AdminController(userManagerMock.Object, dbContext);

    var result = await controller.MakeAdmin("1");

    Assert.IsType<RedirectToActionResult>(result);
}

    [Fact]
    public async Task StorageViolations_ReturnsViewWithViolations()
    {
        var dbContext = GetInMemoryDbContext();
        dbContext.StorageViolations.Add(new StorageViolation
        {
            Id = 1,
            ViolationDate = DateTime.UtcNow,
            AttemptedFileType = "exe",
            UserEmail = "user@example.com",
            UserId = "1"
        });
        dbContext.SaveChanges();

        var controller = new AdminController(GetRealUserManager(dbContext), dbContext);
        var result = await controller.StorageViolations();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task UserDownloads_ReturnsViewWithStats()
    {
        var dbContext = GetInMemoryDbContext();
        var user = new User { Id = "1", Email = "user@example.com", FullName = "User" };
        dbContext.Users.Add(user);
        dbContext.Studies.Add(new Study { Id = 1, UserId = user.Id, Title = "Test Study" });
        dbContext.StudyFiles.Add(new StudyFile
        {
            Id = 1,
            FileSize = 1000,
            UploadDate = DateTime.UtcNow,
            StudyId = 1,
            FileType = "pdf",
            OriginalName = "example.pdf",
            StoragePath = "/fake/path/example.pdf"
        });
        dbContext.SaveChanges();

        var controller = new AdminController(GetRealUserManager(dbContext), dbContext);
        var result = await controller.UserDownloads();

        Assert.IsType<ViewResult>(result);
    }
}
