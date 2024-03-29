﻿using System.Net;

namespace RakSharp.Networking.Session;

/// <summary>
/// A pong result from a RakNet <see cref="IPEndPoint"/>.
/// </summary>
/// <param name="Message">The pong's message.</param>
public sealed record StatusResponse(string Message);