# Transport-Booking-Tcp-CSharp

A C# desktop client-server application for managing transport seat reservations. The server handles concurrent clients and pushes real-time updates on reservation changes. The client is a WPF desktop app communicating over raw TCP with Protobuf serialization.

**Stack:** C# .NET, WPF, TCP sockets, Protobuf, SQLite, Serilog.

---

## Architecture

**Client** — `ServiceProxy` owns the TCP connection and a background `ReadLoop` thread. The reader routes incoming responses by type: push notifications go directly to the registered `IObserver` (the active `MainWindow`), everything else lands in a `BlockingCollection<Response>` consumed by `Exchange()`. UI updates from push arrive on the reader thread and are marshalled to WPF via `Dispatcher.Invoke`.

**Server** — `ConcurrentServer` accepts connections in a loop, handing each client to a `Worker` running on the .NET ThreadPool via `Task.Run`. The `Worker` implements `IObserver` and holds the client's `NetworkStream`. When a reservation change happens, `NetworkServiceImpl` calls `OnPushReceived` directly on each registered `Worker`, which writes the push packet to its own stream. Connected users are tracked in a `ConcurrentDictionary<long, IObserver>`.

**Shared** — Protobuf schema (`reservation.proto`), `ResponseFactory`, and `INetworkService` interface shared between client and server.

---

## Key Technical Decisions

**Protobuf framing** — `WriteDelimitedTo` / `ParseDelimitedFrom` handle length-prefixed framing over the raw TCP stream. No custom packet parser needed; Protobuf handles message boundaries.

**Push via Worker as IObserver** — rather than maintaining a separate notification queue or channel, the `Worker` itself implements `IObserver`. The push call writes directly to the stream it already owns. `lock(_stream)` on both the push path and the response path prevents concurrent writes from the ThreadPool push task and the worker's own response writes racing on the same stream.

**Double-booking prevention** — `reservationLock` in `NetworkServiceImpl` serializes reservation and cancellation operations at the application level. SQLite's default isolation under concurrent threads isn't sufficient on its own; the lock ensures seat availability checks and updates are atomic.

**Unit of Work transactions** — `TransactionManager.Run()` opens a connection, begins a transaction, and binds both to `[ThreadStatic]` fields. Repositories call `DatabaseConnection.GetConnection()` and transparently join the active transaction if one is bound, or get a standalone pooled connection otherwise. `ConnectionHolder` wraps the connection with an `owned` flag — non-owning holders returned during a transaction don't close the shared connection on `Dispose()`. On success `TransactionManager` commits; any exception triggers an automatic rollback.

**BlockingCollection with timeout** — `Exchange()` waits at most 10 seconds for a response. A server disconnect surfaces as a timeout exception rather than an infinite hang, giving the client a clean error path.

**lock(_stream) on send** — push notifications are dispatched via `Task.Run` on the ThreadPool and could race with the worker's own response writes. Both paths lock the stream before writing, keeping the Protobuf framing intact.

---

## Project Structure

```
Server/
  Network/    — ConcurrentServer, Worker, NetworkServiceImpl, ProtoUtils
  Service/    — FacadeService, AuthService, TripService, SeatService, ReservationService
  Repository/ — GenericRepository<T>, concrete repos, Filter builder
  Util/       — DatabaseConnection, ConnectionHolder, TransactionManager

Client/
  Network/    — ServiceProxy, INetworkService
  GUI/        — AppNavigator, LoginWindow, MainWindow

Shared/
  Proto/      — reservation.proto and generated classes
  Network/    — ResponseFactory, INetworkService, IObserver
```

---

## Setup

Prerequisites: .NET 10 SDK.

```bash
git clone https://github.com/LIgrigoriiciuc/Transport-Booking-Tcp-CSharp.git
cd Transport-Booking-Tcp-CSharp
dotnet restore
```

Run the server (initializes SQLite on first start):
```bash
cd Server && dotnet run
```

Run the client:
```bash
cd Client && dotnet run
```

Connection config in `server.properties`, `client.properties` and `appsettings.json`.

## Known Limitations
- No SSL/TLS — traffic is unencrypted over the wire
- Single server instance — no failover or horizontal scaling
- SQLite chosen for simplicity — would need Postgres for production load