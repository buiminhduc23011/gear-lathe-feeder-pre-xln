namespace Server.Api.Services;

public enum DeleteUserResult
{
    Deleted = 0,
    NotFound = 1,
    SystemAccountProtected = 2
}
