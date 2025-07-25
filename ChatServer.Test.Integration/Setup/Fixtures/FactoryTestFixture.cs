using ChatServer.Infrastructure.Persistence.Context;

namespace ChatServer.Test.Integration.Setup.Fixtures;

[CollectionDefinition("ChatServerApplication")]
public class FactoryTestFixture : ICollectionFixture<IntegrationTestFactory<Program, ChatServerDbContext>>
{

}