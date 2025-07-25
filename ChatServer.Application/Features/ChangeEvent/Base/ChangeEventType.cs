namespace ChatServer.Application.Features.ChangeEvent.Base;

public enum ChangeEventType
{
    ProfilePictureChanged,
    DisplayNameChanged,
    StatusChanged,
    FriendRequestSent,
    FriendRequestAccepted,
    FriendRequestRejected,
    FriendRemoved,
    UserBlocked,
    UserUnblocked,
}