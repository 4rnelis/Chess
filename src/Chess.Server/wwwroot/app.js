const statusEl = document.getElementById("status");
const connectButton = document.getElementById("connectButton");
const queueButton = document.getElementById("queueButton");
const refreshButton = document.getElementById("refreshButton");
const moveButton = document.getElementById("moveButton");
const uciInput = document.getElementById("uciInput");
const boardEl = document.getElementById("board");
const gameIdEl = document.getElementById("gameId");
const playerColorEl = document.getElementById("playerColor");
const opponentIdEl = document.getElementById("opponentId");
const lastMoveEl = document.getElementById("lastMove");
const sideToMoveEl = document.getElementById("sideToMove");
const castlingEl = document.getElementById("castling");
const enPassantEl = document.getElementById("enPassant");
const messagesEl = document.getElementById("messages");

let connection = null;
let gameId = null;
let playerColor = null;

if (typeof signalR === "undefined") {
  setStatus("SignalR client missing");
  addMessage("SignalR client not loaded. Check network access to the CDN.");
  connectButton.disabled = true;
}

const pieceLabels = {
  wP: "P",
  wN: "N",
  wB: "B",
  wR: "R",
  wQ: "Q",
  wK: "K",
  bP: "p",
  bN: "n",
  bB: "b",
  bR: "r",
  bQ: "q",
  bK: "k"
};

function addMessage(text) {
  const div = document.createElement("div");
  div.className = "message";
  div.textContent = text;
  messagesEl.prepend(div);
}

function setStatus(text) {
  statusEl.textContent = text;
}

function setConnected(connected) {
  queueButton.disabled = !connected;
  refreshButton.disabled = !connected;
  moveButton.disabled = !connected;
  connectButton.disabled = connected;
}

function renderBoard(board) {
  boardEl.innerHTML = "";
  for (let i = 0; i < 64; i++) {
    const square = document.createElement("div");
    const rank = Math.floor(i / 8);
    const file = i % 8;
    square.className = "square " + ((rank + file) % 2 === 0 ? "light" : "dark");

    const code = board[i];
    if (code && code !== "--") {
      const piece = document.createElement("div");
      const label = pieceLabels[code] || code;
      piece.className = "piece " + (code.startsWith("w") ? "white" : "black");
      piece.textContent = label;
      square.appendChild(piece);
    }

    boardEl.appendChild(square);
  }
}

function applyState(state) {
  gameId = state.gameId;
  gameIdEl.textContent = state.gameId;
  lastMoveEl.textContent = state.lastMoveUci || "-";
  sideToMoveEl.textContent = state.sideToMove;
  castlingEl.textContent = state.castlingRights;
  enPassantEl.textContent = state.enPassantSquare;
  renderBoard(state.board);
  if (state.isCheckmate) {
    addMessage("Checkmate.");
  }
}

async function connect() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chess")
    .withAutomaticReconnect()
    .build();

  connection.onreconnecting(() => {
    setStatus("Reconnecting...");
  });

  connection.onreconnected(() => {
    setStatus("Connected");
  });

  connection.onclose(() => {
    setStatus("Disconnected");
    setConnected(false);
  });

  connection.on("Queued", () => {
    setStatus("Queued");
    addMessage("Queued for a game.");
  });

  connection.on("GameStarted", (info) => {
    gameId = info.gameId;
    playerColor = info.youAre;
    gameIdEl.textContent = info.gameId;
    playerColorEl.textContent = info.youAre;
    opponentIdEl.textContent = info.opponentId;
    addMessage("Game started. You are " + info.youAre + ".");
  });

  connection.on("MoveApplied", (state) => {
    applyState(state);
  });

  connection.on("InvalidMove", (reason) => {
    addMessage("Invalid move: " + reason);
  });

  connection.on("OpponentDisconnected", () => {
    addMessage("Opponent disconnected.");
  });

  await connection.start();
  setStatus("Connected");
  setConnected(true);
}

connectButton.addEventListener("click", () => {
  connect().catch((err) => {
    setStatus("Failed to connect");
    addMessage("Connect failed: " + err.toString());
  });
});

queueButton.addEventListener("click", () => {
  if (!connection) return;
  connection.invoke("JoinQueue").catch((err) => {
    addMessage("Join queue failed: " + err.toString());
  });
});

refreshButton.addEventListener("click", () => {
  if (!connection || !gameId) return;
  connection.invoke("GetState", gameId).catch((err) => {
    addMessage("Refresh failed: " + err.toString());
  });
});

moveButton.addEventListener("click", () => {
  const uci = uciInput.value.trim();
  if (!connection || !gameId || uci.length < 4) return;
  connection.invoke("MakeMove", gameId, uci).catch((err) => {
    addMessage("Move failed: " + err.toString());
  });
  uciInput.value = "";
});
