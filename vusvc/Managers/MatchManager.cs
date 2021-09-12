using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Managers
{
    public class MatchManager : IMatchManager
    {
        /*
         * Phew, this class won't be that complicated to write just complicated to balance everything
         * 
         * This will need to be able to form a game, create a server, move all players into that server and handle the results of that match.
         * 
         * We are source player lobbies from anywhere, it should be platform agnostic. This means that we will need to be able to handle people who haave created lobby
         * on the website, in discord, or in a lobby server in VU itself. This poses somewhat of a challenge, but this design should be able to handle all of this with no issue
         * 
         * Player -> Creates Lobby on backend
         * Player -> Searches for a match
         * 
         * MatchManager -> Throws Lobby into a queue pool
         * MatchManager -> Starts match matchmaking timer (2-5m before game launches regardless if conditions are met)
         * MatchManager -> Checks the number of players and lock status of all lobbies in the queue pool
         * MatchManager -> Checks to see if there are enough players total to form teams
         * MatchManager -> Repeats step 3, 4 until conditions are met
         * MatchManager -> Forms teams and saves them to the match backend
         * MatchManager -> Creates a new server, or utilizes an idle/non-used server
         * MatchManager -> Waits for the server to request for the team/match information
         * MatchManager -> Sends team information to server
         * 
         * MatchManager -> Notifies all players in a lobby that their game is ready, if they are in-game already in a VU lobby server force join them
         * 
         * BR Server -> When join wait time (5m) is complete round restart and run the game
         * 
         * BR Server -> On round over send all stats, players, information to MatchManager
         * 
         * BR Server -> Kicks all players or attempts to re-join them to a lobby server (they will still be in a lobby, so if the leader searches again, it will pull everyone into the game server)
         * 
         * MatchManager -> Saves all stats, moves all lobbies back to the queue (provided they aren't expired)
        */

        private Queue<Guid> m_QueuedLobbyIds;
    }
}
