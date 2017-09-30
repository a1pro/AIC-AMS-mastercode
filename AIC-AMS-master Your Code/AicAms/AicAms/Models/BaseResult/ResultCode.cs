namespace AicAms.Models.BaseResult
{
    public enum ResultCode
    {
        Cancelled = -1,

        UnknownError = 0,

        Success = 1,

        NoAccess = 2,

        AuthError = 3,

        AuthTokenError = 4
    }
}