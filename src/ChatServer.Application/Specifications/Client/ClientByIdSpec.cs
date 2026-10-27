using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientByIdSpec(Guid id) : Specification<Domain.Aggregates.Client>(c => c.Id == id);
