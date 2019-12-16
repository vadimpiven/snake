# Snake Game
Game is built using client-server architecture. Backend implements two methods:
- `[GET] api/gameboard` - returns current game state
- `[POST] api/direction` - changes direction of snake movement

## Frontend
Game frontend is written with Vue.js, to start is - just open [index.html](Frontend/WEB/index.html)
with your browser. Libraries used:
- axios - for AJAX requests
- Bootstrap 4 - for styling

Another version of frontend is made using WPF following MVVM metodology.

SVG icons were taken from [this website](https://www.flaticon.com/).

## Backend
Game backend is written as .Net Core API app, to start it - execute `dotnet run` in [backend folder](Backend) (installed .Net Core 3.0 required).

### Documentation
When backend is started in Development mode - Swagger endpoint is created at `api/swagger`
and opened automatically (NSwag library used).

### Logging
As gathering statistics is essential for any app, structured logging was implemented using NLog library.
Log output could be changed in [NLog.congif](HW1/NLog.config) using [documentation](https://nlog-project.org/config/?tab=layout-renderers&search=package:nlog.web.aspnetcore).
For use with Docker - healthcheck and Prometheus endpoints should be created in addition to logging.

### TODO
Backend CORS policy for production must be configured, but proper configuration requires
accurate information about production environment, and so this was not done.

### Possible improvements
As this is the game - fast response is required, the best solution would be a socket usage
(SignalR library in this case), but it wouldn't correspond to the given technical specification
as it would be one complete method instead of separate GET & POST.

### Multiplayer
It would be rather easy task to add multiplayer to this game (we could just generate a GUID
for each new player and set a cookie with it to identify the game of the player,
stored in dictionary, new games must be stored in a pool like [this](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-create-an-object-pool),
when game is over - it must be returned), but it would cause some problems:
- more synchronization would be required which would significantly increase time lag
between the client and server as we are not using sockets
- if some user would accidentally send two requests for the new game one by one - server would create more game instances then required, so it's a possibility for DDoS attacks

Possible solutions for the second problem are:
- move the whole game to the client side as the game is very simple and intended for single player
- add proper authorization (database is required) so that we can discard all anonymous connections
and so get rid of possibility of DDoS attack

None of this is possible with given technical specification.

Another solution would be to debounce (discard all calls for some time after previous call -
time should be a bit longer then it is required to create new game) the method, creating new games,
and limit the number of active games, but it would cause dropping of harmless connections sometimes
which is not so good.

That is why multiplayer was not implemented in this particular program.
