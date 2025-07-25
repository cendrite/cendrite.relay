using Cypherly.ChatServer.API.Clients;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Cypherly.ChatServer.API.Hubs;

[Authorize]
public class ChangeEventHub(
    ISender sender,
    ILogger<BaseHub<IChangeEventClient>> logger)
    : BaseHub<IChangeEventClient>(sender, logger)
{ }