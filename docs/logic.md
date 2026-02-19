Generally, when making a move: Request(FEN) -> Parse to int -> CheckValidMoves in Board -> yes, update and return; no, game state remains unchanged.


When making an undo state, there are several cases to consider:
- Base: just put the piece back to its origin
- Capture: put the piece back and put back the captured
- EnPassant: the special square is embedded in the board
- Halfmove clock: Will worry about this later, when making the game loop

Have to be compatible with UCI protocol.


SEND THE CASTLING ROOK MOVES AS KING MOVES. THIS REQUIRES FIXING.