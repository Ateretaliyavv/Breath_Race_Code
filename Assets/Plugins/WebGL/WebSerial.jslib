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

    S.goName = goName;
    S.onData = onData;
    S.onStatus = onStatus;
    S.keepReading = true;

    (async function () {
      try {
        SendMessage(goName, onStatus, "Requesting serial port...");
        S.port = await navigator.serial.requestPort(); // Must be called from a user gesture
        await S.port.open({ baudRate: 115200 });

        SendMessage(goName, onStatus, "Serial connected (115200).");

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
              SendMessage(goName, onData, line);
            }
          }
        }

        SendMessage(goName, onStatus, "Serial read loop stopped.");
      } catch (e) {
        SendMessage(goName, onStatus, "Serial error: " + (e && e.message ? e.message : e));
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
