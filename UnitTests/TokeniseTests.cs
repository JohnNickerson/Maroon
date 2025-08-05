using AssimilationSoftware.Maroon.Mappers.Csv;
using Xunit;

namespace UnitTests
{
    
    public class TokeniseTests
    {
        [Fact]
        public void TestTokenise()
        {
            var tokens =
                "438dd600-f852-47f3-9fb9-61e9218e87df,FileHeader,Synchronised,e8c93e42-9e3d-476d-96bb-c040e5a37e06,106801557,6B-2E-3C-2E-64-97-EB-7E-05-5D-6E-1D-20-91-24-41-FD-B4-1C-48,\"Archer (2009)\\Season 8\\Archer (2009).S08E07.Archer Dreamland Gramercy, Halberd!.mkv\",2021-06-18T12:10:25.6212753+10:00,False,dfb61dd3-bb8e-44af-87f9-8baa779ab0f8,,,G:\\TV Backup,false,0001-01-01T00:00:00.0000000"
                    .Tokenise();
            Assert.Equal(15, tokens.Count);
            Assert.True(Guid.TryParse(tokens[0], out _));
            Assert.Equal("Archer (2009)\\Season 8\\Archer (2009).S08E07.Archer Dreamland Gramercy, Halberd!.mkv", tokens[6]);
        }
    }
}
