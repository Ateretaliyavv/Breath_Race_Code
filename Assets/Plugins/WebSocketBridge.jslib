mergeInto(LibraryManager.library, {

  WS_Connect: function (urlPtr, WebSocketReceiver, OnWebSocketMessage) {
    var url = UTF8ToString(urlPtr);
    var goName = UTF8ToString(WebSocketReceiver);
    var methodName = UTF8ToString(OnWebSocketMessage);

    console.log("[WS_Connect] Connecting to", url, "for GameObject", goName);

    var ws = new WebSocket(url);
    ws.onopen = function () {
      console.log("[WebSocket] Connected to " + url);
    };

    ws.onclose = function () {
      console.log("[WebSocket] Closed " + url);
    };

    ws.onerror = function (e) {
      console.log("[WebSocket] Error: " + e);
    };

    ws.onmessage = function (e) {
      // Pass the message string to Unity C# method
      if (typeof e.data === "string") {
        SendMessage(goName, methodName, e.data);
      }
    };

    // Keep reference so we can close later if needed
    Module.websocket = ws;
  },

  WS_Close: function () {
    if (Module.websocket) {
      Module.websocket.close();
      Module.websocket = null;
    }
  }

});
