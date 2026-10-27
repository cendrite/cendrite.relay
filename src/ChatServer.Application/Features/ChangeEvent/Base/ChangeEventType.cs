namespace ChatServer.Application.Features.ChangeEvent.Base;

public enum ChangeEventType
{
    AvatarChanged,
    DisplayNameChanged,
    StatusChanged,
    FriendRequestSent,
    FriendRequestAccepted,
    FriendRequestRejected,
    FriendRemoved,
    UserBlocked,
    UserUnblocked,
    UserLoggedOut,
    UserLoggedIn,
    Deleted,
}
