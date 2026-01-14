mergeInto(LibraryManager.library, {
  WebSerial_IsSupported: function () {
    return (typeof navigator !== "undefined" && navigator.serial) ? 1 : 0;
  },

  WebSerial_Connect: function (goPtr, onDataPtr, onStatusPtr) {
    var goName = UTF8ToString(goPtr);
    var onData = UTF8ToString(onDataPtr);
    var onStatus = UTF8ToString(onStatusPtr);

    if (!navigator.serial) {
      try { SendMessage(goName, onStatus, "WebSerial not supported (use Chrome/Edge)."); } catch (e) {}
      return;
    }

    if (!window.__webSerialUnity) window.__webSerialUnity = {};
    var S = window.__webSerialUnity;

    S.goName = goName;
    S.onData = onData;
    S.onStatus = onStatus;

    S.fallbackGoName = "BreathUSB";

    function safeSend(method, message) {
      try {
        SendMessage(S.goName, method, message);
        return;
      } catch (e1) {}

      try {
        SendMessage(S.fallbackGoName, method, message);
      } catch (e2) {}
    }

    (async function () {
      try {
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

        S.port = await navigator.serial.requestPort();

        await S.port.open({ baudRate: 115200 });

        // NEW: detect physical USB disconnect as an event (more reliable than waiting for read error)
        try {
          S.port.addEventListener("disconnect", () => {
            try { S.keepReading = false; } catch (e) {}
            safeSend(S.onStatus, "USB disconnected.");
          });
        } catch (e) {}

        safeSend(S.onStatus, "Serial connected (115200).");

        const decoder = new TextDecoder();
        S.reader = S.port.readable.getReader();

        let buffer = "";

        while (S.keepReading) {
          const { value, done } = await S.reader.read();
          if (done) break;
          if (!value) continue;

          buffer += decoder.decode(value, { stream: true });

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
        if (!window.__webSerialUnity) return;
        var S = window.__webSerialUnity;

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
