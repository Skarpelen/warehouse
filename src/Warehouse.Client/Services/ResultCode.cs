namespace Warehouse.Client.Services
{
    public enum ResultCode
    {
        Ok = 0,
        Error = 1,
        Unauthorized = 2,
        TwoFactorRequired = 3,
    }
}
