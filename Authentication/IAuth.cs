using System.Threading.Tasks;

namespace Todolist.Authentication
{
    public interface IAuth
    {
        /// <summary>
        ///     Gets a valid authentication token for the selected Authentication provider
        /// </summary>
        /// <remarks>
        ///     Used by the API request generators before making calls to the OneNote APIs.
        /// </remarks>
        /// <returns>valid authentication token</returns>
        Task<string> GetAuthToken();
        
        Task SignOut();

        bool IsSignedIn();

        Task<string> GetUserName();
    }
}
