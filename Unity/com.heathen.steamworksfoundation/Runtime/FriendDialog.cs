#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)

namespace HeathenEngineering.SteamworksIntegration
{
    public enum FriendDialog
    {
        /// <summary>
        /// Opens the overlay web browser to the specified user or groups profile.
        /// </summary>
        steamid,
        /// <summary>
        /// Opens a chat window to the specified user, or joins the group chat.
        /// </summary>
        chat,
        /// <summary>
        /// Opens a window to a Steam Trading session that was started with the ISteamEconomy/StartTrade Web API.
        /// </summary>
        jointrade,
        /// <summary>
        /// Opens the overlay web browser to the specified user's stats.
        /// </summary>
        stats,
        /// <summary>
        /// Opens the overlay web browser to the specified user's achievements.
        /// </summary>
        achievements,
        /// <summary>
        /// Opens the overlay in minimal mode prompting the user to add the target user as a friend.
        /// </summary>
        friendadd,
        /// <summary>
        /// Opens the overlay in minimal mode prompting the user to remove the target friend.
        /// </summary>
        friendremove,
        /// <summary>
        /// Opens the overlay in minimal mode prompting the user to accept an incoming friend invite.
        /// </summary>
        friendrequestaccept,
        /// <summary>
        /// Opens the overlay in minimal mode prompting the user to ignore an incoming friend invite.
        /// </summary>
        friendrequestignore,
    }
    //*/

}
#endif