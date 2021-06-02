using System;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class MockObj : ModelObject
    {
        public MockObj()
        {
            ID = Guid.NewGuid();
        }
    }
}