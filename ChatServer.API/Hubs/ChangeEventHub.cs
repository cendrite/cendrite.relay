using ChatServer.API.Clients;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ChatServer.API.Hubs;

[Authorize]
public class ChangeEventHub(
    ISender sender,
    ILogger<BaseHub<IChangeEventClient>> logger)
    : BaseHub<IChangeEventClient>(sender, logger)
{ }