# 1. Implement Account Management using SQLite

02-03-2026

## Context

For account management and auth, a database is required.

## Decision

SQLite will be used for the current state of prototyping with generic queries.

## Alternatives

Production SQL database

## Decision

Although possible to directly begin by connecting to a production database, SQLite offers easier and faster iteration of prototyping. It is ideal for the current state of the project

## Consequences

SignalR is implemented; first usable server prototype is developed quickly and multiple game sessions are supported.