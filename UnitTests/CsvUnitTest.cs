using AssimilationSoftware.Maroon.Mappers.Csv;

namespace UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test_Basic_Tokenise()
    {
        // Arrange
        var input = "\"Admin\",\"\",\"\",\"\",\"John\",\"\",\"mokalusofborg@gmail.com\",\"wfh\",\"No\",\"02/05/2025\",\"11:38:00\",\"02/05/2025\",\"12:00:11\",\"00:22:11\",\"0.37\",\"0.00\",\"0.00\"";
        var expectedTokens = new List<string> {"Admin","","","","John","","mokalusofborg@gmail.com","wfh","No","02/05/2025","11:38:00","02/05/2025","12:00:11","00:22:11","0.37","0.00","0.00" };

        // Act
        var tokens = input.Tokenise();

        // Assert
        Assert.Equal(expectedTokens, tokens);
    }
}