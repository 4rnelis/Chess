# 1. Implement Client-Server Communication using SignalR

02-03-2026

## Status

Accepted

## Context

When implementing multiplayer chess, the communication between clients has to happen in real time.

## Decision

SignalR will be used for Server-client stream communication.

## Alternatives

Web Sockets
SSE + HTTP POST
Long Polling

## Decision

SignalR offers an easier implementation than custom Web Sockets with an abstractions over reconnect logic, heartbeats and such. Furhtermore, if web sockets get blocked, it utilizes SSE or Long Polling. It is also easier to scale up in the future and has a wide support of tooling for .NET

## Consequences

SignalR is implemented; first usable server prototype is developed quickly and multiple game sessions are supported.