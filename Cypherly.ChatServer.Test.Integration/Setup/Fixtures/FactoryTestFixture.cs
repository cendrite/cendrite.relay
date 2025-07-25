using Cypherly.ChatServer.Infrastructure.Persistence.Context;

namespace Cypherly.ChatServer.Test.Integration.Setup.Fixtures;

[CollectionDefinition("ChatServerApplication")]
public class FactoryTestFixture : ICollectionFixture<IntegrationTestFactory<Program, ChatServerDbContext>>
{

}