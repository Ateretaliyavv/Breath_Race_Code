mergeInto(LibraryManager.library, {
  WebSerial_IsSupported: function () {
    return (typeof navigator !== "undefined" && navigator.serial) ? 1 : 0;
  },

  WebSerial_Connect: function (goPtr, onDataPtr, onStatusPtr) {
    var goName = UTF8ToString(goPtr);
    var onData = UTF8ToString(onDataPtr);
    var onStatus = UTF8ToString(onStatusPtr);

    if (!navigator.serial) {
      SendMessage(goName, onStatus, "WebSerial not supported (use Chrome/Edge).");
      return;
    }

    // Keep state in Module
    if (!Module.webSerial) Module.webSerial = {};
    var S = Module.webSerial;

    // Persist callbacks/target so scene changes won't break JS state.
    S.goName = goName;
    S.onData = onData;
    S.onStatus = onStatus;

    // Optional fixed fallback target name (recommended):
    // If your receiver GameObject is always named "BreathUSB", this keeps working even if
    // you pass a different name from C# or if a scene accidentally renames something.
    S.fallbackGoName = "BreathUSB";

    // Safe SendMessage wrapper:
    //Try the provided goName
    //If it fails (object not found), try fallbackGoName
    function safeSend(method, message) {
      try {
        SendMessage(S.goName, method, message);
        return;
      } catch (e1) {
        // Ignore and try fallback
      }
      try {
        SendMessage(S.fallbackGoName, method, message);
      } catch (e2) {
        // If both fail, there's no receiver object alive right now.
        // We intentionally do not throw to keep the read loop alive.
      }
    }

    // Prevent double-connect: if already connected/reading, disconnect first.
    (async function () {
      try {
        // If we already have a port/reader, shut it down cleanly first.
        if (S.port || S.reader) {
          S.keepReading = false;

          if (S.reader) {
            try { await S.reader.cancel(); } catch (e) {}
            try { S.reader.releaseLock(); } catch (e) {}
            S.reader = null;
          }

          if (S.port) {
            try { await S.port.close(); } catch (e) {}
            S.port = null;
          }
        }

        S.keepReading = true;

        safeSend(S.onStatus, "Requesting serial port...");

        // Must be called from a user gesture (button click).
        S.port = await navigator.serial.requestPort();

        // Make baud rate explicit (matches your ESP32 Serial.begin(115200)).
        await S.port.open({ baudRate: 115200 });

        safeSend(S.onStatus, "Serial connected (115200).");

        const decoder = new TextDecoder();
        S.reader = S.port.readable.getReader();

        let buffer = "";

        while (S.keepReading) {
          const { value, done } = await S.reader.read();
          if (done) break;
          if (!value) continue;

          buffer += decoder.decode(value, { stream: true });

          // Split by newline into complete lines
          let idx;
          while ((idx = buffer.indexOf("\n")) >= 0) {
            let line = buffer.slice(0, idx).trim();
            buffer = buffer.slice(idx + 1);

            if (line.length > 0) {
              safeSend(S.onData, line);
            }
          }
        }

        safeSend(S.onStatus, "Serial read loop stopped.");
      } catch (e) {
        safeSend(S.onStatus, "Serial error: " + (e && e.message ? e.message : e));
      }
    })();
  },

  WebSerial_Disconnect: function () {
    (async function () {
      try {
        if (!Module.webSerial) return;
        var S = Module.webSerial;

        S.keepReading = false;

        if (S.reader) {
          try { await S.reader.cancel(); } catch (e) {}
          try { S.reader.releaseLock(); } catch (e) {}
          S.reader = null;
        }

        if (S.port) {
          try { await S.port.close(); } catch (e) {}
          S.port = null;
        }
      } catch (e) {}
    })();
  }
});
