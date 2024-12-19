
using AutoMapper;

namespace SP_Shopping.Test.Profiles;

[TestClass]
public class ValidateProfiles
{
    [TestMethod]
    public void AutoMapper_ValidateAllProfiles_Succeeds_WhenNotThrowException()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddMaps(new[] {
            nameof(SP_Shopping)
        }));
        var mapper = config.CreateMapper();
        // Act
        // Assert
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
