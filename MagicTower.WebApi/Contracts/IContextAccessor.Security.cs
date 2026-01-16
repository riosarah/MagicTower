//@CodeCopy
#if ACCOUNT_ON
namespace MagicTower.WebApi.Contracts
{
    partial interface IContextAccessor
    {
        string SessionToken { set; }
    }
}
#endif
